using System.Collections.Generic;
using UnityEngine;

namespace LaTranquera.EditorTools
{
    /// <summary>
    /// Piezas para construir el mapa: paredes con aberturas (con textura), ventanas con postigos,
    /// puertas, alambrados, postes de luz, carteles, aljibe, molino... Lo usa SceneBuilder.
    /// </summary>
    public static class WorldParts
    {
        public struct Opening
        {
            public float from, to, bottom, top;
            public bool window;
            public Opening(float f, float t, float b, float tp, bool w = false) { from = f; to = t; bottom = b; top = tp; window = w; }
        }

        // Materiales con textura de uso común
        public static Material Revoque => TexKit.Mat("revoque", Color.white);
        public static Material Ladrillo => TexKit.Mat("ladrillo", Color.white);
        public static Material Madera => TexKit.Mat("madera", Color.white);
        public static Material MaderaVerde => TexKit.Mat("madera", new Color(0.45f, 0.75f, 0.5f));
        public static Material Machimbre => TexKit.Mat("machimbre", new Color(1f, 0.95f, 0.85f));
        public static Material Calcareo => TexKit.Mat("calcareo", Color.white);
        public static Material Cemento => TexKit.Mat("cemento", Color.white);
        public static Material ChapaMat => TexKit.Mat("chapa", Color.white, 0.45f, 0.55f);
        public static Material Hierro => Mats.Get(new Color(0.12f, 0.12f, 0.13f), 0.4f, 0.6f);

        /// <summary>Caja con textura (UV en metros). center = centro de la caja.</summary>
        public static GameObject TBox(Transform p, string name, Vector3 center, Vector3 size, Material mat, float yaw = 0f, bool collider = true)
        {
            var go = MeshKit.Object(p, name, MeshKit.Box(size), new[] { mat }, center - Vector3.up * size.y / 2, new Vector3(0, yaw, 0), false);
            if (collider)
            {
                var bc = go.AddComponent<BoxCollider>();
                bc.center = Vector3.up * size.y / 2;
                bc.size = size;
            }
            return go;
        }

        /// <summary>Caja con textura apoyada (pos = centro de la base).</summary>
        public static GameObject TBoxB(Transform p, string name, Vector3 basePos, Vector3 size, Material mat, float yaw = 0f, bool collider = true)
            => TBox(p, name, basePos + Vector3.up * size.y / 2, size, mat, yaw, collider);

        /// <summary>Pared recta de a a b (en el piso) con puertas y ventanas. baseY = altura del piso.</summary>
        public static void Wall(Transform p, string name, Vector3 a, Vector3 b, float height, float thick, Material mat, params Opening[] openings)
            => Wall(p, name, a, b, height, thick, mat, true, openings);

        /// <summary>outsideRight: el lado de afuera queda a la derecha yendo de a a b (ahí van los postigos).</summary>
        public static void Wall(Transform p, string name, Vector3 a, Vector3 b, float height, float thick, Material mat, bool outsideRight, params Opening[] openings)
        {
            var dir = (b - a).normalized;
            float len = Vector3.Distance(a, b);
            float yaw = Quaternion.LookRotation(dir).eulerAngles.y;
            var list = new List<Opening>(openings);
            list.Sort((x, y) => x.from.CompareTo(y.from));
            float cursor = 0;
            void Piece(float f, float t, float y0, float y1)
            {
                if (t - f < 0.01f || y1 - y0 < 0.01f) return;
                var center = a + dir * ((f + t) / 2) + Vector3.up * ((y0 + y1) / 2);
                TBox(p, name, center, new Vector3(thick, y1 - y0, t - f), mat, yaw);
            }
            foreach (var o in list)
            {
                Piece(cursor, o.from, 0, height);
                Piece(o.from, o.to, 0, o.bottom);
                Piece(o.from, o.to, o.top, height);
                if (o.window) Window(p, a + dir * ((o.from + o.to) / 2) + Vector3.up * o.bottom, o.to - o.from, o.top - o.bottom, yaw, thick, outsideRight ? -1f : 1f);
                cursor = o.to;
            }
            Piece(cursor, len, 0, height);
        }

        /// <summary>Ventana antigua: marco, vidrio repartido en cuadritos y postigos verdes abiertos a los costados (afuera).</summary>
        public static void Window(Transform p, Vector3 bottomCenter, float width, float height, float wallYaw, float wallThick, float outSign)
        {
            var t = Shapes.Empty(p, "Ventana", bottomCenter, new Vector3(0, wallYaw - 90, 0)).transform;
            // en este espacio: X = a lo largo de la pared, Z * outSign = hacia afuera
            var glass = Mats.Glass(new Color(0.7f, 0.85f, 0.95f, 0.28f));
            Shapes.Prim(PrimitiveType.Cube, t, "Vidrio", new Vector3(0, height / 2, 0), new Vector3(width, height, 0.015f), glass, false);
            var frame = MaderaVerde;
            TBox(t, "MarcoArriba", new Vector3(0, height - 0.04f, 0), new Vector3(width, 0.08f, wallThick + 0.02f), frame, 0, false);
            TBox(t, "MarcoAbajo", new Vector3(0, 0.04f, 0.03f * outSign), new Vector3(width + 0.1f, 0.08f, wallThick + 0.08f), frame, 0, false);
            TBox(t, "MarcoMedio", new Vector3(0, height / 2, 0), new Vector3(0.05f, height, 0.05f), frame, 0, false);
            TBox(t, "Travesaño", new Vector3(0, height * 0.55f, 0), new Vector3(width, 0.04f, 0.04f), frame, 0, false);
            // postigos abiertos contra la pared, del lado de afuera
            float off = (wallThick / 2 + 0.03f) * outSign;
            for (int s = -1; s <= 1; s += 2)
            {
                var sh = TBox(t, "Postigo", new Vector3(s * (width / 2 + width / 4 + 0.02f), height / 2, off), new Vector3(width / 2, height, 0.035f), frame, 0, false);
                for (int k = 0; k < 6; k++)
                    TBox(sh.transform, "Tablilla", new Vector3(0, -height / 2 + 0.15f + k * (height - 0.3f) / 5f, 0.02f), new Vector3(width / 2 - 0.08f, 0.03f, 0.02f), frame, 0, false);
            }
            // reja
            for (int k = 1; k < 6; k++)
                Shapes.Prim(PrimitiveType.Cube, t, "Reja", new Vector3(-width / 2 + k * width / 6, height / 2, off + 0.02f * outSign), new Vector3(0.015f, height, 0.015f), Hierro, false);
        }

        /// <summary>Puerta de madera que se abre con E. hingePos = bisagra (piso). La hoja crece hacia +X local.</summary>
        public static LaTranquera.Door MakeDoor(Transform p, string name, Vector3 hingePos, float yaw, float width, float height, bool open, float openAngle = 100f, Material mat = null)
        {
            var root = Shapes.Empty(p, name, hingePos, new Vector3(0, yaw, 0));
            var leaf = Shapes.Empty(root.transform, "Hoja", Vector3.zero).transform;
            var m = mat ?? Madera;
            TBox(leaf, "Tabla", new Vector3(width / 2, height / 2, 0), new Vector3(width - 0.02f, height, 0.05f), m, 0, true);
            TBox(leaf, "Moldura", new Vector3(width / 2, height * 0.7f, 0.03f), new Vector3(width * 0.7f, height * 0.35f, 0.02f), m, 0, false);
            TBox(leaf, "Moldura", new Vector3(width / 2, height * 0.25f, 0.03f), new Vector3(width * 0.7f, height * 0.3f, 0.02f), m, 0, false);
            Shapes.Prim(PrimitiveType.Sphere, leaf, "Picaporte", new Vector3(width - 0.1f, 1.0f, 0.05f), Vector3.one * 0.05f, Mats.Get(new Color(0.75f, 0.6f, 0.3f), 0.8f, 0.8f), false);
            var d = root.AddComponent<LaTranquera.Door>();
            d.leaf = leaf;
            d.open = open;
            d.openAngle = openAngle;
            d.displayName = name;
            return d;
        }

        /// <summary>Alambrado de campo: postes de madera y 5 hilos. Con collider para no pasar.</summary>
        public static void Fence(Transform p, Vector3 a, Vector3 b, float spacing = 3f)
        {
            var root = Shapes.Empty(p, "Alambrado", Vector3.zero).transform;
            float len = Vector3.Distance(a, b);
            int n = Mathf.Max(1, Mathf.RoundToInt(len / spacing));
            var post = TexKit.Mat("madera", new Color(0.75f, 0.7f, 0.62f));
            for (int i = 0; i <= n; i++)
            {
                var pos = Vector3.Lerp(a, b, i / (float)n);
                bool strong = i == 0 || i == n;
                float s = strong ? 0.16f : 0.1f;
                TBoxB(root, "Poste", pos, new Vector3(s, strong ? 1.5f : 1.3f, s), post, 0, false);
            }
            Color wire = new Color(0.55f, 0.55f, 0.55f);
            foreach (float y in new[] { 0.3f, 0.52f, 0.74f, 0.96f, 1.18f })
                Shapes.Beam(root, "Alambre", a + Vector3.up * y, b + Vector3.up * y, 0.01f, wire);
            var col = new GameObject("ColliderAlambrado");
            col.transform.SetParent(root, false);
            col.transform.localPosition = (a + b) / 2 + Vector3.up * 0.75f;
            col.transform.localRotation = Quaternion.LookRotation((b - a).normalized);
            col.AddComponent<BoxCollider>().size = new Vector3(0.15f, 1.5f, len);
        }

        /// <summary>Poste de luz de la ruta. Si withLight, tiene una luz que se prende de noche.</summary>
        public static Light StreetLight(Transform p, Vector3 pos, Vector3 armDir, bool withLight)
        {
            var t = Shapes.Empty(p, "PosteDeLuz", pos).transform;
            var c = Cemento;
            TBoxB(t, "Poste", Vector3.zero, new Vector3(0.2f, 7.5f, 0.2f), c);
            Shapes.Beam(t, "Brazo", new Vector3(0, 7.3f, 0), new Vector3(0, 7.5f, 0) + armDir * 1.6f, 0.08f, new Color(0.5f, 0.5f, 0.5f));
            Shapes.BoxC(t, "Farola", new Vector3(0, 7.45f, 0) + armDir * 1.7f, new Vector3(0.35f, 0.12f, 0.6f), Mats.Emissive(new Color(1f, 0.75f, 0.45f), 2f), false,
                Quaternion.LookRotation(armDir).eulerAngles);
            if (!withLight) return null;
            return PointLight(t, new Vector3(0, 7.2f, 0) + armDir * 1.7f, 20f, 4f, new Color(1f, 0.72f, 0.42f));
        }

        public static Light PointLight(Transform p, Vector3 pos, float range, float intensity, Color color)
        {
            var lgo = Shapes.Empty(p, "Luz", pos);
            var l = lgo.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = range;
            l.intensity = intensity;
            l.color = color;
            l.shadows = LightShadows.None; // sin sombras: muchas luces con sombra saturan el atlas
            return l;
        }

        /// <summary>Lámpara colgante de techo (con luz cálida).</summary>
        public static Light RoomLight(Transform p, Vector3 pos, float range, float intensity)
        {
            Shapes.Beam(p, "Cable", pos + Vector3.up * 0.6f, pos + Vector3.up * 0.12f, 0.01f, Color.black);
            Shapes.Cone(p, "Pantalla", pos + Vector3.up * 0.02f, 0.35f, 0.15f, new Color(0.25f, 0.35f, 0.3f));
            Shapes.Prim(PrimitiveType.Sphere, p, "Lamparita", pos, Vector3.one * 0.1f, Mats.Emissive(new Color(1f, 0.92f, 0.75f), 2.5f), false);
            return PointLight(p, pos - Vector3.up * 0.1f, range, intensity, new Color(1f, 0.88f, 0.7f));
        }

        /// <summary>Cartel de madera sobre dos postes (texto de los dos lados).</summary>
        public static void Sign(Transform p, Vector3 pos, float yaw, string text, Vector2 size)
        {
            var t = Shapes.Empty(p, "Cartel", pos, new Vector3(0, yaw, 0)).transform;
            TBoxB(t, "PosteI", new Vector3(-size.x / 2 + 0.05f, 0, 0), new Vector3(0.08f, 1.1f + size.y, 0.08f), Madera);
            TBoxB(t, "PosteD", new Vector3(size.x / 2 - 0.05f, 0, 0), new Vector3(0.08f, 1.1f + size.y, 0.08f), Madera);
            TBoxB(t, "Tabla", new Vector3(0, 1.0f, 0.05f), new Vector3(size.x, size.y, 0.04f), TexKit.Mat("madera", new Color(1f, 0.95f, 0.85f)), 0, false);
            WorldLabel.Create(t, text, new Vector3(0, 1.0f + size.y / 2, 0.075f), size * 0.95f, 110, new Color(0.2f, 0.12f, 0.05f), Vector3.forward);
            WorldLabel.Create(t, text, new Vector3(0, 1.0f + size.y / 2, 0.025f), size * 0.95f, 110, new Color(0.2f, 0.12f, 0.05f), Vector3.back);
        }

        /// <summary>Techo de chapa acanalada de una sola agua: de la línea "low" (más baja) a la "high".</summary>
        public static void ChapaSlope(Transform p, string name, Vector3 lowCenter, Vector3 highCenter, float width, float overhang = 0.3f)
        {
            var dir = highCenter - lowCenter;
            float len = dir.magnitude + overhang;
            var mesh = MeshKit.Corrugated(width, len);
            var center = (lowCenter + highCenter) / 2 - dir.normalized * overhang / 2;
            var rot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            // la chapa tiene el largo en Z: la giramos para que suba de low a high
            var go = MeshKit.Object(p, name, mesh, new[] { ChapaMat }, center);
            go.transform.rotation = rot;
            var bc = go.AddComponent<BoxCollider>();
            bc.size = new Vector3(width, 0.08f, len);
        }

        /// <summary>Aljibe criollo: brocal de ladrillo y arco de hierro con roldana. Se saca agua acá.</summary>
        public static WaterSource Aljibe(Transform p, Vector3 pos)
        {
            var w = Shapes.Empty(p, "Aljibe", pos).transform;
            var bricks = Ladrillo;
            // brocal octogonal de ladrillo
            var b = new MeshKit.Builder(1);
            MeshKit.AddCylinder(b, 0, Matrix4x4.identity, 0.75f, 0.75f, 0.85f, 10);
            var brocal = MeshKit.Object(w, "Brocal", MeshKit.Persist("aljibe_brocal", () => b.Build()), new[] { bricks }, Vector3.zero);
            var cc = brocal.AddComponent<CapsuleCollider>(); cc.radius = 0.78f; cc.height = 0.85f; cc.center = Vector3.up * 0.42f;
            Shapes.Cyl(w, "Borde", new Vector3(0, 0.85f, 0), 1.62f, 0.07f, new Color(0.85f, 0.83f, 0.78f), false);
            Shapes.Cyl(w, "Agua", new Vector3(0, 0.8f, 0), 1.3f, 0.02f, new Color(0.05f, 0.08f, 0.12f), false);
            // arco de hierro
            var iron = Hierro;
            for (int i = 0; i < 12; i++)
            {
                float a0 = Mathf.PI * i / 12, a1 = Mathf.PI * (i + 1) / 12;
                var p0 = new Vector3(Mathf.Cos(a0) * 0.7f, 0.9f + Mathf.Sin(a0) * 1.4f, 0);
                var p1 = new Vector3(Mathf.Cos(a1) * 0.7f, 0.9f + Mathf.Sin(a1) * 1.4f, 0);
                var beam = Shapes.Beam(w, "Arco", p0, p1, 0.04f, Color.black);
                beam.GetComponent<Renderer>().sharedMaterial = iron;
            }
            Shapes.Prim(PrimitiveType.Sphere, w, "Remate", new Vector3(0, 2.35f, 0), Vector3.one * 0.12f, iron, false);
            Shapes.CylC(w, "Roldana", new Vector3(0, 2.2f, 0), 0.2f, 0.05f, new Color(0.3f, 0.3f, 0.3f), new Vector3(90, 0, 0));
            var bucket = Shapes.Empty(w, "BaldeDelAljibe", new Vector3(0, 1.25f, 0)).transform;
            Shapes.Cyl(bucket, "Balde", Vector3.zero, 0.26f, 0.28f, Mats.Chapa, false);
            Shapes.CylC(bucket, "Soga", new Vector3(0, 0.6f, 0), 0.02f, 0.65f, new Color(0.75f, 0.65f, 0.45f), Vector3.zero);
            var ws = w.gameObject.AddComponent<WaterSource>();
            ws.displayName = "Aljibe";
            ws.wellBucket = bucket;
            ws.bucketDrop = 1.1f;
            return ws;
        }

        /// <summary>Molino australiano (torre de hierro con rueda de aspas y cola) — decorativo.</summary>
        public static void Molino(Transform p, Vector3 pos)
        {
            var m = Shapes.Empty(p, "Molino", pos).transform;
            Color iron = new Color(0.45f, 0.47f, 0.5f);
            float h = 10f, baseW = 1.6f, topW = 0.35f;
            Vector3[] legsB = { new Vector3(-baseW, 0, -baseW), new Vector3(baseW, 0, -baseW), new Vector3(baseW, 0, baseW), new Vector3(-baseW, 0, baseW) };
            for (int i = 0; i < 4; i++)
            {
                var top = new Vector3(Mathf.Sign(legsB[i].x) * topW, h, Mathf.Sign(legsB[i].z) * topW);
                Shapes.Beam(m, "Pata", legsB[i], top, 0.08f, iron);
            }
            for (int k = 1; k < 6; k++)
            {
                float y = k * h / 6f, w = Mathf.Lerp(baseW, topW, y / h);
                Vector3[] c = { new Vector3(-w, y, -w), new Vector3(w, y, -w), new Vector3(w, y, w), new Vector3(-w, y, w) };
                for (int i = 0; i < 4; i++) Shapes.Beam(m, "Travesaño", c[i], c[(i + 1) % 4], 0.04f, iron);
            }
            var head = Shapes.Empty(m, "Cabezal", new Vector3(0, h + 0.3f, 0), new Vector3(0, 35, 0)).transform;
            Shapes.BoxC(head, "Caja", Vector3.zero, new Vector3(0.4f, 0.4f, 0.8f), iron, false);
            Shapes.Beam(head, "Cola", new Vector3(0, 0, -0.3f), new Vector3(0, 0, -2.4f), 0.06f, iron);
            Shapes.BoxC(head, "Veleta", new Vector3(0, 0.3f, -2.4f), new Vector3(0.03f, 0.9f, 1.3f), new Color(0.85f, 0.85f, 0.85f), false);
            var wheel = Shapes.Empty(head, "Rueda", new Vector3(0, 0, 0.5f)).transform;
            for (int i = 0; i < 18; i++)
            {
                var a = Quaternion.Euler(0, 0, i * 20f);
                var blade = Shapes.BoxC(wheel, "Aspa", a * new Vector3(0, 1.05f, 0), new Vector3(0.22f, 1.3f, 0.02f), new Color(0.82f, 0.82f, 0.8f), false);
                blade.transform.localRotation = a * Quaternion.Euler(0, 25, 0);
            }
            Shapes.Beam(m, "Bomba", new Vector3(0, 0, 0), new Vector3(0, h, 0), 0.05f, new Color(0.3f, 0.3f, 0.3f));
            var col = m.gameObject.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 1.5f, 0); col.size = new Vector3(3.2f, 3f, 3.2f);
        }

        /// <summary>Tanque australiano de chapa (redondo y bajo) — decorativo.</summary>
        public static void TanqueAustraliano(Transform p, Vector3 pos, float diameter = 7f)
        {
            var b = new MeshKit.Builder(1);
            MeshKit.AddCylinder(b, 0, Matrix4x4.identity, diameter / 2, diameter / 2, 1.5f, 28, false);
            var mesh = MeshKit.Persist("tanque_" + Mathf.RoundToInt(diameter * 10), () => b.Build());
            var t = MeshKit.Object(p, "TanqueAustraliano", mesh, new[] { ChapaMat }, pos);
            Shapes.Cyl(t.transform, "Agua", new Vector3(0, 1.25f, 0), diameter - 0.1f, 0.02f, new Color(0.15f, 0.25f, 0.3f), false);
            var cc = t.AddComponent<CapsuleCollider>(); cc.radius = diameter / 2; cc.height = 1.5f; cc.center = Vector3.up * 0.75f;
        }
    }
}
