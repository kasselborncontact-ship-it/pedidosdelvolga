using System.Collections;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>Base de las herramientas: tienen un lugar fijo (gancho) donde se cuelgan.</summary>
    public class Tool : Carryable
    {
        protected Transform model;

        protected IEnumerator Swing(float angle = 60f, float time = 0.25f)
        {
            if (model == null) yield break;
            var start = model.localRotation;
            float t = 0;
            while (t < time)
            {
                t += Time.deltaTime;
                float k = Mathf.Sin(t / time * Mathf.PI);
                model.localRotation = start * Quaternion.Euler(angle * k, 0, 0);
                yield return null;
            }
            model.localRotation = start;
        }
    }

    /// <summary>Cualquier cosa que lleva agua (regadera, balde).</summary>
    public interface IWaterHolder
    {
        float Liters { get; set; }
        float Capacity { get; }
        string Name { get; }
    }
}
