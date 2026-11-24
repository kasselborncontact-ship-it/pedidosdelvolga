using System.Collections.Generic;
using UnityEngine;

namespace LaTranquera
{
    public enum Storage { Estanteria, Heladera }

    public enum ProductShape { Lechuga, Tomate, Zanahoria, Yerba, Fideos, Harina, Aceite, Leche, Queso, Manteca }

    /// <summary>Un producto que se vende en el almacén.</summary>
    public class ProductDef
    {
        public string id;
        public string name;          // nombre que se ve
        public ProductShape shape;   // cómo se dibuja
        public Storage storage;      // Heladera = solo va en la heladera (lácteos y carnes)
        public int refPrice;         // precio de referencia (lo que el cliente considera "normal")
        public int cost;             // lo que cuesta cada unidad al proveedor (0 = no se compra)
        public int pack;             // unidades por caja del proveedor
        public int slotCap;          // cuántas entran en un lugar de la estantería
        public int crateCap;         // cuántas entran en un cajón de madera
        public Vector3 size;         // tamaño de una unidad (para acomodarla)
        public bool FromSupplier => cost > 0;
    }

    /// <summary>
    /// TODOS los productos del juego. Para agregar uno nuevo, sumá una línea acá
    /// y un dibujo en ProductVisuals.cs. Precios en pesos argentinos (aprox. 2025/2026).
    /// </summary>
    public static class ProductDB
    {
        public static readonly List<ProductDef> All = new List<ProductDef>
        {
            // --- De la huerta ---
            new ProductDef { id = "lechuga", name = "Lechuga", shape = ProductShape.Lechuga, storage = Storage.Estanteria,
                refPrice = 1200, slotCap = 6, crateCap = 8, size = new Vector3(0.22f, 0.16f, 0.22f) },
            new ProductDef { id = "tomate", name = "Tomate", shape = ProductShape.Tomate, storage = Storage.Estanteria,
                refPrice = 600, slotCap = 20, crateCap = 24, size = new Vector3(0.085f, 0.08f, 0.085f) },
            new ProductDef { id = "zanahoria", name = "Atado de zanahorias", shape = ProductShape.Zanahoria, storage = Storage.Estanteria,
                refPrice = 900, slotCap = 8, crateCap = 12, size = new Vector3(0.26f, 0.07f, 0.11f) },

            // --- Del proveedor (almacén) ---
            new ProductDef { id = "yerba", name = "Yerba 1 kg", shape = ProductShape.Yerba, storage = Storage.Estanteria,
                refPrice = 4300, cost = 3200, pack = 6, slotCap = 8, crateCap = 6, size = new Vector3(0.13f, 0.21f, 0.08f) },
            new ProductDef { id = "fideos", name = "Fideos 500 g", shape = ProductShape.Fideos, storage = Storage.Estanteria,
                refPrice = 1300, cost = 900, pack = 12, slotCap = 12, crateCap = 12, size = new Vector3(0.13f, 0.2f, 0.055f) },
            new ProductDef { id = "harina", name = "Harina 1 kg", shape = ProductShape.Harina, storage = Storage.Estanteria,
                refPrice = 1000, cost = 700, pack = 10, slotCap = 8, crateCap = 10, size = new Vector3(0.13f, 0.21f, 0.09f) },
            new ProductDef { id = "aceite", name = "Aceite de girasol 1,5 L", shape = ProductShape.Aceite, storage = Storage.Estanteria,
                refPrice = 3500, cost = 2600, pack = 6, slotCap = 8, crateCap = 6, size = new Vector3(0.09f, 0.31f, 0.09f) },
            new ProductDef { id = "leche", name = "Leche 1 L (botella)", shape = ProductShape.Leche, storage = Storage.Heladera,
                refPrice = 1400, cost = 1000, pack = 12, slotCap = 10, crateCap = 12, size = new Vector3(0.09f, 0.25f, 0.09f) },
            new ProductDef { id = "queso", name = "Queso cremoso", shape = ProductShape.Queso, storage = Storage.Heladera,
                refPrice = 6000, cost = 4500, pack = 4, slotCap = 6, crateCap = 4, size = new Vector3(0.16f, 0.09f, 0.16f) },
            new ProductDef { id = "manteca", name = "Manteca 200 g", shape = ProductShape.Manteca, storage = Storage.Heladera,
                refPrice = 2200, cost = 1600, pack = 10, slotCap = 12, crateCap = 10, size = new Vector3(0.11f, 0.05f, 0.07f) },
        };

        /// <summary>Cosas que los clientes a veces piden y todavía no vendemos (fases siguientes).</summary>
        public static readonly string[] NotYetSold = { "huevos", "pan casero", "carne", "chorizos", "dulce de leche", "sandía" };

        static Dictionary<string, ProductDef> map;
        public static ProductDef Get(string id)
        {
            if (map == null)
            {
                map = new Dictionary<string, ProductDef>();
                foreach (var p in All) map[p.id] = p;
            }
            return id != null && map.TryGetValue(id, out var d) ? d : null;
        }

        public static string NameOf(string id) => Loc.T(Get(id)?.name ?? id);
    }

    /// <summary>Un cultivo de la huerta.</summary>
    public class CropDef
    {
        public string id;
        public string name;
        public string productId;   // qué se cosecha
        public int yield;          // cuántas unidades da cada planta
        public float growHours;    // horas de juego regadas hasta estar lista
        public int seedsPerPacket;
        public int packetCost;
    }

    public static class CropDB
    {
        public static readonly List<CropDef> All = new List<CropDef>
        {
            new CropDef { id = "lechuga",   name = "Lechuga",   productId = "lechuga",   yield = 1, growHours = 48, seedsPerPacket = 12, packetCost = 2000 },
            new CropDef { id = "tomate",    name = "Tomate",    productId = "tomate",    yield = 4, growHours = 96, seedsPerPacket = 6,  packetCost = 2500 },
            new CropDef { id = "zanahoria", name = "Zanahoria", productId = "zanahoria", yield = 1, growHours = 72, seedsPerPacket = 12, packetCost = 1800 },
            // Fase siguiente: cebolla, papa, maíz, zapallo, sandía, frutilla, uva
        };

        public static CropDef Get(string id) => All.Find(c => c.id == id);
    }
}
