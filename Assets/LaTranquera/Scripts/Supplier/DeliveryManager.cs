using System;
using System.Collections.Generic;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Pedidos al proveedor. Cuando confirmás un pedido, la camioneta llega más o menos
    /// 1 hora y media después (de 8 a 19 h; si no, a la mañana siguiente) y deja las cajas
    /// en la puerta del galpón/depósito. Las tenés que entrar vos.
    /// </summary>
    public class DeliveryManager : MonoBehaviour
    {
        public static DeliveryManager I;

        [Serializable]
        public class OrderLine { public string entryId; public int qty; }

        [Serializable]
        public class Order
        {
            public List<OrderLine> lines = new List<OrderLine>();
            public double arriveAt;
        }

        public List<Order> pending = new List<Order>();
        public Transform dropPoint;
        public Transform[] truckRoute;   // entrada: desde la ruta hasta la puerta del galpón
        public Transform[] truckExit;    // salida
        public float deliveryMinutes = 90f;

        DeliveryTruck activeTruck;

        void Awake() => I = this;

        public void PlaceOrder(List<OrderLine> lines)
        {
            var o = new Order { lines = lines };
            var tm = TimeManager.I;
            double at = tm.totalMinutes + deliveryMinutes;
            float hourAt = (float)((at % 1440) / 60.0);
            if (hourAt < 8f || hourAt > 19f) at = tm.totalMinutes + tm.MinutesUntil(8.5f);
            o.arriveAt = at;
            pending.Add(o);
            HUD.Toast("Pedido confirmado. Llega: " + TimeManager.FormatClock(at));
        }

        void Update()
        {
            if (activeTruck != null || pending.Count == 0 || TimeManager.I == null) return;
            var o = pending[0];
            if (TimeManager.I.totalMinutes < o.arriveAt) return;
            pending.RemoveAt(0);
            var go = new GameObject("CamionetaProveedor");
            activeTruck = go.AddComponent<DeliveryTruck>();
            activeTruck.Run(this, o);
            HUD.Toast("¡Llegó la camioneta del proveedor!");
        }

        /// <summary>La camioneta llegó a la puerta: deja la mercadería en el piso.</summary>
        public void Unload(Order o)
        {
            int i = 0;
            foreach (var l in o.lines)
            {
                var e = SupplierCatalog.Get(l.entryId);
                if (e == null) continue;
                for (int q = 0; q < l.qty; q++)
                {
                    int row = i / 4, col = i % 4;
                    Vector3 p = dropPoint.position + dropPoint.right * (col - 1.5f) * 0.55f + dropPoint.forward * row * 0.45f + Vector3.up * 0.05f;
                    Quaternion r = dropPoint.rotation;
                    switch (e.kind)
                    {
                        case "box": WorldFactory.Box(e.refId, e.units, false, p, r); break;
                        case "seeds": WorldFactory.Seeds(e.refId, e.units, p + Vector3.up * 0.1f, r); break;
                        case "crate": WorldFactory.Crate(p, r); break;
                    }
                    i++;
                }
            }
        }
    }
}
