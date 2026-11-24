using System;
using System.Collections.Generic;

namespace LaTranquera
{
    /// <summary>
    /// Cosas que se compran con la plata ganada (pestaña Mejoras de la computadora):
    /// terrenos nuevos y semillas nuevas. Más adelante: corrales, animales, máquinas, ampliaciones.
    /// </summary>
    public static class Upgrades
    {
        public class Def
        {
            public string id, name, detail;
            public int price;
            public string requires; // otra mejora que hay que tener antes (o null)
        }

        public static readonly List<Def> All = new List<Def>
        {
            new Def { id = "sem_papa_cebolla", name = "Semillas de papa y cebolla", detail = "Aparecen en el catálogo del proveedor", price = 6000 },
            new Def { id = "lote_2", name = "Lote 2 (14 x 14 m)", detail = "Más tierra para sembrar, al lado de tu terreno", price = 35000 },
            new Def { id = "sem_maiz_zapallo", name = "Semillas de maíz y zapallo", detail = "El zapallo necesita lugar: 1,1 m entre plantas", price = 9000, requires = "sem_papa_cebolla" },
            new Def { id = "sem_sandia_frutilla", name = "Semillas de sandía y plantines de frutilla", detail = "La sandía es rastrera: 1,3 m entre plantas", price = 12000, requires = "sem_maiz_zapallo" },
            new Def { id = "lote_3", name = "Lote 3 (16 x 30 m)", detail = "Un terreno grande hacia el oeste", price = 60000, requires = "lote_2" },
        };

        static readonly HashSet<string> owned = new HashSet<string>();
        public static event Action Changed;

        public static bool Has(string id) => string.IsNullOrEmpty(id) || owned.Contains(id);
        public static IEnumerable<string> Owned => owned;

        public static bool CanBuy(Def d) => !Has(d.id) && Has(d.requires);

        public static bool Buy(string id)
        {
            var d = All.Find(x => x.id == id);
            if (d == null || !CanBuy(d)) return false;
            if (!Economy.I.TrySpend(d.price, d.name)) return false;
            Give(id);
            return true;
        }

        /// <summary>Da la mejora sin cobrar (al cargar la partida).</summary>
        public static void Give(string id)
        {
            owned.Add(id);
            if (id.StartsWith("lote_")) LandPlot.Find(id)?.SetOwned(true);
            Changed?.Invoke();
        }

        public static void Reset()
        {
            owned.Clear();
            Changed?.Invoke();
        }
    }
}
