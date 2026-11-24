using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LaTranquera
{
    /// <summary>Ayudantes para armar interfaz (uGUI) por código sin tener que dibujarla a mano.</summary>
    public static class UIKit
    {
        public static Font Font => WorldLabel.UIFont;

        public static RectTransform Rect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offMin, Vector2 offMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = (RectTransform)go.transform;
            r.anchorMin = anchorMin; r.anchorMax = anchorMax;
            r.offsetMin = offMin; r.offsetMax = offMax;
            return r;
        }

        public static RectTransform Fill(Transform parent, string name) => Rect(parent, name, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        public static Image Panel(Transform parent, string name, Color color, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax)
        {
            var r = Rect(parent, name, aMin, aMax, oMin, oMax);
            var img = r.gameObject.AddComponent<Image>();
            img.color = color;
            return img;
        }

        public static Text Label(Transform parent, string text, int size, Color color, TextAnchor align = TextAnchor.MiddleLeft)
        {
            var r = Fill(parent, "Texto");
            var t = r.gameObject.AddComponent<Text>();
            t.font = Font; t.fontSize = size; t.color = color; t.alignment = align; t.text = text;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        public static Text LabelAt(Transform parent, string text, int size, Color color, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax, TextAnchor align = TextAnchor.MiddleLeft)
        {
            var r = Rect(parent, "Texto", aMin, aMax, oMin, oMax);
            var t = r.gameObject.AddComponent<Text>();
            t.font = Font; t.fontSize = size; t.color = color; t.alignment = align; t.text = text;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        public static Button Button(Transform parent, string text, Color bg, UnityAction onClick, int fontSize = 22)
        {
            var go = new GameObject("Boton_" + text, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = bg;
            var b = go.AddComponent<Button>();
            b.targetGraphic = img;
            var colors = b.colors;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            b.colors = colors;
            b.onClick.AddListener(onClick);
            Label(go.transform, text, fontSize, Color.white, TextAnchor.MiddleCenter);
            return b;
        }

        public static LayoutElement Size(Component c, float w, float h)
        {
            var le = c.gameObject.GetComponent<LayoutElement>();
            if (le == null) le = c.gameObject.AddComponent<LayoutElement>();
            if (w >= 0) { le.preferredWidth = w; le.minWidth = w; }
            if (h >= 0) { le.preferredHeight = h; le.minHeight = h; }
            return le;
        }

        public static VerticalLayoutGroup VList(Transform t, float spacing, int pad = 8)
        {
            var v = t.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = spacing; v.padding = new RectOffset(pad, pad, pad, pad);
            v.childControlHeight = true; v.childControlWidth = true;
            v.childForceExpandHeight = false; v.childForceExpandWidth = true;
            return v;
        }

        public static HorizontalLayoutGroup HList(Transform t, float spacing, int pad = 4)
        {
            var h = t.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = spacing; h.padding = new RectOffset(pad, pad, pad, pad);
            h.childControlHeight = true; h.childControlWidth = true;
            h.childForceExpandHeight = true; h.childForceExpandWidth = false;
            h.childAlignment = TextAnchor.MiddleLeft;
            return h;
        }

        /// <summary>Área con scroll vertical. Devuelve el "Content" donde poner cosas.</summary>
        public static RectTransform ScrollArea(Transform parent, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax)
        {
            var view = Panel(parent, "Scroll", new Color(0, 0, 0, 0.15f), aMin, aMax, oMin, oMax);
            var sr = view.gameObject.AddComponent<ScrollRect>();
            view.gameObject.AddComponent<RectMask2D>();
            var content = Rect(view.transform, "Contenido", new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            content.pivot = new Vector2(0.5f, 1);
            var fit = content.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.content = content;
            sr.horizontal = false;
            sr.scrollSensitivity = 30f;
            sr.viewport = (RectTransform)view.transform;
            return content;
        }
    }
}
