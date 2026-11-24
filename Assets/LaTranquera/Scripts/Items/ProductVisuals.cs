using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Dibuja cada producto con formas simples (sin colliders). La base del objeto queda en y = 0.
    /// Para usar un modelo 3D descargado: poné un prefab en Assets/LaTranquera/Resources/Productos/
    /// con el id del producto como nombre (por ejemplo "tomate.prefab") y se usa ese en vez del dibujo.
    /// </summary>
    public static class ProductVisuals
    {
        public static GameObject Build(string productId, Transform parent)
        {
            var def = ProductDB.Get(productId);
            var root = new GameObject("Vis_" + productId);
            root.transform.SetParent(parent, false);
            if (def == null) return root;

            var prefab = Resources.Load<GameObject>("Productos/" + productId);
            if (prefab != null)
            {
                var inst = Object.Instantiate(prefab, root.transform);
                Shapes.RemoveAllColliders(inst);
                return root;
            }

            var t = root.transform;
            var s = def.size;
            switch (def.shape)
            {
                case ProductShape.Lechuga:
                {
                    var g1 = new Color(0.45f, 0.75f, 0.28f);
                    var g2 = new Color(0.62f, 0.85f, 0.38f);
                    for (int i = 0; i < 7; i++)
                    {
                        float a = i * 360f / 7f;
                        var dir = Quaternion.Euler(0, a, 0) * Vector3.forward;
                        var leaf = Shapes.Sphere(t, "Hoja", dir * 0.06f + Vector3.up * 0.07f, new Vector3(0.12f, 0.1f, 0.06f), g1);
                        leaf.transform.localRotation = Quaternion.LookRotation(dir) * Quaternion.Euler(-35, 0, 0);
                    }
                    Shapes.Sphere(t, "Cogollo", Vector3.up * 0.08f, new Vector3(0.12f, 0.12f, 0.12f), g2);
                    break;
                }
                case ProductShape.Tomate:
                    Shapes.Sphere(t, "Tomate", Vector3.up * 0.038f, new Vector3(0.08f, 0.07f, 0.08f), new Color(0.85f, 0.12f, 0.08f));
                    Shapes.Cyl(t, "Cabito", Vector3.up * 0.07f, 0.035f, 0.01f, new Color(0.2f, 0.5f, 0.15f), false);
                    break;
                case ProductShape.Zanahoria:
                {
                    var orange = new Color(0.95f, 0.5f, 0.1f);
                    for (int i = 0; i < 3; i++)
                    {
                        float z = (i - 1) * 0.03f;
                        Shapes.Cone(t, "Zanahoria", new Vector3(-0.08f, 0.03f, z), 0.035f, 0.17f, orange, new Vector3(0, 0, -90));
                        for (int k = 0; k < 3; k++)
                            Shapes.CylC(t, "Hojita", new Vector3(-0.12f, 0.035f + k * 0.008f, z + (k - 1) * 0.01f), 0.008f, 0.09f,
                                new Color(0.25f, 0.6f, 0.2f), new Vector3(0, (k - 1) * 20, 80));
                    }
                    Shapes.CylC(t, "Atadura", new Vector3(-0.085f, 0.03f, 0), 0.012f, 0.1f, new Color(0.9f, 0.85f, 0.6f), new Vector3(90, 0, 0));
                    break;
                }
                case ProductShape.Yerba:
                    Shapes.Box(t, "Paquete", Vector3.zero, s, new Color(0.95f, 0.8f, 0.15f), false);
                    Shapes.Box(t, "Banda", new Vector3(0, s.y * 0.35f, 0), new Vector3(s.x * 1.01f, s.y * 0.3f, s.z * 1.01f), new Color(0.15f, 0.45f, 0.18f), false);
                    break;
                case ProductShape.Fideos:
                    Shapes.Box(t, "Paquete", Vector3.zero, s, new Color(0.15f, 0.3f, 0.75f), false);
                    Shapes.Box(t, "Ventana", new Vector3(0, s.y * 0.3f, 0), new Vector3(s.x * 0.6f, s.y * 0.35f, s.z * 1.02f), new Color(0.95f, 0.85f, 0.4f), false);
                    break;
                case ProductShape.Harina:
                    Shapes.Box(t, "Bolsa", Vector3.zero, s, new Color(0.96f, 0.95f, 0.9f), false);
                    Shapes.Box(t, "Banda", new Vector3(0, s.y * 0.55f, 0), new Vector3(s.x * 1.01f, s.y * 0.2f, s.z * 1.01f), new Color(0.8f, 0.15f, 0.15f), false);
                    break;
                case ProductShape.Aceite:
                    Shapes.Cyl(t, "Botella", Vector3.zero, s.x, s.y * 0.78f, new Color(0.95f, 0.8f, 0.2f), false);
                    Shapes.Cyl(t, "Cuello", Vector3.up * s.y * 0.78f, s.x * 0.45f, s.y * 0.15f, new Color(0.95f, 0.85f, 0.4f), false);
                    Shapes.Cyl(t, "Tapa", Vector3.up * s.y * 0.93f, s.x * 0.5f, s.y * 0.07f, new Color(0.1f, 0.5f, 0.2f), false);
                    Shapes.Cyl(t, "Etiqueta", Vector3.up * s.y * 0.25f, s.x * 1.02f, s.y * 0.25f, new Color(0.1f, 0.45f, 0.2f), false);
                    break;
                case ProductShape.Leche:
                    // Botella de vidrio con leche y tapa azul
                    Shapes.Cyl(t, "Botella", Vector3.zero, s.x, s.y * 0.7f, new Color(0.97f, 0.97f, 0.95f), false);
                    Shapes.Cyl(t, "Hombro", Vector3.up * s.y * 0.7f, s.x * 0.7f, s.y * 0.12f, new Color(0.94f, 0.95f, 0.95f), false);
                    Shapes.Cyl(t, "Cuello", Vector3.up * s.y * 0.82f, s.x * 0.45f, s.y * 0.12f, new Color(0.85f, 0.92f, 0.95f), false);
                    Shapes.Cyl(t, "Tapa", Vector3.up * s.y * 0.94f, s.x * 0.5f, s.y * 0.06f, new Color(0.15f, 0.35f, 0.8f), false);
                    break;
                case ProductShape.Queso:
                    Shapes.Cyl(t, "Horma", Vector3.zero, s.x, s.y, new Color(0.98f, 0.9f, 0.6f), false);
                    Shapes.Cyl(t, "Film", Vector3.up * s.y * 0.95f, s.x * 1.01f, s.y * 0.06f, new Color(0.95f, 0.75f, 0.3f), false);
                    break;
                case ProductShape.Manteca:
                    Shapes.Box(t, "Pan", Vector3.zero, s, new Color(0.95f, 0.85f, 0.35f), false);
                    Shapes.Box(t, "Banda", new Vector3(0, 0, 0), new Vector3(s.x * 0.3f, s.y * 1.02f, s.z * 1.02f), new Color(0.2f, 0.35f, 0.75f), false);
                    break;
                case ProductShape.Cebolla:
                    Shapes.Sphere(t, "Bulbo", Vector3.up * 0.04f, new Vector3(0.08f, 0.075f, 0.08f), new Color(0.78f, 0.55f, 0.3f));
                    Shapes.Cone(t, "Cuello", Vector3.up * 0.07f, 0.03f, 0.04f, new Color(0.7f, 0.5f, 0.28f));
                    break;
                case ProductShape.Papa:
                {
                    var g = Shapes.Sphere(t, "Papa", Vector3.up * 0.03f, new Vector3(0.1f, 0.06f, 0.07f), new Color(0.72f, 0.58f, 0.38f));
                    g.transform.localRotation = Quaternion.Euler(0, 20, 8);
                    Shapes.Sphere(t, "Bulto", new Vector3(0.025f, 0.035f, 0.01f), new Vector3(0.05f, 0.045f, 0.05f), new Color(0.7f, 0.56f, 0.36f));
                    break;
                }
                case ProductShape.Choclo:
                {
                    var c = Shapes.CylC(t, "Granos", new Vector3(0, 0.035f, 0), 0.055f, 0.2f, new Color(0.98f, 0.85f, 0.25f), new Vector3(0, 0, 90));
                    Shapes.CylC(t, "Chala", new Vector3(-0.06f, 0.035f, 0.012f), 0.06f, 0.16f, new Color(0.55f, 0.72f, 0.35f), new Vector3(0, 8, 90));
                    Shapes.CylC(t, "Chala2", new Vector3(-0.07f, 0.03f, -0.012f), 0.058f, 0.15f, new Color(0.62f, 0.78f, 0.4f), new Vector3(0, -10, 90));
                    break;
                }
                case ProductShape.Zapallo:
                {
                    // zapallo anco: forma de pera acostada
                    var tan = new Color(0.88f, 0.68f, 0.38f);
                    Shapes.Sphere(t, "Panza", new Vector3(-0.06f, 0.07f, 0), new Vector3(0.16f, 0.14f, 0.15f), tan);
                    Shapes.CylC(t, "Cuello", new Vector3(0.06f, 0.06f, 0), 0.09f, 0.16f, tan, new Vector3(0, 0, 90));
                    Shapes.Sphere(t, "Punta", new Vector3(0.14f, 0.06f, 0), new Vector3(0.09f, 0.09f, 0.09f), tan);
                    break;
                }
                case ProductShape.Sandia:
                {
                    var sph = Shapes.Prim(PrimitiveType.Sphere, t, "Sandia", Vector3.up * 0.12f, new Vector3(0.24f, 0.36f, 0.24f), TexKit.Mat("sandia", Color.white, 0.4f), false);
                    sph.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    break;
                }
                case ProductShape.Pan:
                    Shapes.Box(t, "Miga", Vector3.zero, new Vector3(s.x, s.y * 0.6f, s.z), new Color(0.85f, 0.62f, 0.32f), false);
                    Shapes.Sphere(t, "Corteza", new Vector3(0, s.y * 0.6f, 0), new Vector3(s.x, s.y * 0.8f, s.z), new Color(0.78f, 0.52f, 0.25f));
                    break;
                case ProductShape.Frutilla:
                    Shapes.Box(t, "Bandeja", Vector3.zero, new Vector3(0.17f, 0.035f, 0.12f), new Color(0.95f, 0.95f, 0.92f), false);
                    for (int i = 0; i < 6; i++)
                        Shapes.Sphere(t, "Frutilla", new Vector3((i % 3 - 1) * 0.045f, 0.045f, (i / 3 - 0.5f) * 0.05f), new Vector3(0.04f, 0.045f, 0.04f), new Color(0.85f, 0.08f, 0.1f));
                    break;
            }
            return root;
        }

        // ---------- Otros objetos (también se usan para las imágenes del catálogo) ----------

        public static GameObject SeedPacket(string cropId, Transform parent)
        {
            var root = new GameObject("Vis_semillas_" + cropId);
            root.transform.SetParent(parent, false);
            Color c;
            switch (cropId)
            {
                case "tomate": c = new Color(0.85f, 0.2f, 0.15f); break;
                case "zanahoria": c = new Color(0.95f, 0.55f, 0.15f); break;
                case "cebolla": c = new Color(0.75f, 0.5f, 0.3f); break;
                case "papa": c = new Color(0.65f, 0.5f, 0.3f); break;
                case "maiz": c = new Color(0.95f, 0.82f, 0.2f); break;
                case "zapallo": c = new Color(0.88f, 0.65f, 0.3f); break;
                case "sandia": c = new Color(0.2f, 0.5f, 0.2f); break;
                case "frutilla": c = new Color(0.9f, 0.1f, 0.2f); break;
                default: c = new Color(0.4f, 0.75f, 0.3f); break;
            }
            Shapes.Box(root.transform, "Sobre", Vector3.zero, new Vector3(0.1f, 0.14f, 0.012f), new Color(0.95f, 0.93f, 0.85f), false);
            Shapes.Box(root.transform, "Dibujo", new Vector3(0, 0.035f, 0), new Vector3(0.08f, 0.08f, 0.014f), c, false);
            return root;
        }

        public static GameObject Crate(Transform parent)
        {
            var root = new GameObject("Vis_cajon");
            root.transform.SetParent(parent, false);
            var t = root.transform;
            Color w = Mats.MaderaClara, w2 = Mats.Madera;
            Vector3 size = new Vector3(0.5f, 0.28f, 0.35f);
            Shapes.Box(t, "Fondo", Vector3.zero, new Vector3(size.x, 0.02f, size.z), w2, false);
            for (int i = 0; i < 3; i++)
            {
                float y = 0.03f + i * 0.085f;
                Shapes.Box(t, "TablaF", new Vector3(0, y, size.z / 2), new Vector3(size.x, 0.06f, 0.015f), w, false);
                Shapes.Box(t, "TablaB", new Vector3(0, y, -size.z / 2), new Vector3(size.x, 0.06f, 0.015f), w, false);
            }
            Shapes.Box(t, "Cabecera1", new Vector3(size.x / 2, 0, 0), new Vector3(0.02f, size.y, size.z), w2, false);
            Shapes.Box(t, "Cabecera2", new Vector3(-size.x / 2, 0, 0), new Vector3(0.02f, size.y, size.z), w2, false);
            return root;
        }

        public static GameObject CardboardBox(Transform parent, bool open)
        {
            var root = new GameObject("Vis_caja");
            root.transform.SetParent(parent, false);
            var t = root.transform;
            Color k = new Color(0.72f, 0.55f, 0.35f);
            Vector3 size = new Vector3(0.45f, 0.3f, 0.35f);
            Shapes.Box(t, "Fondo", Vector3.zero, new Vector3(size.x, 0.01f, size.z), k, false);
            Shapes.Box(t, "LadoF", new Vector3(0, 0, size.z / 2), new Vector3(size.x, size.y, 0.01f), k, false);
            Shapes.Box(t, "LadoB", new Vector3(0, 0, -size.z / 2), new Vector3(size.x, size.y, 0.01f), k, false);
            Shapes.Box(t, "LadoI", new Vector3(-size.x / 2, 0, 0), new Vector3(0.01f, size.y, size.z), k, false);
            Shapes.Box(t, "LadoD", new Vector3(size.x / 2, 0, 0), new Vector3(0.01f, size.y, size.z), k, false);
            if (!open)
            {
                Shapes.Box(t, "Tapa", new Vector3(0, size.y, 0), new Vector3(size.x, 0.01f, size.z), new Color(0.68f, 0.5f, 0.3f), false);
                Shapes.Box(t, "Cinta", new Vector3(0, size.y + 0.003f, 0), new Vector3(0.06f, 0.01f, size.z + 0.01f), new Color(0.85f, 0.8f, 0.6f), false);
            }
            else
            {
                Shapes.BoxC(t, "Solapa1", new Vector3(0, size.y + 0.08f, size.z / 2 + 0.06f), new Vector3(size.x, 0.01f, 0.17f), k, false, new Vector3(-50, 0, 0));
                Shapes.BoxC(t, "Solapa2", new Vector3(0, size.y + 0.08f, -size.z / 2 - 0.06f), new Vector3(size.x, 0.01f, 0.17f), k, false, new Vector3(50, 0, 0));
            }
            return root;
        }
    }
}
