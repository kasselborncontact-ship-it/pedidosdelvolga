using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LaTranquera
{
    /// <summary>
    /// Fábrica de materiales URP por color. Cuando se arma la escena desde el editor,
    /// los materiales se guardan como archivos en Assets/LaTranquera/Generated/Materials
    /// para que no se pierdan. En el juego se crean en memoria.
    /// </summary>
    public static class Mats
    {
        public const string Folder = "Assets/LaTranquera/Generated/Materials";
        static readonly Dictionary<string, Material> cache = new Dictionary<string, Material>();

        public static Material Get(Color c, float smoothness = 0.15f, float metallic = 0f)
        {
            string key = "c_" + ColorUtility.ToHtmlStringRGB(c) + "_" + Mathf.RoundToInt(smoothness * 100) + "_" + Mathf.RoundToInt(metallic * 100);
            return Get(key, c, smoothness, metallic);
        }

        public static Material Glass(Color tint)
        {
            return Get("vidrio_" + ColorUtility.ToHtmlStringRGBA(tint), tint, 0.9f, 0f, transparent: true);
        }

        public static Material Emissive(Color c, float intensity = 2f)
        {
            return Get("luz_" + ColorUtility.ToHtmlStringRGB(c), c, 0.5f, 0f, emission: c * intensity);
        }

        public static Material Get(string name, Color c, float smoothness, float metallic,
            bool transparent = false, Color? emission = null, bool doubleSided = false, bool instancing = false)
        {
            if (cache.TryGetValue(name, out var m) && m != null)
            {
#if UNITY_EDITOR
                // en el editor (sin Play) solo sirven los materiales guardados como archivo
                if (Application.isPlaying || AssetDatabase.Contains(m)) return m;
#else
                return m;
#endif
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EnsureFolder(Folder);
                string path = Folder + "/" + name + ".mat";
                m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (m == null)
                {
                    m = Create(name, c, smoothness, metallic, transparent, emission, doubleSided, instancing);
                    AssetDatabase.CreateAsset(m, path);
                }
                cache[name] = m;
                return m;
            }
#endif
            m = Create(name, c, smoothness, metallic, transparent, emission, doubleSided, instancing);
            cache[name] = m;
            return m;
        }

        static Material Create(string name, Color c, float smoothness, float metallic,
            bool transparent, Color? emission, bool doubleSided, bool instancing)
        {
            Shader sh = Shader.Find("Universal Render Pipeline/Lit");
            if (sh == null) sh = Shader.Find("Standard");
            var m = new Material(sh) { name = name };
            m.SetColor("_BaseColor", c);
            m.color = c;
            m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_Metallic", metallic);
            m.enableInstancing = instancing;
            if (doubleSided) m.SetFloat("_Cull", 0f);
            if (transparent)
            {
                m.SetFloat("_Surface", 1f);
                m.SetFloat("_Blend", 0f);
                m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetFloat("_ZWrite", 0f);
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.renderQueue = 3000;
                m.SetOverrideTag("RenderType", "Transparent");
            }
            if (emission.HasValue)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", emission.Value);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            return m;
        }

        /// <summary>
        /// Material con un shader propio del juego (pasto, hojas con viento). Si el shader no existe,
        /// usa el Lit común de URP. setup = poner colores y valores.
        /// </summary>
        public static Material Custom(string name, string shader, Color color, System.Action<Material> setup)
        {
            var sh = Shader.Find(shader);
            if (cache.TryGetValue(name, out var m) && m != null)
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
                EnsureFolder(Folder);
                string path = Folder + "/" + name + ".mat";
                m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (m == null)
                {
                    m = sh != null ? new Material(sh) : Create(name, color, 0.2f, 0, false, null, true, true);
                    AssetDatabase.CreateAsset(m, path);
                }
                if (sh != null && m.shader != sh) m.shader = sh;
                m.SetColor("_BaseColor", color);
                m.enableInstancing = true;
                setup?.Invoke(m);
                EditorUtility.SetDirty(m);
                cache[name] = m;
                return m;
            }
#endif
            m = sh != null ? new Material(sh) : Create(name, color, 0.2f, 0, false, null, true, true);
            m.SetColor("_BaseColor", color);
            setup?.Invoke(m);
            cache[name] = m;
            return m;
        }

        /// <summary>
        /// Copia de un material con "recorte" de transparencia (hojas, flores, pasto de los modelos):
        /// lo transparente de la textura no se dibuja, y se ven las dos caras.
        /// </summary>
        public static Material ClipCopy(Material src)
        {
            string name = "recorte_" + src.name.Replace(" ", "_").Replace("#", "").Replace("/", "_");
            if (cache.TryGetValue(name, out var m) && m != null) return m;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EnsureFolder(Folder);
                string path = Folder + "/" + name + ".mat";
                m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (m == null)
                {
                    m = MakeClip(src, name);
                    AssetDatabase.CreateAsset(m, path);
                }
                cache[name] = m;
                return m;
            }
#endif
            m = MakeClip(src, name);
            cache[name] = m;
            return m;
        }

        static Material MakeClip(Material src, string name)
        {
            var sh = Shader.Find("Universal Render Pipeline/Lit");
            var m = new Material(sh != null ? sh : src.shader) { name = name };
            var tex = src.HasProperty("_BaseMap") ? src.GetTexture("_BaseMap") : src.mainTexture;
            if (tex != null) { m.SetTexture("_BaseMap", tex); m.mainTexture = tex; }
            m.SetColor("_BaseColor", Color.white);
            m.SetFloat("_AlphaClip", 1f);
            m.SetFloat("_Cutoff", 0.45f);
            m.EnableKeyword("_ALPHATEST_ON");
            m.SetFloat("_Cull", 0f);
            m.SetFloat("_Smoothness", 0.1f);
            m.renderQueue = 2450;
            m.SetOverrideTag("RenderType", "TransparentCutout");
            m.doubleSidedGI = true;
            return m;
        }

#if UNITY_EDITOR
        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            string leaf = System.IO.Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
#endif

        // Paleta común del juego
        public static readonly Color Madera = new Color(0.55f, 0.38f, 0.22f);
        public static readonly Color MaderaClara = new Color(0.72f, 0.56f, 0.36f);
        public static readonly Color MaderaGris = new Color(0.55f, 0.52f, 0.47f);
        public static readonly Color Tierra = new Color(0.42f, 0.30f, 0.20f);
        public static readonly Color TierraMojada = new Color(0.25f, 0.17f, 0.11f);
        public static readonly Color Ripio = new Color(0.62f, 0.58f, 0.52f);
        public static readonly Color Asfalto = new Color(0.20f, 0.20f, 0.22f);
        public static readonly Color Chapa = new Color(0.62f, 0.66f, 0.68f);
        public static readonly Color Revoque = new Color(0.93f, 0.90f, 0.82f);
        public static readonly Color VerdeHoja = new Color(0.30f, 0.62f, 0.22f);
        public static readonly Color VerdeOscuro = new Color(0.16f, 0.40f, 0.14f);
        public static readonly Color Blanco = new Color(0.95f, 0.95f, 0.95f);
        public static readonly Color Negro = new Color(0.08f, 0.08f, 0.08f);
    }
}
