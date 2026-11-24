using System.Collections.Generic;

namespace LaTranquera
{
    /// <summary>Un artículo del catálogo del proveedor.</summary>
    public class CatalogEntry
    {
        public string id;
        public string name;
        public string detail;
        public int price;
        public string kind;     // "box" (caja de producto), "seeds" (sobre), "crate" (cajón vacío)
        public string refId;    // producto o cultivo
        public int units;       // unidades por caja / semillas por sobre

        /// <summary>Las semillas de cultivos nuevos aparecen cuando comprás la mejora.</summary>
        public bool Available => kind != "seeds" || Upgrades.Has(CropDB.Get(refId)?.unlock);
    }

    public static class SupplierCatalog
    {
        static List<CatalogEntry> all;

        public static List<CatalogEntry> All
        {
            get
            {
                if (all != null) return all;
                all = new List<CatalogEntry>();
                foreach (var c in CropDB.All)
                    all.Add(new CatalogEntry
                    {
                        id = "sem_" + c.id, name = "Semillas de " + c.name.ToLower(), detail = "Sobre x" + c.seedsPerPacket,
                        price = c.packetCost, kind = "seeds", refId = c.id, units = c.seedsPerPacket
                    });
                foreach (var p in ProductDB.All)
                {
                    if (!p.FromSupplier) continue;
                    all.Add(new CatalogEntry
                    {
                        id = "caja_" + p.id, name = p.name, detail = "Caja x" + p.pack + (p.storage == Storage.Heladera ? " (heladera)" : ""),
                        price = p.cost * p.pack, kind = "box", refId = p.id, units = p.pack
                    });
                }
                all.Add(new CatalogEntry { id = "cajon", name = "Cajón de madera", detail = "Vacío, para cosechar", price = 3500, kind = "crate", units = 1 });
                return all;
            }
        }

        public static CatalogEntry Get(string id) => All.Find(e => e.id == id);
    }
}
