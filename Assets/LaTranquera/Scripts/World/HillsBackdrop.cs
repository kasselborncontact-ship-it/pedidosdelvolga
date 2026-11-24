using System.Collections.Generic;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Anillo de lomas y sierras bajas alrededor del mapa (paisaje de fondo).
    /// Se genera solo, low poly con caras planas. No se puede llegar caminando.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HillsBackdrop : MonoBehaviour
    {
        public float innerRadius = 230f;
        public float outerRadius = 600f;
        public float maxHeight = 70f;
        public int segments = 96;
        public int rings = 6;
        public int seed = 3;

        void OnEnable()
        {
            var mf = GetComponent<MeshFilter>();
            if (mf.sharedMesh == null || mf.sharedMesh.name != "Sierras") mf.sharedMesh = Build();
        }

        Mesh Build()
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var cols = new List<Color>();
            float ox = seed * 13.1f, oz = seed * 7.7f;
            Vector3 P(int ring, int seg)
            {
                float a = (seg % segments) / (float)segments * Mathf.PI * 2f;
                float k = ring / (float)rings;
                float r = Mathf.Lerp(innerRadius, outerRadius, k);
                float x = Mathf.Cos(a) * r, z = Mathf.Sin(a) * r;
                float n = Mathf.PerlinNoise(x * 0.008f + ox, z * 0.008f + oz) * 0.7f + Mathf.PerlinNoise(x * 0.03f, z * 0.03f) * 0.3f;
                float profile = Mathf.Sin(k * Mathf.PI * 0.9f + 0.1f);       // sube y baja hacia el fondo
                float sierra = Mathf.Clamp01((Mathf.Sin(a * 2f + 1.3f) + 0.3f));  // más altas hacia un lado (sierras)
                float h = ring == 0 ? -1f : maxHeight * n * profile * (0.35f + sierra);
                return new Vector3(x, h, z);
            }
            for (int ring = 0; ring < rings; ring++)
                for (int s = 0; s < segments; s++)
                {
                    var a = P(ring, s); var b = P(ring, s + 1); var c = P(ring + 1, s); var d = P(ring + 1, s + 1);
                    AddTri(verts, tris, a, b, c);
                    AddTri(verts, tris, b, d, c);
                }
            var m = new Mesh { name = "Sierras" };
            m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.SetVertices(verts);
            m.SetTriangles(tris, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            m.hideFlags = HideFlags.DontSave;
            return m;
        }

        static void AddTri(List<Vector3> v, List<int> t, Vector3 a, Vector3 b, Vector3 c)
        {
            int i = v.Count;
            v.Add(a); v.Add(b); v.Add(c);
            t.Add(i); t.Add(i + 1); t.Add(i + 2);
        }
    }
}
