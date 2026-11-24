using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Dibuja cada cultivo en sus 4 etapas con formas simples.
    /// En la última etapa se reconoce cada uno: lechuga = roseta de hojas,
    /// tomate = planta con tutor y tomates rojos, zanahoria = follaje con la punta naranja asomando.
    /// Para usar modelos: poné prefabs en Resources/Plantas/ con nombre "tomate_3" (cultivo_etapa).
    /// </summary>
    public static class PlantVisuals
    {
        public static GameObject Build(string cropId, int stage, bool wilted, bool dead, Transform parent)
        {
            var root = new GameObject("Planta_" + cropId + "_" + stage);
            root.transform.SetParent(parent, false);
            var prefab = Resources.Load<GameObject>("Plantas/" + cropId + "_" + stage);
            if (prefab != null)
            {
                var inst = Object.Instantiate(prefab, root.transform);
                Shapes.RemoveAllColliders(inst);
                if (wilted) root.transform.localScale = new Vector3(1, 0.7f, 1);
                return root;
            }

            Color leaf = dead ? new Color(0.45f, 0.33f, 0.18f) : wilted ? new Color(0.55f, 0.55f, 0.2f) : Mats.VerdeHoja;
            Color leafLight = dead ? new Color(0.5f, 0.38f, 0.2f) : wilted ? new Color(0.62f, 0.6f, 0.28f) : new Color(0.55f, 0.8f, 0.35f);
            var t = root.transform;
            float droop = wilted || dead ? 0.6f : 1f;

            switch (cropId)
            {
                case "lechuga": Lettuce(t, stage, leaf, leafLight, droop); break;
                case "tomate": Tomato(t, stage, leaf, droop, dead); break;
                case "zanahoria": Carrot(t, stage, leaf, droop); break;
                case "cebolla": Onion(t, stage, leaf, droop); break;
                case "papa": Potato(t, stage, leaf, droop); break;
                case "maiz": Corn(t, stage, leaf, droop, dead); break;
                case "zapallo": Vine(t, stage, leaf, droop, false); break;
                case "sandia": Vine(t, stage, leaf, droop, true); break;
                case "frutilla": Strawberry(t, stage, leaf, droop); break;
                default: Shapes.Sphere(t, "Planta", Vector3.up * 0.1f, Vector3.one * 0.2f, leaf); break;
            }
            return root;
        }

        static void Sprout(Transform t, Color leaf)
        {
            Shapes.CylC(t, "Tallito", new Vector3(0, 0.03f, 0), 0.01f, 0.06f, leaf, Vector3.zero);
            Shapes.Sphere(t, "Hojita1", new Vector3(0.02f, 0.065f, 0), new Vector3(0.04f, 0.01f, 0.025f), leaf);
            Shapes.Sphere(t, "Hojita2", new Vector3(-0.02f, 0.065f, 0), new Vector3(0.04f, 0.01f, 0.025f), leaf);
        }

        static void Lettuce(Transform t, int stage, Color leaf, Color light, float droop)
        {
            if (stage == 0) { Sprout(t, leaf); return; }
            int leaves = stage == 1 ? 4 : stage == 2 ? 6 : 9;
            float r = stage == 1 ? 0.04f : stage == 2 ? 0.08f : 0.12f;
            float size = stage == 1 ? 0.07f : stage == 2 ? 0.13f : 0.2f;
            for (int i = 0; i < leaves; i++)
            {
                float a = i * 360f / leaves + (i % 2) * 15f;
                var dir = Quaternion.Euler(0, a, 0) * Vector3.forward;
                var l = Shapes.Sphere(t, "Hoja", dir * r + Vector3.up * size * 0.3f * droop,
                    new Vector3(size, size * 0.8f * droop, size * 0.35f), i % 2 == 0 ? leaf : light);
                l.transform.localRotation = Quaternion.LookRotation(dir) * Quaternion.Euler(-40 * droop, 0, 0);
            }
            if (stage == 3)
                Shapes.Sphere(t, "Cogollo", Vector3.up * 0.1f * droop, new Vector3(0.16f, 0.14f * droop, 0.16f), light);
        }

        static void Tomato(Transform t, int stage, Color leaf, float droop, bool dead)
        {
            if (stage == 0) { Sprout(t, leaf); return; }
            float h = (stage == 1 ? 0.25f : stage == 2 ? 0.7f : 1.0f) * (droop < 1 ? 0.8f : 1f);
            if (stage >= 2) Shapes.Cyl(t, "Tutor", new Vector3(0.08f, 0, 0), 0.025f, 1.25f, Mats.MaderaClara, false); // caña de tutor
            Shapes.Cyl(t, "Tallo", Vector3.zero, 0.025f, h, new Color(0.3f, 0.5f, 0.2f), false);
            int clusters = stage == 1 ? 3 : stage == 2 ? 6 : 8;
            for (int i = 0; i < clusters; i++)
            {
                float y = h * (0.25f + 0.75f * i / clusters);
                float a = i * 137f;
                var dir = Quaternion.Euler(0, a, 0) * Vector3.forward;
                float s = stage == 1 ? 0.1f : 0.2f;
                Shapes.Sphere(t, "Follaje", dir * s * 0.5f + Vector3.up * y, new Vector3(s, s * 0.6f * droop, s), leaf);
            }
            if (stage == 2)
                for (int i = 0; i < 4; i++)
                    Shapes.Sphere(t, "Flor", Quaternion.Euler(0, i * 90 + 30, 0) * Vector3.forward * 0.12f + Vector3.up * (h * 0.6f + i * 0.05f),
                        Vector3.one * 0.03f, new Color(1f, 0.85f, 0.1f));
            if (stage == 3)
            {
                Color fruit = dead ? new Color(0.45f, 0.25f, 0.15f) : new Color(0.88f, 0.12f, 0.08f);
                for (int i = 0; i < 6; i++)
                {
                    var dir = Quaternion.Euler(0, i * 60 + 20, 0) * Vector3.forward;
                    Shapes.Sphere(t, "Tomate", dir * 0.13f + Vector3.up * (0.35f + (i % 3) * 0.17f), Vector3.one * 0.075f, fruit);
                }
                Shapes.BoxC(t, "Atadura", new Vector3(0.05f, 0.6f, 0), new Vector3(0.06f, 0.01f, 0.01f), new Color(0.9f, 0.85f, 0.7f), false);
            }
        }

        static void Onion(Transform t, int stage, Color leaf, float droop)
        {
            if (stage == 0) { Sprout(t, leaf); return; }
            int n = stage == 1 ? 3 : 6;
            float len = stage == 1 ? 0.15f : stage == 2 ? 0.3f : 0.38f;
            for (int i = 0; i < n; i++)
            {
                var rot = new Vector3((8 + i * 5) / droop, i * 360f / n, 0);
                Shapes.CylC(t, "Hoja", Quaternion.Euler(rot) * Vector3.up * len * 0.5f + Vector3.up * 0.03f, 0.018f, len, leaf, rot);
            }
            if (stage >= 2)
                Shapes.Sphere(t, "Bulbo", Vector3.up * 0.03f, Vector3.one * (stage == 3 ? 0.09f : 0.05f), new Color(0.82f, 0.6f, 0.4f));
        }

        static void Potato(Transform t, int stage, Color leaf, float droop)
        {
            if (stage == 0) { Sprout(t, leaf); return; }
            float s = stage == 1 ? 0.18f : stage == 2 ? 0.4f : 0.45f;
            Color l = stage == 3 ? Color.Lerp(leaf, new Color(0.65f, 0.6f, 0.3f), 0.45f) : leaf; // al madurar la mata se pone amarilla
            for (int i = 0; i < 5; i++)
            {
                var d = Quaternion.Euler(0, i * 72, 0) * Vector3.forward * s * 0.25f;
                Shapes.Sphere(t, "Mata", d + Vector3.up * s * 0.4f * droop, new Vector3(s * 0.6f, s * 0.5f * droop, s * 0.6f), l);
            }
            if (stage == 2)
                for (int i = 0; i < 4; i++)
                    Shapes.Sphere(t, "Flor", Quaternion.Euler(0, i * 90 + 20, 0) * Vector3.forward * 0.12f + Vector3.up * s * 0.7f, Vector3.one * 0.035f, new Color(0.95f, 0.95f, 1f));
            if (stage == 3)
                for (int i = 0; i < 3; i++)
                    Shapes.Sphere(t, "Papa", Quaternion.Euler(0, i * 120, 0) * Vector3.forward * 0.18f + Vector3.up * 0.02f, new Vector3(0.08f, 0.05f, 0.06f), new Color(0.72f, 0.58f, 0.38f));
        }

        static void Corn(Transform t, int stage, Color leaf, float droop, bool dead)
        {
            if (stage == 0) { Sprout(t, leaf); return; }
            float h = (stage == 1 ? 0.4f : stage == 2 ? 1.4f : 2.0f) * (droop < 1 ? 0.85f : 1f);
            Shapes.Cyl(t, "Caña", Vector3.zero, 0.04f, h, Color.Lerp(leaf, new Color(0.6f, 0.65f, 0.3f), 0.3f), false);
            int leaves = stage == 1 ? 3 : 8;
            for (int i = 0; i < leaves; i++)
            {
                float y = h * (0.15f + 0.75f * i / leaves);
                var rot = new Vector3(55 / droop + (i % 2) * 10, i * 137, 0);
                var dir = Quaternion.Euler(rot) * Vector3.up;
                Shapes.Prim(PrimitiveType.Cube, t, "Hoja", Vector3.up * y + dir * 0.25f, new Vector3(0.07f, 0.5f, 0.006f), leaf, false, rot);
            }
            if (stage >= 2) Shapes.Cone(t, "Panoja", Vector3.up * h, 0.08f, 0.25f, new Color(0.85f, 0.75f, 0.45f));
            if (stage == 3)
                for (int i = 0; i < 2; i++)
                {
                    var rot = new Vector3(25, i * 180 + 40, 0);
                    Shapes.CylC(t, "Choclo", Vector3.up * (h * 0.5f + i * 0.15f) + Quaternion.Euler(rot) * Vector3.up * 0.12f + Quaternion.Euler(0, i * 180 + 40, 0) * Vector3.forward * 0.05f,
                        0.07f, 0.22f, dead ? new Color(0.5f, 0.42f, 0.25f) : new Color(0.65f, 0.78f, 0.4f), rot);
                }
        }

        /// <summary>Zapallo y sandía: plantas rastreras con hojas grandes y el fruto en el piso.</summary>
        static void Vine(Transform t, int stage, Color leaf, float droop, bool watermelon)
        {
            if (stage == 0) { Sprout(t, leaf); return; }
            float r = stage == 1 ? 0.2f : stage == 2 ? 0.55f : 0.7f;
            int n = stage == 1 ? 4 : 9;
            for (int i = 0; i < n; i++)
            {
                float a = i * 137f;
                float d = r * (0.3f + 0.7f * (i % 3) / 2f);
                var p = Quaternion.Euler(0, a, 0) * Vector3.forward * d;
                Shapes.Sphere(t, "Hoja", p + Vector3.up * 0.06f * droop, new Vector3(0.22f, 0.05f, 0.22f) * (stage == 1 ? 0.6f : 1f), leaf);
            }
            if (stage >= 2)
                for (int i = 0; i < 3; i++)
                    Shapes.Sphere(t, "Flor", Quaternion.Euler(0, i * 120 + 30, 0) * Vector3.forward * r * 0.6f + Vector3.up * 0.1f, Vector3.one * 0.05f, new Color(1f, 0.8f, 0.15f));
            if (stage == 3)
            {
                var fruitRoot = Shapes.Empty(t, "Fruto", new Vector3(0.25f, 0, 0.15f), new Vector3(0, 30, 0)).transform;
                if (watermelon)
                {
                    var s = Shapes.Prim(PrimitiveType.Sphere, fruitRoot, "Sandia", Vector3.up * 0.13f, new Vector3(0.26f, 0.4f, 0.26f), TexKit.Mat("sandia", Color.white, 0.4f), false);
                    s.transform.localRotation = Quaternion.Euler(0, 0, 90);
                }
                else
                {
                    var tan = new Color(0.88f, 0.68f, 0.38f);
                    Shapes.Sphere(fruitRoot, "Panza", new Vector3(-0.07f, 0.08f, 0), new Vector3(0.18f, 0.16f, 0.17f), tan);
                    Shapes.CylC(fruitRoot, "Cuello", new Vector3(0.07f, 0.07f, 0), 0.1f, 0.18f, tan, new Vector3(0, 0, 90));
                }
            }
        }

        static void Strawberry(Transform t, int stage, Color leaf, float droop)
        {
            if (stage == 0) { Sprout(t, leaf); return; }
            int n = stage == 1 ? 4 : 8;
            for (int i = 0; i < n; i++)
            {
                var d = Quaternion.Euler(0, i * 360f / n, 0) * Vector3.forward * 0.08f;
                // hoja de tres foliolos
                for (int k = -1; k <= 1; k++)
                    Shapes.Sphere(t, "Foliolo", d + Quaternion.Euler(0, i * 360f / n + k * 35, 0) * Vector3.forward * 0.05f + Vector3.up * 0.1f * droop,
                        new Vector3(0.05f, 0.015f, 0.06f), leaf);
            }
            if (stage == 2)
                for (int i = 0; i < 3; i++)
                    Shapes.Sphere(t, "Flor", Quaternion.Euler(0, i * 120, 0) * Vector3.forward * 0.13f + Vector3.up * 0.08f, Vector3.one * 0.03f, Color.white);
            if (stage == 3)
                for (int i = 0; i < 5; i++)
                    Shapes.Sphere(t, "Frutilla", Quaternion.Euler(0, i * 72 + 15, 0) * Vector3.forward * 0.15f + Vector3.up * 0.04f, new Vector3(0.035f, 0.045f, 0.035f), new Color(0.88f, 0.08f, 0.1f));
        }

        static void Carrot(Transform t, int stage, Color leaf, float droop)
        {
            if (stage == 0) { Sprout(t, leaf); return; }
            int stems = stage == 1 ? 4 : stage == 2 ? 8 : 12;
            float len = stage == 1 ? 0.12f : stage == 2 ? 0.25f : 0.35f;
            for (int i = 0; i < stems; i++)
            {
                float a = i * 360f / stems;
                float tilt = 15 + (i % 3) * 10;
                var rot = new Vector3(tilt / droop, a, 0);
                var q = Quaternion.Euler(rot);
                var tip = q * Vector3.up * len;
                Shapes.CylC(t, "Tallo", tip * 0.5f, 0.012f, len, leaf, rot);
                // follaje plumoso
                Shapes.Sphere(t, "Pluma", tip * 0.85f, new Vector3(0.05f, 0.03f, 0.05f) * (stage == 3 ? 1.4f : 1f), leaf);
            }
            if (stage == 3)
                Shapes.Cone(t, "Hombro", new Vector3(0, 0.03f, 0), 0.05f, 0.04f, new Color(0.95f, 0.5f, 0.1f), new Vector3(180, 0, 0));
            if (stage == 3)
                Shapes.Cyl(t, "Cuello", Vector3.zero, 0.05f, 0.03f, new Color(0.95f, 0.5f, 0.1f), false);
        }
    }
}
