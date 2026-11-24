using System.Collections.Generic;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Vehículos argentinos viejos con su silueta de verdad (perfil lateral extruido):
    /// la chata F-100 de los 70, el Rastrojero y el Falcon. Frente hacia +Z, base en y = 0.
    /// Cada vehículo es UNA malla con 8 materiales: pintura, techo, cromado, vidrio, goma, negro, faros, luces traseras.
    /// Para usar modelos: prefabs en Resources/Vehiculos/ llamados "F100", "Rastrojero", "Falcon".
    /// </summary>
    public static class VehicleVisuals
    {
        public enum Kind { F100, Rastrojero, Falcon }

        public static readonly Color[] ClassicColors =
        {
            new Color(0.55f, 0.75f, 0.85f), new Color(0.88f, 0.85f, 0.72f), new Color(0.7f, 0.15f, 0.12f),
            new Color(0.2f, 0.35f, 0.25f), new Color(0.92f, 0.92f, 0.9f), new Color(0.25f, 0.3f, 0.5f), new Color(0.8f, 0.6f, 0.2f),
            new Color(0.45f, 0.55f, 0.35f)
        };

        static readonly Dictionary<Kind, Mesh> meshes = new Dictionary<Kind, Mesh>();

        const int PAINT = 0, ROOF = 1, CHROME = 2, GLASS = 3, TIRE = 4, DARK = 5, HEAD = 6, TAIL = 7;

        public static GameObject Build(Kind kind, Transform parent, Color paint) => Build(kind, parent, paint, null);

        public static GameObject Build(Kind kind, Transform parent, Color paint, Color? roof)
        {
            var prefab = Resources.Load<GameObject>("Vehiculos/" + kind);
            if (prefab != null)
            {
                var inst = Object.Instantiate(prefab, parent);
                Shapes.RemoveAllColliders(inst);
                return inst;
            }
            if (!meshes.TryGetValue(kind, out var mesh) || mesh == null)
            {
                mesh = BuildMesh(kind);
                meshes[kind] = mesh;
            }
            var mats = new[]
            {
                Mats.Get(paint, 0.55f, 0.1f),
                Mats.Get(roof ?? paint, 0.55f, 0.1f),
                Mats.Get(new Color(0.85f, 0.85f, 0.88f), 0.85f, 0.9f),
                Mats.Get(new Color(0.12f, 0.16f, 0.2f), 0.9f, 0.2f),
                Mats.Get(new Color(0.07f, 0.07f, 0.07f), 0.1f),
                Mats.Get(new Color(0.12f, 0.12f, 0.12f), 0.2f),
                Mats.Emissive(new Color(1f, 0.95f, 0.8f), 1.2f),
                Mats.Get(new Color(0.75f, 0.08f, 0.06f), 0.6f),
            };
            return MeshKit.Object(parent, "Vehiculo_" + kind, mesh, mats, Vector3.zero);
        }

        static List<Vector2> P(params float[] zy)
        {
            var l = new List<Vector2>();
            for (int i = 0; i < zy.Length; i += 2) l.Add(new Vector2(zy[i], zy[i + 1]));
            return l;
        }

        static void Wheels(MeshKit.Builder b, float w, float r, float zf, float zr)
        {
            const float tw = 0.24f;
            foreach (var z in new[] { zf, zr })
            {
                // hueco del guardabarros (disco negro un poco más grande que la rueda)
                MeshKit.AddCylinder(b, DARK, Matrix4x4.TRS(new Vector3(w / 2 + 0.006f, r, z), Quaternion.Euler(0, 0, 90), Vector3.one), r + 0.07f, r + 0.07f, w + 0.012f, 14);
                // ruedas
                MeshKit.AddCylinder(b, TIRE, Matrix4x4.TRS(new Vector3(w / 2 + 0.03f, r, z), Quaternion.Euler(0, 0, 90), Vector3.one), r, r, tw, 14);
                MeshKit.AddCylinder(b, TIRE, Matrix4x4.TRS(new Vector3(-w / 2 + tw - 0.03f, r, z), Quaternion.Euler(0, 0, 90), Vector3.one), r, r, tw, 14);
                // tazas cromadas
                MeshKit.AddCylinder(b, CHROME, Matrix4x4.TRS(new Vector3(w / 2 + 0.035f, r, z), Quaternion.Euler(0, 0, 90), Vector3.one), r * 0.5f, r * 0.42f, 0.01f, 12);
                MeshKit.AddCylinder(b, CHROME, Matrix4x4.TRS(new Vector3(-w / 2 - 0.035f, r, z), Quaternion.Euler(0, 0, -90), Vector3.one), r * 0.5f, r * 0.42f, 0.01f, 12);
            }
        }

        static void RoundLight(MeshKit.Builder b, int sub, Vector3 pos, float radius, bool front)
        {
            MeshKit.AddCylinder(b, CHROME, Matrix4x4.TRS(pos, Quaternion.Euler(front ? 90 : -90, 0, 0), Vector3.one), radius * 1.15f, radius * 1.15f, 0.02f, 12);
            MeshKit.AddCylinder(b, sub, Matrix4x4.TRS(pos + (front ? Vector3.forward : Vector3.back) * 0.02f, Quaternion.Euler(front ? 90 : -90, 0, 0), Vector3.one), radius, radius * 0.9f, 0.012f, 12);
        }

        static void Pillars(MeshKit.Builder b, float w, params List<Vector2>[] profiles)
        {
            foreach (var p in profiles)
            {
                MeshKit.AddExtrusion(b, ROOF, p, 0.06f, w / 2 - 0.07f);
                MeshKit.AddExtrusion(b, ROOF, p, 0.06f, -w / 2 + 0.07f);
            }
        }

        static Mesh BuildMesh(Kind kind)
        {
            var b = new MeshKit.Builder(8);
            switch (kind)
            {
                case Kind.F100:
                {
                    float w = 2.0f;
                    MeshKit.AddExtrusion(b, PAINT, P(-2.6f, 0.42f, 2.55f, 0.42f, 2.62f, 0.62f, 2.6f, 1.02f, 2.45f, 1.1f, 0.95f, 1.18f, 0.88f, 1.22f, -0.78f, 1.22f, -0.8f, 1.12f, -2.6f, 1.12f), w);
                    MeshKit.AddExtrusion(b, GLASS, P(-0.76f, 1.2f, 0.9f, 1.2f, 0.6f, 1.8f, -0.62f, 1.8f), w - 0.14f);
                    MeshKit.AddExtrusion(b, ROOF, P(-0.66f, 1.78f, 0.62f, 1.78f, 0.58f, 1.87f, -0.62f, 1.87f), w - 0.1f);
                    Pillars(b, w, P(0.84f, 1.2f, 0.92f, 1.2f, 0.62f, 1.8f, 0.55f, 1.8f), P(-0.12f, 1.2f, -0.02f, 1.2f, -0.02f, 1.8f, -0.12f, 1.8f), P(-0.78f, 1.2f, -0.7f, 1.2f, -0.56f, 1.8f, -0.64f, 1.8f));
                    MeshKit.AddBox(b, DARK, new Vector3(0, 1.0f, -1.68f), new Vector3(w - 0.16f, 0.25f, 1.65f)); // caja de carga por dentro
                    MeshKit.AddBox(b, CHROME, new Vector3(0, 0.84f, 2.615f), new Vector3(w - 0.35f, 0.3f, 0.03f)); // parrilla
                    for (int i = 0; i < 3; i++) MeshKit.AddBox(b, DARK, new Vector3(0, 0.76f + i * 0.08f, 2.632f), new Vector3(w - 0.5f, 0.025f, 0.01f));
                    RoundLight(b, HEAD, new Vector3(0.72f, 0.86f, 2.62f), 0.11f, true);
                    RoundLight(b, HEAD, new Vector3(-0.72f, 0.86f, 2.62f), 0.11f, true);
                    MeshKit.AddBox(b, CHROME, new Vector3(0, 0.52f, 2.66f), new Vector3(w + 0.04f, 0.16f, 0.1f));
                    MeshKit.AddBox(b, CHROME, new Vector3(0, 0.52f, -2.65f), new Vector3(w + 0.04f, 0.16f, 0.1f));
                    MeshKit.AddBox(b, TAIL, new Vector3(0.85f, 0.95f, -2.61f), new Vector3(0.18f, 0.22f, 0.03f));
                    MeshKit.AddBox(b, TAIL, new Vector3(-0.85f, 0.95f, -2.61f), new Vector3(0.18f, 0.22f, 0.03f));
                    MeshKit.AddBox(b, CHROME, new Vector3(w / 2 + 0.12f, 1.45f, 0.75f), new Vector3(0.04f, 0.14f, 0.1f)); // espejo
                    Wheels(b, w, 0.38f, 1.55f, -1.5f);
                    break;
                }
                case Kind.Rastrojero:
                {
                    float w = 1.9f;
                    MeshKit.AddExtrusion(b, PAINT, P(-2.3f, 0.42f, 2.1f, 0.42f, 2.3f, 0.62f, 2.28f, 0.95f, 2.0f, 1.18f, 1.1f, 1.25f, 0.9f, 1.25f, -0.7f, 1.2f, -0.75f, 1.1f, -2.3f, 1.1f), w);
                    // guardabarros delanteros redondeados
                    MeshKit.AddBlob(b, PAINT, new Vector3(w / 2 - 0.2f, 0.85f, 1.6f), new Vector3(0.5f, 0.6f, 1.2f), 3, 0.02f);
                    MeshKit.AddBlob(b, PAINT, new Vector3(-w / 2 + 0.2f, 0.85f, 1.6f), new Vector3(0.5f, 0.6f, 1.2f), 4, 0.02f);
                    MeshKit.AddExtrusion(b, GLASS, P(-0.72f, 1.2f, 0.92f, 1.22f, 0.72f, 1.84f, -0.6f, 1.86f), w - 0.14f);
                    MeshKit.AddExtrusion(b, ROOF, P(-0.64f, 1.84f, 0.74f, 1.82f, 0.7f, 1.92f, -0.6f, 1.93f), w - 0.1f);
                    Pillars(b, w, P(0.85f, 1.22f, 0.93f, 1.22f, 0.74f, 1.84f, 0.67f, 1.84f), P(-0.05f, 1.22f, 0.05f, 1.22f, 0.05f, 1.85f, -0.05f, 1.85f), P(-0.74f, 1.2f, -0.66f, 1.2f, -0.54f, 1.85f, -0.62f, 1.86f));
                    MeshKit.AddBox(b, DARK, new Vector3(0, 0.98f, -1.55f), new Vector3(w - 0.16f, 0.25f, 1.45f));
                    for (int i = 0; i < 4; i++) MeshKit.AddBox(b, CHROME, new Vector3(0, 0.62f + i * 0.08f, 2.3f), new Vector3(0.8f, 0.03f, 0.04f));
                    RoundLight(b, HEAD, new Vector3(0.7f, 1.0f, 2.2f), 0.1f, true);
                    RoundLight(b, HEAD, new Vector3(-0.7f, 1.0f, 2.2f), 0.1f, true);
                    MeshKit.AddBox(b, CHROME, new Vector3(0, 0.5f, 2.36f), new Vector3(w, 0.13f, 0.08f));
                    MeshKit.AddBox(b, CHROME, new Vector3(0, 0.5f, -2.35f), new Vector3(w, 0.13f, 0.08f));
                    MeshKit.AddBox(b, TAIL, new Vector3(0.8f, 0.9f, -2.31f), new Vector3(0.14f, 0.14f, 0.03f));
                    MeshKit.AddBox(b, TAIL, new Vector3(-0.8f, 0.9f, -2.31f), new Vector3(0.14f, 0.14f, 0.03f));
                    Wheels(b, w, 0.36f, 1.45f, -1.45f);
                    break;
                }
                case Kind.Falcon:
                {
                    float w = 1.85f;
                    MeshKit.AddExtrusion(b, PAINT, P(-2.4f, 0.32f, 2.4f, 0.32f, 2.47f, 0.5f, 2.43f, 0.78f, 1.0f, 0.86f, -1.55f, 0.86f, -2.4f, 0.83f), w);
                    MeshKit.AddExtrusion(b, GLASS, P(-1.6f, 0.85f, 1.0f, 0.85f, 0.3f, 1.36f, -0.85f, 1.38f), w - 0.14f);
                    MeshKit.AddExtrusion(b, ROOF, P(-0.9f, 1.36f, 0.32f, 1.34f, 0.28f, 1.42f, -0.85f, 1.44f), w - 0.1f);
                    Pillars(b, w, P(0.95f, 0.85f, 1.02f, 0.85f, 0.33f, 1.36f, 0.27f, 1.36f), P(-0.32f, 0.85f, -0.22f, 0.85f, -0.22f, 1.38f, -0.32f, 1.38f), P(-1.62f, 0.85f, -1.5f, 0.85f, -0.82f, 1.38f, -0.9f, 1.38f));
                    MeshKit.AddBox(b, CHROME, new Vector3(0, 0.58f, 2.45f), new Vector3(w - 0.3f, 0.2f, 0.03f));
                    RoundLight(b, HEAD, new Vector3(0.68f, 0.62f, 2.46f), 0.085f, true);
                    RoundLight(b, HEAD, new Vector3(0.48f, 0.62f, 2.46f), 0.075f, true);
                    RoundLight(b, HEAD, new Vector3(-0.68f, 0.62f, 2.46f), 0.085f, true);
                    RoundLight(b, HEAD, new Vector3(-0.48f, 0.62f, 2.46f), 0.075f, true);
                    MeshKit.AddBox(b, CHROME, new Vector3(0, 0.38f, 2.48f), new Vector3(w + 0.04f, 0.12f, 0.08f));
                    MeshKit.AddBox(b, CHROME, new Vector3(0, 0.38f, -2.45f), new Vector3(w + 0.04f, 0.12f, 0.08f));
                    MeshKit.AddBox(b, TAIL, new Vector3(0.62f, 0.68f, -2.41f), new Vector3(0.45f, 0.1f, 0.03f));
                    MeshKit.AddBox(b, TAIL, new Vector3(-0.62f, 0.68f, -2.41f), new Vector3(0.45f, 0.1f, 0.03f));
                    MeshKit.AddBox(b, CHROME, new Vector3(w / 2 + 0.005f, 0.6f, 0), new Vector3(0.01f, 0.03f, 4.2f)); // moldura lateral
                    MeshKit.AddBox(b, CHROME, new Vector3(-w / 2 - 0.005f, 0.6f, 0), new Vector3(0.01f, 0.03f, 4.2f));
                    Wheels(b, w, 0.33f, 1.45f, -1.4f);
                    break;
                }
            }
            return b.Build("Vehiculo_" + kind);
        }
    }
}
