using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Hace aparecer clientes caminando por la banquina mientras es de día.
    /// Si el almacén está cerrado igual pasan, pero no entran.
    /// </summary>
    public class CustomerSpawner : MonoBehaviour
    {
        public static CustomerSpawner I;
        public int maxCustomers = 4;
        public float openHour = 7.5f, closeHour = 20.5f;
        [Tooltip("Segundos reales entre clientes (mínimo y máximo)")]
        public Vector2 interval = new Vector2(35f, 75f);

        float next;
        int seed = 1;

        void Awake() => I = this;
        void Start() => next = Time.time + 8f;

        void Update()
        {
            if (TimeManager.I == null || StoreNav.I == null) return;
            if (Time.time < next) return;
            float h = TimeManager.I.Hour;
            next = Time.time + Random.Range(interval.x, interval.y);
            if (h < openHour || h > closeHour) return;
            if (Customer.All.Count >= maxCustomers) return;
            // Si el almacén está cerrado pasan menos
            if (!StoreManager.I.isOpen && Random.value < 0.6f) return;
            Spawn();
        }

        public Customer Spawn()
        {
            bool west = Random.value < 0.5f;
            var nav = StoreNav.I;
            var go = new GameObject("Cliente_" + seed);
            go.transform.position = (west ? nav.spawnWest : nav.spawnEast).position;
            var c = go.AddComponent<Customer>();
            c.Init(seed++ * 7919 + System.Environment.TickCount, west);
            return c;
        }

        /// <summary>Al dormir se van todos (y devuelven lo que tenían en la canasta).</summary>
        public void ClearAll()
        {
            for (int i = Customer.All.Count - 1; i >= 0; i--)
            {
                var c = Customer.All[i];
                if (c.TransactionDone) { Destroy(c.gameObject); continue; }
                c.ReturnItems();
                Destroy(c.gameObject);
            }
            if (CheckoutCounter.I != null) CheckoutCounter.I.ResetAll();
        }
    }
}
