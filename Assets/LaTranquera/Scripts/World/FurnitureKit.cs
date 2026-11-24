using System.Collections.Generic;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Muebles y objetos de la casa con forma de verdad (patas torneadas, bordes biselados, piezas redondas):
    /// mesa, silla, bañera con patas, inodoro con mochila alta, lavatorio de pie, cama de hierro, ropero,
    /// mesa de luz y velador, radio antigua, heladera Siam, cocina económica a leña (con fuego), pava, mate...
    /// Cada mueble es una malla guardada (se comparte entre muebles iguales) con sus materiales.
    /// </summary>
    public static class FurnitureKit
    {
        // Materiales
        public static Material Wood => TexKit.Mat("madera", Color.white);
        public static Material WoodDark => TexKit.Mat("madera", new Color(0.65f, 0.5f, 0.4f));
        public static Material Porcelain => Mats.Get(new Color(0.96f, 0.96f, 0.94f), 0.85f);
        public static Material Iron => Mats.Get(new Color(0.1f, 0.1f, 0.11f), 0.45f, 0.6f);
        public static Material Chrome => Mats.Get(new Color(0.85f, 0.85f, 0.88f), 0.9f, 0.95f);
        public static Material Brass => Mats.Get(new Color(0.8f, 0.62f, 0.3f), 0.8f, 0.9f);
        public static Material Fabric(Color c) => Mats.Get(c, 0.05f);
        public static Material Glass => Mats.Get(new Color(0.7f, 0.82f, 0.88f), 0.95f, 0.3f);

        static Matrix4x4 T(float x, float y, float z) => Matrix4x4.Translate(new Vector3(x, y, z));
        static Matrix4x4 TR(Vector3 p, Vector3 e) => Matrix4x4.TRS(p, Quaternion.Euler(e), Vector3.one);

        static GameObject Make(Transform parent, string name, string key, System.Func<MeshKit.Builder> build, Material[] mats, Vector3 pos, float yaw, bool collider = true)
        {
            var mesh = MeshKit.Persist(key, () => build().Build(key));
            return MeshKit.Object(parent, name, mesh, mats, pos, new Vector3(0, yaw, 0), collider);
        }

        /// <summary>Pata torneada (estilo antiguo).</summary>
        static void TurnedLeg(MeshKit.Builder b, int sub, Vector3 basePos, float h, float r)
        {
            MeshKit.AddLathe(b, sub, T(basePos.x, basePos.y, basePos.z),
                MeshKit.Prof(0, 0, r * 0.7f, 0, r * 0.8f, h * 0.05f, r * 0.55f, h * 0.15f, r * 0.9f, h * 0.3f, r * 0.55f, h * 0.45f,
                             r * 0.75f, h * 0.7f, r, h * 0.85f, r, h, 0, h), 10);
        }

        // ------------------------------------------------------------------
        public static GameObject Table(Transform parent, Vector3 pos, float yaw, float w, float d)
        {
            string key = "mesa_" + Mathf.RoundToInt(w * 100) + "x" + Mathf.RoundToInt(d * 100);
            return Make(parent, "Mesa", key, () =>
            {
                var b = new MeshKit.Builder(1);
                MeshKit.AddChamferBox(b, 0, new Vector3(0, 0.72f, 0), new Vector3(w, 0.045f, d), 0.015f);
                MeshKit.AddBox(b, 0, new Vector3(0, 0.65f, d / 2 - 0.07f), new Vector3(w - 0.16f, 0.1f, 0.025f));
                MeshKit.AddBox(b, 0, new Vector3(0, 0.65f, -d / 2 + 0.07f), new Vector3(w - 0.16f, 0.1f, 0.025f));
                MeshKit.AddBox(b, 0, new Vector3(w / 2 - 0.07f, 0.65f, 0), new Vector3(0.025f, 0.1f, d - 0.16f));
                MeshKit.AddBox(b, 0, new Vector3(-w / 2 + 0.07f, 0.65f, 0), new Vector3(0.025f, 0.1f, d - 0.16f));
                foreach (var c in new[] { new Vector2(1, 1), new Vector2(-1, 1), new Vector2(1, -1), new Vector2(-1, -1) })
                    TurnedLeg(b, 0, new Vector3(c.x * (w / 2 - 0.07f), 0, c.y * (d / 2 - 0.07f)), 0.72f, 0.035f);
                return b;
            }, new[] { Wood }, pos, yaw);
        }

        public static GameObject Chair(Transform parent, Vector3 pos, float yaw)
        {
            var go = Make(parent, "Silla", "silla", () =>
            {
                var b = new MeshKit.Builder(2);
                MeshKit.AddChamferBox(b, 1, new Vector3(0, 0.44f, 0), new Vector3(0.42f, 0.04f, 0.4f), 0.012f); // asiento de esterilla
                foreach (var c in new[] { new Vector2(1, 1), new Vector2(-1, 1) })
                    TurnedLeg(b, 0, new Vector3(c.x * 0.18f, 0, c.y * 0.16f), 0.44f, 0.022f);
                foreach (var x in new[] { -0.18f, 0.18f })
                    MeshKit.AddCylinder(b, 0, T(x, 0, -0.17f), 0.022f, 0.018f, 0.95f, 8);
                for (int i = 0; i < 3; i++)
                    MeshKit.AddBox(b, 0, new Vector3(0, 0.6f + i * 0.11f, -0.17f), new Vector3(0.36f, 0.05f, 0.02f));
                MeshKit.AddBox(b, 0, new Vector3(0, 0.15f, 0), new Vector3(0.36f, 0.02f, 0.02f));
                return b;
            }, new[] { WoodDark, Mats.Get(new Color(0.8f, 0.7f, 0.45f), 0.1f) }, pos, yaw, false);
            var bc = go.AddComponent<BoxCollider>(); bc.center = new Vector3(0, 0.3f, 0); bc.size = new Vector3(0.42f, 0.6f, 0.42f);
            return go;
        }

        /// <summary>Mesa con sillas alrededor.</summary>
        public static void DiningSet(Transform parent, Vector3 pos, float w, float d, int chairs)
        {
            Table(parent, pos, 0, w, d);
            int perSide = Mathf.Max(1, chairs / 2);
            for (int i = 0; i < chairs; i++)
            {
                float side = i % 2 == 0 ? -1 : 1;
                int k = i / 2;
                float x = perSide == 1 ? 0 : Mathf.Lerp(-w / 2 + 0.35f, w / 2 - 0.35f, k / (float)(perSide - 1));
                Chair(parent, pos + new Vector3(x, 0, side * (d / 2 + 0.25f)), side < 0 ? 0 : 180);
            }
        }

        // ------------------------------------------------------------------
        public static GameObject Bathtub(Transform parent, Vector3 pos, float yaw)
        {
            return Make(parent, "Bañera", "banera", () =>
            {
                var b = new MeshKit.Builder(3);
                MeshKit.AddBasin(b, 0, new Vector3(0, 0.14f, 0), 1.6f, 0.75f, 0.33f, 0.5f, 0.05f, 0.08f);
                // patas de garra
                foreach (var c in new[] { new Vector2(1, 1), new Vector2(-1, 1), new Vector2(1, -1), new Vector2(-1, -1) })
                {
                    var p = new Vector3(c.x * 0.6f, 0, c.y * 0.25f);
                    MeshKit.AddLathe(b, 1, T(p.x, 0, p.z), MeshKit.Prof(0, 0, 0.06f, 0, 0.05f, 0.04f, 0.03f, 0.1f, 0.05f, 0.16f, 0, 0.17f), 8);
                }
                // canilla y grifos
                MeshKit.AddCylinder(b, 2, TR(new Vector3(-0.72f, 0.62f, 0), new Vector3(0, 0, 0)), 0.02f, 0.02f, 0.25f, 8);
                MeshKit.AddCylinder(b, 2, TR(new Vector3(-0.72f, 0.86f, 0), new Vector3(0, 0, -90)), 0.02f, 0.015f, 0.15f, 8);
                MeshKit.AddCylinder(b, 2, TR(new Vector3(-0.72f, 0.7f, 0.1f), Vector3.zero), 0.03f, 0.03f, 0.04f, 8);
                MeshKit.AddCylinder(b, 2, TR(new Vector3(-0.72f, 0.7f, -0.1f), Vector3.zero), 0.03f, 0.03f, 0.04f, 8);
                return b;
            }, new[] { Porcelain, Iron, Chrome }, pos, yaw);
        }

        public static GameObject Toilet(Transform parent, Vector3 pos, float yaw)
        {
            return Make(parent, "Inodoro", "inodoro", () =>
            {
                var b = new MeshKit.Builder(3);
                // pie y taza (óvalo), con el hueco adentro
                MeshKit.AddLathe(b, 0, T(0, 0, 0.05f), MeshKit.Prof(0.11f, 0, 0.12f, 0.15f, 0.15f, 0.28f, 0.19f, 0.38f, 0.19f, 0.4f, 0.15f, 0.4f, 0.1f, 0.3f, 0.05f, 0.22f, 0, 0.22f), 16, 1.3f);
                // asiento y tapa de madera
                MeshKit.AddLathe(b, 1, T(0, 0.4f, 0.05f), MeshKit.Prof(0.13f, 0, 0.2f, 0, 0.2f, 0.025f, 0.13f, 0.025f, 0.13f, 0), 16, 1.3f);
                MeshKit.AddChamferBox(b, 1, new Vector3(0, 0.38f, -0.22f), new Vector3(0.3f, 0.06f, 0.12f), 0.01f);
                // mochila alta con caño y cadena
                MeshKit.AddChamferBox(b, 0, new Vector3(0, 1.75f, -0.3f), new Vector3(0.45f, 0.3f, 0.2f), 0.03f);
                MeshKit.AddCylinder(b, 2, T(0, 0.35f, -0.28f), 0.025f, 0.025f, 1.4f, 8);
                for (int i = 0; i < 8; i++) MeshKit.AddBox(b, 2, new Vector3(0.18f, 1.72f - i * 0.06f, -0.22f), new Vector3(0.008f, 0.04f, 0.008f));
                MeshKit.AddLathe(b, 1, T(0.18f, 1.18f, -0.22f), MeshKit.Prof(0, 0, 0.025f, 0.02f, 0.02f, 0.08f, 0, 0.1f), 8);
                return b;
            }, new[] { Porcelain, Wood, Chrome }, pos, yaw);
        }

        public static GameObject PedestalSink(Transform parent, Vector3 pos, float yaw)
        {
            return Make(parent, "Lavatorio", "lavatorio", () =>
            {
                var b = new MeshKit.Builder(2);
                MeshKit.AddLathe(b, 0, Matrix4x4.identity, MeshKit.Prof(0, 0, 0.14f, 0, 0.12f, 0.05f, 0.08f, 0.2f, 0.07f, 0.6f, 0.1f, 0.72f, 0, 0.72f), 14);
                MeshKit.AddLathe(b, 0, T(0, 0.7f, 0), MeshKit.Prof(0, 0, 0.12f, 0, 0.26f, 0.1f, 0.27f, 0.15f, 0.24f, 0.15f, 0.2f, 0.08f, 0, 0.05f), 16, 0.75f);
                MeshKit.AddCylinder(b, 1, T(0, 0.85f, -0.17f), 0.02f, 0.02f, 0.15f, 8);
                MeshKit.AddCylinder(b, 1, TR(new Vector3(0, 0.99f, -0.17f), new Vector3(90, 0, 0)), 0.018f, 0.012f, 0.12f, 8);
                return b;
            }, new[] { Porcelain, Chrome }, pos, yaw);
        }

        // ------------------------------------------------------------------
        public static GameObject IronBed(Transform parent, Vector3 pos, float yaw, Color quilt)
        {
            var go = Make(parent, "Cama", "camaHierro_" + ColorUtility.ToHtmlStringRGB(quilt), () =>
            {
                var b = new MeshKit.Builder(5);
                float L = 2.0f, W = 1.45f;
                // respaldo y piecera de barrotes con perillas de bronce
                foreach (var end in new[] { -1f, 1f })
                {
                    float x = end * L / 2, h = end < 0 ? 1.2f : 0.85f;
                    foreach (var z in new[] { -W / 2, W / 2 })
                    {
                        MeshKit.AddCylinder(b, 0, T(x, 0, z), 0.03f, 0.03f, h, 8);
                        MeshKit.AddLathe(b, 1, T(x, h, z), MeshKit.Prof(0, 0, 0.04f, 0.02f, 0.045f, 0.05f, 0, 0.09f), 10);
                    }
                    MeshKit.AddCylinder(b, 0, TR(new Vector3(x, h - 0.05f, -W / 2), new Vector3(90, 0, 0)), 0.02f, 0.02f, W, 8);
                    MeshKit.AddCylinder(b, 0, TR(new Vector3(x, 0.35f, -W / 2), new Vector3(90, 0, 0)), 0.02f, 0.02f, W, 8);
                    for (int i = 1; i < 8; i++)
                        MeshKit.AddCylinder(b, 0, T(x, 0.35f, -W / 2 + i * W / 8), 0.01f, 0.01f, h - 0.4f, 6);
                }
                MeshKit.AddBox(b, 0, new Vector3(0, 0.3f, -W / 2 + 0.02f), new Vector3(L, 0.06f, 0.03f));
                MeshKit.AddBox(b, 0, new Vector3(0, 0.3f, W / 2 - 0.02f), new Vector3(L, 0.06f, 0.03f));
                MeshKit.AddChamferBox(b, 2, new Vector3(0, 0.33f, 0), new Vector3(L - 0.08f, 0.2f, W - 0.06f), 0.06f);   // colchón
                MeshKit.AddChamferBox(b, 3, new Vector3(0.22f, 0.5f, 0), new Vector3(L - 0.5f, 0.05f, W + 0.02f), 0.02f);  // colcha
                MeshKit.AddChamferBox(b, 3, new Vector3(0.22f, 0.32f, W / 2 + 0.005f), new Vector3(L - 0.5f, 0.22f, 0.02f), 0.005f);
                MeshKit.AddChamferBox(b, 3, new Vector3(0.22f, 0.32f, -W / 2 - 0.005f), new Vector3(L - 0.5f, 0.22f, 0.02f), 0.005f);
                MeshKit.AddBlob(b, 4, new Vector3(-L / 2 + 0.3f, 0.6f, -0.33f), new Vector3(0.35f, 0.14f, 0.55f), 3, 0.05f);  // almohadas
                MeshKit.AddBlob(b, 4, new Vector3(-L / 2 + 0.3f, 0.6f, 0.33f), new Vector3(0.35f, 0.14f, 0.55f), 4, 0.05f);
                return b;
            }, new[] { Iron, Brass, Fabric(new Color(0.92f, 0.9f, 0.85f)), Fabric(quilt), Fabric(Color.white) }, pos, yaw, false);
            var bc = go.AddComponent<BoxCollider>(); bc.center = new Vector3(0, 0.3f, 0); bc.size = new Vector3(2.05f, 0.6f, 1.5f);
            return go;
        }

        public static GameObject Wardrobe(Transform parent, Vector3 pos, float yaw)
        {
            return Make(parent, "Ropero", "ropero", () =>
            {
                var b = new MeshKit.Builder(3);
                float w = 1.6f, d = 0.6f, h = 2.0f;
                MeshKit.AddChamferBox(b, 0, new Vector3(0, 0.12f, 0), new Vector3(w, h - 0.12f, d), 0.02f);
                MeshKit.AddChamferBox(b, 0, new Vector3(0, h, 0), new Vector3(w + 0.1f, 0.1f, d + 0.06f), 0.03f); // cornisa
                foreach (var x in new[] { -w / 2 + 0.06f, w / 2 - 0.06f }) foreach (var z in new[] { -d / 2 + 0.06f, d / 2 - 0.06f })
                    TurnedLeg(b, 0, new Vector3(x, 0, z), 0.12f, 0.04f);
                // puertas con molduras (frente = +Z)
                for (int i = 0; i < 2; i++)
                {
                    float x = (i - 0.5f) * w / 2;
                    MeshKit.AddBox(b, 0, new Vector3(x, 1.45f, d / 2 + 0.012f), new Vector3(w / 2 - 0.12f, 0.6f, 0.02f));
                    MeshKit.AddBox(b, 0, new Vector3(x, 0.55f, d / 2 + 0.012f), new Vector3(w / 2 - 0.12f, 0.6f, 0.02f));
                    MeshKit.AddLathe(b, 1, TR(new Vector3(x + (i == 0 ? 0.3f : -0.3f), 1.05f, d / 2 + 0.02f), new Vector3(90, 0, 0)), MeshKit.Prof(0, 0, 0.02f, 0, 0.025f, 0.03f, 0, 0.04f), 8);
                }
                MeshKit.AddBox(b, 2, new Vector3(0, 1.05f, d / 2 + 0.005f), new Vector3(0.03f, 1.7f, 0.01f));
                return b;
            }, new[] { WoodDark, Brass, Iron }, pos, yaw);
        }

        public static GameObject Nightstand(Transform parent, Vector3 pos, float yaw)
        {
            return Make(parent, "MesaDeLuz", "mesaDeLuz", () =>
            {
                var b = new MeshKit.Builder(2);
                MeshKit.AddChamferBox(b, 0, new Vector3(0, 0.12f, 0), new Vector3(0.45f, 0.45f, 0.4f), 0.015f);
                MeshKit.AddChamferBox(b, 0, new Vector3(0, 0.57f, 0), new Vector3(0.5f, 0.03f, 0.45f), 0.01f);
                foreach (var x in new[] { -0.18f, 0.18f }) foreach (var z in new[] { -0.15f, 0.15f }) TurnedLeg(b, 0, new Vector3(x, 0, z), 0.12f, 0.025f);
                MeshKit.AddLathe(b, 1, TR(new Vector3(0, 0.45f, 0.21f), new Vector3(90, 0, 0)), MeshKit.Prof(0, 0, 0.018f, 0, 0.02f, 0.02f, 0, 0.03f), 8);
                return b;
            }, new[] { WoodDark, Brass }, pos, yaw);
        }

        /// <summary>Velador de mesa de luz (devuelve la luz para poder prenderla y apagarla).</summary>
        public static Light Lamp(Transform parent, Vector3 pos)
        {
            var go = Make(parent, "Velador", "velador", () =>
            {
                var b = new MeshKit.Builder(2);
                MeshKit.AddLathe(b, 0, Matrix4x4.identity, MeshKit.Prof(0, 0, 0.08f, 0, 0.08f, 0.02f, 0.03f, 0.05f, 0.025f, 0.15f, 0.045f, 0.2f, 0.02f, 0.28f, 0, 0.3f), 12);
                MeshKit.AddLathe(b, 1, T(0, 0.24f, 0), MeshKit.Prof(0.13f, 0, 0.12f, 0.005f, 0.07f, 0.16f, 0.06f, 0.16f, 0.11f, 0.005f), 14, 1f, false);
                return b;
            }, new[] { Brass, Mats.Emissive(new Color(1f, 0.88f, 0.65f), 0.6f) }, pos, 0, false);
            var bc = go.AddComponent<BoxCollider>(); bc.center = new Vector3(0, 0.2f, 0); bc.size = new Vector3(0.28f, 0.42f, 0.28f);
            var lgo = Shapes.Empty(go.transform, "Luz", new Vector3(0, 0.3f, 0));
            var l = lgo.AddComponent<Light>();
            l.type = LightType.Point; l.range = 3.5f; l.intensity = 0.9f; l.color = new Color(1f, 0.82f, 0.55f); l.shadows = LightShadows.None;
            return l;
        }

        // ------------------------------------------------------------------
        public static GameObject Radio(Transform parent, Vector3 pos, float yaw)
        {
            return Make(parent, "Radio", "radio", () =>
            {
                var b = new MeshKit.Builder(4);
                // gabinete de madera con el techo en arco (frente hacia +Z)
                var prof = new List<Vector2>();
                prof.Add(new Vector2(-0.18f, 0)); prof.Add(new Vector2(0.18f, 0));
                for (int i = 0; i <= 12; i++)
                {
                    float a = Mathf.PI * i / 12f;
                    prof.Add(new Vector2(Mathf.Cos(a) * 0.18f, 0.16f + Mathf.Sin(a) * 0.11f));
                }
                // el perfil es el frente (ancho, alto); se extruye 18 cm de fondo y se gira para que mire a +Z
                var tmp = new MeshKit.Builder(1);
                MeshKit.AddExtrusion(tmp, 0, prof, 0.18f);
                b.Add(tmp.Build(), Matrix4x4.Rotate(Quaternion.Euler(0, 90, 0)), 0);
                // tela del parlante, dial y perillas
                MeshKit.AddBox(b, 1, new Vector3(0, 0.08f, 0.092f), new Vector3(0.22f, 0.13f, 0.005f));
                MeshKit.AddBox(b, 2, new Vector3(0, 0.21f, 0.092f), new Vector3(0.14f, 0.06f, 0.006f));
                foreach (var x in new[] { -0.1f, 0.1f })
                    MeshKit.AddLathe(b, 3, TR(new Vector3(x, 0.21f, 0.09f), new Vector3(90, 0, 0)), MeshKit.Prof(0, 0, 0.022f, 0, 0.02f, 0.02f, 0, 0.022f), 10);
                return b;
            }, new[] { WoodDark, Fabric(new Color(0.75f, 0.65f, 0.45f)), Mats.Emissive(new Color(1f, 0.85f, 0.5f), 0.4f), Mats.Get(new Color(0.2f, 0.15f, 0.1f), 0.5f) }, pos, yaw);
        }

        public static GameObject SiamFridge(Transform parent, Vector3 pos, float yaw)
        {
            return Make(parent, "HeladeraSiam", "heladeraSiam", () =>
            {
                var b = new MeshKit.Builder(3);
                // silueta de costado con el techo redondeado (frente +Z)
                var prof = new List<Vector2> { new Vector2(-0.32f, 0.08f), new Vector2(0.34f, 0.08f) };
                for (int i = 0; i <= 8; i++)
                {
                    float a = Mathf.PI / 2 * i / 8f;
                    prof.Add(new Vector2(0.18f + Mathf.Cos(a) * 0.16f, 1.45f + Mathf.Sin(a) * 0.16f));
                }
                prof.Add(new Vector2(-0.32f, 1.61f));
                MeshKit.AddExtrusion(b, 0, prof, 0.68f);
                MeshKit.AddBox(b, 1, new Vector3(-0.3f, 0.95f, 0.35f), new Vector3(0.04f, 0.3f, 0.04f));       // manija cromada
                MeshKit.AddBox(b, 1, new Vector3(0, 1.3f, 0.345f), new Vector3(0.25f, 0.05f, 0.01f));          // placa con la marca
                MeshKit.AddBox(b, 2, new Vector3(0, 0.04f, 0), new Vector3(0.6f, 0.08f, 0.55f));              // zócalo
                MeshKit.AddBox(b, 1, new Vector3(0, 1.1f, 0.342f), new Vector3(0.66f, 0.01f, 0.005f));         // línea de la puerta
                return b;
            }, new[] { Mats.Get(new Color(0.94f, 0.94f, 0.9f), 0.7f), Chrome, Iron }, pos, yaw);
        }

        /// <summary>Cocina económica a leña: hierro negro, horno, caño y el fuego (se prende con E). Frente = +Z.</summary>
        public static GameObject WoodStove(Transform parent, Vector3 pos, float yaw, out GameObject fire)
        {
            var go = Make(parent, "CocinaALeña", "cocinaLena", () =>
            {
                var b = new MeshKit.Builder(3);
                MeshKit.AddChamferBox(b, 0, new Vector3(0, 0.1f, 0), new Vector3(1.2f, 0.72f, 0.65f), 0.03f);
                MeshKit.AddChamferBox(b, 0, new Vector3(0, 0.82f, 0), new Vector3(1.26f, 0.05f, 0.7f), 0.015f);   // plancha
                foreach (var x in new[] { -0.55f, 0.55f }) foreach (var z in new[] { -0.28f, 0.28f }) TurnedLeg(b, 0, new Vector3(x, 0, z), 0.1f, 0.04f);
                foreach (var x in new[] { -0.3f, 0.15f })
                    MeshKit.AddLathe(b, 0, T(x, 0.86f, 0), MeshKit.Prof(0.13f, 0, 0.12f, 0.008f, 0.03f, 0.008f, 0, 0.012f), 14);         // hornallas (aros)
                MeshKit.AddBox(b, 1, new Vector3(0.25f, 0.42f, 0.33f), new Vector3(0.4f, 0.38f, 0.02f));             // puerta del horno
                MeshKit.AddBox(b, 1, new Vector3(0.25f, 0.65f, 0.36f), new Vector3(0.3f, 0.025f, 0.03f));            // manija
                MeshKit.AddBox(b, 2, new Vector3(-0.3f, 0.35f, 0.326f), new Vector3(0.3f, 0.26f, 0.01f));            // boca del fogón (negra)
                MeshKit.AddBox(b, 1, new Vector3(0, 0.78f, 0.36f), new Vector3(1.24f, 0.02f, 0.02f));                // baranda
                MeshKit.AddCylinder(b, 0, T(-0.45f, 0.84f, -0.22f), 0.08f, 0.08f, 2.6f, 10);                          // caño
                return b;
            }, new[] { Iron, Chrome, Mats.Get(new Color(0.02f, 0.02f, 0.02f), 0.1f) }, pos, yaw);
            // el fuego se ve por la boca del fogón
            var f = Fire.Build(go.transform, new Vector3(-0.3f, 0.24f, 0.2f), 0.7f);
            fire = f.gameObject;
            return go;
        }

        public static GameObject Kettle(Transform parent, Vector3 pos, float yaw)
        {
            return Make(parent, "Pava", "pava", () =>
            {
                var b = new MeshKit.Builder(2);
                MeshKit.AddLathe(b, 0, Matrix4x4.identity, MeshKit.Prof(0, 0, 0.08f, 0, 0.095f, 0.04f, 0.09f, 0.1f, 0.05f, 0.15f, 0.03f, 0.16f, 0, 0.16f), 14);
                MeshKit.AddLathe(b, 1, T(0, 0.155f, 0), MeshKit.Prof(0, 0, 0.03f, 0, 0.02f, 0.02f, 0, 0.03f), 10);
                MeshKit.AddCylinder(b, 0, TR(new Vector3(0.07f, 0.05f, 0), new Vector3(0, 0, -55)), 0.018f, 0.01f, 0.13f, 8);
                for (int i = 0; i < 8; i++)
                {
                    float a0 = Mathf.PI * i / 8, a1 = Mathf.PI * (i + 1) / 8;
                    var p0 = new Vector3(Mathf.Cos(a0) * 0.06f, 0.16f + Mathf.Sin(a0) * 0.08f, 0);
                    var p1 = new Vector3(Mathf.Cos(a1) * 0.06f, 0.16f + Mathf.Sin(a1) * 0.08f, 0);
                    MeshKit.AddCylinder(b, 1, Matrix4x4.TRS(p0, Quaternion.FromToRotation(Vector3.up, p1 - p0), Vector3.one), 0.007f, 0.007f, (p1 - p0).magnitude, 6, false);
                }
                return b;
            }, new[] { Mats.Get(new Color(0.75f, 0.75f, 0.78f), 0.8f, 0.8f), Mats.Get(new Color(0.12f, 0.1f, 0.08f), 0.4f) }, pos, yaw, false);
        }

        public static GameObject Mate(Transform parent, Vector3 pos)
        {
            return Make(parent, "Mate", "mate", () =>
            {
                var b = new MeshKit.Builder(3);
                MeshKit.AddLathe(b, 0, Matrix4x4.identity, MeshKit.Prof(0, 0, 0.025f, 0, 0.04f, 0.02f, 0.042f, 0.05f, 0.033f, 0.08f, 0.036f, 0.09f, 0.03f, 0.09f, 0, 0.075f), 12);
                MeshKit.AddLathe(b, 2, T(0, 0.075f, 0), MeshKit.Prof(0, 0, 0.03f, 0, 0.025f, 0.012f, 0, 0.014f), 10); // yerba
                MeshKit.AddCylinder(b, 1, TR(new Vector3(0.005f, 0.06f, 0), new Vector3(0, 0, -14)), 0.004f, 0.004f, 0.13f, 6);
                return b;
            }, new[] { Mats.Get(new Color(0.42f, 0.27f, 0.15f), 0.3f), Chrome, Mats.Get(new Color(0.45f, 0.55f, 0.2f), 0.05f) }, pos, 0, false);
        }

        public static GameObject Sideboard(Transform parent, Vector3 pos, float yaw)
        {
            return Make(parent, "Aparador", "aparador", () =>
            {
                var b = new MeshKit.Builder(3);
                MeshKit.AddChamferBox(b, 0, new Vector3(0, 0.12f, 0), new Vector3(1.8f, 0.78f, 0.5f), 0.02f);
                MeshKit.AddChamferBox(b, 0, new Vector3(0, 0.9f, 0), new Vector3(1.86f, 0.04f, 0.54f), 0.015f);
                foreach (var x in new[] { -0.82f, 0.82f }) foreach (var z in new[] { -0.2f, 0.2f }) TurnedLeg(b, 0, new Vector3(x, 0, z), 0.12f, 0.04f);
                for (int i = 0; i < 3; i++)
                {
                    float x = (i - 1) * 0.58f;
                    MeshKit.AddBox(b, 0, new Vector3(x, 0.75f, 0.258f), new Vector3(0.52f, 0.14f, 0.02f));
                    MeshKit.AddBox(b, 0, new Vector3(x, 0.4f, 0.258f), new Vector3(0.52f, 0.45f, 0.02f));
                    MeshKit.AddLathe(b, 1, TR(new Vector3(x, 0.75f, 0.27f), new Vector3(90, 0, 0)), MeshKit.Prof(0, 0, 0.018f, 0, 0.02f, 0.02f, 0, 0.03f), 8);
                }
                // vitrina arriba con vidrio
                MeshKit.AddChamferBox(b, 0, new Vector3(0, 0.94f, -0.1f), new Vector3(1.6f, 0.9f, 0.3f), 0.02f);
                MeshKit.AddBox(b, 2, new Vector3(0, 1.39f, 0.052f), new Vector3(1.45f, 0.75f, 0.01f));
                return b;
            }, new[] { WoodDark, Brass, Glass }, pos, yaw);
        }

        /// <summary>Mesada de cocina con bacha y canilla. Frente = +Z.</summary>
        public static GameObject KitchenCounter(Transform parent, Vector3 pos, float yaw, float w)
        {
            return Make(parent, "Mesada", "mesada_" + Mathf.RoundToInt(w * 100), () =>
            {
                var b = new MeshKit.Builder(4);
                MeshKit.AddChamferBox(b, 0, new Vector3(0, 0.08f, 0), new Vector3(w, 0.66f, 0.58f), 0.01f);
                MeshKit.AddBox(b, 0, new Vector3(0, 0.74f, 0.28f), new Vector3(w, 0.12f, 0.02f));
                MeshKit.AddBox(b, 0, new Vector3(0, 0.74f, -0.28f), new Vector3(w, 0.12f, 0.02f));
                MeshKit.AddBox(b, 0, new Vector3(w / 2 - 0.01f, 0.74f, 0), new Vector3(0.02f, 0.12f, 0.58f));
                MeshKit.AddBox(b, 0, new Vector3(-w / 2 + 0.01f, 0.74f, 0), new Vector3(0.02f, 0.12f, 0.58f));
                int doors = Mathf.Max(2, Mathf.RoundToInt(w / 0.55f));
                for (int i = 0; i < doors; i++)
                {
                    float x = -w / 2 + (i + 0.5f) * w / doors;
                    MeshKit.AddBox(b, 0, new Vector3(x, 0.47f, 0.295f), new Vector3(w / doors - 0.04f, 0.7f, 0.015f));
                    MeshKit.AddBox(b, 1, new Vector3(x + (i % 2 == 0 ? 0.18f : -0.18f), 0.7f, 0.31f), new Vector3(0.015f, 0.1f, 0.015f));
                }
                // tapa de granito con la bacha
                // tapa de granito en partes, dejando el hueco de la bacha
                float bx = -w / 4, hole = 0.29f, left = bx - hole - (-w / 2 - 0.02f), right = (w / 2 + 0.02f) - (bx + hole);
                MeshKit.AddBox(b, 2, new Vector3(-w / 2 - 0.02f + left / 2, 0.88f, 0), new Vector3(left, 0.04f, 0.62f));
                MeshKit.AddBox(b, 2, new Vector3(bx + hole + right / 2, 0.88f, 0), new Vector3(right, 0.04f, 0.62f));
                MeshKit.AddBox(b, 2, new Vector3(bx, 0.88f, 0.26f), new Vector3(hole * 2, 0.04f, 0.1f));
                MeshKit.AddBox(b, 2, new Vector3(bx, 0.88f, -0.26f), new Vector3(hole * 2, 0.04f, 0.1f));
                MeshKit.AddBasin(b, 1, new Vector3(-w / 4, 0.75f, 0), 0.55f, 0.42f, 0.06f, 0.16f, 0.012f, 0.01f);
                MeshKit.AddCylinder(b, 1, T(-w / 4, 0.9f, -0.26f), 0.02f, 0.02f, 0.3f, 8);
                MeshKit.AddCylinder(b, 1, TR(new Vector3(-w / 4, 1.18f, -0.26f), new Vector3(90, 0, 0)), 0.018f, 0.014f, 0.18f, 8);
                // azulejos atrás
                MeshKit.AddBox(b, 3, new Vector3(0, 0.88f, -0.3f), new Vector3(w, 0.6f, 0.01f));
                return b;
            }, new[] { TexKit.Mat("madera", new Color(0.85f, 0.95f, 0.85f)), Chrome, Mats.Get(new Color(0.3f, 0.3f, 0.32f), 0.7f), TexKit.Mat("calcareo", new Color(0.85f, 0.95f, 1f)) }, pos, yaw);
        }
    }
}
