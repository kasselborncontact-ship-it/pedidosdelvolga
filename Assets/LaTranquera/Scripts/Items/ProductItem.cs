using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// UNA unidad de producto suelta (una lechuga, una botella de leche...).
    /// Aparece cuando sacás algo de la estantería con las manos vacías.
    /// </summary>
    public class ProductItem : Carryable
    {
        public string productId;

        public void Setup(string pid)
        {
            productId = pid;
            kind = "product";
            displayName = ProductDB.Get(pid)?.name ?? pid;
            holdOffset = new Vector3(0.25f, -0.3f, 0.55f);
            Shapes.DestroyChildren(transform);
            ProductVisuals.Build(pid, transform);
            FitCollider();
        }

        public override string GetUsePrompt(UseContext c)
        {
            var cont = c.Get<Container>();
            if (cont != null && cont != this) return cont.CanAccept(productId) ? "[Clic] Guardar en " + Loc.T(cont.displayName) : null;
            var shelf = c.Get<Shelf>();
            if (shelf != null) return "[Clic] Poner en " + Loc.T(shelf.displayName);
            return null;
        }

        public override void Use(UseContext c)
        {
            var cont = c.Get<Container>();
            if (cont != null)
            {
                if (cont.Add(productId)) c.hands.ConsumeHeld();
                else HUD.Toast(cont.WhyNot(productId));
                return;
            }
            var shelf = c.Get<Shelf>();
            if (shelf != null)
            {
                if (shelf.TryAdd(productId, c.hit.point, out string why)) c.hands.ConsumeHeld();
                else HUD.Toast(why);
            }
        }

        public override string SaveExtra() => productId;
        public override void LoadExtra(string data) => Setup(data);
    }
}
