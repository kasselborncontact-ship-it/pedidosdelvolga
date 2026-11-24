using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LaTranquera
{
    /// <summary>
    /// Interfaz mínima: punto en el centro, nombre de lo que mirás y teclas,
    /// plata, hora y si el almacén está abierto. También los avisos cortos y la pantalla negra al dormir.
    /// Se arma sola por código.
    /// </summary>
    public class HUD : MonoBehaviour
    {
        public static HUD I;

        Text targetName, prompt, clock, money, store, held, toast, fadeText;
        Image fade, dot;
        GameObject pausePanel;
        readonly Queue<string> toasts = new Queue<string>();
        float toastUntil;

        void Awake()
        {
            I = this;
            var go = new GameObject("HUD", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            var t = go.transform;

            dot = UIKit.Panel(t, "Punto", new Color(1, 1, 1, 0.9f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-4, -4), new Vector2(4, 4));
            targetName = Outlined(UIKit.LabelAt(t, "", 26, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-600, -70), new Vector2(600, -20), TextAnchor.MiddleCenter));
            prompt = Outlined(UIKit.LabelAt(t, "", 22, new Color(1f, 0.92f, 0.6f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-600, -200), new Vector2(600, -72), TextAnchor.UpperCenter));

            var topLeft = UIKit.Panel(t, "Reloj", new Color(0, 0, 0, 0.45f), new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -110), new Vector2(380, -20));
            clock = UIKit.LabelAt(topLeft.transform, "", 30, Color.white, Vector2.zero, Vector2.one, new Vector2(16, 40), new Vector2(-10, -6));
            store = UIKit.LabelAt(topLeft.transform, "", 22, Color.white, Vector2.zero, Vector2.one, new Vector2(16, 4), new Vector2(-10, -50));
            store.supportRichText = true;

            var topRight = UIKit.Panel(t, "Plata", new Color(0, 0, 0, 0.45f), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-400, -110), new Vector2(-20, -20));
            money = UIKit.LabelAt(topRight.transform, "", 26, Color.white, Vector2.zero, Vector2.one, new Vector2(16, 4), new Vector2(-16, -6), TextAnchor.MiddleRight);

            held = Outlined(UIKit.LabelAt(t, "", 22, Color.white, new Vector2(0, 0), new Vector2(0, 0), new Vector2(24, 20), new Vector2(900, 60)));
            toast = Outlined(UIKit.LabelAt(t, "", 26, new Color(0.7f, 1f, 0.7f), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(-700, -200), new Vector2(700, -120), TextAnchor.MiddleCenter));

            fade = UIKit.Panel(t, "Fundido", new Color(0, 0, 0, 0), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            fade.raycastTarget = false;
            fadeText = UIKit.LabelAt(fade.transform, "", 34, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-700, -250), new Vector2(700, 250), TextAnchor.MiddleCenter);

            BuildPause(t);
        }

        static Text Outlined(Text t)
        {
            var o = t.gameObject.AddComponent<Outline>();
            o.effectColor = new Color(0, 0, 0, 0.85f);
            o.effectDistance = new Vector2(2, -2);
            return t;
        }

        void BuildPause(Transform t)
        {
            pausePanel = UIKit.Panel(t, "Pausa", new Color(0, 0, 0, 0.7f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
            var box = UIKit.Rect(pausePanel.transform, "Caja", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-260, -260), new Vector2(260, 260));
            UIKit.VList(box, 16, 20);
            UIKit.Size(UIKit.Label(box, "Pausa", 44, Color.white, TextAnchor.MiddleCenter), -1, 70);
            UIKit.Size(UIKit.Button(box, "Seguir jugando", new Color(0.2f, 0.5f, 0.3f), () => GameManager.I.TogglePause(), 26), -1, 64);
            UIKit.Size(UIKit.Button(box, "Guardar partida", new Color(0.2f, 0.35f, 0.6f), () => { SaveSystem.Save(); Toast("Partida guardada"); }, 26), -1, 64);
            UIKit.Size(UIKit.Button(box, "Nueva partida (borra la guardada)", new Color(0.6f, 0.4f, 0.1f), () => GameManager.I.NewGame(), 22), -1, 64);
            UIKit.Size(UIKit.Button(box, "Salir del juego", new Color(0.6f, 0.2f, 0.2f), () => GameManager.I.Quit(), 26), -1, 64);
            UIKit.Size(UIKit.Label(box, "WASD caminar · Shift correr · E agarrar/usar · G soltar · Clic usar herramienta · F abrir caja · Rueda precio (etiquetadora) · F5 guardar", 18, new Color(0.85f, 0.85f, 0.85f), TextAnchor.MiddleCenter), -1, 70);
            pausePanel.SetActive(false);
        }

        public void ShowPause(bool show) => pausePanel.SetActive(show);

        public static void Toast(string msg)
        {
            if (I == null) { Debug.Log(msg); return; }
            I.toasts.Enqueue(Loc.T(msg));
        }

        void Update()
        {
            targetName.text = Interactor.CurrentName ?? "";
            prompt.text = Interactor.CurrentPrompt ?? "";
            dot.color = Interactor.CurrentName != null ? new Color(1f, 0.85f, 0.3f, 1f) : new Color(1, 1, 1, 0.8f);

            var tm = TimeManager.I;
            if (tm != null) clock.text = Loc.F("Día {0}", tm.Day) + "   " + tm.Clock;
            if (StoreManager.I != null)
                store.text = Loc.T("Almacén") + ": " + (StoreManager.I.isOpen ? "<color=#7f7>" + Loc.T("ABIERTO") + "</color>" : "<color=#f77>" + Loc.T("CERRADO") + "</color>");
            if (Economy.I != null)
                money.text = Loc.T("Banco") + "  " + Money.Fmt(Economy.I.bank) + "\n" + Loc.T("Caja") + "  " + Money.Fmt(Economy.I.CashInDrawer);

            var h = PlayerHands.I != null ? PlayerHands.I.Held : null;
            held.text = Loc.F("En las manos: {0}", h != null ? h.GetName(new UseContext { hands = PlayerHands.I }) : Loc.T("Nada"));

            if (Time.time > toastUntil)
            {
                if (toasts.Count > 0) { toast.text = toasts.Dequeue(); toastUntil = Time.time + 3f; }
                else toast.text = "";
            }
        }

        /// <summary>Pantalla negra con un texto (para dormir).</summary>
        public IEnumerator FadeTo(float alpha, float seconds, string text = null)
        {
            if (text != null) fadeText.text = text;
            float start = fade.color.a, t = 0;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.Lerp(start, alpha, t / seconds);
                fade.color = new Color(0, 0, 0, a);
                fadeText.color = new Color(1, 1, 1, a);
                yield return null;
            }
            fade.color = new Color(0, 0, 0, alpha);
            fadeText.color = new Color(1, 1, 1, alpha);
        }
    }
}
