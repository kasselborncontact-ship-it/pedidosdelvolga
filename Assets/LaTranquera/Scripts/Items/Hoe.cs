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
            var patch = c.Get<SoilPatch>();
            if (patch != null)
            {
                if (!patch.Empty && patch.dead) return "[Clic] Sacar la planta seca";
                if (patch.Empty && !patch.Tilled) return "[Clic] Carpir (" + Mathf.RoundToInt(patch.till * 100) + "%)";
                return null;
            }
            if (!c.hasHit || Vector3.Dot(c.hit.normal, Vector3.up) < 0.7f) return null;
            var lot = LandPlot.At(c.hit.point);
            if (lot == null) return null;
            if (!lot.owned) return lot.displayName + ": " + Loc.T("no es tuyo (se compra en la computadora)");
            if (!LandPlot.CanFarm(c.hit.point)) return null;
            return SoilPatch.FreeAt(c.hit.point, SoilPatch.MinDistance) ? "[Clic] Carpir acá" : "Muy pegado a otro surco";
        }

        public override void Use(UseContext c)
        {
            var patch = c.Get<SoilPatch>();
            if (patch != null)
            {
                StartCoroutine(Swing());
                if (!patch.Empty && patch.dead) { patch.Clear(); Sfx.Play("dig"); }
                else if (patch.Empty && !patch.Tilled) { patch.Till(0.5f); Sfx.Play("dig"); }
                return;
            }
            if (!c.hasHit || Vector3.Dot(c.hit.normal, Vector3.up) < 0.7f) return;
            var p = c.hit.point;
            if (!LandPlot.CanFarm(p))
            {
                var lot = LandPlot.At(p);
                if (lot != null && !lot.owned) HUD.Toast(lot.displayName + " no es tuyo todavía. Se compra en la computadora (Mejoras).");
                return;
            }
            if (!SoilPatch.FreeAt(p, SoilPatch.MinDistance)) { HUD.Toast("Muy pegado a otro surco"); return; }
            StartCoroutine(Swing());
            var np = SoilPatch.Create(p);
            np.till = 0.5f;
            Sfx.Play("dig");
        }
    }
}
