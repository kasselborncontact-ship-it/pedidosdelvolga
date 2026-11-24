using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Fuego estilizado: llamas (conos que titilan) y una luz anaranjada que parpadea.
    /// Se usa en la cocina a leña. Se prende y apaga activando/desactivando este objeto.
    /// </summary>
    public class Fire : MonoBehaviour
    {
        public Light fireLight;
        public float baseIntensity = 1.6f;
        Transform[] flames;
        Vector3[] baseScale;
        float seed;

        void Awake()
        {
            seed = Random.value * 100f;
            var list = new System.Collections.Generic.List<Transform>();
            foreach (Transform t in transform) if (t.name.StartsWith("Llama")) list.Add(t);
            flames = list.ToArray();
            baseScale = new Vector3[flames.Length];
            for (int i = 0; i < flames.Length; i++) baseScale[i] = flames[i].localScale;
        }

        void OnEnable() => Sfx.Play("dig", 0.3f);

        void Update()
        {
            float t = Time.time * 9f + seed;
            for (int i = 0; i < flames.Length; i++)
            {
                float k = 0.75f + 0.35f * Mathf.PerlinNoise(t + i * 3.1f, i * 1.7f);
                flames[i].localScale = new Vector3(baseScale[i].x * (1.1f - k * 0.2f), baseScale[i].y * k, baseScale[i].z * (1.1f - k * 0.2f));
                flames[i].localRotation = Quaternion.Euler(Mathf.Sin(t * 0.7f + i) * 6f, i * 47f, Mathf.Cos(t * 0.9f + i) * 6f);
            }
            if (fireLight != null) fireLight.intensity = baseIntensity * (0.75f + 0.5f * Mathf.PerlinNoise(t * 0.8f, 3.3f));
        }

        /// <summary>Arma las llamas, los leños y la luz adentro de "parent".</summary>
        public static Fire Build(Transform parent, Vector3 localPos, float size = 1f)
        {
            var go = Shapes.Empty(parent, "Fuego", localPos);
            var t = go.transform;
            for (int i = 0; i < 3; i++)
                Shapes.CylC(t, "Leño", new Vector3(0, 0.03f * size, (i - 1) * 0.06f * size), 0.06f * size, 0.32f * size, new Color(0.25f, 0.16f, 0.1f), new Vector3(0, i * 25f, 90));
            Shapes.Sphere(t, "Brasas", new Vector3(0, 0.03f * size, 0), new Vector3(0.28f, 0.05f, 0.2f) * size, new Color(1f, 0.35f, 0.05f));
            var orange = Mats.Emissive(new Color(1f, 0.45f, 0.08f), 3f);
            var yellow = Mats.Emissive(new Color(1f, 0.85f, 0.3f), 3f);
            for (int i = 0; i < 6; i++)
            {
                var c = Shapes.Empty(t, "Llama" + i, new Vector3(((i % 3) - 1) * 0.07f * size, 0.04f * size, (i / 3 - 0.5f) * 0.07f * size));
                var mf = c.AddComponent<MeshFilter>(); mf.sharedMesh = Meshes.Cone;
                c.AddComponent<MeshRenderer>().sharedMaterial = i % 2 == 0 ? orange : yellow;
                c.transform.localScale = new Vector3(0.09f, 0.22f + (i % 3) * 0.06f, 0.09f) * size;
            }
            var l = Shapes.Empty(t, "LuzFuego", new Vector3(0, 0.25f * size, 0)).AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.55f, 0.2f);
            l.range = 4.5f * size;
            l.intensity = 1.6f;
            l.shadows = LightShadows.None;
            var f = go.AddComponent<Fire>();
            f.fireLight = l;
            return f;
        }
    }
}
