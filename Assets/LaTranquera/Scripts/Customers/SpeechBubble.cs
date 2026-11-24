using UnityEngine;
using UnityEngine.UI;

namespace LaTranquera
{
    /// <summary>Globito de diálogo arriba de la cabeza de un cliente. Siempre mira a la cámara.</summary>
    public class SpeechBubble : MonoBehaviour
    {
        Text text;
        float hideAt;

        public static SpeechBubble Create(Transform parent, float height)
        {
            var go = Shapes.Empty(parent, "Globito", Vector3.up * height);
            var b = go.AddComponent<SpeechBubble>();
            b.text = WorldLabel.Create(go.transform, "", Vector3.zero, new Vector2(1.7f, 0.42f), 60, Color.black, Vector3.forward, new Color(1, 1, 1, 0.92f));
            b.text.transform.parent.gameObject.SetActive(false);
            return b;
        }

        public void Say(string msg, float seconds = 4f)
        {
            text.text = Loc.T(msg);
            text.transform.parent.gameObject.SetActive(true);
            hideAt = Time.time + seconds;
        }

        void LateUpdate()
        {
            if (text == null) return;
            var canvas = text.transform.parent;
            if (canvas.gameObject.activeSelf && Time.time > hideAt) canvas.gameObject.SetActive(false);
            var cam = Camera.main;
            if (cam != null && canvas.gameObject.activeSelf)
                canvas.rotation = Quaternion.LookRotation(canvas.position - cam.transform.position);
        }
    }
}
