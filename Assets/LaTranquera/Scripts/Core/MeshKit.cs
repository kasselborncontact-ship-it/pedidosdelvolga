using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LaTranquera
{
    /// <summary>
    /// Mallas hechas por código, más lindas que los cubos de Unity:
    /// cajas con UV en metros (para que las texturas de ladrillo/madera no se estiren),
    /// chapa acanalada, siluetas extruidas (autos), "nubes" de hojas facetadas (árboles), etc.
    /// Cuando se arma la escena desde el editor, las mallas se guardan en Generated/Meshes.
    /// </summary>
    public static class MeshKit
    {
        public const string Folder = "Assets/LaTranquera/Generated/Meshes";
        static readonly Dictionary<string, Mesh> cache = new Dictionary<string, Mesh>();

        /// <summary>Guarda la malla como archivo si estamos en el editor (sin Play). Si ya existe, usa esa.</summary>
        public static Mesh Persist(string key, System.Func<Mesh> build)
        {
            if (cache.TryGetValue(key, out var m) && m != null)
            {
#if UNITY_EDITOR
                if (Application.isPlaying || AssetDatabase.Contains(m)) return m;
#else
                return m;
#endif
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Mats.EnsureFolder(Folder);
                string path = Folder + "/" + key + ".asset";
                m = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (m == null)
                {
                    m = build();
                    m.name = key;
                    AssetDatabase.CreateAsset(m, path);
                }
                cache[key] = m;
                return m;
            }
#endif
            m = build();
            m.name = key;
            cache[key] = m;
            return m;
        }

        // ------------------------------------------------------------------
        // Constructor de mallas con varias "sub-mallas" (una por material)
        public class Builder
        {
            public readonly List<Vector3> v = new List<Vector3>();
            public readonly List<Vector3> n = new List<Vector3>();
            public readonly List<Vector2> uv = new List<Vector2>();
            public readonly List<List<int>> subs = new List<List<int>>();

            public readonly List<Vector2Int> smoothRanges = new List<Vector2Int>(); // vértices de piezas de torno (normales suaves)

            public Builder(int submeshes = 1) { for (int i = 0; i < submeshes; i++) subs.Add(new List<int>()); }

            /// <summary>Triángulo con cara plana (low poly).</summary>
            public void Tri(int sub, Vector3 a, Vector3 b, Vector3 c, Vector2 ua, Vector2 ub, Vector2 uc)
            {
                var nrm = Vector3.Cross(b - a, c - a).normalized;
                int i = v.Count;
                v.Add(a); v.Add(b); v.Add(c);
                n.Add(nrm); n.Add(nrm); n.Add(nrm);
                uv.Add(ua); uv.Add(ub); uv.Add(uc);
                subs[sub].Add(i); subs[sub].Add(i + 1); subs[sub].Add(i + 2);
            }

            public void Tri(int sub, Vector3 a, Vector3 b, Vector3 c) => Tri(sub, a, b, c, Vector2.zero, Vector2.right, Vector2.up);

            /// <summary>Cuadrilátero a-b-c-d (en sentido horario visto de frente).</summary>
            public void Quad(int sub, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector2 ua, Vector2 ub, Vector2 uc, Vector2 ud)
            {
                Tri(sub, a, b, c, ua, ub, uc);
                Tri(sub, a, c, d, ua, uc, ud);
            }

            /// <summary>Promedia normales en las piezas de torno para que se vean redondas (respeta los bordes filosos).</summary>
            void SmoothLathe()
            {
                Vector3Int K(Vector3 p) => new Vector3Int(Mathf.RoundToInt(p.x * 2000), Mathf.RoundToInt(p.y * 2000), Mathf.RoundToInt(p.z * 2000));
                foreach (var r in smoothRanges)
                {
                    var groups = new Dictionary<Vector3Int, List<int>>();
                    for (int i = r.x; i < r.y; i++)
                    {
                        var k = K(v[i]);
                        if (!groups.TryGetValue(k, out var l)) groups[k] = l = new List<int>();
                        l.Add(i);
                    }
                    var res = new Vector3[r.y - r.x];
                    foreach (var g in groups.Values)
                        foreach (int i in g)
                        {
                            Vector3 acc = Vector3.zero;
                            foreach (int j in g) if (Vector3.Dot(n[i], n[j]) > 0.75f) acc += n[j];
                            res[i - r.x] = acc.normalized;
                        }
                    for (int i = r.x; i < r.y; i++) n[i] = res[i - r.x];
                }
                smoothRanges.Clear();
            }

            /// <summary>Copia otra malla aplicándole una transformación.</summary>
            public void Add(Mesh m, Matrix4x4 mtx, int sub)
            {
                var mv = m.vertices; var mn = m.normals; var mu = m.uv;
                int baseIdx = v.Count;
                for (int i = 0; i < mv.Length; i++)
                {
                    v.Add(mtx.MultiplyPoint3x4(mv[i]));
                    n.Add(mtx.MultiplyVector(mn.Length > i ? mn[i] : Vector3.up).normalized);
                    uv.Add(mu.Length > i ? mu[i] : Vector2.zero);
                }
                for (int s = 0; s < m.subMeshCount; s++)
                    foreach (var t in m.GetTriangles(s)) subs[sub].Add(baseIdx + t);
            }

            public Mesh Build(string name = "Malla")
            {
                SmoothLathe();
                var m = new Mesh { name = name };
                if (v.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                m.SetVertices(v); m.SetNormals(n); m.SetUVs(0, uv);
                m.subMeshCount = subs.Count;
                for (int i = 0; i < subs.Count; i++) m.SetTriangles(subs[i], i);
                m.RecalculateBounds();
                return m;
            }
        }

        // ------------------------------------------------------------------
        /// <summary>Caja con la base en y=0 y UV en metros (la textura se repite cada 1 m).</summary>
        public static Mesh Box(Vector3 size)
        {
            string key = "caja_" + Mathf.RoundToInt(size.x * 100) + "x" + Mathf.RoundToInt(size.y * 100) + "x" + Mathf.RoundToInt(size.z * 100);
            return Persist(key, () =>
            {
                var b = new Builder();
                AddBox(b, 0, new Vector3(0, size.y / 2, 0), size);
                return b.Build();
            });
        }

        public static void AddBox(Builder b, int sub, Vector3 center, Vector3 size)
        {
            Vector3 h = size / 2;
            Vector3 c = center;
            Vector3 P(float x, float y, float z) => c + new Vector3(x * h.x, y * h.y, z * h.z);
            float sx = size.x, sy = size.y, sz = size.z;
            // frente (+z)
            b.Quad(sub, P(1, -1, 1), P(1, 1, 1), P(-1, 1, 1), P(-1, -1, 1), new Vector2(0, 0), new Vector2(0, sy), new Vector2(sx, sy), new Vector2(sx, 0));
            // atrás (-z)
            b.Quad(sub, P(-1, -1, -1), P(-1, 1, -1), P(1, 1, -1), P(1, -1, -1), new Vector2(0, 0), new Vector2(0, sy), new Vector2(sx, sy), new Vector2(sx, 0));
            // derecha (+x)
            b.Quad(sub, P(1, -1, -1), P(1, 1, -1), P(1, 1, 1), P(1, -1, 1), new Vector2(0, 0), new Vector2(0, sy), new Vector2(sz, sy), new Vector2(sz, 0));
            // izquierda (-x)
            b.Quad(sub, P(-1, -1, 1), P(-1, 1, 1), P(-1, 1, -1), P(-1, -1, -1), new Vector2(0, 0), new Vector2(0, sy), new Vector2(sz, sy), new Vector2(sz, 0));
            // arriba
            b.Quad(sub, P(-1, 1, -1), P(-1, 1, 1), P(1, 1, 1), P(1, 1, -1), new Vector2(0, 0), new Vector2(0, sz), new Vector2(sx, sz), new Vector2(sx, 0));
            // abajo
            b.Quad(sub, P(-1, -1, 1), P(-1, -1, -1), P(1, -1, -1), P(1, -1, 1), new Vector2(0, 0), new Vector2(0, sz), new Vector2(sx, sz), new Vector2(sx, 0));
        }

        /// <summary>Chapa acanalada (ancho en X, largo en Z, apoyada en y=0). Las canaletas corren a lo largo de Z.</summary>
        public static Mesh Corrugated(float width, float length, float wave = 0.15f, float depth = 0.035f)
        {
            string key = "chapa_" + Mathf.RoundToInt(width * 100) + "x" + Mathf.RoundToInt(length * 100);
            return Persist(key, () =>
            {
                var b = new Builder();
                int steps = Mathf.Max(4, Mathf.RoundToInt(width / wave * 4));
                float hz = length / 2;
                for (int i = 0; i < steps; i++)
                {
                    float x0 = -width / 2 + width * i / steps, x1 = -width / 2 + width * (i + 1) / steps;
                    float y0 = Mathf.Sin(x0 / wave * Mathf.PI * 2) * depth, y1 = Mathf.Sin(x1 / wave * Mathf.PI * 2) * depth;
                    var a = new Vector3(x0, y0, -hz); var bb = new Vector3(x0, y0, hz); var c = new Vector3(x1, y1, hz); var d = new Vector3(x1, y1, -hz);
                    b.Quad(0, a, bb, c, d, new Vector2(x0, 0), new Vector2(x0, length), new Vector2(x1, length), new Vector2(x1, 0));
                    b.Quad(0, d, c, bb, a, new Vector2(x1, 0), new Vector2(x1, length), new Vector2(x0, length), new Vector2(x0, 0)); // cara de abajo
                }
                return b.Build();
            });
        }

        /// <summary>Chapa curva (techo de galpón tipo parabólico): arco sobre X, largo en Z.</summary>
        public static Mesh CurvedRoof(float span, float rise, float length, int segments = 24)
        {
            string key = "techoCurvo_" + Mathf.RoundToInt(span * 10) + "_" + Mathf.RoundToInt(rise * 10) + "_" + Mathf.RoundToInt(length * 10);
            return Persist(key, () =>
            {
                var b = new Builder();
                float hz = length / 2;
                Vector3 Arc(float t) { float x = Mathf.Lerp(-span / 2, span / 2, t); float y = rise * (1 - Mathf.Pow(2 * t - 1, 2)); return new Vector3(x, y, 0); }
                float u = 0;
                for (int i = 0; i < segments; i++)
                {
                    var p0 = Arc(i / (float)segments); var p1 = Arc((i + 1) / (float)segments);
                    float du = Vector3.Distance(p0, p1);
                    var a = p0 + Vector3.back * hz; var bb = p0 + Vector3.forward * hz; var c = p1 + Vector3.forward * hz; var d = p1 + Vector3.back * hz;
                    b.Quad(0, a, bb, c, d, new Vector2(u, 0), new Vector2(u, length), new Vector2(u + du, length), new Vector2(u + du, 0));
                    b.Quad(0, d, c, bb, a, new Vector2(u + du, 0), new Vector2(u + du, length), new Vector2(u, length), new Vector2(u, 0));
                    u += du;
                }
                return b.Build();
            });
        }

        /// <summary>Cilindro (o cono truncado) con caras planas. Base en y=0.</summary>
        public static void AddCylinder(Builder b, int sub, Matrix4x4 mtx, float rBottom, float rTop, float height, int sides, bool caps = true)
        {
            for (int i = 0; i < sides; i++)
            {
                float a0 = i * Mathf.PI * 2 / sides, a1 = (i + 1) * Mathf.PI * 2 / sides;
                Vector3 d0 = new Vector3(Mathf.Cos(a0), 0, Mathf.Sin(a0)), d1 = new Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1));
                var p0 = mtx.MultiplyPoint3x4(d0 * rBottom); var p1 = mtx.MultiplyPoint3x4(d1 * rBottom);
                var p2 = mtx.MultiplyPoint3x4(d1 * rTop + Vector3.up * height); var p3 = mtx.MultiplyPoint3x4(d0 * rTop + Vector3.up * height);
                float u0 = i / (float)sides * rBottom * 6.28f, u1 = (i + 1) / (float)sides * rBottom * 6.28f;
                b.Quad(sub, p0, p3, p2, p1, new Vector2(u0, 0), new Vector2(u0, height), new Vector2(u1, height), new Vector2(u1, 0));
                if (caps)
                {
                    b.Tri(sub, mtx.MultiplyPoint3x4(Vector3.up * height), p2, p3);
                    b.Tri(sub, mtx.MultiplyPoint3x4(Vector3.zero), p0, p1);
                }
            }
        }

        static List<Vector3> icoV;
        static List<int> icoT;

        static void Icosphere()
        {
            if (icoV != null) return;
            float t = (1f + Mathf.Sqrt(5f)) / 2f;
            icoV = new List<Vector3>
            {
                new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
                new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
                new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1)
            };
            for (int i = 0; i < icoV.Count; i++) icoV[i] = icoV[i].normalized;
            icoT = new List<int> { 0,11,5, 0,5,1, 0,1,7, 0,7,10, 0,10,11, 1,5,9, 5,11,4, 11,10,2, 10,7,6, 7,1,8,
                                   3,9,4, 3,4,2, 3,2,6, 3,6,8, 3,8,9, 4,9,5, 2,4,11, 6,2,10, 8,6,7, 9,8,1 };
            // una subdivisión
            var mid = new Dictionary<long, int>();
            int Mid(int a, int b)
            {
                long k = a < b ? ((long)a << 32) + b : ((long)b << 32) + a;
                if (mid.TryGetValue(k, out int r)) return r;
                icoV.Add(((icoV[a] + icoV[b]) / 2).normalized);
                mid[k] = icoV.Count - 1;
                return icoV.Count - 1;
            }
            var nt = new List<int>();
            for (int i = 0; i < icoT.Count; i += 3)
            {
                int a = icoT[i], b = icoT[i + 1], c = icoT[i + 2];
                int ab = Mid(a, b), bc = Mid(b, c), ca = Mid(c, a);
                nt.AddRange(new[] { a, ab, ca, b, bc, ab, c, ca, bc, ab, bc, ca });
            }
            icoT = nt;
        }

        /// <summary>"Nube" de hojas facetada: esfera deformada con ruido. Ideal para copas de árboles y arbustos.</summary>
        public static void AddBlob(Builder b, int sub, Vector3 center, Vector3 size, int seed, float noise = 0.22f)
        {
            Icosphere();
            var r = new System.Random(seed);
            var disp = new float[icoV.Count];
            for (int i = 0; i < disp.Length; i++) disp[i] = 1f + ((float)r.NextDouble() * 2 - 1) * noise;
            for (int i = 0; i < icoT.Count; i += 3)
            {
                Vector3 P(int k) => center + Vector3.Scale(icoV[k] * disp[k], size * 0.5f);
                // las caras de abajo un poco más aplanadas (copas con "panza" plana)
                b.Tri(sub, P(icoT[i]), P(icoT[i + 1]), P(icoT[i + 2]));
            }
        }

        /// <summary>
        /// Extruye una silueta 2D (puntos en (z, y), sentido antihorario) a lo ancho en X.
        /// Sirve para hacer la forma de un auto vista de costado.
        /// </summary>
        public static void AddExtrusion(Builder b, int sub, List<Vector2> profile, float width, float xOffset = 0f)
        {
            float x0 = xOffset - width / 2, x1 = xOffset + width / 2;
            var tris = Triangulate(profile);
            // tapas laterales
            for (int i = 0; i < tris.Count; i += 3)
            {
                Vector2 a = profile[tris[i]], c = profile[tris[i + 1]], d = profile[tris[i + 2]];
                b.Tri(sub, new Vector3(x1, a.y, a.x), new Vector3(x1, d.y, d.x), new Vector3(x1, c.y, c.x));
                b.Tri(sub, new Vector3(x0, a.y, a.x), new Vector3(x0, c.y, c.x), new Vector3(x0, d.y, d.x));
            }
            // contorno
            for (int i = 0; i < profile.Count; i++)
            {
                Vector2 p = profile[i], q = profile[(i + 1) % profile.Count];
                b.Quad(sub, new Vector3(x0, p.y, p.x), new Vector3(x1, p.y, p.x), new Vector3(x1, q.y, q.x), new Vector3(x0, q.y, q.x),
                    Vector2.zero, Vector2.right, Vector2.one, Vector2.up);
            }
        }

        /// <summary>
        /// Torno: gira un perfil (radio, altura) alrededor del eje Y. Sirve para patas torneadas,
        /// inodoros, pavas, mates, botellas, lámparas... scaleZ estira para hacer óvalos.
        /// El perfil va de abajo hacia arriba por AFUERA; si vuelve hacia adentro (bordes), queda hueco.
        /// </summary>
        public static void AddLathe(Builder b, int sub, Matrix4x4 mtx, List<Vector2> profile, int sides, float scaleZ = 1f, bool smooth = true)
        {
            int start = b.v.Count;
            for (int i = 0; i < profile.Count - 1; i++)
            {
                Vector2 p0 = profile[i], p1 = profile[i + 1];
                for (int k = 0; k < sides; k++)
                {
                    float a0 = k * Mathf.PI * 2 / sides, a1 = (k + 1) * Mathf.PI * 2 / sides;
                    Vector3 R(Vector2 p, float a) => mtx.MultiplyPoint3x4(new Vector3(Mathf.Cos(a) * p.x, p.y, Mathf.Sin(a) * p.x * scaleZ));
                    var v00 = R(p0, a0); var v01 = R(p0, a1); var v10 = R(p1, a0); var v11 = R(p1, a1);
                    float u0 = k / (float)sides, u1 = (k + 1) / (float)sides;
                    if (p0.x < 0.0001f) b.Tri(sub, v00, v10, v11, new Vector2(u0, p0.y), new Vector2(u0, p1.y), new Vector2(u1, p1.y));
                    else if (p1.x < 0.0001f) b.Tri(sub, v00, v10, v01, new Vector2(u0, p0.y), new Vector2(u0, p1.y), new Vector2(u1, p0.y));
                    else b.Quad(sub, v00, v10, v11, v01, new Vector2(u0, p0.y), new Vector2(u0, p1.y), new Vector2(u1, p1.y), new Vector2(u1, p0.y));
                }
            }
            if (smooth) b.smoothRanges.Add(new Vector2Int(start, b.v.Count));
        }

        /// <summary>Perfil para AddLathe a partir de pares (radio, altura).</summary>
        public static List<Vector2> Prof(params float[] ry)
        {
            var l = new List<Vector2>();
            for (int i = 0; i < ry.Length; i += 2) l.Add(new Vector2(ry[i], ry[i + 1]));
            return l;
        }

        /// <summary>Puntos de un rectángulo con esquinas redondeadas (en XZ), sentido horario visto de arriba.</summary>
        public static List<Vector2> RoundedRect(float w, float l, float r, int perCorner = 5)
        {
            r = Mathf.Min(r, Mathf.Min(w, l) / 2 - 0.001f);
            var pts = new List<Vector2>();
            Vector2[] centers = { new Vector2(w / 2 - r, l / 2 - r), new Vector2(w / 2 - r, -l / 2 + r), new Vector2(-w / 2 + r, -l / 2 + r), new Vector2(-w / 2 + r, l / 2 - r) };
            float[] start = { 90, 0, -90, -180 };
            for (int c = 0; c < 4; c++)
                for (int i = 0; i <= perCorner; i++)
                {
                    float a = (start[c] - 90f * i / perCorner) * Mathf.Deg2Rad;
                    pts.Add(centers[c] + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r);
                }
            return pts;
        }

        /// <summary>
        /// Recipiente con forma de rectángulo redondeado (bañera, pileta, bandeja): paredes con espesor,
        /// borde arriba y fondo adentro. Base en y = 0.
        /// </summary>
        public static void AddBasin(Builder b, int sub, Vector3 center, float w, float l, float radius, float height, float wall, float floorH)
        {
            var outer = RoundedRect(w, l, radius);
            var inner = RoundedRect(w - wall * 2, l - wall * 2, Mathf.Max(0.01f, radius - wall));
            int n = outer.Count;
            Vector3 O(int i, float y) => center + new Vector3(outer[i % n].x, y, outer[i % n].y);
            Vector3 I(int i, float y) => center + new Vector3(inner[i % n].x, y, inner[i % n].y);
            for (int i = 0; i < n; i++)
            {
                // pared de afuera
                b.Quad(sub, O(i, 0), O(i + 1, 0), O(i + 1, height), O(i, height), Vector2.zero, Vector2.up, Vector2.one, Vector2.right);
                // borde de arriba
                b.Quad(sub, O(i, height), O(i + 1, height), I(i + 1, height), I(i, height), Vector2.zero, Vector2.up, Vector2.one, Vector2.right);
                // pared de adentro
                b.Quad(sub, I(i, height), I(i + 1, height), I(i + 1, floorH), I(i, floorH), Vector2.zero, Vector2.up, Vector2.one, Vector2.right);
                // fondo de adentro y de abajo
                b.Tri(sub, center + Vector3.up * floorH, I(i, floorH), I(i + 1, floorH));
                b.Tri(sub, center, O(i + 1, 0), O(i, 0));
            }
        }

        /// <summary>Caja con los bordes biselados (se ve mucho menos "cubo"). Base en y = 0 del centro dado.</summary>
        public static void AddChamferBox(Builder b, int sub, Vector3 center, Vector3 size, float bevel)
        {
            bevel = Mathf.Min(bevel, Mathf.Min(size.x, Mathf.Min(size.y, size.z)) * 0.45f);
            float hx = size.x / 2, hz = size.z / 2, h = size.y;
            // perfil (z, y) biselado, extruido a lo largo de X (menos el bisel de las puntas)
            var prof = new List<Vector2>
            {
                new Vector2(-hz + bevel, 0), new Vector2(hz - bevel, 0), new Vector2(hz, bevel), new Vector2(hz, h - bevel),
                new Vector2(hz - bevel, h), new Vector2(-hz + bevel, h), new Vector2(-hz, h - bevel), new Vector2(-hz, bevel)
            };
            var tmp = new Builder(1);
            AddExtrusion(tmp, 0, prof, size.x - bevel * 2);
            // tapas de las puntas, un poco más chicas (bisel en X)
            var small = new List<Vector2>();
            foreach (var p in prof)
            {
                var q = new Vector2(Mathf.Clamp(p.x, -hz + bevel, hz - bevel), Mathf.Clamp(p.y, bevel, h - bevel));
                if (small.Count == 0 || (small[small.Count - 1] - q).sqrMagnitude > 1e-8f) small.Add(q);
            }
            if (small.Count > 1 && (small[0] - small[small.Count - 1]).sqrMagnitude < 1e-8f) small.RemoveAt(small.Count - 1);
            AddExtrusion(tmp, 0, small, size.x);
            var m = tmp.Build();
            b.Add(m, Matrix4x4.Translate(center), sub);
        }

        /// <summary>Triangula un polígono simple (método de "orejas"). Devuelve índices.</summary>
        public static List<int> Triangulate(List<Vector2> pts)
        {
            var res = new List<int>();
            var idx = new List<int>();
            for (int i = 0; i < pts.Count; i++) idx.Add(i);
            float area = 0;
            for (int i = 0; i < pts.Count; i++) { var p = pts[i]; var q = pts[(i + 1) % pts.Count]; area += p.x * q.y - q.x * p.y; }
            if (area < 0) idx.Reverse();
            int guard = 0;
            while (idx.Count > 3 && guard++ < 1000)
            {
                bool clipped = false;
                for (int i = 0; i < idx.Count; i++)
                {
                    int ia = idx[(i + idx.Count - 1) % idx.Count], ib = idx[i], ic = idx[(i + 1) % idx.Count];
                    Vector2 a = pts[ia], bb = pts[ib], c = pts[ic];
                    if ((bb.x - a.x) * (c.y - a.y) - (bb.y - a.y) * (c.x - a.x) <= 0) continue; // no convexo
                    bool inside = false;
                    foreach (int j in idx)
                    {
                        if (j == ia || j == ib || j == ic) continue;
                        if (InTri(pts[j], a, bb, c)) { inside = true; break; }
                    }
                    if (inside) continue;
                    res.Add(ia); res.Add(ib); res.Add(ic);
                    idx.RemoveAt(i);
                    clipped = true;
                    break;
                }
                if (!clipped) break;
            }
            if (idx.Count == 3) { res.Add(idx[0]); res.Add(idx[1]); res.Add(idx[2]); }
            return res;
        }

        static bool InTri(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = (p.x - b.x) * (a.y - b.y) - (a.x - b.x) * (p.y - b.y);
            float d2 = (p.x - c.x) * (b.y - c.y) - (b.x - c.x) * (p.y - c.y);
            float d3 = (p.x - a.x) * (c.y - a.y) - (c.x - a.x) * (p.y - a.y);
            bool neg = d1 < 0 || d2 < 0 || d3 < 0, pos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(neg && pos);
        }

        /// <summary>Crea un objeto con malla y materiales (uno por sub-malla).</summary>
        public static GameObject Object(Transform parent, string name, Mesh mesh, Material[] mats, Vector3 pos, Vector3? euler = null, bool collider = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            if (euler.HasValue) go.transform.localEulerAngles = euler.Value;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterials = mats;
            if (collider)
            {
                var bc = go.AddComponent<BoxCollider>();
                bc.center = mesh.bounds.center;
                bc.size = mesh.bounds.size;
            }
            return go;
        }
    }
}
