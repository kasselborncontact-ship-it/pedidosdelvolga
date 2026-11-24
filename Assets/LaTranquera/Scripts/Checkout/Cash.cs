using System.Collections.Generic;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>Billetes argentinos y cómo pagan los clientes.</summary>
    public static class Cash
    {
        public static readonly int[] Denoms = { 100, 200, 500, 1000, 2000, 10000, 20000 };

        /// <summary>Colores aproximados de cada billete (para reconocerlos rápido).</summary>
        public static Color ColorOf(int d)
        {
            switch (d)
            {
                case 100: return new Color(0.58f, 0.40f, 0.68f);   // violeta
                case 200: return new Color(0.33f, 0.52f, 0.78f);   // azul
                case 500: return new Color(0.40f, 0.62f, 0.42f);   // verde
                case 1000: return new Color(0.92f, 0.56f, 0.28f);  // naranja
                case 2000: return new Color(0.80f, 0.42f, 0.62f);  // rosa/violáceo
                case 10000: return new Color(0.30f, 0.62f, 0.72f); // celeste
                case 20000: return new Color(0.85f, 0.42f, 0.40f); // rojizo
            }
            return Color.gray;
        }

        public static int IndexOf(int d) => System.Array.IndexOf(Denoms, d);

        public static int Sum(List<int> bills)
        {
            int s = 0;
            foreach (var b in bills) s += b;
            return s;
        }

        /// <summary>Descompone un monto en billetes, del más grande al más chico.</summary>
        public static List<int> Decompose(int amount)
        {
            var list = new List<int>();
            for (int i = Denoms.Length - 1; i >= 0; i--)
                while (amount >= Denoms[i]) { list.Add(Denoms[i]); amount -= Denoms[i]; }
            return list;
        }

        /// <summary>
        /// Con qué billetes paga el cliente un total. A veces justo, a veces redondea
        /// para arriba (con uno de $10.000 para una compra chica, como en la vida real).
        /// </summary>
        public static List<int> CustomerPays(int total, System.Random r)
        {
            double roll = r.NextDouble();
            int amount;
            if (roll < 0.25) amount = total;                                    // justo
            else
            {
                int[] steps = { 1000, 2000, 10000, 20000 };
                int step = steps[r.Next(steps.Length)];
                if (step >= 10000 && total < 1500 && r.NextDouble() < 0.5) step = 2000;
                amount = Mathf.CeilToInt(total / (float)step) * step;
                if (amount == total && r.NextDouble() < 0.5) amount += step;
            }
            var bills = Decompose(amount);
            // Si da muchos billetes chicos, lo junta en billetes de 1000/2000
            return bills.Count > 9 ? Decompose(Mathf.CeilToInt(total / 2000f) * 2000) : bills;
        }

        /// <summary>Crea un billete (papel de color con el número arriba). textUp = hacia dónde queda "arriba" el texto.</summary>
        public static GameObject BillVisual(int d, Transform parent, Vector3 localPos, float yaw = 0, Vector3? textUp = null)
        {
            var root = Shapes.Empty(parent, "Billete_" + d, localPos, new Vector3(0, yaw, 0));
            Shapes.Prim(PrimitiveType.Cube, root.transform, "Papel", Vector3.zero, new Vector3(0.155f, 0.002f, 0.066f), ColorOf(d), false);
            WorldLabel.Create(root.transform, Money.Fmt(d).Replace("$ ", "$"), Vector3.up * 0.0015f,
                new Vector2(0.14f, 0.05f), 26, new Color(1, 1, 1, 0.95f), Vector3.up, null, textUp ?? Vector3.forward);
            return root;
        }
    }
}
