using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Base de cajones y cajas: guardan UN tipo de producto, con capacidad limitada,
    /// y lo que tienen adentro se VE.
    /// Con el contenedor en la mano y clic izquierdo:
    ///   - sobre una planta madura: la cosecha adentro
    ///   - sobre una estantería/heladera: pone una unidad
    ///   - sobre otro contenedor: le pasa una unidad
    ///   - sobre un producto suelto: lo guarda
    /// </summary>
    public class Container : Carryable
    {
        public string productId;
        public int count;
        public bool isOpen = true;
        /// <summary>Si es mayor a 0, la capacidad es fija (cajas del proveedor).</summary>
        public int fixedCapacity;
        /// <summary>Espacio interior (ancho, alto, fondo) para acomodar lo de adentro.</summary>
        public Vector3 innerArea = new Vector3(0.46f, 0.25f, 0.31f);
        public Transform contentRoot;

        public bool IsEmpty => count <= 0;

        public int CapacityFor(string pid)
        {
            if (fixedCapacity > 0) return fixedCapacity;
            var d = ProductDB.Get(pid);
            return d != null ? d.crateCap : 0;
        }

        public bool CanAccept(string pid, int n = 1)
        {
            if (!isOpen || ProductDB.Get(pid) == null) return false;
            if (count > 0 && productId != pid) return false;
            return count + n <= CapacityFor(pid);
        }

        public string WhyNot(string pid, int n = 1)
        {
            if (!isOpen) return "La caja está cerrada (F para abrir)";
            if (count > 0 && productId != pid) return "Ya tiene " + ProductDB.NameOf(productId) + ". Cada cajón lleva un solo producto.";
            return Loc.T(displayName) + " está lleno";
        }

        public bool Add(string pid, int n = 1)
        {
            if (!CanAccept(pid, n)) return false;
            productId = pid;
            count += n;
            RefreshVisual();
            return true;
        }

        public bool TakeOne(out string pid)
        {
            pid = productId;
            if (count <= 0 || !isOpen) return false;
            count--;
            if (count == 0 && fixedCapacity == 0) productId = null;
            RefreshVisual();
            return true;
        }

        public override string GetName(UseContext c)
        {
            string n = Loc.T(displayName);
            if (!isOpen) return n + " (" + ProductDB.NameOf(productId) + " x" + count + ", cerrada)";
            if (count == 0) return n + " (vacío)";
            return n + " — " + ProductDB.NameOf(productId) + " " + count + "/" + CapacityFor(productId);
        }

        public override string GetUsePrompt(UseContext c)
        {
            var plant = c.Get<SoilPatch>();
            if (plant != null && plant.Ripe) return "[Clic] Cosechar " + Loc.T(plant.Crop.name) + " en el cajón";
            if (count > 0 && isOpen)
            {
                var shelf = c.Get<Shelf>();
                if (shelf != null) return "[Clic] Poner 1 " + ProductDB.NameOf(productId) + " en " + Loc.T(shelf.displayName);
                var other = c.Get<Container>();
                if (other != null && other != this) return "[Clic] Pasar 1 " + ProductDB.NameOf(productId);
            }
            var item = c.Get<ProductItem>();
            if (item != null) return "[Clic] Guardar " + Loc.T(item.displayName);
            return null;
        }

        public override void Use(UseContext c)
        {
            var plant = c.Get<SoilPatch>();
            if (plant != null && plant.Ripe)
            {
                var crop = plant.Crop;
                if (CanAccept(crop.productId, crop.yield))
                {
                    plant.Harvest();
                    Add(crop.productId, crop.yield);
                    Sfx.Play("pick");
                }
                else HUD.Toast(WhyNot(crop.productId, crop.yield));
                return;
            }

            if (count > 0 && isOpen)
            {
                var shelf = c.Get<Shelf>();
                if (shelf != null)
                {
                    if (shelf.TryAdd(productId, c.hit.point, out string why)) TakeOne(out _);
                    else HUD.Toast(why);
                    return;
                }
                var other = c.Get<Container>();
                if (other != null && other != this)
                {
                    if (other.CanAccept(productId)) { other.Add(productId); TakeOne(out _); }
                    else HUD.Toast(other.WhyNot(productId));
                    return;
                }
            }

            var item = c.Get<ProductItem>();
            if (item != null)
            {
                if (Add(item.productId)) Destroy(item.gameObject);
                else HUD.Toast(WhyNot(item.productId));
            }
        }

        /// <summary>Redibuja lo que hay adentro para que se vea.</summary>
        public void RefreshVisual()
        {
            if (contentRoot == null) return;
            Shapes.DestroyChildren(contentRoot);
            if (!isOpen || count <= 0) return;
            var def = ProductDB.Get(productId);
            if (def == null) return;
            var unit = def.size * 1.05f;
            var pos = Shapes.Arrange(count, unit, innerArea);
            foreach (var p in pos)
            {
                if (p.y + unit.y > innerArea.y + 0.12f) break; // no dibujar más de lo que entra a la vista
                var v = ProductVisuals.Build(productId, contentRoot);
                v.transform.localPosition = p;
            }
        }

        public override string SaveExtra() => (productId ?? "") + "|" + count + "|" + (isOpen ? 1 : 0);

        public override void LoadExtra(string data)
        {
            var parts = data.Split('|');
            if (parts.Length < 3) return;
            productId = string.IsNullOrEmpty(parts[0]) ? null : parts[0];
            int.TryParse(parts[1], out count);
            isOpen = parts[2] == "1";
            OnLoaded();
            RefreshVisual();
        }

        protected virtual void OnLoaded() { }
    }
}
