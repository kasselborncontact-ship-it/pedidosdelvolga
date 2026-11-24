using System.Collections.Generic;
using UnityEngine;

namespace LaTranquera.EditorTools
{
    /// <summary>
    /// Piezas para construir el mapa: paredes con aberturas, alambrados, árboles, postes de luz...
    /// Todo con formas simples. Lo usa SceneBuilder.
    /// </summary>
    public static class WorldParts
    {
        public struct Opening
        {
            public float from, to, bottom, top;
            public bool glass;
            public Opening(float f, float t, float b, float tp, bool g = false) { from = f; to = t; bottom = b; top = tp; glass = g; }
        }

        /// <summary>Pared recta de a a b (en el piso, coordenadas del padre) con puertas/ventanas.</summary>
        public static void Wall(Transform p, string name, Vector3 a, Vector3 b, float height, float thick, Color color, params Opening[] openings)
        {
            var dir = (b - a).normalized;
            float len = Vector3.Distance(a, b);
            var rot = Quaternion.LookRotation(dir).eulerAngles;
            var list = new List<Opening>(openings);
            list.Sort((x, y) => x.from.CompareTo(y.from));
            float cursor = 0;
            void Piece(float f, float t, float y0, float y1)
            {
                if (t - f < 0.01f || y1 - y0 < 0.01f) return;
                var center = a + dir * ((f + t) / 2) + Vector3.up * ((y0 + y1) / 2);
                Shapes.BoxC(p, name, center, new Vector3(thick, y1 - y0, t - f), color, true, rot);
            }
            foreach (var o in list)
            {
                Piece(cursor, o.from, 0, height);
                Piece(o.from, o.to, 0, o.bottom);
                Piece(o.from, o.to, o.top, height);
                if (o.glass)
                {
                    var center = a + dir * ((o.from + o.to) / 2) + Vector3.up * ((o.bottom + o.top) / 2);
                    var g = Shapes.Prim(PrimitiveType.Cube, p, "Vidrio", center, new Vector3(0.02f, o.top - o.bottom, o.to - o.from), Mats.Glass(new Color(0.7f, 0.85f, 0.95f, 0.25f)), false, rot);
                    // marco
                    Shapes.BoxC(p, "Marco", center + Vector3.up * ((o.top - o.bottom) / 2), new Vector3(thick + 0.04f, 0.06f, o.to - o.from + 0.08f), Mats.Madera, false, rot);
                    Shapes.BoxC(p, "Marco", center - Vector3.up * ((o.top - o.bottom) / 2), new Vector3(thick + 0.06f, 0.06f, o.to - o.from + 0.1f), Mats.Madera, false, rot);
                    g.name = "Vidrio";
                }
                cursor = o.to;
            }
            Piece(cursor, len, 0, height);
        }

        /// <summary>Alambrado de campo: postes de madera y 4 hilos de alambre. Con collider para no pasar.</summary>
        public static void Fence(Transform p, Vector3 a, Vector3 b, float spacing = 3f)
        {
            var root = Shapes.Empty(p, "Alambrado", Vector3.zero).transform;
            float len = Vector3.Distance(a, b);
            int n = Mathf.Max(1, Mathf.RoundToInt(len / spacing));
            Color post = new Color(0.45f, 0.36f, 0.26f);
            for (int i = 0; i <= n; i++)
            {
                var pos = Vector3.Lerp(a, b, i / (float)n);
                bool strong = i == 0 || i == n;
                Shapes.Cyl(root, "Poste", pos, strong ? 0.16f : 0.1f, strong ? 1.45f : 1.3f, post, false);
            }
            Color wire = new Color(0.55f, 0.55f, 0.55f);
            foreach (float y in new[] { 0.35f, 0.6f, 0.85f, 1.1f })
                Shapes.Beam(root, "Alambre", a + Vector3.up * y, b + Vector3.up * y, 0.012f, wire);
            // pared invisible
            var col = new GameObject("ColliderAlambrado");
            col.transform.SetParent(root, false);
            col.transform.localPosition = (a + b) / 2 + Vector3.up * 0.75f;
            col.transform.localRotation = Quaternion.LookRotation((b - a).normalized);
            col.AddComponent<BoxCollider>().size = new Vector3(0.15f, 1.5f, len);
        }

        public static void Eucalyptus(Transform p, Vector3 pos, System.Random r)
        {
            var t = Shapes.Empty(p, "Eucalipto", pos, new Vector3(0, (float)r.NextDouble() * 360, (float)(r.NextDouble() * 6 - 3))).transform;
            float h = 14f + (float)r.NextDouble() * 6f;
            Shapes.Cyl(t, "Tronco", Vector3.zero, 0.5f, h * 0.75f, new Color(0.8f, 0.76f, 0.68f), true);
            Shapes.CylC(t, "Rama", new Vector3(0.8f, h * 0.55f, 0), 0.18f, 2.5f, new Color(0.75f, 0.7f, 0.62f), new Vector3(0, 0, -40));
            Shapes.CylC(t, "Rama", new Vector3(-0.7f, h * 0.62f, 0.3f), 0.16f, 2.2f, new Color(0.75f, 0.7f, 0.62f), new Vector3(20, 0, 40));
            Color[] leaves = { new Color(0.42f, 0.52f, 0.4f), new Color(0.36f, 0.47f, 0.36f), new Color(0.5f, 0.58f, 0.45f) };
            for (int i = 0; i < 8; i++)
            {
                float y = h * (0.55f + (float)r.NextDouble() * 0.45f);
                var off = new Vector3((float)(r.NextDouble() * 4 - 2), y, (float)(r.NextDouble() * 4 - 2));
                Shapes.Sphere(t, "Copa", off, new Vector3(2.4f + (float)r.NextDouble() * 1.6f, 1.6f + (float)r.NextDouble() * 1.4f, 2.4f + (float)r.NextDouble() * 1.6f), leaves[i % 3]);
            }
        }

        public static void Poplar(Transform p, Vector3 pos, System.Random r)
        {
            var t = Shapes.Empty(p, "Alamo", pos, new Vector3(0, (float)r.NextDouble() * 360, 0)).transform;
            float h = 12f + (float)r.NextDouble() * 5f;
            Shapes.Cyl(t, "Tronco", Vector3.zero, 0.42f, 3.5f, new Color(0.4f, 0.38f, 0.34f), true);
            Color g = new Color(0.24f + (float)r.NextDouble() * 0.05f, 0.45f, 0.2f);
            Shapes.Capsule(t, "Copa", new Vector3(0, h * 0.55f, 0), new Vector3(2.6f, h * 0.4f, 2.6f), g);
            Shapes.Capsule(t, "Copa2", new Vector3(0.5f, h * 0.45f, 0.3f), new Vector3(2.0f, h * 0.28f, 2.0f), g * 0.92f);
        }

        public static void Paraiso(Transform p, Vector3 pos, System.Random r)
        {
            var t = Shapes.Empty(p, "Paraiso", pos, new Vector3(0, (float)r.NextDouble() * 360, 0)).transform;
            Color bark = new Color(0.33f, 0.24f, 0.17f);
            Shapes.Cyl(t, "Tronco", Vector3.zero, 0.42f, 2.6f, bark, true);
            Shapes.CylC(t, "Rama", new Vector3(0.7f, 3.0f, 0), 0.2f, 1.8f, bark, new Vector3(0, 0, -45));
            Shapes.CylC(t, "Rama", new Vector3(-0.6f, 3.0f, 0.3f), 0.2f, 1.8f, bark, new Vector3(15, 0, 45));
            Color g = new Color(0.3f, 0.52f, 0.22f);
            Shapes.Sphere(t, "Copa", new Vector3(0, 4.3f, 0), new Vector3(6.5f, 2.8f, 6.5f), g);
            for (int i = 0; i < 5; i++)
            {
                var d = Quaternion.Euler(0, i * 72, 0) * Vector3.forward * 2.4f;
                Shapes.Sphere(t, "Copa", d + Vector3.up * (4.0f + (float)r.NextDouble()), new Vector3(3.2f, 2.2f, 3.2f), i % 2 == 0 ? g : g * 1.08f);
            }
        }

        /// <summary>Poste de luz de la ruta. Si withLight, tiene una luz que se prende de noche.</summary>
        public static Light StreetLight(Transform p, Vector3 pos, Vector3 armDir, bool withLight)
        {
            var t = Shapes.Empty(p, "PosteDeLuz", pos).transform;
            Color c = new Color(0.65f, 0.65f, 0.62f);
            Shapes.Cyl(t, "Poste", Vector3.zero, 0.2f, 7.5f, c, true);
            Shapes.Beam(t, "Brazo", new Vector3(0, 7.3f, 0), new Vector3(0, 7.5f, 0) + armDir * 1.6f, 0.1f, c);
            Shapes.BoxC(t, "Farola", new Vector3(0, 7.45f, 0) + armDir * 1.7f, new Vector3(0.35f, 0.12f, 0.6f), Mats.Emissive(new Color(1f, 0.75f, 0.45f), 2f), false,
                Quaternion.LookRotation(armDir).eulerAngles);
            if (!withLight) return null;
            var lgo = Shapes.Empty(t, "Luz", new Vector3(0, 7.2f, 0) + armDir * 1.7f);
            var l = lgo.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = 20f;
            l.intensity = 4f;
            l.color = new Color(1f, 0.72f, 0.42f);
            l.shadows = LightShadows.None;
            return l;
        }

        public static Light RoomLight(Transform p, Vector3 pos, float range, float intensity)
        {
            Shapes.Prim(PrimitiveType.Sphere, p, "Lampara", pos + Vector3.up * 0.1f, Vector3.one * 0.18f, Mats.Emissive(new Color(1f, 0.92f, 0.75f), 2.5f), false);
            var lgo = Shapes.Empty(p, "Luz", pos);
            var l = lgo.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = range;
            l.intensity = intensity;
            l.color = new Color(1f, 0.9f, 0.75f);
            l.shadows = LightShadows.None; // sin sombras: muchas luces con sombra saturan el atlas
            return l;
        }

        /// <summary>Cartel de madera sobre dos postes.</summary>
        public static void Sign(Transform p, Vector3 pos, float yaw, string text, Vector2 size)
        {
            var t = Shapes.Empty(p, "Cartel", pos, new Vector3(0, yaw, 0)).transform;
            Shapes.Box(t, "PosteI", new Vector3(-size.x / 2 + 0.05f, 0, 0), new Vector3(0.08f, 1.1f + size.y, 0.08f), Mats.Madera, true);
            Shapes.Box(t, "PosteD", new Vector3(size.x / 2 - 0.05f, 0, 0), new Vector3(0.08f, 1.1f + size.y, 0.08f), Mats.Madera, true);
            Shapes.Box(t, "Tabla", new Vector3(0, 1.0f, 0.05f), new Vector3(size.x, size.y, 0.04f), Mats.MaderaClara, false);
            WorldLabel.Create(t, text, new Vector3(0, 1.0f + size.y / 2, 0.075f), size * 0.95f, 110, new Color(0.2f, 0.12f, 0.05f), Vector3.forward);
            WorldLabel.Create(t, text, new Vector3(0, 1.0f + size.y / 2, 0.025f), size * 0.95f, 110, new Color(0.2f, 0.12f, 0.05f), Vector3.back);
        }
    }
}
