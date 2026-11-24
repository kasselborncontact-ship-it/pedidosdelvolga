using UnityEngine;

namespace LaTranquera
{
    /// <summary>Cajón de madera de la huerta. Lleva un solo tipo de producto (ej.: 8 lechugas o 24 tomates).</summary>
    public class Crate : Container
    {
        public void Setup()
        {
            kind = "crate";
            displayName = "Cajón de madera";
            mass = 2.5f;
            holdOffset = new Vector3(0f, -0.45f, 0.75f);
            innerArea = new Vector3(0.46f, 0.25f, 0.31f);
            ProductVisuals.Crate(transform);
            contentRoot = Shapes.Empty(transform, "Contenido", new Vector3(0, 0.02f, 0)).transform;
            var bc = gameObject.AddComponent<BoxCollider>();
            bc.center = new Vector3(0, 0.14f, 0);
            bc.size = new Vector3(0.52f, 0.28f, 0.37f);
        }
    }
}
