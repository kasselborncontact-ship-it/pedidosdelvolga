using System.Collections.Generic;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>Los billetes que el cliente dejó sobre el mostrador. E para guardarlos en el cajón.</summary>
    public class CustomerCash : MonoBehaviour, IInteractable
    {
        List<int> bills;
        CheckoutCounter counter;

        public void Setup(List<int> b, CheckoutCounter c)
        {
            bills = b;
            counter = c;
            for (int i = 0; i < bills.Count; i++)
                Cash.BillVisual(bills[i], transform, new Vector3(i * 0.012f, 0.003f + i * 0.0025f, i * 0.03f), 90 + i * 8);
            var bc = gameObject.AddComponent<BoxCollider>();
            bc.center = new Vector3(0, 0.02f, 0.05f);
            bc.size = new Vector3(0.2f, 0.05f, 0.25f);
            bc.isTrigger = true;
        }

        public string GetName(UseContext c) => Loc.T("Plata del cliente") + ": " + Money.Fmt(Cash.Sum(bills));
        public string GetPrompt(UseContext c) => c.hands.IsEmpty ? "[E] Guardar los billetes en la caja" : null;

        public void Interact(UseContext c)
        {
            if (!c.hands.IsEmpty) return;
            if (counter.StoreCustomerCash()) Destroy(gameObject);
        }
    }
}
