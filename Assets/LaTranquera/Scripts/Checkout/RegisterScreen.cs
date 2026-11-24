using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace LaTranquera
{
    /// <summary>Pantalla de la caja registradora: lista lo que pasaste por el lector, el total y el vuelto.</summary>
    public class RegisterScreen : MonoBehaviour
    {
        public CheckoutCounter counter;
        public Text text;
        float thanksUntil;
        int thanksTotal;

        void Awake()
        {
            if (transform.Find("Monitor") == null) Build();
            if (text == null) text = GetComponentInChildren<Text>(true);
        }

        void Start() => Refresh();

        /// <summary>Monitor sobre un pie. La pantalla mira hacia +Z local (hacia el jugador).</summary>
        public void Build()
        {
            Color dark = new Color(0.12f, 0.12f, 0.14f);
            Shapes.Box(transform, "Pie", Vector3.zero, new Vector3(0.12f, 0.12f, 0.1f), dark, true);
            var mon = Shapes.Empty(transform, "Monitor", new Vector3(0, 0.3f, 0), new Vector3(-12, 0, 0));
            Shapes.BoxC(mon.transform, "Marco", Vector3.zero, new Vector3(0.36f, 0.27f, 0.04f), dark, true);
            text = WorldLabel.Create(mon.transform, "", new Vector3(0, 0, 0.021f), new Vector2(0.33f, 0.24f), 13, new Color(0.85f, 1f, 0.85f),
                Vector3.forward, new Color(0.05f, 0.1f, 0.2f));
            text.alignment = TextAnchor.UpperLeft;
        }

        public void ShowThanks(int total)
        {
            thanksUntil = Time.time + 4f;
            thanksTotal = total;
            Refresh();
        }

        void Update()
        {
            if (thanksUntil > 0 && Time.time > thanksUntil) { thanksUntil = 0; Refresh(); }
        }

        public void Refresh()
        {
            if (text == null) return;
            var sb = new StringBuilder();
            sb.AppendLine(" ALMACÉN LA TRANQUERA");
            sb.AppendLine(" ------------------------------");
            var t = counter != null ? counter.Current : null;
            if (t == null)
            {
                if (thanksUntil > Time.time) sb.AppendLine("\n  ¡GRACIAS POR SU COMPRA!\n  Total cobrado: " + Money.Fmt(thanksTotal));
                else sb.AppendLine("\n  CAJA LIBRE");
                text.text = sb.ToString();
                return;
            }
            int start = Mathf.Max(0, t.lines.Count - 7);
            for (int i = start; i < t.lines.Count; i++)
            {
                var p = t.lines[i].Split('|');
                int.TryParse(p[1], out int price);
                sb.AppendLine(" " + p[0].PadRight(22).Substring(0, 22) + Money.Fmt(price).PadLeft(9));
            }
            int pending = 0;
            foreach (var it in t.items) if (!it.scanned) pending++;
            if (pending > 0) sb.AppendLine(" (faltan " + pending + " por pasar)");
            sb.AppendLine(" ------------------------------");
            sb.AppendLine(" TOTAL".PadRight(23) + Money.Fmt(t.total).PadLeft(9));
            if (t.stage == CheckoutCounter.Stage.Cash)
            {
                sb.AppendLine(" EFECTIVO".PadRight(23) + Money.Fmt(t.received).PadLeft(9));
                sb.AppendLine(" VUELTO".PadRight(23) + Money.Fmt(t.Change).PadLeft(9));
                if (!t.cashStored) sb.AppendLine(" > Guardá los billetes del cliente");
            }
            else if (t.stage == CheckoutCounter.Stage.Card)
                sb.AppendLine(" TARJETA: tipeá el monto en el posnet");
            text.text = sb.ToString();
        }
    }
}
