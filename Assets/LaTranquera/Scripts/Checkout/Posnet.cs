using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LaTranquera
{
    /// <summary>
    /// Posnet para cobrar con tarjeta. Mirá cada tecla y apretá E para tipear el monto,
    /// después OK. El cliente pasa la tarjeta: si el monto es el correcto, APROBADA.
    /// Teclas: 0-9, BORRAR (borra todo), OK.
    /// El frente (teclado) mira hacia +Z local (hacia el jugador). La pantalla queda atrás.
    /// </summary>
    public class Posnet : MonoBehaviour
    {
        public CheckoutCounter counter;
        public Text display;
        string typed = "";
        bool waiting, busy;
        string status = "";

        void Awake()
        {
            if (transform.Find("Cuerpo") == null) Build();
            if (display == null) display = transform.Find("Pantalla").GetComponentInChildren<Text>(true);
            foreach (var k in GetComponentsInChildren<PosnetKey>(true)) k.posnet = this;
        }

        void Start() => Refresh();

        public void Build()
        {
            Color body = new Color(0.18f, 0.2f, 0.24f);
            Shapes.Box(transform, "Cuerpo", Vector3.zero, new Vector3(0.14f, 0.035f, 0.24f), body, true);
            Shapes.Box(transform, "PantallaMarco", new Vector3(0, 0.035f, -0.075f), new Vector3(0.12f, 0.004f, 0.07f), new Color(0.05f, 0.05f, 0.05f), false);
            var screen = Shapes.Empty(transform, "Pantalla", new Vector3(0, 0.041f, -0.075f));
            display = WorldLabel.Create(screen.transform, "", Vector3.zero, new Vector2(0.11f, 0.06f), 12, new Color(0.6f, 1f, 0.6f),
                Vector3.up, new Color(0.05f, 0.12f, 0.08f), Vector3.back);
            string[] keys = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "BORRAR", "0", "OK" };
            for (int i = 0; i < keys.Length; i++)
            {
                int col = i % 3, row = i / 3;
                var pos = new Vector3((col - 1) * 0.04f, 0.035f, -0.015f + row * 0.033f);
                Color kc = keys[i] == "OK" ? new Color(0.2f, 0.65f, 0.25f) : keys[i] == "BORRAR" ? new Color(0.8f, 0.2f, 0.2f) : new Color(0.85f, 0.85f, 0.85f);
                var key = Shapes.Box(transform, "Tecla_" + keys[i], pos, new Vector3(0.032f, 0.008f, 0.026f), kc, false);
                var bc = key.AddComponent<BoxCollider>();
                bc.size = new Vector3(1.15f, 3f, 1.2f);
                var pk = key.AddComponent<PosnetKey>();
                pk.key = keys[i];
                WorldLabel.Create(transform, keys[i] == "BORRAR" ? "C" : keys[i], pos + Vector3.up * 0.0085f, new Vector2(0.03f, 0.024f), 14,
                    Color.black, Vector3.up, null, Vector3.back);
            }
        }

        public void SetWaiting(bool w)
        {
            waiting = w;
            if (w) { typed = ""; status = "INGRESE MONTO"; }
            else if (!busy) status = "";
            Refresh();
        }

        public void Press(string key)
        {
            if (busy) return;
            Sfx.Play("key", 0.5f);
            if (key == "BORRAR") typed = "";
            else if (key == "OK") { StartCoroutine(Process()); return; }
            else if (typed.Length < 7) typed += key;
            Refresh();
        }

        IEnumerator Process()
        {
            if (!waiting || counter.Current == null || counter.Current.stage != CheckoutCounter.Stage.Card)
            {
                status = "SIN VENTA";
                Sfx.Play("error");
                Refresh();
                yield break;
            }
            int.TryParse(typed, out int amount);
            busy = true;
            status = "PASE LA TARJETA...";
            Refresh();
            yield return new WaitForSeconds(1.6f);
            busy = false;
            bool ok = counter.CardPayment(amount);
            status = ok ? "APROBADA" : "RECHAZADA";
            Sfx.Play(ok ? "ok" : "error");
            if (ok) waiting = false;
            typed = "";
            Refresh();
        }

        void Refresh()
        {
            if (display == null) return;
            string amount = typed.Length > 0 ? Money.Fmt(int.Parse(typed)) : "$ 0";
            display.text = (status.Length > 0 ? status + "\n" : "\n") + amount;
        }
    }
}
