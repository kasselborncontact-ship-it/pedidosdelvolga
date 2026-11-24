using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LaTranquera
{
    /// <summary>
    /// Texturas dibujadas por código (ladrillo, revoque, madera, baldosa calcárea, tierra, ripio...).
    /// En el editor se guardan como PNG en Generated/Textures, así las podés abrir, editar o
    /// reemplazar por fotos de Poly Haven con el mismo nombre.
    /// Cada textura representa "metros" metros del mundo (las mallas de MeshKit tienen UV en metros).
    /// </summary>
    public static class TexKit
    {
        public const string Folder = "Assets/LaTranquera/Generated/Textures";
        static readonly Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>();

        public struct Info { public string name; public float meters; }

        /// <summary>Material con textura. tint multiplica el color. meters = cuántos metros cubre una repetición.</summary>
        public static Material Mat(string tex, Color tint, float smoothness = 0.15f, float metallic = 0f)
        {
            string key = "tx_" + tex + "_" + ColorUtility.ToHtmlStringRGB(tint);
            var m = Mats.Get(key, tint, smoothness, metallic);
            if (m.GetTexture("_BaseMap") == null)
            {
                var t = Get(tex, out float meters);
                m.SetTexture("_BaseMap", t);
                m.mainTexture = t;
                var scale = new Vector2(1f / meters, 1f / meters);
                m.SetTextureScale("_BaseMap", scale);
                m.mainTextureScale = scale;
#if UNITY_EDITOR
                if (!Application.isPlaying) EditorUtility.SetDirty(m);
#endif
            }
            return m;
        }

        /// <summary>Material con textura y repetición distinta en cada eje (por ejemplo la ruta: 9 m a lo largo, el ancho entero a lo ancho).</summary>
        public static Material MatST(string tex, Color tint, Vector2 metersXY, float smoothness = 0.15f)
        {
            string key = "tx_" + tex + "_" + ColorUtility.ToHtmlStringRGB(tint) + "_st";
            var m = Mats.Get(key, tint, smoothness, 0f);
            if (m.GetTexture("_BaseMap") == null)
            {
                var t = Get(tex, out _);
                m.SetTexture("_BaseMap", t);
                m.mainTexture = t;
                var scale = new Vector2(1f / metersXY.x, 1f / metersXY.y);
                m.SetTextureScale("_BaseMap", scale);
                m.mainTextureScale = scale;
#if UNITY_EDITOR
                if (!Application.isPlaying) EditorUtility.SetDirty(m);
#endif
            }
            return m;
        }

        public static Texture2D Get(string name, out float meters)
        {
            meters = MetersOf(name);
            if (cache.TryGetValue(name, out var t) && t != null) return t;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Mats.EnsureFolder(Folder);
                string path = Folder + "/" + name + ".png";
                t = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (t == null)
                {
                    var gen = Generate(name);
                    System.IO.File.WriteAllBytes(path, gen.EncodeToPNG());
                    AssetDatabase.ImportAsset(path);
                    t = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                }
                cache[name] = t;
                return t;
            }
#endif
            t = Generate(name);
            cache[name] = t;
            return t;
        }

        static float MetersOf(string name)
        {
            switch (name)
            {
                case "ladrillo": return 1.0f;
                case "revoque": return 2.0f;
                case "madera": return 1.0f;
                case "machimbre": return 1.0f;
                case "calcareo": return 0.8f;
                case "tierra": return 2.0f;
                case "tierraArada": return 2.0f;
                case "ripio": return 1.5f;
                case "asfalto": return 3.0f;
                case "pasto": return 3.0f;
                case "chapa": return 1.0f;
                case "teja": return 1.0f;
                case "sandia": return 1.0f;
                case "cemento": return 2.0f;
                case "ruta": return 9.0f;
            }
            return 1f;
        }

        // ------------------------------------------------------------------
        static float Hash(int x, int y, int s)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263 + s * 982451653;
                h = (h ^ (h >> 13)) * 1274126177;
                return ((h ^ (h >> 16)) & 0xffff) / 65535f;
            }
        }

        static float Noise(float x, float y, int s)
        {
            int xi = Mathf.FloorToInt(x), yi = Mathf.FloorToInt(y);
            float fx = x - xi, fy = y - yi;
            fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy);
            float a = Hash(xi, yi, s), b = Hash(xi + 1, yi, s), c = Hash(xi, yi + 1, s), d = Hash(xi + 1, yi + 1, s);
            return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
        }

        /// <summary>Ruido que se repite sin cortes cada "period" unidades.</summary>
        static float TNoise(float x, float y, int period, int s)
        {
            int xi = Mathf.FloorToInt(x), yi = Mathf.FloorToInt(y);
            float fx = x - xi, fy = y - yi;
            fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy);
            int x0 = ((xi % period) + period) % period, y0 = ((yi % period) + period) % period;
            int x1 = (x0 + 1) % period, y1 = (y0 + 1) % period;
            float a = Hash(x0, y0, s), b = Hash(x1, y0, s), c = Hash(x0, y1, s), d = Hash(x1, y1, s);
            return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
        }

        /// <summary>Ruido fractal repetible (u, v entre 0 y 1).</summary>
        static float Fbm(float u, float v, int baseFreq, int s, int oct = 4)
        {
            float sum = 0, amp = 0.5f, tot = 0;
            int f = baseFreq;
            for (int i = 0; i < oct; i++)
            {
                sum += TNoise(u * f, v * f, f, s + i) * amp;
                tot += amp; amp *= 0.5f; f *= 2;
            }
            return sum / tot;
        }

        static Texture2D Generate(string name)
        {
            const int N = 256;
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, true) { name = name, wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear, anisoLevel = 4 };
            var px = new Color[N * N];
            for (int y = 0; y < N; y++)
                for (int x = 0; x < N; x++)
                {
                    float u = (float)x / N, v = (float)y / N;
                    px[y * N + x] = Pixel(name, u, v);
                }
            tex.SetPixels(px);
            tex.Apply(true);
            return tex;
        }

        static Color Pixel(string name, float u, float v)
        {
            float n = Fbm(u, v, 8, 1);
            switch (name)
            {
                case "ladrillo":
                {
                    // 4 hileras por metro de ladrillo visto, junta de cal
                    float rows = 14f, row = v * rows;
                    int ri = Mathf.FloorToInt(row);
                    float off = (ri % 2) * 0.5f;
                    float cols = 4f, col = u * cols + off;
                    int ci = Mathf.FloorToInt(col);
                    float fr = row - ri, fc = col - ci;
                    bool mortar = fr < 0.12f || fc < 0.04f;
                    if (mortar) return new Color(0.78f, 0.74f, 0.66f) * (0.9f + n * 0.15f);
                    float k = Hash(ci % 4, ri % 14, 7);
                    var c = Color.Lerp(new Color(0.62f, 0.27f, 0.18f), new Color(0.75f, 0.42f, 0.28f), k);
                    return c * (0.85f + n * 0.3f);
                }
                case "revoque":
                {
                    // revoque a la cal, viejo, con manchas
                    float stain = Fbm(u, v, 3, 5);
                    var c = Color.Lerp(new Color(0.95f, 0.93f, 0.86f), new Color(0.84f, 0.8f, 0.7f), Mathf.SmoothStep(0.45f, 0.75f, stain));
                    return c * (0.93f + n * 0.12f);
                }
                case "madera":
                case "machimbre":
                {
                    float planks = name == "madera" ? 6f : 10f;
                    float p = u * planks; int pi = Mathf.FloorToInt(p);
                    float fp = p - pi;
                    float grain = Mathf.Sin((v * 30f + Fbm(u, v, 4, 3) * 6f + pi * 1.7f) * 3.1416f) * 0.5f + 0.5f;
                    float k = Hash(pi % (int)planks, 1, 11);
                    var c = Color.Lerp(new Color(0.55f, 0.38f, 0.22f), new Color(0.7f, 0.52f, 0.33f), k) * (0.85f + grain * 0.2f);
                    if (fp < 0.04f) c *= 0.55f;
                    return c;
                }
                case "calcareo":
                {
                    // baldosa calcárea: cuadros con un dibujo de flor (4 baldosas por textura)
                    float tu = (u * 4) % 1f, tv = (v * 4) % 1f;
                    int ti = Mathf.FloorToInt(u * 4), tj = Mathf.FloorToInt(v * 4);
                    float dx = Mathf.Abs(tu - 0.5f), dy = Mathf.Abs(tv - 0.5f);
                    bool grout = tu < 0.02f || tv < 0.02f;
                    Color cream = new Color(0.9f, 0.86f, 0.76f), red = new Color(0.62f, 0.2f, 0.16f), grey = new Color(0.35f, 0.38f, 0.4f), ochre = new Color(0.78f, 0.6f, 0.32f);
                    Color c = (ti + tj) % 2 == 0 ? cream : new Color(0.86f, 0.82f, 0.72f);
                    float d = dx + dy;
                    if (d < 0.18f) c = red;
                    else if (d < 0.24f) c = cream;
                    else if (Mathf.Abs(d - 0.33f) < 0.03f) c = grey;
                    if (dx > 0.42f || dy > 0.42f) c = ochre;
                    if (grout) c = new Color(0.6f, 0.58f, 0.55f);
                    return c * (0.92f + n * 0.12f);
                }
                case "tierra":
                {
                    float big = Fbm(u, v, 4, 21);
                    var c = Color.Lerp(new Color(0.42f, 0.31f, 0.2f), new Color(0.55f, 0.43f, 0.3f), big);
                    float pebble = Hash(Mathf.FloorToInt(u * 128), Mathf.FloorToInt(v * 128), 9);
                    if (pebble > 0.97f) c = new Color(0.6f, 0.55f, 0.48f);
                    return c * (0.88f + n * 0.2f);
                }
                case "tierraArada":
                {
                    float furrow = Mathf.Sin(v * 3.1416f * 2 * 8) * 0.5f + 0.5f;
                    var c = Color.Lerp(new Color(0.3f, 0.21f, 0.14f), new Color(0.46f, 0.34f, 0.23f), furrow * 0.7f + Fbm(u, v, 6, 4) * 0.3f);
                    return c * (0.9f + n * 0.15f);
                }
                case "ripio":
                {
                    float cell = Hash(Mathf.FloorToInt(u * 96), Mathf.FloorToInt(v * 96), 3);
                    var c = Color.Lerp(new Color(0.5f, 0.47f, 0.42f), new Color(0.75f, 0.72f, 0.66f), cell);
                    return c * (0.85f + n * 0.2f);
                }
                case "asfalto":
                {
                    float g = Hash(Mathf.FloorToInt(u * 200), Mathf.FloorToInt(v * 200), 5);
                    var c = new Color(0.2f, 0.2f, 0.21f) * (0.85f + g * 0.25f);
                    float patch = Fbm(u, v, 2, 8);
                    if (patch > 0.68f) c *= 0.82f;
                    return c;
                }
                case "pasto":
                {
                    float big = Fbm(u, v, 3, 31);
                    var c = Color.Lerp(new Color(0.3f, 0.45f, 0.18f), new Color(0.45f, 0.55f, 0.24f), big);
                    if (Fbm(u, v, 5, 17) > 0.66f) c = Color.Lerp(c, new Color(0.55f, 0.5f, 0.3f), 0.5f);
                    return c * (0.85f + Hash(Mathf.FloorToInt(u * 256), Mathf.FloorToInt(v * 256), 2) * 0.2f);
                }
                case "chapa":
                {
                    var c = new Color(0.7f, 0.72f, 0.73f);
                    float rust = Fbm(u, v, 4, 13);
                    if (rust > 0.62f) c = Color.Lerp(c, new Color(0.55f, 0.35f, 0.22f), Mathf.InverseLerp(0.62f, 0.8f, rust));
                    return c * (0.9f + n * 0.12f);
                }
                case "teja":
                {
                    float row = v * 8; int ri = Mathf.FloorToInt(row); float fr = row - ri;
                    float col = u * 6 + (ri % 2) * 0.5f; float fc = col - Mathf.Floor(col);
                    float shade = 0.75f + 0.25f * Mathf.Sin(fc * 3.1416f);
                    var c = new Color(0.68f, 0.32f, 0.2f) * shade * (fr < 0.08f ? 0.6f : 1f);
                    return c * (0.9f + n * 0.15f);
                }
                case "sandia":
                {
                    // rayas a lo largo (la U da la vuelta a la esfera)
                    float w = Mathf.Sin((u * 16 + Fbm(u, v, 4, 6) * 1.2f) * 3.1416f * 2);
                    return w > 0.1f ? new Color(0.12f, 0.35f, 0.12f) : new Color(0.45f, 0.66f, 0.3f);
                }
                case "cemento":
                    return new Color(0.66f, 0.65f, 0.62f) * (0.88f + n * 0.2f);
                case "ruta":
                {
                    // U = a lo largo (9 m por repetición), V = a lo ancho (toda la ruta, 7,2 m)
                    float g = Hash(Mathf.FloorToInt(u * 256), Mathf.FloorToInt(v * 256), 5);
                    var c = new Color(0.21f, 0.21f, 0.22f) * (0.82f + g * 0.3f);
                    float lane = Mathf.Abs(v - 0.5f);
                    // huellas de las ruedas, más gastadas y claras
                    foreach (float wv in new[] { 0.16f, 0.34f })
                        if (Mathf.Abs(lane - wv) < 0.05f) c *= 1.12f;
                    // parches de arreglo
                    if (Fbm(u, v, 3, 44) > 0.7f) c *= 0.78f;
                    // grietas
                    float crack = Mathf.Abs(Fbm(u * 2, v * 2, 4, 9) - 0.5f);
                    if (crack < 0.008f) c *= 0.55f;
                    // banquina gastada en los bordes
                    if (v < 0.015f || v > 0.985f) c = Color.Lerp(c, new Color(0.45f, 0.42f, 0.36f), 0.6f);
                    // líneas: bordes blancos continuos y línea central amarilla discontinua
                    Color paint = new Color(0.92f, 0.92f, 0.88f) * (0.85f + n * 0.2f);
                    if (Mathf.Abs(v - 0.035f) < 0.009f || Mathf.Abs(v - 0.965f) < 0.009f) c = Color.Lerp(c, paint, 0.9f);
                    if (Mathf.Abs(v - 0.5f) < 0.009f && u % 1f < 0.39f) c = Color.Lerp(c, new Color(0.95f, 0.8f, 0.25f), 0.9f);
                    return c;
                }
            }
            return Color.white;
        }
    }
}
