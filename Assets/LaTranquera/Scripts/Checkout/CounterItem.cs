using System.Collections;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>Un producto que el cliente apoyó en el mostrador. E para pasarlo por el lector.</summary>
    public class CounterItem : MonoBehaviour, IInteractable
    {
        public string productId;
        public bool scanned;
        CheckoutCounter counter;

        public void Setup(string pid, CheckoutCounter c)
        {
            productId = pid;
            counter = c;
            ProductVisuals.Build(pid, transform);
            var b = Shapes.RenderBounds(gameObject);
            var bc = gameObject.AddComponent<BoxCollider>();
            bc.center = transform.InverseTransformPoint(b.center);
            bc.size = new Vector3(Mathf.Max(0.08f, b.size.x), Mathf.Max(0.06f, b.size.y), Mathf.Max(0.08f, b.size.z));
            bc.isTrigger = true;
        }

        public string GetName(UseContext c) => ProductDB.NameOf(productId) + (scanned ? " (cobrado)" : " — " + Money.Fmt(PriceBook.Get(productId)));
        public string GetPrompt(UseContext c) => !scanned && c.hands.IsEmpty ? "[E] Pasar por el lector" : null;

        public void Interact(UseContext c)
        {
            if (!scanned && c.hands.IsEmpty) counter.Scan(this);
        }

        public void MoveTo(Vector3 scanPos, Vector3 bagPos) => StartCoroutine(Move(scanPos, bagPos));

        IEnumerator Move(Vector3 a, Vector3 b)
        {
            Vector3 start = transform.position;
            float t = 0;
            while (t < 1) { t += Time.deltaTime * 5f; transform.position = Vector3.Lerp(start, a, t); yield return null; }
            t = 0;
            while (t < 1) { t += Time.deltaTime * 4f; transform.position = Vector3.Lerp(a, b, t); yield return null; }
        }
    }
}
