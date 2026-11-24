using UnityEngine;
using UnityEngine.UI;

namespace LaTranquera
{
    /// <summary>Compartimento de un billete dentro del cajón.</summary>
    public class BillSlot : MonoBehaviour, IInteractable
    {
        public int index;
        public CashDrawer drawer;
        public Transform stack;
        public Text label;

        int Denom => Cash.Denoms[index];

        public void Refresh()
        {
            if (drawer == null) return;
            int n = drawer.counts[index];
            if (label != null) label.text = Money.Fmt(Denom) + "\nx" + n;
            if (stack != null)
            {
                stack.gameObject.SetActive(n > 0);
                float h = Mathf.Clamp(n * 0.0015f, 0.003f, 0.05f);
                stack.localScale = new Vector3(stack.localScale.x, h, stack.localScale.z);
                stack.localPosition = new Vector3(0, h / 2, 0);
            }
        }

        public string GetName(UseContext c) => Loc.T("Billetes de") + " " + Money.Fmt(Denom) + " (x" + drawer.counts[index] + ")";

        public string GetPrompt(UseContext c)
        {
            if (!drawer.isOpen || !c.hands.IsEmpty) return null;
            return drawer.counts[index] > 0 ? "[E] Sacar un billete de " + Money.Fmt(Denom) + " para el vuelto" : "No quedan billetes de " + Money.Fmt(Denom);
        }

        public void Interact(UseContext c)
        {
            if (!drawer.isOpen || !c.hands.IsEmpty) return;
            if (drawer.Remove(Denom))
            {
                drawer.tray.Add(Denom);
                Sfx.Play("key", 0.4f);
            }
        }
    }
}
