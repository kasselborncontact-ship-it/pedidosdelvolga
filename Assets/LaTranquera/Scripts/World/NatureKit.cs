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
            var leafMat = LaTranquera.Mats.Custom("Hojas_" + ColorUtility.ToHtmlStringRGB(leaf), "LaTranquera/Hojas", leaf, m =>
            {
                m.SetFloat("_Wind", t == Tree.Arbusto ? 0.05f : 0.14f);
                m.SetFloat("_Variation", 0.2f);
            });
            return new[] { TexKit.Mat("madera", bark * 1.6f), leafMat };
        }

        /// <summary>Pone un árbol. variant elige una de 3 formas (se comparten entre árboles iguales).</summary>
        public static GameObject Place(Tree t, Transform parent, Vector3 pos, int variant, float yaw, float scale = 1f)
        {
            variant = Mathf.Abs(variant) % 3;
            var mesh = MeshKit.Persist("arbol2_" + t + "_" + variant, () => Build(t, variant));
            var go = MeshKit.Object(parent, t.ToString(), mesh, Mats(t, variant), pos, new Vector3(0, yaw, 0));
            go.transform.localScale = Vector3.one * scale;
            // collider solo en el tronco
            var col = go.AddComponent<CapsuleCollider>();
            col.radius = t == Tree.Arbusto ? 0.6f : 0.35f;
            col.height = t == Tree.Arbusto ? 1.2f : 4f;
            col.center = Vector3.up * col.height / 2;
            return go;
        }

        /// <summary>Tronco torneado con raíces ensanchadas abajo.</summary>
        static void Trunk(MeshKit.Builder b, Matrix4x4 m, float r, float h)
        {
            MeshKit.AddLathe(b, 0, m, MeshKit.Prof(r * 1.7f, -0.05f, r * 1.2f, 0.25f, r, 0.7f, r * 0.85f, h * 0.5f, r * 0.45f, h, 0, h + 0.05f), 9);
        }

        /// <summary>Racimo de hojas: varias "nubes" juntas, con la de adentro más oscura (da profundidad).</summary>
        static void Cluster(MeshKit.Builder b, System.Random r, Vector3 c, Vector3 size, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var off = new Vector3(((float)r.NextDouble() - 0.5f) * size.x * 0.6f, ((float)r.NextDouble() - 0.4f) * size.y * 0.5f, ((float)r.NextDouble() - 0.5f) * size.z * 0.6f);
                float k = 0.55f + (float)r.NextDouble() * 0.35f;
                MeshKit.AddBlob(b, 1, c + off, Vector3.Scale(size, new Vector3(k, k * 0.85f, k)), r.Next(), 0.28f);
            }
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
                    float h = 14f + variant * 1.5f;
                    float lean = (R() - 0.5f) * 6f;
                    var lm = M(Vector3.zero, new Vector3(0, 0, lean));
                    Trunk(b, lm, 0.36f, h * 0.85f);
                    var dir = Quaternion.Euler(0, 0, lean) * Vector3.up;
                    for (int i = 0; i < 6; i++)
                    {
                        float y = h * (0.42f + i * 0.08f);
                        var basePt = dir * y;
                        var ang = new Vector3(40 + R() * 25, i * 65 + R() * 40, 0);
                        float len = 2.2f + R() * 1.4f;
                        MeshKit.AddCylinder(b, 0, M(basePt, ang), 0.11f, 0.04f, len, 5);
                        var tip = basePt + Quaternion.Euler(ang) * Vector3.up * len;
                        Cluster(b, r, tip + Vector3.down * 0.3f, new Vector3(2.4f, 2.6f, 2.4f), 4); // el eucalipto tiene la copa caída y rala
                    }
                    Cluster(b, r, dir * h * 0.95f, new Vector3(3f, 2.8f, 3f), 5);
                    break;
                }
                case Tree.Alamo:
                {
                    float h = 14f + variant;
                    Trunk(b, Matrix4x4.identity, 0.3f, h * 0.8f);
                    int n = 10;
                    for (int i = 0; i < n; i++)
                    {
                        float k = i / (float)(n - 1);
                        float w = Mathf.Lerp(2.4f, 0.9f, k * k) + R() * 0.3f;
                        Cluster(b, r, new Vector3((R() - 0.5f) * 0.3f, 2.4f + k * (h - 3f), (R() - 0.5f) * 0.3f), new Vector3(w, 2.2f, w), 2);
                    }
                    break;
                }
                case Tree.Paraiso:
                {
                    Trunk(b, Matrix4x4.identity, 0.32f, 2.8f);
                    for (int i = 0; i < 5; i++)
                    {
                        var ang = new Vector3(42 + R() * 15, i * 72 + R() * 25, 0);
                        var start = Vector3.up * 2.4f;
                        float len = 2.0f + R() * 0.6f;
                        MeshKit.AddCylinder(b, 0, M(start, ang), 0.17f, 0.07f, len, 6);
                        var tip = start + Quaternion.Euler(ang) * Vector3.up * len;
                        Cluster(b, r, tip + Vector3.up * 0.5f, new Vector3(3.2f, 2.0f, 3.2f), 4);
                    }
                    Cluster(b, r, Vector3.up * 5.0f, new Vector3(4f, 2.4f, 4f), 5);
                    break;
                }
                default:
                {
                    Cluster(b, r, new Vector3(0, 0.55f, 0), new Vector3(1.7f, 1.2f, 1.7f), 6);
                    MeshKit.AddCylinder(b, 0, Matrix4x4.identity, 0.08f, 0.05f, 0.5f, 5);
                    break;
                }
            }
            return b.Build("Arbol");
        }
    }
}
