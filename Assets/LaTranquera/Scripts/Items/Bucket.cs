using System.Collections;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>Balde de chapa: se llena en el pozo y se vuelca en la regadera (en la fase 2, en el bebedero).</summary>
    public class Bucket : Tool, IWaterHolder
    {
        public float liters;
        public float capacity = 10f;
        Transform waterLevel;

        public float Liters { get => liters; set { liters = Mathf.Clamp(value, 0, capacity); Refresh(); } }
        public float Capacity => capacity;
        public string Name => Loc.T(displayName);

        public void Setup()
        {
            kind = "bucket";
            displayName = "Balde";
            mass = 1f;
            holdOffset = new Vector3(0.3f, -0.55f, 0.55f);
            model = Shapes.Empty(transform, "Modelo", Vector3.zero).transform;
            Shapes.Cyl(model, "Balde", Vector3.zero, 0.28f, 0.3f, Mats.Chapa, false);
            Shapes.CylC(model, "Asa", new Vector3(0, 0.36f, 0), 0.012f, 0.28f, new Color(0.4f, 0.4f, 0.4f), new Vector3(0, 0, 90));
            waterLevel = Shapes.Cyl(model, "Agua", new Vector3(0, 0.01f, 0), 0.26f, 0.27f, new Color(0.3f, 0.5f, 0.8f), false).transform;
            var bc = gameObject.AddComponent<BoxCollider>();
            bc.center = new Vector3(0, 0.18f, 0);
            bc.size = new Vector3(0.3f, 0.38f, 0.3f);
            Refresh();
        }

        void Refresh()
        {
            if (waterLevel == null) return;
            float k = liters / capacity;
            waterLevel.gameObject.SetActive(k > 0.01f);
            waterLevel.localScale = new Vector3(0.26f, 0.14f * k, 0.26f);
            waterLevel.localPosition = new Vector3(0, 0.28f * k * 0.5f + 0.01f, 0);
        }

        public override string GetName(UseContext c) => Loc.T(displayName) + " (" + liters.ToString("0.#") + " L)";

        public override string GetUsePrompt(UseContext c)
        {
            if (c.Get<WaterSource>() != null) return "[Clic] Llenar el balde";
            var can = c.Get<WateringCan>();
            if (can != null && liters > 0) return "[Clic] Volcar el agua en la regadera";
            return null;
        }

        public override void Use(UseContext c)
        {
            var src = c.Get<WaterSource>();
            if (src != null) { src.Fill(this); return; }
            var can = c.Get<WateringCan>();
            if (can != null && liters > 0)
            {
                float move = Mathf.Min(liters, can.Capacity - can.Liters);
                can.Liters += move;
                Liters -= move;
                Sfx.Play("water", 0.4f);
            }
        }

        public override string SaveExtra() => liters.ToString(System.Globalization.CultureInfo.InvariantCulture);
        public override void LoadExtra(string data)
        {
            float.TryParse(data, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float l);
            Liters = l;
        }
    }
}
