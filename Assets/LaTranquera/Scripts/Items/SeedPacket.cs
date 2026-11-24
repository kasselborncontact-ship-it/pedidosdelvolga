using System.Collections;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>Sobre de semillas: clic sobre un lugar del cantero carpido para sembrar una.</summary>
    public class SeedPacket : Carryable
    {
        public string cropId;
        public int seeds;

        public void Setup(string crop, int n)
        {
            kind = "seeds";
            cropId = crop;
            seeds = n;
            displayName = "Semillas de " + (CropDB.Get(crop)?.name.ToLower() ?? crop);
            mass = 0.05f;
            holdOffset = new Vector3(0.22f, -0.22f, 0.45f);
            holdEuler = new Vector3(0, 0, 0);
            Shapes.DestroyChildren(transform);
            ProductVisuals.SeedPacket(crop, transform);
            FitCollider();
        }

        public override string GetName(UseContext c) => Loc.T(displayName) + " (quedan " + seeds + ")";

        public override string GetUsePrompt(UseContext c)
        {
            var plant = c.Get<Plant>();
            var bed = c.Get<GardenBed>();
            if (bed == null || bed.locked) return null;
            if (!bed.tilled) return "Primero carpí el cantero con la azada";
            if (plant != null && plant.Empty) return "[Clic] Sembrar " + CropDB.Get(cropId).name.ToLower() + " acá";
            if (plant == null && bed.FirstEmpty() != null) return "[Clic] Sembrar " + CropDB.Get(cropId).name.ToLower();
            return null;
        }

        public override void Use(UseContext c)
        {
            var bed = c.Get<GardenBed>();
            if (bed == null || bed.locked || !bed.tilled) return;
            var plant = c.Get<Plant>();
            if (plant == null || !plant.Empty) plant = bed.NearestEmpty(c.hit.point);
            if (plant == null) { HUD.Toast("No queda lugar en este cantero"); return; }
            plant.Sow(cropId);
            seeds--;
            Sfx.Play("dig", 0.3f);
            if (seeds <= 0)
            {
                HUD.Toast("Se terminó el sobre de semillas");
                c.hands.ConsumeHeld();
            }
        }

        public override string SaveExtra() => cropId + "|" + seeds;
        public override void LoadExtra(string data)
        {
            var p = data.Split('|');
            if (p.Length < 2) return;
            int.TryParse(p[1], out int n);
            Setup(p[0], n);
        }
    }
}
