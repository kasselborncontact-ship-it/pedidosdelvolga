using System.Collections.Generic;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Mueve el sol según la hora, cambia el color de la luz y del cielo,
    /// y prende las luces de noche (postes de la ruta, almacén, casa).
    /// Va en la "Directional Light" (el sol).
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class DayNightCycle : MonoBehaviour
    {
        public Gradient sunColor;
        public Color ambientDay = new Color(0.62f, 0.66f, 0.72f);
        public Color ambientNight = new Color(0.07f, 0.08f, 0.14f);
        public Color fogDay = new Color(0.72f, 0.8f, 0.88f);
        public Color fogNight = new Color(0.04f, 0.05f, 0.09f);
        public float maxIntensity = 1.3f;
        /// <summary>Luces que se prenden cuando oscurece.</summary>
        public List<Light> nightLights = new List<Light>();

        Light sun;
        bool lightsOn, lightsInit;

        void Awake()
        {
            sun = GetComponent<Light>();
            if (sunColor == null || sunColor.colorKeys.Length < 2)
            {
                sunColor = new Gradient();
                sunColor.SetKeys(
                    new[] { new GradientColorKey(new Color(1f, 0.45f, 0.25f), 0f), new GradientColorKey(new Color(1f, 0.85f, 0.7f), 0.25f), new GradientColorKey(new Color(1f, 0.97f, 0.92f), 1f) },
                    new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) });
            }
            RenderSettings.sun = sun;
            if (RenderSettings.skybox != null) RenderSettings.skybox = new Material(RenderSettings.skybox); // copia para no tocar el archivo
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 120f;
            RenderSettings.fogEndDistance = 650f;
        }

        void Update()
        {
            if (TimeManager.I == null) return;
            float h = TimeManager.I.Hour;
            // 6:00 sale el sol por el este, 12:00 arriba, 19:30 se pone
            float dayK = Mathf.InverseLerp(6f, 19.5f, h);
            float elevation = Mathf.Sin(dayK * Mathf.PI) * 70f;
            bool isDay = h > 6f && h < 19.5f;
            if (!isDay) elevation = -15f;
            float azimuth = Mathf.Lerp(-80f, 260f, dayK);
            transform.rotation = Quaternion.Euler(elevation, azimuth, 0);

            float sunK = Mathf.Clamp01(elevation / 25f);
            sun.intensity = isDay ? Mathf.Lerp(0.05f, maxIntensity, sunK) : 0.04f;
            sun.color = sunColor.Evaluate(sunK);
            sun.shadows = isDay ? LightShadows.Soft : LightShadows.None;
            RenderSettings.ambientLight = Color.Lerp(ambientNight, ambientDay, isDay ? Mathf.Clamp01(elevation / 15f + 0.15f) : 0);
            RenderSettings.fogColor = Color.Lerp(fogNight, fogDay, isDay ? Mathf.Clamp01(elevation / 15f + 0.1f) : 0);

            if (RenderSettings.skybox != null && RenderSettings.skybox.HasProperty("_Exposure"))
                RenderSettings.skybox.SetFloat("_Exposure", isDay ? Mathf.Lerp(0.35f, 1.2f, Mathf.Clamp01(elevation / 20f)) : 0.12f);

            bool wantLights = h >= 19f || h < 6.5f;
            if (wantLights != lightsOn || !lightsInit)
            {
                lightsOn = wantLights;
                lightsInit = true;
                foreach (var l in nightLights) if (l != null) l.enabled = lightsOn;
            }
        }
    }
}
