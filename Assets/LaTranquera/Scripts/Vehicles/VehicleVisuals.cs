using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Vehículos argentinos viejos hechos con formas simples: Ford F-100 de los 70,
    /// Rastrojero y Ford Falcon. El frente mira hacia +Z y la base queda en y = 0.
    /// Para usar modelos: prefabs en Resources/Vehiculos/ llamados "F100", "Rastrojero", "Falcon".
    /// </summary>
    public static class VehicleVisuals
    {
        public enum Kind { F100, Rastrojero, Falcon }

        public static readonly Color[] ClassicColors =
        {
            new Color(0.55f, 0.75f, 0.85f), new Color(0.85f, 0.82f, 0.7f), new Color(0.7f, 0.15f, 0.12f),
            new Color(0.2f, 0.35f, 0.25f), new Color(0.9f, 0.9f, 0.88f), new Color(0.25f, 0.3f, 0.5f), new Color(0.8f, 0.6f, 0.2f)
        };

        static readonly Color Chrome = new Color(0.8f, 0.8f, 0.82f);
        static readonly Color Tire = new Color(0.08f, 0.08f, 0.08f);
        static readonly Color GlassDark = new Color(0.15f, 0.2f, 0.25f);

        public static GameObject Build(Kind kind, Transform parent, Color paint)
        {
            var prefab = Resources.Load<GameObject>("Vehiculos/" + kind);
            if (prefab != null)
            {
                var inst = Object.Instantiate(prefab, parent);
                Shapes.RemoveAllColliders(inst);
                return inst;
            }
            var root = Shapes.Empty(parent, "Vehiculo_" + kind, Vector3.zero);
            var t = root.transform;
            switch (kind)
            {
                case Kind.F100: Pickup(t, paint, false); break;
                case Kind.Rastrojero: Pickup(t, paint, true); break;
                case Kind.Falcon: Falcon(t, paint); break;
            }
            return root;
        }

        static void Wheels(Transform t, float halfW, float front, float rear, float d)
        {
            foreach (var z in new[] { front, rear })
                foreach (var x in new[] { -halfW, halfW })
                {
                    Shapes.CylC(t, "Rueda", new Vector3(x, d / 2, z), d, 0.25f, Tire, new Vector3(0, 0, 90));
                    Shapes.CylC(t, "Llanta", new Vector3(x * 1.04f, d / 2, z), d * 0.45f, 0.24f, Chrome, new Vector3(0, 0, 90));
                }
        }

        static void Lights(Transform t, float z, float y, float halfW)
        {
            var lamp = Mats.Emissive(new Color(1f, 0.95f, 0.75f), 1.5f);
            foreach (var x in new[] { -halfW, halfW })
                Shapes.Prim(PrimitiveType.Sphere, t, "Faro", new Vector3(x, y, z), new Vector3(0.2f, 0.2f, 0.08f), lamp, false);
        }

        static void Pickup(Transform t, Color paint, bool rastrojero)
        {
            float w = 1.9f;
            Wheels(t, w / 2 - 0.05f, 1.55f, -1.45f, 0.75f);
            // Chasis y parachoques
            Shapes.Box(t, "Chasis", new Vector3(0, 0.45f, 0), new Vector3(w * 0.9f, 0.25f, 4.9f), new Color(0.15f, 0.15f, 0.15f), false);
            Shapes.Box(t, "ParagolpesD", new Vector3(0, 0.5f, 2.55f), new Vector3(w + 0.05f, 0.18f, 0.12f), Chrome, false);
            Shapes.Box(t, "ParagolpesT", new Vector3(0, 0.5f, -2.5f), new Vector3(w + 0.05f, 0.18f, 0.12f), Chrome, false);
            // Capot
            if (rastrojero)
            {
                Shapes.Box(t, "Capot", new Vector3(0, 0.65f, 1.55f), new Vector3(w * 0.95f, 0.55f, 1.6f), paint, false);
                Shapes.Prim(PrimitiveType.Sphere, t, "Trompa", new Vector3(0, 1.0f, 2.2f), new Vector3(w * 0.95f, 0.75f, 0.7f), paint, false);
                Shapes.Box(t, "Parrilla", new Vector3(0, 0.7f, 2.45f), new Vector3(0.9f, 0.35f, 0.06f), Chrome, false);
                Lights(t, 2.48f, 0.95f, 0.65f);
            }
            else
            {
                Shapes.Box(t, "Capot", new Vector3(0, 0.65f, 1.6f), new Vector3(w, 0.65f, 1.75f), paint, false);
                Shapes.Box(t, "Parrilla", new Vector3(0, 0.75f, 2.47f), new Vector3(w * 0.9f, 0.4f, 0.06f), Chrome, false);
                Lights(t, 2.5f, 0.95f, 0.7f);
            }
            // Cabina
            Shapes.Box(t, "Cabina", new Vector3(0, 0.65f, 0.15f), new Vector3(w, 1.35f, 1.4f), paint, false);
            Shapes.Box(t, "Techo", new Vector3(0, 2.0f, 0.15f), new Vector3(w * 0.98f, 0.06f, 1.42f), new Color(paint.r * 0.9f + 0.1f, paint.g * 0.9f + 0.1f, paint.b * 0.9f + 0.1f), false);
            Shapes.Box(t, "Parabrisas", new Vector3(0, 1.35f, 0.86f), new Vector3(w * 0.85f, 0.55f, 0.03f), GlassDark, false);
            Shapes.Box(t, "VentanaI", new Vector3(-w / 2 - 0.005f, 1.35f, 0.15f), new Vector3(0.02f, 0.5f, 1.0f), GlassDark, false);
            Shapes.Box(t, "VentanaD", new Vector3(w / 2 + 0.005f, 1.35f, 0.15f), new Vector3(0.02f, 0.5f, 1.0f), GlassDark, false);
            // Caja de carga
            Shapes.Box(t, "PisoCaja", new Vector3(0, 0.6f, -1.55f), new Vector3(w, 0.08f, 2.0f), paint * 0.85f, false);
            Shapes.Box(t, "BaranaI", new Vector3(-w / 2 + 0.04f, 0.68f, -1.55f), new Vector3(0.08f, 0.55f, 2.0f), paint, false);
            Shapes.Box(t, "BaranaD", new Vector3(w / 2 - 0.04f, 0.68f, -1.55f), new Vector3(0.08f, 0.55f, 2.0f), paint, false);
            Shapes.Box(t, "Portón", new Vector3(0, 0.68f, -2.52f), new Vector3(w, 0.55f, 0.08f), paint, false);
            Shapes.Box(t, "LuzTraseraI", new Vector3(-w / 2 + 0.12f, 1.0f, -2.57f), new Vector3(0.15f, 0.12f, 0.03f), new Color(0.8f, 0.1f, 0.1f), false);
            Shapes.Box(t, "LuzTraseraD", new Vector3(w / 2 - 0.12f, 1.0f, -2.57f), new Vector3(0.15f, 0.12f, 0.03f), new Color(0.8f, 0.1f, 0.1f), false);
        }

        static void Falcon(Transform t, Color paint)
        {
            float w = 1.8f;
            Wheels(t, w / 2 - 0.05f, 1.45f, -1.35f, 0.65f);
            Shapes.Box(t, "Carroceria", new Vector3(0, 0.35f, 0), new Vector3(w, 0.62f, 4.7f), paint, false);
            Shapes.Box(t, "Cabina", new Vector3(0, 0.97f, -0.2f), new Vector3(w * 0.88f, 0.5f, 2.3f), paint, false);
            Shapes.Box(t, "Parabrisas", new Vector3(0, 0.98f, 0.96f), new Vector3(w * 0.82f, 0.45f, 0.03f), GlassDark, false, new Vector3(-25, 0, 0));
            Shapes.Box(t, "Luneta", new Vector3(0, 0.98f, -1.36f), new Vector3(w * 0.82f, 0.45f, 0.03f), GlassDark, false, new Vector3(25, 0, 0));
            Shapes.Box(t, "VentanasI", new Vector3(-w * 0.44f - 0.005f, 1.0f, -0.2f), new Vector3(0.02f, 0.38f, 2.0f), GlassDark, false);
            Shapes.Box(t, "VentanasD", new Vector3(w * 0.44f + 0.005f, 1.0f, -0.2f), new Vector3(0.02f, 0.38f, 2.0f), GlassDark, false);
            Shapes.Box(t, "ParagolpesD", new Vector3(0, 0.38f, 2.38f), new Vector3(w + 0.05f, 0.14f, 0.1f), Chrome, false);
            Shapes.Box(t, "ParagolpesT", new Vector3(0, 0.38f, -2.38f), new Vector3(w + 0.05f, 0.14f, 0.1f), Chrome, false);
            Shapes.Box(t, "Parrilla", new Vector3(0, 0.55f, 2.36f), new Vector3(w * 0.8f, 0.25f, 0.04f), Chrome, false);
            Lights(t, 2.37f, 0.68f, 0.65f);
            Shapes.Box(t, "LuzTraseraI", new Vector3(-w / 2 + 0.15f, 0.7f, -2.36f), new Vector3(0.25f, 0.12f, 0.03f), new Color(0.8f, 0.1f, 0.1f), false);
            Shapes.Box(t, "LuzTraseraD", new Vector3(w / 2 - 0.15f, 0.7f, -2.36f), new Vector3(0.25f, 0.12f, 0.03f), new Color(0.8f, 0.1f, 0.1f), false);
        }
    }
}
