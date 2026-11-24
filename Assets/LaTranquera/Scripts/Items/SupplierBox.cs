using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Caja de cartón del proveedor. Llega cerrada: mirala y apretá F para abrirla.
    /// Una vez abierta funciona como un cajón: la llevás a la estantería y clic para reponer.
    /// Cuando queda vacía, tirala en el tacho de basura.
    /// </summary>
    public class SupplierBox : Container, IOpenable
    {
        GameObject visual;

        public void Setup(string pid, int units, bool open)
        {
            kind = "box";
            displayName = "Caja de " + ProductDB.NameOf(pid);
            productId = pid;
            count = units;
            fixedCapacity = ProductDB.Get(pid)?.pack ?? units;
            isOpen = open;
            mass = 4f;
            holdOffset = new Vector3(0f, -0.45f, 0.75f);
            innerArea = new Vector3(0.42f, 0.28f, 0.32f);
            BuildVisual();
        }

        void BuildVisual()
        {
            if (visual != null) DestroyImmediate(visual);
            visual = ProductVisuals.CardboardBox(transform, isOpen);
            if (contentRoot == null) contentRoot = Shapes.Empty(transform, "Contenido", new Vector3(0, 0.015f, 0)).transform;
            var bc = GetComponent<BoxCollider>();
            if (bc == null) bc = gameObject.AddComponent<BoxCollider>();
            bc.center = new Vector3(0, 0.15f, 0);
            bc.size = new Vector3(0.45f, 0.3f, 0.35f);
            RefreshVisual();
        }

        public string GetOpenPrompt() => isOpen ? null : "[F] Abrir la caja";

        public void Open()
        {
            if (isOpen) return;
            isOpen = true;
            Sfx.Play("dig", 0.4f);
            BuildVisual();
        }

        protected override void OnLoaded()
        {
            displayName = "Caja de " + ProductDB.NameOf(productId);
            fixedCapacity = ProductDB.Get(productId)?.pack ?? count;
            BuildVisual();
        }
    }
}
