using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LaTranquera
{
    /// <summary>
    /// Bandejita del vuelto sobre el mostrador. Acá se juntan los billetes que sacás del cajón.
    /// E = devolver el último billete al cajón (si te equivocaste).
    /// </summary>
    public class ChangeTray : MonoBehaviour, IInteractable
    {
        public CashDrawer drawer;
        public Text totalLabel;
        readonly List<int> bills = new List<int>();
        Transform billsRoot;

        public int Total => Cash.Sum(bills);

        void Awake()
        {
            if (transform.Find("Bandeja") == null) Build();
            billsRoot = transform.Find("BilletesBandeja");
            if (totalLabel == null) totalLabel = GetComponentInChildren<Text>(true);
        }

        void Start() => Refresh();

        public void Build()
        {
            Shapes.Box(transform, "Bandeja", Vector3.zero, new Vector3(0.22f, 0.012f, 0.3f), new Color(0.3f, 0.3f, 0.32f), true);
            Shapes.Empty(transform, "BilletesBandeja", new Vector3(0, 0.014f, 0));
            totalLabel = WorldLabel.Create(transform, "", new Vector3(0, 0.08f, -0.12f), new Vector2(0.3f, 0.06f), 28, Color.white,
                new Vector3(1, 0.6f, 0), new Color(0, 0, 0, 0.7f));
        }

        public void Add(int denom)
        {
            bills.Add(denom);
            Refresh();
        }

        public void ReturnAllToDrawer()
        {
            foreach (var b in bills) drawer.Add(b, 1);
            bills.Clear();
            Refresh();
        }

        /// <summary>El vuelto se va con el cliente.</summary>
        public void GiveAway()
        {
            bills.Clear();
            Refresh();
        }

        void Refresh()
        {
            if (billsRoot == null) return;
            Shapes.DestroyChildren(billsRoot);
            for (int i = 0; i < bills.Count; i++)
                Cash.BillVisual(bills[i], billsRoot, new Vector3(0, i * 0.0025f, -0.09f + (i % 6) * 0.035f), 90 + (i % 3 - 1) * 6);
            if (totalLabel != null) totalLabel.text = Loc.T("Vuelto armado") + ": " + Money.Fmt(Total);
        }

        public string GetName(UseContext c) => Loc.T("Bandeja del vuelto") + ": " + Money.Fmt(Total);
        public string GetPrompt(UseContext c) => bills.Count > 0 && c.hands.IsEmpty ? "[E] Devolver el último billete al cajón" : null;

        public void Interact(UseContext c)
        {
            if (bills.Count == 0 || !c.hands.IsEmpty) return;
            int last = bills[bills.Count - 1];
            bills.RemoveAt(bills.Count - 1);
            drawer.Add(last, 1);
            Refresh();
        }
    }
}
