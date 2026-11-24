using System.Collections;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>Regadera: se llena en el pozo y riega un cantero entero (2,5 L por pasada).</summary>
    public class WateringCan : Tool, IWaterHolder
    {
        public float liters;
        public float capacity = 10f;
        public const float PerPlant = 1f;
        public const float Radius = 1.0f;
        Transform waterLevel;

        public float Liters { get => liters; set { liters = Mathf.Clamp(value, 0, capacity); RefreshWater(); } }
        public float Capacity => capacity;
        public string Name => Loc.T(displayName);

        public void Setup()
        {
            kind = "can";
            displayName = "Regadera";
            mass = 1f;
            holdOffset = new Vector3(0.3f, -0.4f, 0.6f);
            model = Shapes.Empty(transform, "Modelo", Vector3.zero).transform;
            var green = new Color(0.2f, 0.55f, 0.3f);
            Shapes.Cyl(model, "Cuerpo", Vector3.zero, 0.24f, 0.28f, green, false);
            Shapes.CylC(model, "Pico", new Vector3(0, 0.22f, 0.2f), 0.03f, 0.35f, green, new Vector3(55, 0, 0));
            Shapes.Cyl(model, "Flor", new Vector3(0, 0.31f, 0.34f), 0.07f, 0.03f, new Color(0.25f, 0.6f, 0.35f), false);
            Shapes.BoxC(model, "Manija", new Vector3(0, 0.34f, -0.05f), new Vector3(0.025f, 0.025f, 0.22f), green, false);
            waterLevel = Shapes.Cyl(model, "Agua", new Vector3(0, 0.005f, 0), 0.2f, 0.26f, new Color(0.3f, 0.5f, 0.8f), false).transform;
            var bc = gameObject.AddComponent<BoxCollider>();
            bc.center = new Vector3(0, 0.17f, 0.08f);
            bc.size = new Vector3(0.26f, 0.36f, 0.5f);
            RefreshWater();
        }

        void RefreshWater()
        {
            if (waterLevel == null) return;
            float k = liters / capacity;
            waterLevel.gameObject.SetActive(k > 0.01f);
            waterLevel.localScale = new Vector3(0.2f, 0.13f * k, 0.2f);
            waterLevel.localPosition = new Vector3(0, 0.27f * k * 0.5f + 0.01f, 0);
        }

        public override string GetName(UseContext c) => Loc.T(displayName) + " (" + liters.ToString("0.#") + " L / " + capacity + " L)";

        static System.Collections.Generic.List<SoilPatch> Near(Vector3 p)
        {
            var list = new System.Collections.Generic.List<SoilPatch>();
            foreach (var s in SoilPatch.All)
            {
                var d = s.transform.position - p; d.y = 0;
                if (d.magnitude <= Radius) list.Add(s);
            }
            return list;
        }

        public override string GetUsePrompt(UseContext c)
        {
            if (c.Get<WaterSource>() != null) return liters < capacity ? "[Clic] Llenar la regadera" : "La regadera está llena";
            if (!c.hasHit) return null;
            var near = Near(c.hit.point);
            if (near.Count == 0) return null;
            if (liters < PerPlant) return "La regadera está vacía: llenala en el aljibe";
            float min = 1f;
            foreach (var s in near) min = Mathf.Min(min, s.water);
            return "[Clic] Regar " + near.Count + (near.Count == 1 ? " planta" : " plantas") + " (humedad " + Mathf.RoundToInt(min * 100) + "%)";
        }

        public override void Use(UseContext c)
        {
            var src = c.Get<WaterSource>();
            if (src != null) { src.Fill(this); return; }
            if (!c.hasHit) return;
            var near = Near(c.hit.point);
            if (near.Count == 0) return;
            if (liters < PerPlant) { HUD.Toast("La regadera está vacía"); return; }
            int n = 0;
            foreach (var s in near)
            {
                if (liters < PerPlant) break;
                s.AddWater(0.5f);
                Liters -= PerPlant;
                n++;
            }
            StartCoroutine(Swing(35f, 0.6f));
            Sfx.Play("water", 0.5f);
        }

        public override string SaveExtra() => liters.ToString(System.Globalization.CultureInfo.InvariantCulture);
        public override void LoadExtra(string data)
        {
            float.TryParse(data, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float l);
            Liters = l;
        }
    }
}
