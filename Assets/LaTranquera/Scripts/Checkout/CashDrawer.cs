using UnityEngine;
using UnityEngine.UI;

namespace LaTranquera
{
    /// <summary>
    /// Cajón de la caja registradora con un compartimento por billete ($100 a $20.000).
    /// E en el frente para abrirlo o cerrarlo. Con el cajón abierto, E en un compartimento
    /// saca UN billete y lo pone en la bandeja del vuelto.
    /// El frente del cajón mira hacia +Z local (hacia el jugador).
    /// </summary>
    public class CashDrawer : MonoBehaviour, IInteractable
    {
        public static CashDrawer I;

        public int[] counts = new int[7];
        public bool isOpen;
        public ChangeTray tray;
        public float slideDistance = 0.3f;

        Transform slider;
        BillSlot[] slotsUI;
        float k;

        const float W = 0.63f, D = 0.36f, H = 0.08f;

        public int Total
        {
            get
            {
                int t = 0;
                for (int i = 0; i < counts.Length; i++) t += counts[i] * Cash.Denoms[i];
                return t;
            }
        }

        void Awake()
        {
            I = this;
            if (transform.Find("Corredera") == null) Build();
            slider = transform.Find("Corredera");
            slotsUI = GetComponentsInChildren<BillSlot>(true);
            foreach (var s in slotsUI) s.drawer = this;
        }

        void Start() => RefreshAll();

        public void Build()
        {
            slider = Shapes.Empty(transform, "Corredera", Vector3.zero).transform;
            Color body = new Color(0.25f, 0.25f, 0.27f);
            Shapes.Box(slider, "Fondo", new Vector3(0, 0, 0), new Vector3(W, 0.01f, D), body, false);
            Shapes.Box(slider, "LadoI", new Vector3(-W / 2, 0, 0), new Vector3(0.01f, H, D), body, false);
            Shapes.Box(slider, "LadoD", new Vector3(W / 2, 0, 0), new Vector3(0.01f, H, D), body, false);
            Shapes.Box(slider, "Atras", new Vector3(0, 0, -D / 2), new Vector3(W, H, 0.01f), body, false);
            var front = Shapes.Box(slider, "Frente", new Vector3(0, -0.01f, D / 2 + 0.01f), new Vector3(W + 0.02f, H + 0.03f, 0.02f), new Color(0.15f, 0.15f, 0.17f), true);
            Shapes.Box(front.transform.parent, "Manija", new Vector3(0, 0.02f, D / 2 + 0.025f), new Vector3(0.12f, 0.015f, 0.015f), new Color(0.7f, 0.7f, 0.7f), false);

            float sw = W / Cash.Denoms.Length;
            for (int i = 0; i < Cash.Denoms.Length; i++)
            {
                float x = -W / 2 + sw * (i + 0.5f);
                if (i > 0) Shapes.Box(slider, "Divisor", new Vector3(x - sw / 2, 0, 0), new Vector3(0.004f, H * 0.8f, D * 0.9f), new Color(0.35f, 0.35f, 0.37f), false);
                var slotGo = Shapes.Empty(slider, "Billetes_" + Cash.Denoms[i], new Vector3(x, 0.01f, -0.02f));
                var bs = slotGo.AddComponent<BillSlot>();
                bs.index = i;
                var bc = slotGo.AddComponent<BoxCollider>();
                bc.center = new Vector3(0, 0.03f, 0);
                bc.size = new Vector3(sw * 0.95f, 0.06f, D * 0.8f);
                var stack = Shapes.Prim(PrimitiveType.Cube, slotGo.transform, "Pila", new Vector3(0, 0.005f, 0), new Vector3(sw * 0.8f, 0.01f, 0.155f), Cash.ColorOf(Cash.Denoms[i]), false);
                bs.stack = stack.transform;
                bs.label = WorldLabel.Create(slotGo.transform, "", new Vector3(0, 0.065f, D * 0.42f), new Vector2(sw * 0.95f, 0.05f), 14, Color.white,
                    new Vector3(0, 1, 1), new Color(0, 0, 0, 0.6f));
            }
        }

        void Update()
        {
            k = Mathf.MoveTowards(k, isOpen ? 1 : 0, Time.deltaTime * 4f);
            if (slider != null) slider.localPosition = new Vector3(0, 0, k * slideDistance);
        }

        public void SetCounts(int[] c)
        {
            for (int i = 0; i < counts.Length && i < c.Length; i++) counts[i] = c[i];
            RefreshAll();
        }

        public void RefreshAll()
        {
            if (slotsUI == null) return;
            foreach (var s in slotsUI) s.Refresh();
            Economy.NotifyChanged();
        }

        public void Add(int denom, int n)
        {
            int i = Cash.IndexOf(denom);
            if (i < 0) return;
            counts[i] += n;
            RefreshAll();
        }

        public bool Remove(int denom)
        {
            int i = Cash.IndexOf(denom);
            if (i < 0 || counts[i] <= 0) return false;
            counts[i]--;
            RefreshAll();
            return true;
        }

        /// <summary>Saca todo lo que sobra del cambio para depositarlo en el banco.</summary>
        public int TakeForDeposit()
        {
            int[] keep = { 15, 15, 10, 10, 5, 0, 0 };
            int moved = 0;
            for (int i = 0; i < counts.Length; i++)
            {
                int extra = Mathf.Max(0, counts[i] - keep[i]);
                counts[i] -= extra;
                moved += extra * Cash.Denoms[i];
            }
            RefreshAll();
            return moved;
        }

        // --- IInteractable (el frente del cajón) ---
        public string GetName(UseContext c) => Loc.T("Cajón de la caja") + " (" + Money.Fmt(Total) + ")";
        public string GetPrompt(UseContext c) => isOpen ? "[E] Cerrar el cajón" : "[E] Abrir el cajón";
        public void Interact(UseContext c)
        {
            isOpen = !isOpen;
            Sfx.Play(isOpen ? "cash" : "drop", 0.4f);
        }
    }
}
