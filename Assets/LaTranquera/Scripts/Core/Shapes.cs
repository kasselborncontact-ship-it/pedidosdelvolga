using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LaTranquera
{
    /// <summary>
    /// Ayudantes para armar objetos low poly con primitivas de Unity (cubo, cilindro, esfera...).
    /// Así la Fase 1 funciona SIN descargar modelos. Después podés reemplazar cada cosa por
    /// un modelo de Quaternius/Kenney (ver README).
    /// </summary>
    public static class Shapes
    {
        public static GameObject Empty(Transform parent, string name, Vector3 localPos, Vector3? euler = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            if (euler.HasValue) go.transform.localEulerAngles = euler.Value;
            return go;
        }

        public static GameObject Prim(PrimitiveType type, Transform parent, string name, Vector3 localPos,
            Vector3 localScale, Color color, bool collider = true, Vector3? euler = null)
        {
            return Prim(type, parent, name, localPos, localScale, Mats.Get(color), collider, euler);
        }

        public static GameObject Prim(PrimitiveType type, Transform parent, string name, Vector3 localPos,
            Vector3 localScale, Material mat, bool collider = true, Vector3? euler = null)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            if (euler.HasValue) go.transform.localEulerAngles = euler.Value;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!collider) RemoveCollider(go);
            return go;
        }

        /// <summary>Caja apoyada: "pos" es el centro de la BASE (más fácil para apoyar cosas en el piso).</summary>
        public static GameObject Box(Transform p, string n, Vector3 basePos, Vector3 size, Color c, bool col = true, Vector3? euler = null)
            => Prim(PrimitiveType.Cube, p, n, basePos + Vector3.up * size.y * 0.5f, size, c, col, euler);

        public static GameObject BoxM(Transform p, string n, Vector3 basePos, Vector3 size, Material m, bool col = true, Vector3? euler = null)
            => Prim(PrimitiveType.Cube, p, n, basePos + Vector3.up * size.y * 0.5f, size, m, col, euler);

        public static GameObject Box(Transform p, string n, Vector3 basePos, Vector3 size, Material m, bool col = true, Vector3? euler = null)
            => Prim(PrimitiveType.Cube, p, n, basePos + Vector3.up * size.y * 0.5f, size, m, col, euler);

        public static GameObject BoxC(Transform p, string n, Vector3 center, Vector3 size, Material m, bool col = true, Vector3? euler = null)
            => Prim(PrimitiveType.Cube, p, n, center, size, m, col, euler);

        /// <summary>Caja por su centro.</summary>
        public static GameObject BoxC(Transform p, string n, Vector3 center, Vector3 size, Color c, bool col = true, Vector3? euler = null)
            => Prim(PrimitiveType.Cube, p, n, center, size, c, col, euler);

        /// <summary>Cilindro apoyado (diámetro, alto).</summary>
        public static GameObject Cyl(Transform p, string n, Vector3 basePos, float diameter, float height, Color c, bool col = true)
            => Prim(PrimitiveType.Cylinder, p, n, basePos + Vector3.up * height * 0.5f, new Vector3(diameter, height * 0.5f, diameter), c, col);

        /// <summary>Cilindro por su centro, con rotación libre.</summary>
        public static GameObject CylC(Transform p, string n, Vector3 center, float diameter, float length, Color c, Vector3 euler, bool col = false)
            => Prim(PrimitiveType.Cylinder, p, n, center, new Vector3(diameter, length * 0.5f, diameter), c, col, euler);

        public static GameObject Sphere(Transform p, string n, Vector3 center, Vector3 size, Color c, bool col = false)
            => Prim(PrimitiveType.Sphere, p, n, center, size, c, col);

        public static GameObject Capsule(Transform p, string n, Vector3 center, Vector3 size, Color c, bool col = false)
            => Prim(PrimitiveType.Capsule, p, n, center, size, c, col);

        /// <summary>Un palo/viga entre dos puntos (para alambres, travesaños, tutores).</summary>
        public static GameObject Beam(Transform p, string n, Vector3 a, Vector3 b, float thickness, Color c, bool col = false)
        {
            var go = Prim(PrimitiveType.Cube, p, n, (a + b) * 0.5f, new Vector3(thickness, thickness, Vector3.Distance(a, b)), c, col);
            if (b != a) go.transform.localRotation = Quaternion.LookRotation(b - a);
            return go;
        }

        public static GameObject Cone(Transform p, string n, Vector3 basePos, float diameter, float height, Color c, Vector3? euler = null)
        {
            var go = new GameObject(n);
            go.transform.SetParent(p, false);
            go.transform.localPosition = basePos;
            if (euler.HasValue) go.transform.localEulerAngles = euler.Value;
            go.transform.localScale = new Vector3(diameter, height, diameter);
            go.AddComponent<MeshFilter>().sharedMesh = Meshes.Cone;
            go.AddComponent<MeshRenderer>().sharedMaterial = Mats.Get(c);
            return go;
        }

        public static void RemoveCollider(GameObject go)
        {
            var c = go.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);
        }

        public static void RemoveAllColliders(GameObject go)
        {
            foreach (var c in go.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
        }

        public static void DestroyChildren(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying) Object.Destroy(t.GetChild(i).gameObject);
                else Object.DestroyImmediate(t.GetChild(i).gameObject);
            }
        }

        /// <summary>Bounds de todos los renderers de un objeto (en mundo).</summary>
        public static Bounds RenderBounds(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.one * 0.1f);
            var b = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
            return b;
        }

        /// <summary>
        /// Acomoda "count" unidades de tamaño "unit" dentro de un área (ancho x, alto y, fondo z).
        /// Devuelve posiciones locales de la base de cada unidad, centradas en el área.
        /// </summary>
        public static List<Vector3> Arrange(int count, Vector3 unit, Vector3 area)
        {
            var list = new List<Vector3>();
            int cols = Mathf.Max(1, Mathf.FloorToInt(area.x / unit.x));
            int rows = Mathf.Max(1, Mathf.FloorToInt(area.z / unit.z));
            int perLayer = cols * rows;
            for (int i = 0; i < count; i++)
            {
                int layer = i / perLayer;
                int idx = i % perLayer;
                int cx = idx % cols, rz = idx / cols;
                float x = (cx - (cols - 1) * 0.5f) * unit.x;
                float z = (rz - (rows - 1) * 0.5f) * unit.z;
                list.Add(new Vector3(x, layer * unit.y, z));
            }
            return list;
        }
    }

    /// <summary>Mallas hechas por código.</summary>
    public static class Meshes
    {
        /// <summary>Cono de 1 de diámetro y 1 de alto, base en y=0 (se guarda como archivo en el editor).</summary>
        public static Mesh Cone => MeshKit.Persist("cono", BuildCone);

        static Mesh BuildCone()
        {
            const int seg = 8;
            var v = new List<Vector3>();
            var t = new List<int>();
            for (int i = 0; i < seg; i++)
            {
                float a0 = i * Mathf.PI * 2 / seg, a1 = (i + 1) * Mathf.PI * 2 / seg;
                var p0 = new Vector3(Mathf.Cos(a0) * 0.5f, 0, Mathf.Sin(a0) * 0.5f);
                var p1 = new Vector3(Mathf.Cos(a1) * 0.5f, 0, Mathf.Sin(a1) * 0.5f);
                int b = v.Count;
                v.Add(p0); v.Add(Vector3.up); v.Add(p1);
                t.Add(b); t.Add(b + 1); t.Add(b + 2);
                b = v.Count;
                v.Add(p0); v.Add(p1); v.Add(Vector3.zero);
                t.Add(b); t.Add(b + 1); t.Add(b + 2);
            }
            var m = new Mesh { name = "Cono" };
            m.SetVertices(v);
            m.SetTriangles(t, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }
    }

    /// <summary>Textos en el mundo 3D (carteles, precios, pantallas). Usan un Canvas chiquito.</summary>
    public static class WorldLabel
    {
        static Font font;
        public static Font UIFont
        {
            get
            {
                if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return font;
            }
        }

        /// <summary>
        /// Crea un texto. sizeMeters = tamaño del recuadro. fontSize en milímetros (40 = letras de 4 cm).
        /// "faceTowards" = hacia dónde mira el texto (desde dónde se lee), en coordenadas locales del padre.
        /// </summary>
        public static Text Create(Transform parent, string text, Vector3 localPos, Vector2 sizeMeters,
            int fontSize, Color color, Vector3 faceTowards, Color? background = null, Vector3? textUp = null)
        {
            var go = new GameObject("Texto", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = sizeMeters * 1000f;
            rt.localScale = Vector3.one * 0.001f;
            rt.localPosition = localPos;
            Vector3 f = -faceTowards.normalized;
            Vector3 up = textUp ?? (Mathf.Abs(Vector3.Dot(f, Vector3.up)) > 0.95f ? Vector3.forward : Vector3.up);
            rt.localRotation = Quaternion.LookRotation(f, up);

            if (background.HasValue)
            {
                var bg = new GameObject("Fondo", typeof(RectTransform));
                bg.transform.SetParent(go.transform, false);
                Stretch((RectTransform)bg.transform);
                bg.AddComponent<Image>().color = background.Value;
            }

            var tgo = new GameObject("T", typeof(RectTransform));
            tgo.transform.SetParent(go.transform, false);
            Stretch((RectTransform)tgo.transform);
            var t = tgo.AddComponent<Text>();
            t.font = UIFont;
            t.fontSize = fontSize;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = color;
            t.text = text;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        static void Stretch(RectTransform r)
        {
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
            r.localScale = Vector3.one;
        }
    }
}
