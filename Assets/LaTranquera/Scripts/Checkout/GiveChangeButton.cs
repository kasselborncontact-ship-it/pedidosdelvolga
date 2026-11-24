using UnityEngine;

namespace LaTranquera
{
    /// <summary>Botón "ENTREGAR VUELTO" del mostrador: le das al cliente lo que armaste en la bandeja.</summary>
    public class GiveChangeButton : MonoBehaviour, IInteractable
    {
        public CheckoutCounter counter;

        public string GetName(UseContext c) => Loc.T("Botón ENTREGAR VUELTO");

        public string GetPrompt(UseContext c)
        {
            var t = counter.Current;
            if (t == null || t.stage != CheckoutCounter.Stage.Cash) return null;
            return "[E] Entregar " + Money.Fmt(counter.tray.Total) + " de vuelto";
        }

        public void Interact(UseContext c) => counter.GiveChange();
    }
}
