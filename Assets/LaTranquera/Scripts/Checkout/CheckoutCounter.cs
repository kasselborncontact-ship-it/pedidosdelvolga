using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// El mostrador con la caja. Maneja la fila y la venta:
    ///  1) el cliente apoya sus productos
    ///  2) el jugador pasa cada uno por el lector (E) -> bip y se suma
    ///  3) el cliente paga en efectivo o con tarjeta
    ///  4) efectivo: guardás sus billetes (E), armás el vuelto billete por billete y apretás ENTREGAR
    ///     tarjeta: tipeás el monto en el posnet y OK
    /// </summary>
    public class CheckoutCounter : MonoBehaviour
    {
        public static CheckoutCounter I;

        [Header("Puntos")]
        public Transform customerPoint;     // donde se para el cliente que está pagando
        public Transform[] queuePoints;     // la fila
        public Transform itemArea;          // donde apoya los productos (lado del cliente)
        public Transform scanPoint;         // el lector
        public Transform bagPoint;          // donde van las cosas ya cobradas
        public Transform cashPoint;         // donde el cliente deja los billetes

        [Header("Partes")]
        public RegisterScreen screen;
        public CashDrawer drawer;
        public ChangeTray tray;
        public Posnet posnet;

        public enum Stage { Scanning, WaitingPayment, Cash, Card, Done }

        public class Transaction
        {
            public Customer customer;
            public List<CounterItem> items = new List<CounterItem>();
            public List<string> lines = new List<string>();
            public int total;
            public Stage stage;
            public List<int> paidBills = new List<int>();
            public int received;
            public bool cashStored;
            public float lastActionTime;
            public CustomerCash cashObject;
            public int mistakes;
            public int Change => received - total;
        }

        public Transaction Current { get; private set; }
        readonly List<Customer> line = new List<Customer>();
        readonly System.Random rnd = new System.Random();

        public Vector3 CustomerFacing => customerPoint != null ? customerPoint.forward : Vector3.forward;

        void Awake() => I = this;

        // ---------------- Fila ----------------
        public void Enqueue(Customer c) { if (!line.Contains(c)) line.Add(c); }
        public void Remove(Customer c) => line.Remove(c);
        public int IndexOf(Customer c) => line.IndexOf(c);
        public bool CanStart(Customer c) => Current == null && line.Count > 0 && line[0] == c;

        public Vector3 QueuePosition(int idx)
        {
            if (idx <= 0) return customerPoint.position;
            int q = Mathf.Min(idx - 1, queuePoints.Length - 1);
            Vector3 p = queuePoints[q].position;
            if (idx - 1 >= queuePoints.Length) p += Vector3.left * 0.8f * (idx - queuePoints.Length); // fila más larga
            return p;
        }

        // ---------------- Venta ----------------
        public void BeginTransaction(Customer c, List<string> products)
        {
            Current = new Transaction { customer = c, stage = Stage.Scanning, lastActionTime = Time.time };
            int i = 0;
            foreach (var pid in products)
            {
                var go = new GameObject("EnMostrador_" + pid);
                go.transform.SetParent(itemArea, false);
                int col = i % 4, row = i / 4;
                go.transform.localPosition = new Vector3((row % 2) * 0.16f - 0.08f, 0.01f * (row / 2), col * 0.17f - 0.25f);
                go.transform.localRotation = Quaternion.Euler(0, rnd.Next(-20, 20), 0);
                var it = go.AddComponent<CounterItem>();
                it.Setup(pid, this);
                Current.items.Add(it);
                i++;
            }
            c.Say(rnd.NextDouble() < 0.5 ? "¡Buen día! Llevo esto." : "Hola, ¿cómo andás? Esto nomás.");
            screen?.Refresh();
        }

        public void Scan(CounterItem item)
        {
            if (Current == null || Current.stage != Stage.Scanning || item.scanned) return;
            item.scanned = true;
            int price = PriceBook.Get(item.productId);
            Current.total += price;
            Current.lines.Add(ProductDB.NameOf(item.productId) + "|" + price);
            Current.lastActionTime = Time.time;
            item.MoveTo(scanPoint.position, bagPoint.position + new Vector3(0, 0.01f * Current.lines.Count, 0));
            Sfx.Play("beep", 0.7f);
            screen?.Refresh();

            bool all = true;
            foreach (var it in Current.items) if (!it.scanned) all = false;
            if (all) StartCoroutine(AskPayment());
        }

        IEnumerator AskPayment()
        {
            var t = Current;
            t.stage = Stage.WaitingPayment;
            yield return new WaitForSeconds(1.2f);
            if (Current != t) yield break;

            bool card = rnd.NextDouble() < (t.total < 1500 ? 0.15 : 0.4);
            if (card)
            {
                t.stage = Stage.Card;
                t.customer.Say("Te pago con tarjeta. Son " + Money.Fmt(t.total) + ", ¿no?");
                posnet?.SetWaiting(true);
            }
            else
            {
                t.stage = Stage.Cash;
                t.paidBills = Cash.CustomerPays(t.total, rnd);
                t.received = Cash.Sum(t.paidBills);
                var go = new GameObject("PlataDelCliente");
                go.transform.SetParent(cashPoint, false);
                t.cashObject = go.AddComponent<CustomerCash>();
                t.cashObject.Setup(t.paidBills, this);
                t.customer.Say(t.received == t.total ? "Tomá, justo." : "Tomá, te pago con " + Money.Fmt(t.received) + ".");
            }
            t.lastActionTime = Time.time;
            screen?.Refresh();
        }

        /// <summary>El jugador guarda en el cajón los billetes que dejó el cliente.</summary>
        public bool StoreCustomerCash()
        {
            if (Current == null || Current.cashStored) return false;
            if (!drawer.isOpen) { HUD.Toast("Abrí el cajón de la caja primero (E en el frente del cajón)"); return false; }
            foreach (var b in Current.paidBills) drawer.Add(b, 1);
            Current.cashStored = true;
            Current.lastActionTime = Time.time;
            Sfx.Play("cash", 0.5f);
            screen?.Refresh();
            return true;
        }

        /// <summary>Botón ENTREGAR: el cliente cuenta el vuelto.</summary>
        public void GiveChange()
        {
            var t = Current;
            if (t == null || t.stage != Stage.Cash)
            {
                HUD.Toast(t != null && t.stage == Stage.Card ? "Este cliente paga con tarjeta: usá el posnet" : "No hay vuelto para entregar");
                return;
            }
            if (!t.cashStored) { HUD.Toast("Primero guardá en la caja los billetes del cliente"); return; }
            int given = tray.Total;
            int due = t.Change;
            t.lastActionTime = Time.time;
            if (given == due)
            {
                tray.GiveAway();
                Sfx.Play("ok");
                Complete(false);
            }
            else if (given < due)
            {
                t.mistakes++;
                Sfx.Play("error");
                t.customer.Say("¡Ey! Me diste " + Money.Fmt(given) + " y el vuelto es " + Money.Fmt(due) + ".");
            }
            else
            {
                t.mistakes++;
                Sfx.Play("error");
                t.customer.Say("Me estás dando de más: el vuelto es " + Money.Fmt(due) + ".");
            }
        }

        /// <summary>El posnet manda el monto tipeado. Devuelve true si se aprobó.</summary>
        public bool CardPayment(int typed)
        {
            var t = Current;
            if (t == null || t.stage != Stage.Card) return false;
            t.lastActionTime = Time.time;
            if (typed == t.total)
            {
                Economy.I.Deposit(t.total);
                Complete(true);
                return true;
            }
            t.mistakes++;
            t.customer.Say("Me salió rechazada... Son " + Money.Fmt(t.total) + ", fijate el monto.");
            return false;
        }

        void Complete(bool card)
        {
            var t = Current;
            t.stage = Stage.Done;
            Economy.I.RecordSale(t.total, card);
            t.customer.TransactionDone = true;
            t.customer.basket.Clear();
            t.customer.Say(t.mistakes == 0 ? "¡Gracias! Hasta luego." : "Bueno, gracias...");
            foreach (var it in t.items) if (it != null) Destroy(it.gameObject);
            if (t.cashObject != null) Destroy(t.cashObject.gameObject);
            line.Remove(t.customer);
            posnet?.SetWaiting(false);
            screen?.ShowThanks(t.total);
            Current = null;
        }

        /// <summary>El cliente se cansó y se fue a mitad de la venta.</summary>
        public void AbortTransaction(Customer c)
        {
            line.Remove(c);
            if (Current == null || Current.customer != c) return;
            tray.ReturnAllToDrawer();
            if (Current.cashStored) foreach (var b in Current.paidBills) drawer.Remove(b); // le devolvés la plata
            foreach (var it in Current.items) if (it != null) Destroy(it.gameObject);
            if (Current.cashObject != null) Destroy(Current.cashObject.gameObject);
            posnet?.SetWaiting(false);
            Current = null;
            screen?.Refresh();
        }

        public void ResetAll()
        {
            if (Current != null)
            {
                foreach (var it in Current.items) if (it != null) Destroy(it.gameObject);
                if (Current.cashObject != null) Destroy(Current.cashObject.gameObject);
            }
            Current = null;
            line.Clear();
            tray?.ReturnAllToDrawer();
            posnet?.SetWaiting(false);
            screen?.Refresh();
        }
    }
}
