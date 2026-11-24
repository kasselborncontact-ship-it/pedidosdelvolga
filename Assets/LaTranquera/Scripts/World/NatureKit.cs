using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Árboles y plantas decorativas low poly con forma de verdad:
    /// eucalipto (alto, tronco claro, copa rala), álamo (columna angosta),
    /// paraíso (copa redonda tipo sombrilla), arbusto y cardo.
    /// Cada árbol es UNA malla con 2 materiales (corteza y hojas): muy liviano.
    /// </summary>
    public static class NatureKit
    {
        public enum Tree { Eucalipto, Alamo, Paraiso, Arbusto }

        static Material[] Mats(Tree t, int variant)
        {
            Color bark, leaf;
            switch (t)
            {
                case Tree.Eucalipto: bark = new Color(0.78f, 0.74f, 0.66f); leaf = variant % 2 == 0 ? new Color(0.42f, 0.52f, 0.4f) : new Color(0.36f, 0.47f, 0.37f); break;
                case Tree.Alamo: bark = new Color(0.42f, 0.4f, 0.36f); leaf = variant % 2 == 0 ? new Color(0.28f, 0.48f, 0.2f) : new Color(0.33f, 0.52f, 0.22f); break;
                case Tree.Paraiso: bark = new Color(0.33f, 0.24f, 0.17f); leaf = variant % 2 == 0 ? new Color(0.3f, 0.52f, 0.2f) : new Color(0.36f, 0.56f, 0.22f); break;
                default: bark = new Color(0.35f, 0.27f, 0.2f); leaf = new Color(0.3f, 0.46f, 0.22f); break;
            }
            return new[] { LaTranquera.Mats.Get(bark, 0.05f), LaTranquera.Mats.Get(leaf, 0.2f) };
        }

        /// <summary>Pone un árbol. variant elige una de 3 formas (se comparten entre árboles iguales).</summary>
        public static GameObject Place(Tree t, Transform parent, Vector3 pos, int variant, float yaw, float scale = 1f)
        {
            variant = Mathf.Abs(variant) % 3;
            var mesh = MeshKit.Persist("arbol_" + t + "_" + variant, () => Build(t, variant));
            var go = MeshKit.Object(parent, t.ToString(), mesh, Mats(t, variant), pos, new Vector3(0, yaw, 0));
            go.transform.localScale = Vector3.one * scale;
            // collider solo en el tronco
            var col = go.AddComponent<CapsuleCollider>();
            col.radius = t == Tree.Arbusto ? 0.6f : 0.35f;
            col.height = t == Tree.Arbusto ? 1.2f : 4f;
            col.center = Vector3.up * col.height / 2;
            return go;
        }

        static Matrix4x4 M(Vector3 pos, Vector3 euler) => Matrix4x4.TRS(pos, Quaternion.Euler(euler), Vector3.one);

        static Mesh Build(Tree t, int variant)
        {
            var b = new MeshKit.Builder(2);
            var r = new System.Random(100 + variant * 17 + (int)t * 31);
            float R() => (float)r.NextDouble();
            switch (t)
            {
                case Tree.Eucalipto:
                {
                    float h = 13f + variant * 1.5f;
                    float lean = (R() - 0.5f) * 6f;
                    MeshKit.AddCylinder(b, 0, M(Vector3.zero, new Vector3(0, 0, lean)), 0.32f, 0.12f, h * 0.85f, 7);
                    var top = Quaternion.Euler(0, 0, lean) * Vector3.up * h * 0.85f;
                    for (int i = 0; i < 4; i++)
                    {
                        float y = h * (0.45f + i * 0.1f);
                        var basePt = Quaternion.Euler(0, 0, lean) * Vector3.up * y;
                        var ang = new Vector3(35 + R() * 25, i * 95 + R() * 40, 0);
                        MeshKit.AddCylinder(b, 0, M(basePt, ang), 0.1f, 0.05f, 2.6f, 5);
                        var tip = basePt + Quaternion.Euler(ang) * Vector3.up * 2.6f;
                        MeshKit.AddBlob(b, 1, tip + Vector3.up * 0.4f, new Vector3(2.8f + R(), 2.2f + R(), 2.8f + R()), r.Next());
                    }
                    for (int i = 0; i < 4; i++)
                        MeshKit.AddBlob(b, 1, top + new Vector3((R() - 0.5f) * 2.5f, -0.5f + R() * 1.5f, (R() - 0.5f) * 2.5f), new Vector3(3f + R(), 2.6f + R(), 3f + R()), r.Next());
                    break;
                }
                case Tree.Alamo:
                {
                    float h = 14f + variant;
                    MeshKit.AddCylinder(b, 0, Matrix4x4.identity, 0.28f, 0.1f, h * 0.8f, 6);
                    int n = 7;
                    for (int i = 0; i < n; i++)
                    {
                        float k = i / (float)(n - 1);
                        float w = Mathf.Lerp(2.6f, 1.0f, k) + R() * 0.4f;
                        MeshKit.AddBlob(b, 1, new Vector3((R() - 0.5f) * 0.4f, 2.6f + k * (h - 3.5f), (R() - 0.5f) * 0.4f), new Vector3(w, 2.8f, w), r.Next(), 0.18f);
                    }
                    break;
                }
                case Tree.Paraiso:
                {
                    MeshKit.AddCylinder(b, 0, Matrix4x4.identity, 0.3f, 0.22f, 2.6f, 7);
                    for (int i = 0; i < 4; i++)
                    {
                        var ang = new Vector3(40 + R() * 15, i * 90 + R() * 30, 0);
                        var start = Vector3.up * 2.4f;
                        MeshKit.AddCylinder(b, 0, M(start, ang), 0.16f, 0.08f, 2.2f, 6);
                        var tip = start + Quaternion.Euler(ang) * Vector3.up * 2.2f;
                        MeshKit.AddBlob(b, 1, tip + Vector3.up * 0.6f, new Vector3(3.6f, 2.4f, 3.6f), r.Next());
                    }
                    MeshKit.AddBlob(b, 1, Vector3.up * 5.2f, new Vector3(4.5f, 2.8f, 4.5f), r.Next());
                    break;
                }
                default:
                {
                    for (int i = 0; i < 4; i++)
                        MeshKit.AddBlob(b, 1, new Vector3((R() - 0.5f) * 1.2f, 0.5f + R() * 0.3f, (R() - 0.5f) * 1.2f), new Vector3(1.3f, 1.1f, 1.3f), r.Next());
                    MeshKit.AddCylinder(b, 0, Matrix4x4.identity, 0.08f, 0.05f, 0.5f, 5);
                    break;
                }
            }
            return b.Build("Arbol");
        }
    }
}
