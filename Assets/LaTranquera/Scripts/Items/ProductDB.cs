using System.Collections.Generic;
using UnityEngine;

namespace LaTranquera
{
    public enum Storage { Estanteria, Heladera }

    public enum ProductShape { Lechuga, Tomate, Zanahoria, Yerba, Fideos, Harina, Aceite, Leche, Queso, Manteca, Cebolla, Papa, Choclo, Zapallo, Sandia, Frutilla, Pan }

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
                refPrice = 900, slotCap = 8, crateCap = 12, size = new Vector3(0.13f, 0.26f, 0.13f) },

            new ProductDef { id = "cebolla", name = "Cebolla", shape = ProductShape.Cebolla, storage = Storage.Estanteria,
                refPrice = 350, slotCap = 16, crateCap = 30, size = new Vector3(0.08f, 0.09f, 0.08f) },
            new ProductDef { id = "papa", name = "Papa", shape = ProductShape.Papa, storage = Storage.Estanteria,
                refPrice = 300, slotCap = 16, crateCap = 30, size = new Vector3(0.1f, 0.06f, 0.07f) },
            new ProductDef { id = "choclo", name = "Choclo", shape = ProductShape.Choclo, storage = Storage.Estanteria,
                refPrice = 700, slotCap = 8, crateCap = 12, size = new Vector3(0.2f, 0.19f, 0.18f) },
            new ProductDef { id = "zapallo", name = "Zapallo", shape = ProductShape.Zapallo, storage = Storage.Estanteria,
                refPrice = 2200, slotCap = 4, crateCap = 3, size = new Vector3(0.27f, 0.15f, 0.27f) },
            new ProductDef { id = "sandia", name = "Sandía", shape = ProductShape.Sandia, storage = Storage.Estanteria,
                refPrice = 5000, slotCap = 2, crateCap = 2, size = new Vector3(0.36f, 0.24f, 0.24f) },
            new ProductDef { id = "frutilla", name = "Frutillas (bandeja)", shape = ProductShape.Frutilla, storage = Storage.Heladera,
                refPrice = 3000, slotCap = 6, crateCap = 8, size = new Vector3(0.17f, 0.065f, 0.12f) },

            // --- Del proveedor (almacén) ---
            new ProductDef { id = "yerba", name = "Yerba 1 kg", shape = ProductShape.Yerba, storage = Storage.Estanteria,
                refPrice = 4300, cost = 3200, pack = 6, slotCap = 8, crateCap = 6, size = new Vector3(0.13f, 0.21f, 0.08f) },
            new ProductDef { id = "fideos", name = "Fideos 500 g", shape = ProductShape.Fideos, storage = Storage.Estanteria,
                refPrice = 1300, cost = 900, pack = 12, slotCap = 12, crateCap = 12, size = new Vector3(0.13f, 0.2f, 0.055f) },
            new ProductDef { id = "harina", name = "Harina 1 kg", shape = ProductShape.Harina, storage = Storage.Estanteria,
                refPrice = 1000, cost = 700, pack = 10, slotCap = 8, crateCap = 10, size = new Vector3(0.13f, 0.21f, 0.09f) },
            new ProductDef { id = "aceite", name = "Aceite de girasol 1,5 L", shape = ProductShape.Aceite, storage = Storage.Estanteria,
                refPrice = 3500, cost = 2600, pack = 6, slotCap = 8, crateCap = 6, size = new Vector3(0.09f, 0.31f, 0.09f) },
            new ProductDef { id = "pan", name = "Pan casero", shape = ProductShape.Pan, storage = Storage.Estanteria,
                refPrice = 1800, cost = 1200, pack = 6, slotCap = 4, crateCap = 4, size = new Vector3(0.3f, 0.23f, 0.19f) },
            new ProductDef { id = "leche", name = "Leche 1 L (botella)", shape = ProductShape.Leche, storage = Storage.Heladera,
                refPrice = 1400, cost = 1000, pack = 12, slotCap = 10, crateCap = 12, size = new Vector3(0.09f, 0.25f, 0.09f) },
            new ProductDef { id = "queso", name = "Queso cremoso", shape = ProductShape.Queso, storage = Storage.Heladera,
                refPrice = 6000, cost = 4500, pack = 4, slotCap = 6, crateCap = 4, size = new Vector3(0.16f, 0.06f, 0.16f) },
            new ProductDef { id = "manteca", name = "Manteca 200 g", shape = ProductShape.Manteca, storage = Storage.Heladera,
                refPrice = 2200, cost = 1600, pack = 10, slotCap = 12, crateCap = 10, size = new Vector3(0.11f, 0.05f, 0.07f) },
        };

        /// <summary>Cosas que los clientes a veces piden y todavía no vendemos (fases siguientes).</summary>
        public static readonly string[] NotYetSold = { "huevos", "facturas", "carne", "chorizos", "dulce de leche", "queso de campo" };

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
        public float spacing = 0.55f;  // distancia mínima a otra planta (metros)
        public string unlock;          // mejora que hay que comprar para tener la semilla (null = desde el principio)
    }

    public static class CropDB
    {
        public static readonly List<CropDef> All = new List<CropDef>
        {
            new CropDef { id = "lechuga",   name = "Lechuga",   productId = "lechuga",   yield = 1, growHours = 48,  seedsPerPacket = 12, packetCost = 2000, spacing = 0.55f },
            new CropDef { id = "tomate",    name = "Tomate",    productId = "tomate",    yield = 4, growHours = 96,  seedsPerPacket = 6,  packetCost = 2500, spacing = 0.7f },
            new CropDef { id = "zanahoria", name = "Zanahoria", productId = "zanahoria", yield = 1, growHours = 72,  seedsPerPacket = 12, packetCost = 1800, spacing = 0.55f },
            new CropDef { id = "cebolla",   name = "Cebolla",   productId = "cebolla",   yield = 3, growHours = 72,  seedsPerPacket = 20, packetCost = 1500, spacing = 0.55f, unlock = "sem_papa_cebolla" },
            new CropDef { id = "papa",      name = "Papa",      productId = "papa",      yield = 5, growHours = 96,  seedsPerPacket = 10, packetCost = 3000, spacing = 0.6f,  unlock = "sem_papa_cebolla" },
            new CropDef { id = "maiz",      name = "Maíz",      productId = "choclo",    yield = 2, growHours = 120, seedsPerPacket = 12, packetCost = 2000, spacing = 0.6f,  unlock = "sem_maiz_zapallo" },
            new CropDef { id = "zapallo",   name = "Zapallo",   productId = "zapallo",   yield = 2, growHours = 144, seedsPerPacket = 6,  packetCost = 2500, spacing = 1.1f,  unlock = "sem_maiz_zapallo" },
            new CropDef { id = "sandia",    name = "Sandía",    productId = "sandia",    yield = 1, growHours = 144, seedsPerPacket = 6,  packetCost = 3000, spacing = 1.3f,  unlock = "sem_sandia_frutilla" },
            new CropDef { id = "frutilla",  name = "Frutilla",  productId = "frutilla",  yield = 2, growHours = 96,  seedsPerPacket = 8,  packetCost = 4000, spacing = 0.55f, unlock = "sem_sandia_frutilla" },
            // Más adelante: uva (parral), girasol...
        };

        public static CropDef Get(string id) => All.Find(c => c.id == id);
    }
}
