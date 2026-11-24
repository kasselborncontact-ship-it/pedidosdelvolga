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
            var patch = c.Get<SoilPatch>();
            var crop = CropDB.Get(cropId);
            if (patch == null)
                return c.hasHit && LandPlot.CanFarm(c.hit.point) ? "Primero carpí con la azada donde quieras plantar" : null;
            if (!patch.Empty) return null;
            if (!patch.Tilled) return "Terminá de carpir con la azada";
            var near = patch.CrowdingNeighbor(crop);
            if (near != null) return Loc.T(crop.name) + " " + Loc.T("necesita más lugar") + " (" + crop.spacing.ToString("0.0") + " m)";
            return "[Clic] Sembrar " + crop.name.ToLower() + " acá";
        }

        public override void Use(UseContext c)
        {
            var patch = c.Get<SoilPatch>();
            if (patch == null || !patch.Empty || !patch.Tilled) return;
            var crop = CropDB.Get(cropId);
            if (patch.CrowdingNeighbor(crop) != null) { HUD.Toast(crop.name + " necesita " + crop.spacing.ToString("0.0") + " m libres alrededor"); Sfx.Play("error"); return; }
            patch.Sow(cropId);
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
