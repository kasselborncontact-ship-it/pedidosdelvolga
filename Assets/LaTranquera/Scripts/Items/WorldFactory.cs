using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Crea los objetos que se pueden llevar. Se usa al empezar una partida nueva,
    /// al cargar una partida guardada y cuando llega un pedido del proveedor.
    /// </summary>
    public static class WorldFactory
    {
        static GameObject Root(string name, Vector3 pos, Quaternion rot)
        {
            var go = new GameObject(name);
            go.transform.SetPositionAndRotation(pos, rot);
            return go;
        }

        public static Crate Crate(Vector3 pos, Quaternion rot)
        {
            var c = Root("Cajon", pos, rot).AddComponent<Crate>();
            c.Setup();
            return c;
        }

        public static SupplierBox Box(string productId, int units, bool open, Vector3 pos, Quaternion rot)
        {
            var b = Root("Caja_" + productId, pos, rot).AddComponent<SupplierBox>();
            b.Setup(productId, units, open);
            return b;
        }

        public static ProductItem Product(string productId, Vector3 pos, Quaternion rot)
        {
            var p = Root("Producto_" + productId, pos, rot).AddComponent<ProductItem>();
            p.Setup(productId);
            return p;
        }

        public static SeedPacket Seeds(string cropId, int seeds, Vector3 pos, Quaternion rot)
        {
            var s = Root("Semillas_" + cropId, pos, rot).AddComponent<SeedPacket>();
            s.Setup(cropId, seeds);
            return s;
        }

        public static Carryable Create(string kind, string extra, Vector3 pos, Quaternion rot)
        {
            Carryable c = null;
            switch (kind)
            {
                case "crate": c = Crate(pos, rot); break;
                case "box":
                {
                    string pid = extra.Split('|')[0];
                    c = Box(pid, 0, false, pos, rot);
                    break;
                }
                case "product": c = Product(extra, pos, rot); break;
                case "seeds": c = Seeds("lechuga", 1, pos, rot); break;
                case "hoe": { var h = Root("Azada", pos, rot).AddComponent<Hoe>(); h.Setup(); c = h; break; }
                case "can": { var w = Root("Regadera", pos, rot).AddComponent<WateringCan>(); w.Setup(); c = w; break; }
                case "bucket": { var b = Root("Balde", pos, rot).AddComponent<Bucket>(); b.Setup(); c = b; break; }
                case "labeler": { var l = Root("Etiquetadora", pos, rot).AddComponent<PriceLabeler>(); l.Setup(); c = l; break; }
            }
            if (c != null && !string.IsNullOrEmpty(extra)) c.LoadExtra(extra);
            return c;
        }
    }
}
