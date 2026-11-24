using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Un cliente. Llega caminando por la banquina, entra si está ABIERTO, recorre las
    /// estanterías buscando lo de su lista, mira el precio, hace la fila, apoya las cosas
    /// en el mostrador, paga y se va. Tiene paciencia limitada.
    /// </summary>
    public class Customer : MonoBehaviour
    {
        public float speed = 1.4f;
        public float patience = 90f;          // segundos reales esperando en la fila / caja
        public List<string> wishList = new List<string>();
        public List<string> basket = new List<string>();

        public bool ReadyAtCounter { get; private set; }
        public bool TransactionDone { get; set; }

        CustomerVisuals.Parts parts;
        SpeechBubble bubble;
        float walkPhase;
        bool moving;
        bool westSide;
        System.Random rnd;

        public static readonly List<Customer> All = new List<Customer>();

        public void Init(int seed, bool fromWest)
        {
            rnd = new System.Random(seed);
            westSide = fromWest;
            parts = CustomerVisuals.Build(transform, rnd);
            bubble = SpeechBubble.Create(transform, 2.15f);
            var col = gameObject.AddComponent<CapsuleCollider>();
            col.center = Vector3.up * 0.9f; col.height = 1.8f; col.radius = 0.25f;
            patience = 70f + (float)rnd.NextDouble() * 60f;
            BuildWishList();
            StartCoroutine(Run());
        }

        void OnEnable() => All.Add(this);
        void OnDisable() => All.Remove(this);

        public void Say(string msg, float seconds = 4f) => bubble.Say(msg, seconds);

        void BuildWishList()
        {
            var stocked = StoreManager.I.StockedProducts();
            int n = 1 + rnd.Next(3);
            for (int i = 0; i < n; i++)
            {
                double roll = rnd.NextDouble();
                if (roll < 0.7 && stocked.Count > 0) wishList.Add(stocked[rnd.Next(stocked.Count)]);
                else if (roll < 0.9) wishList.Add(ProductDB.All[rnd.Next(ProductDB.All.Count)].id);
                else wishList.Add("?" + ProductDB.NotYetSold[rnd.Next(ProductDB.NotYetSold.Length)]);
            }
        }

        // ---------------- Movimiento ----------------
        IEnumerator WalkTo(Vector3 target)
        {
            moving = true;
            while (!StepTowards(target))
            {
                yield return null;
            }
            moving = false;
        }

        /// <summary>Un paso hacia el destino. Devuelve true si llegó.</summary>
        bool StepTowards(Vector3 target)
        {
            var to = target - transform.position;
            to.y = 0;
            if (to.sqrMagnitude < 0.0025f) { moving = false; return true; }
            moving = true;
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            var look = Quaternion.LookRotation(new Vector3(to.x, 0, to.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 10f * Time.deltaTime);
            return false;
        }

        void Face(Vector3 worldDir)
        {
            worldDir.y = 0;
            if (worldDir.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(worldDir);
        }

        void Update()
        {
            // Animación simple de caminar: piernas y brazos van y vienen
            walkPhase += (moving ? 8f : 0f) * Time.deltaTime;
            float a = moving ? Mathf.Sin(walkPhase) * 28f : 0f;
            if (parts?.legL != null)
            {
                parts.legL.localRotation = Quaternion.Euler(a, 0, 0);
                parts.legR.localRotation = Quaternion.Euler(-a, 0, 0);
                parts.armL.localRotation = Quaternion.Euler(-a * 0.7f, 0, 0);
                parts.armR.localRotation = Quaternion.Euler(a * 0.5f, 0, 0);
            }
        }

        // ---------------- Comportamiento ----------------
        IEnumerator Run()
        {
            var nav = StoreNav.I;
            // Llega por la banquina
            yield return WalkTo((westSide ? nav.curbWest : nav.curbEast).position);
            yield return WalkTo(nav.parking.position + new Vector3((float)rnd.NextDouble() - 0.5f, 0, 0));
            yield return WalkTo(nav.doorOutside.position);

            if (!StoreManager.I.isOpen)
            {
                Say("Uh, está cerrado...");
                yield return new WaitForSeconds(2f);
                yield return Leave(false);
                yield break;
            }

            yield return WalkTo(nav.doorInside.position);
            yield return Shop();

            if (basket.Count == 0)
            {
                yield return new WaitForSeconds(1f);
                Say("Bueno, otra vez será.");
                Economy.I.today.angry++;
                yield return Leave(true);
                yield break;
            }

            // Hacer la fila
            var co = CheckoutCounter.I;
            co.Enqueue(this);
            float waited = 0f;
            bool complainedOnce = false;
            while (true)
            {
                int idx = co.IndexOf(this);
                Vector3 target = co.QueuePosition(idx);
                bool arrived = StepTowards(target);
                if (arrived) Face(idx == 0 ? co.CustomerFacing : (co.QueuePosition(idx - 1) - transform.position));
                if (idx == 0 && arrived && co.CanStart(this)) break;
                waited += Time.deltaTime;
                if (!complainedOnce && waited > patience * 0.6f) { Say("¿Falta mucho?"); complainedOnce = true; }
                if (waited > patience)
                {
                    Say("¡Me cansé de esperar!");
                    Economy.I.today.angry++;
                    co.Remove(this);
                    ReturnItems();
                    yield return Leave(true);
                    yield break;
                }
                yield return null;
            }

            // En el mostrador
            ReadyAtCounter = true;
            parts.basket.gameObject.SetActive(false);
            co.BeginTransaction(this, basket);
            float counterWait = 0f;
            while (!TransactionDone)
            {
                counterWait += Time.deltaTime * (co.Current != null && co.Current.lastActionTime > Time.time - 8f ? 0.2f : 1f);
                if (counterWait > patience * 1.2f)
                {
                    Say("¡Me voy, no me atienden!");
                    Economy.I.today.angry++;
                    co.AbortTransaction(this);
                    ReturnItems();
                    yield return Leave(true);
                    yield break;
                }
                yield return null;
            }
            yield return new WaitForSeconds(1.2f);
            yield return Leave(true);
        }

        IEnumerator Shop()
        {
            foreach (var want in wishList)
            {
                if (want.StartsWith("?"))
                {
                    Say("¿No tenés " + want.Substring(1) + "?");
                    yield return new WaitForSeconds(2.5f);
                    continue;
                }
                var shelf = StoreManager.I.FindShelfWith(want);
                if (shelf == null)
                {
                    Say("¿No te queda " + ProductDB.NameOf(want).ToLower() + "?");
                    yield return new WaitForSeconds(2.5f);
                    continue;
                }
                Vector3 stand = shelf.customerPoint != null ? shelf.customerPoint.position : shelf.transform.position + shelf.transform.forward;
                yield return WalkTo(stand);
                Face(shelf.transform.position - transform.position);
                yield return new WaitForSeconds(1f + (float)rnd.NextDouble());

                // ¿Está caro?
                float ratio = PriceBook.Ratio(want);
                double buyChance = ratio <= 1.05f ? 1.0 : ratio <= 1.35f ? Mathf.Lerp(0.85f, 0.15f, (ratio - 1.05f) / 0.3f) : 0.0;
                if (rnd.NextDouble() > buyChance)
                {
                    Say(ratio > 1.35f ? "¡¿" + Money.Fmt(PriceBook.Get(want)) + " " + ProductDB.NameOf(want).ToLower() + "?! ¡Qué caro!"
                                      : "Mmm, está caro esto...");
                    Economy.I.today.angry++;
                    yield return new WaitForSeconds(2f);
                    continue;
                }
                int qty = 1 + (rnd.NextDouble() < 0.35 ? 1 : 0) + (ratio < 0.85f ? 1 : 0);
                for (int i = 0; i < qty; i++)
                {
                    if (!shelf.TakeOne(want)) break;
                    basket.Add(want);
                    parts.basket.gameObject.SetActive(true);
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        public void ReturnItems()
        {
            foreach (var pid in basket)
            {
                bool done = false;
                foreach (var s in Shelf.All) if (s.PutBack(pid)) { done = true; break; }
                if (!done) Debug.Log("No hubo lugar para devolver " + pid);
            }
            basket.Clear();
        }

        IEnumerator Leave(bool fromInside)
        {
            var nav = StoreNav.I;
            ReadyAtCounter = false;
            if (parts != null) parts.basket.gameObject.SetActive(TransactionDone); // se va con la compra
            if (fromInside)
            {
                yield return WalkTo(nav.doorInside.position);
                yield return WalkTo(nav.doorOutside.position);
            }
            bool west = rnd.NextDouble() < 0.5;
            yield return WalkTo(nav.parking.position);
            yield return WalkTo((west ? nav.curbWest : nav.curbEast).position);
            yield return WalkTo((west ? nav.spawnWest : nav.spawnEast).position);
            Destroy(gameObject);
        }
    }
}
