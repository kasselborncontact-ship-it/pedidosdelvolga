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
