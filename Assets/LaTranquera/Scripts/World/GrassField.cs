using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LaTranquera
{
    /// <summary>
    /// Pasto con volumen: miles de matas de hojitas dibujadas con "GPU instancing"
    /// (muy rápido). No pone pasto sobre la ruta, los caminos ni adentro de los edificios
    /// (zonas "exclusions"). Solo dibuja lo que está cerca de la cámara.
    /// </summary>
    public class GrassField : MonoBehaviour
    {
        public Vector2 areaMin = new Vector2(-90, -40);
        public Vector2 areaMax = new Vector2(90, 90);
        [Tooltip("Matas por metro cuadrado")]
        public float density = 2.5f;
        public float drawDistance = 60f;
        public float chunkSize = 10f;
        public int seed = 12345;
        /// <summary>Rectángulos sin pasto (x, z, ancho, largo) en coordenadas del mundo.</summary>
        public List<Rect> exclusions = new List<Rect>();
        /// <summary>Materiales (varios verdes y uno seco). Tienen que tener "Enable GPU Instancing".</summary>
        public Material[] materials;

        class Chunk
        {
            public Vector3 center;
            public List<Matrix4x4>[] perMat;
            public Matrix4x4[][] arrays;
        }

        readonly List<Chunk> chunks = new List<Chunk>();
        Mesh tuft;

        void Start()
        {
            if (materials == null || materials.Length == 0) { enabled = false; return; }
            tuft = BuildTuft();
            Generate();
        }

        static Mesh BuildTuft()
        {
            var v = new List<Vector3>();
            var n = new List<Vector3>();
            var tri = new List<int>();
            var r = new System.Random(7);
            int blades = 7;
            for (int i = 0; i < blades; i++)
            {
                float a = (float)(r.NextDouble() * Mathf.PI * 2);
                float h = 0.22f + (float)r.NextDouble() * 0.25f;
                float w = 0.025f + (float)r.NextDouble() * 0.015f;
                var dir = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                var side = new Vector3(-dir.z, 0, dir.x);
                var basePos = dir * (float)r.NextDouble() * 0.08f;
                var lean = dir * (0.05f + (float)r.NextDouble() * 0.12f);
                int b = v.Count;
                v.Add(basePos - side * w); v.Add(basePos + side * w); v.Add(basePos + lean + Vector3.up * h);
                for (int k = 0; k < 3; k++) n.Add(Vector3.up); // normales hacia arriba: se ilumina como un colchón de pasto
                tri.Add(b); tri.Add(b + 2); tri.Add(b + 1);
            }
            var m = new Mesh { name = "MataDePasto" };
            m.SetVertices(v); m.SetNormals(n); m.SetTriangles(tri, 0);
            m.RecalculateBounds();
            return m;
        }

        bool Excluded(float x, float z)
        {
            foreach (var e in exclusions) if (e.Contains(new Vector2(x, z))) return true;
            return false;
        }

        void Generate()
        {
            var r = new System.Random(seed);
            int nx = Mathf.CeilToInt((areaMax.x - areaMin.x) / chunkSize);
            int nz = Mathf.CeilToInt((areaMax.y - areaMin.y) / chunkSize);
            int perChunk = Mathf.RoundToInt(chunkSize * chunkSize * density);
            for (int cx = 0; cx < nx; cx++)
                for (int cz = 0; cz < nz; cz++)
                {
                    var c = new Chunk { perMat = new List<Matrix4x4>[materials.Length] };
                    for (int m = 0; m < materials.Length; m++) c.perMat[m] = new List<Matrix4x4>();
                    float x0 = areaMin.x + cx * chunkSize, z0 = areaMin.y + cz * chunkSize;
                    c.center = new Vector3(x0 + chunkSize / 2, 0, z0 + chunkSize / 2);
                    for (int i = 0; i < perChunk; i++)
                    {
                        float x = x0 + (float)r.NextDouble() * chunkSize;
                        float z = z0 + (float)r.NextDouble() * chunkSize;
                        if (Excluded(x, z)) continue;
                        float s = 0.7f + (float)r.NextDouble() * 0.8f;
                        var mtx = Matrix4x4.TRS(new Vector3(x, 0, z), Quaternion.Euler(0, (float)r.NextDouble() * 360f, 0), new Vector3(s, s * (0.8f + (float)r.NextDouble() * 0.5f), s));
                        // el último material (pasto seco) sale menos
                        int mi = r.NextDouble() < 0.12 ? materials.Length - 1 : r.Next(Mathf.Max(1, materials.Length - 1));
                        c.perMat[mi].Add(mtx);
                    }
                    c.arrays = new Matrix4x4[materials.Length][];
                    for (int m = 0; m < materials.Length; m++) c.arrays[m] = c.perMat[m].ToArray();
                    chunks.Add(c);
                }
        }

        void Update()
        {
            var cam = Camera.main;
            if (cam == null || tuft == null) return;
            Vector3 cp = cam.transform.position;
            float maxD = (drawDistance + chunkSize) * (drawDistance + chunkSize);
            for (int i = 0; i < chunks.Count; i++)
            {
                var c = chunks[i];
                Vector3 d = c.center - cp; d.y = 0;
                if (d.sqrMagnitude > maxD) continue;
                for (int m = 0; m < materials.Length; m++)
                {
                    var arr = c.arrays[m];
                    if (arr.Length == 0) continue;
                    var rp = new RenderParams(materials[m])
                    {
                        shadowCastingMode = ShadowCastingMode.Off,
                        receiveShadows = true,
                        worldBounds = new Bounds(c.center + Vector3.up, new Vector3(chunkSize + 1, 3, chunkSize + 1))
                    };
                    for (int start = 0; start < arr.Length; start += 1000)
                        Graphics.RenderMeshInstanced(rp, tuft, 0, arr, Mathf.Min(1000, arr.Length - start), start);
                }
            }
        }
    }
}
