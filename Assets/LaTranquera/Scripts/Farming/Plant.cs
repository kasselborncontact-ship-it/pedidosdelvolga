using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Un lugar del cantero donde puede haber una planta.
    /// 4 etapas visuales: brote (0), plantín (1), creciendo (2), lista para cosechar (3).
    /// Solo crece las horas en que el cantero tiene humedad.
    /// </summary>
    public class Plant : MonoBehaviour, IInteractable
    {
        public GardenBed bed;
        public string cropId;
        public float growth;      // horas de crecimiento acumuladas
        public float dryHours;    // horas seguidas sin agua
        public bool wilted;       // marchita (no crece)
        public bool dead;         // seca (hay que sacarla)

        /// <summary>Horas sin agua para marchitarse y para morirse.</summary>
        public const float WiltAfter = 12f, DieAfter = 48f;

        Transform visual;
        int shownStage = -2;
        bool shownWilted, shownDead;

        public bool Empty => string.IsNullOrEmpty(cropId);
        public CropDef Crop => CropDB.Get(cropId);

        public int Stage
        {
            get
            {
                if (Empty) return -1;
                float k = growth / Crop.growHours;
                if (k >= 1f) return 3;
                if (k >= 0.6f) return 2;
                if (k >= 0.25f) return 1;
                return 0;
            }
        }

        public bool Ripe => !Empty && !dead && Stage == 3;

        void Awake()
        {
            if (bed == null) bed = GetComponentInParent<GardenBed>();
        }

        void Start() => Refresh();

        public void Sow(string crop)
        {
            cropId = crop;
            growth = 0; dryHours = 0; wilted = false; dead = false;
            Refresh();
        }

        public void Clear()
        {
            cropId = null;
            growth = 0; dryHours = 0; wilted = false; dead = false;
            Refresh();
            if (bed != null) bed.OnPlantHarvested();
        }

        public void Harvest() => Clear();

        public void OnWatered()
        {
            if (Empty || dead) return;
            dryHours = 0;
            if (wilted) { wilted = false; Refresh(); }
        }

        public void TickHour(bool hasWater)
        {
            if (Empty || dead) return;
            if (hasWater)
            {
                dryHours = 0;
                if (!wilted && growth < Crop.growHours) growth = Mathf.Min(Crop.growHours, growth + 1f);
            }
            else
            {
                dryHours += 1f;
                if (dryHours >= DieAfter) dead = true;
                else if (dryHours >= WiltAfter) wilted = true;
            }
            Refresh();
        }

        /// <summary>Redibuja solo si cambió la etapa o el estado.</summary>
        public void Refresh()
        {
            int st = Stage;
            if (st == shownStage && wilted == shownWilted && dead == shownDead && visual != null) return;
            shownStage = st; shownWilted = wilted; shownDead = dead;
            if (visual != null) Destroy(visual.gameObject);
            visual = null;
            if (Empty) return;
            visual = PlantVisuals.Build(cropId, st, wilted || dead, dead, transform).transform;
        }

        // --- IInteractable ---
        public string GetName(UseContext c)
        {
            if (Empty) return bed != null ? bed.GetName(c) : "";
            string n = Loc.T(Crop.name);
            if (dead) return n + " — " + Loc.T("seca (sacala con la azada o con E)");
            if (wilted) return n + " — " + Loc.T("¡marchita! Regala ya");
            if (Ripe) return n + " — " + Loc.T("lista para cosechar");
            int pct = Mathf.RoundToInt(growth / Crop.growHours * 100);
            return n + " — " + Loc.T("creciendo") + " " + pct + "% · " + Loc.T("humedad") + " " + Mathf.RoundToInt(bed.water * 100) + "%";
        }

        public string GetPrompt(UseContext c)
        {
            if (Empty) return bed != null ? bed.GetPrompt(c) : null;
            if (dead && c.hands.IsEmpty) return "[E] Arrancar la planta seca";
            if (Ripe && c.hands.IsEmpty) return "Traé un cajón para cosechar";
            return null;
        }

        public void Interact(UseContext c)
        {
            if (!Empty && dead && c.hands.IsEmpty) Clear();
        }
    }
}
