using System.Collections;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Etiquetadora de precios: mirando una estantería, la rueda del mouse sube o baja el
    /// precio del producto que mirás (de a $100). Clic = volver al precio sugerido.
    /// </summary>
    public class PriceLabeler : Tool
    {
        public void Setup()
        {
            kind = "labeler";
            displayName = "Etiquetadora";
            mass = 0.4f;
            holdOffset = new Vector3(0.25f, -0.25f, 0.5f);
            model = Shapes.Empty(transform, "Modelo", Vector3.zero).transform;
            Shapes.Box(model, "Cuerpo", Vector3.zero, new Vector3(0.07f, 0.05f, 0.16f), new Color(0.9f, 0.3f, 0.1f), false);
            Shapes.Box(model, "Mango", new Vector3(0, -0.09f, -0.04f), new Vector3(0.05f, 0.1f, 0.04f), new Color(0.2f, 0.2f, 0.2f), false);
            Shapes.Box(model, "Rollo", new Vector3(0, 0.05f, 0.03f), new Vector3(0.05f, 0.03f, 0.05f), Color.white, false);
            var bc = gameObject.AddComponent<BoxCollider>();
            bc.center = new Vector3(0, 0, 0);
            bc.size = new Vector3(0.08f, 0.2f, 0.17f);
        }

        string LookedProduct(UseContext c)
        {
            var shelf = c.Get<Shelf>();
            if (shelf == null) return null;
            var slot = shelf.NearestSlot(c.hit.point, s => s.count > 0);
            return slot?.productId;
        }

        public override string GetUsePrompt(UseContext c)
        {
            string pid = LookedProduct(c);
            if (pid == null) return null;
            int p = PriceBook.Get(pid), r = ProductDB.Get(pid).refPrice;
            return "[Rueda] Precio de " + ProductDB.NameOf(pid) + ": " + Money.Fmt(p) + " (sugerido " + Money.Fmt(r) + ")\n[Clic] Volver al precio sugerido";
        }

        public override void OnHeldUpdate(UseContext c)
        {
            int s = GameInput.Scroll;
            if (s == 0) return;
            string pid = LookedProduct(c);
            if (pid == null) return;
            PriceBook.Set(pid, PriceBook.Get(pid) + s * 100);
            Sfx.Play("key", 0.4f);
        }

        public override void Use(UseContext c)
        {
            string pid = LookedProduct(c);
            if (pid == null) return;
            PriceBook.Set(pid, ProductDB.Get(pid).refPrice);
            Sfx.Play("ok", 0.4f);
        }
    }
}
