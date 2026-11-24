using System.Collections;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Pozo (o canilla). Con la regadera o el balde en la mano, clic sobre el pozo para llenarlo.
    /// En el pozo se ve bajar y subir el balde con la soga.
    /// </summary>
    public class WaterSource : MonoBehaviour, IInteractable
    {
        public string displayName = "Pozo de agua";
        public float fillSeconds = 2.5f;
        /// <summary>El balde del pozo que baja y sube (opcional).</summary>
        public Transform wellBucket;
        public float bucketDrop = 1.2f;
        bool busy;

        public string GetName(UseContext c) => Loc.T(displayName) + (busy ? " (sacando agua...)" : "");
        public string GetPrompt(UseContext c) => c.hands.IsEmpty ? "Traé la regadera o el balde para sacar agua" : null;
        public void Interact(UseContext c) { }

        public void Fill(IWaterHolder holder)
        {
            if (busy) return;
            if (holder.Liters >= holder.Capacity - 0.01f) { HUD.Toast(holder.Name + " ya está llena"); return; }
            StartCoroutine(FillRoutine(holder));
        }

        IEnumerator FillRoutine(IWaterHolder holder)
        {
            busy = true;
            Vector3 start = wellBucket != null ? wellBucket.localPosition : Vector3.zero;
            float t = 0;
            Sfx.Play("water", 0.4f);
            while (t < fillSeconds)
            {
                t += Time.deltaTime;
                if (wellBucket != null)
                {
                    float k = Mathf.Sin(t / fillSeconds * Mathf.PI);
                    wellBucket.localPosition = start - Vector3.up * bucketDrop * k;
                }
                yield return null;
            }
            if (wellBucket != null) wellBucket.localPosition = start;
            holder.Liters = holder.Capacity;
            Sfx.Play("water", 0.5f);
            HUD.Toast(holder.Name + " llena (" + holder.Capacity + " L)");
            busy = false;
        }
    }
}
