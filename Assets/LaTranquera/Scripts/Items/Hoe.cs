using System.Collections;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>Azada: carpir el cantero (4 golpes) y sacar plantas secas.</summary>
    public class Hoe : Tool
    {
        public void Setup()
        {
            kind = "hoe";
            displayName = "Azada";
            mass = 1.5f;
            holdOffset = new Vector3(0.35f, -0.55f, 0.6f);
            holdEuler = new Vector3(55, 0, 0);
            model = Shapes.Empty(transform, "Modelo", Vector3.zero).transform;
            Shapes.Cyl(model, "Cabo", Vector3.zero, 0.035f, 1.3f, Mats.MaderaClara, false);
            Shapes.BoxC(model, "Hoja", new Vector3(0, 1.3f, 0.09f), new Vector3(0.18f, 0.12f, 0.01f), new Color(0.35f, 0.35f, 0.38f), false, new Vector3(70, 0, 0));
            var bc = gameObject.AddComponent<BoxCollider>();
            bc.center = new Vector3(0, 0.68f, 0.05f);
            bc.size = new Vector3(0.2f, 1.4f, 0.2f);
        }

        public override string GetUsePrompt(UseContext c)
        {
            var bed = c.Get<GardenBed>();
            if (bed == null || bed.locked) return null;
            var plant = c.Get<Plant>();
            if (plant != null && plant.dead) return "[Clic] Sacar planta seca";
            if (!bed.tilled) return "[Clic] Carpir el cantero (" + Mathf.RoundToInt(bed.tillProgress * 100) + "%)";
            return null;
        }

        public override void Use(UseContext c)
        {
            var bed = c.Get<GardenBed>();
            if (bed == null || bed.locked) return;
            StartCoroutine(Swing());
            var plant = c.Get<Plant>();
            if (plant != null && plant.dead) { plant.Clear(); Sfx.Play("dig"); return; }
            if (!bed.tilled) { bed.Till(0.25f); Sfx.Play("dig"); }
            else HUD.Toast("El cantero ya está carpido. Ahora sembrá.");
        }
    }
}
