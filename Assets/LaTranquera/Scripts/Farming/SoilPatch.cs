using System.Collections.Generic;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Un pedacito de tierra carpida (60 x 60 cm) que el jugador hace DONDE QUIERA dentro de su terreno.
    /// Ahí se siembra UNA planta. Ciclo: azada (2 golpes) -> semilla -> regar -> esperar -> cosechar con cajón.
    /// 4 etapas visuales: brote, plantín, creciendo, lista. Solo crece las horas en que tiene humedad.
    /// </summary>
    public class SoilPatch : MonoBehaviour, IInteractable
    {
        [Range(0, 1)] public float till;   // 1 = carpido y listo
        [Range(0, 1)] public float water;  // humedad
        public string cropId;
        public float growth, dryHours;
        public bool wilted, dead;

        public const float Size = 0.6f;
        public const float MinDistance = 0.55f;
        public const float WiltAfter = 12f, DieAfter = 48f;
        public const float DryPerHour = 1f / 24f;

        public static readonly List<SoilPatch> All = new List<SoilPatch>();

        Renderer soil;
        Transform visual;
        int shownStage = -2;
        bool shownWilted, shownDead, shownWet, shownTilled;
        BoxCollider trigger;

        public bool Empty => string.IsNullOrEmpty(cropId);
        public bool Tilled => till >= 0.999f;
        public CropDef Crop => CropDB.Get(cropId);

        public int Stage
        {
            get
            {
                if (Empty) return -1;
                float k = growth / Crop.growHours;
                return k >= 1f ? 3 : k >= 0.6f ? 2 : k >= 0.25f ? 1 : 0;
            }
        }

        public bool Ripe => !Empty && !dead && Stage == 3;

        // ------------------------------------------------------------------
        public static SoilPatch Create(Vector3 pos)
        {
            var go = new GameObject("Surco");
            go.transform.position = new Vector3(pos.x, 0.01f, pos.z);
            go.transform.rotation = Quaternion.Euler(0, Random.Range(-8f, 8f), 0);
            return go.AddComponent<SoilPatch>();
        }

        /// <summary>¿Hay lugar para un surco nuevo acá?</summary>
        public static bool FreeAt(Vector3 p, float minDist)
        {
            foreach (var s in All)
            {
                var d = s.transform.position - p; d.y = 0;
                if (d.magnitude < minDist) return false;
            }
            return true;
        }

        /// <summary>Plantas vecinas que quedan demasiado cerca para este cultivo (ej.: la sandía necesita espacio).</summary>
        public SoilPatch CrowdingNeighbor(CropDef crop)
        {
            foreach (var s in All)
            {
                if (s == this || s.Empty) continue;
                var d = s.transform.position - transform.position; d.y = 0;
                float need = Mathf.Max(crop.spacing, s.Crop.spacing);
                if (d.magnitude < need) return s;
            }
            return null;
        }

        void Awake()
        {
            var mound = new GameObject("Tierra");
            mound.transform.SetParent(transform, false);
            mound.AddComponent<MeshFilter>().sharedMesh = MeshKit.Box(new Vector3(Size, 0.08f, Size));
            soil = mound.AddComponent<MeshRenderer>();
            trigger = gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0, 0.3f, 0);
            trigger.size = new Vector3(Size, 0.6f, Size);
        }

        void OnEnable() { All.Add(this); TimeManager.HourPassed += OnHour; }
        void OnDisable() { All.Remove(this); TimeManager.HourPassed -= OnHour; }
        void Start() => Refresh();

        public void Till(float amount)
        {
            till = Mathf.Clamp01(till + amount);
            Refresh();
        }

        public void AddWater(float amount)
        {
            water = Mathf.Clamp01(water + amount);
            if (!Empty && !dead) { dryHours = 0; wilted = false; }
            Refresh();
        }

        public void Sow(string crop)
        {
            cropId = crop;
            growth = 0; dryHours = 0; wilted = false; dead = false;
            Refresh();
        }

        /// <summary>Saca la planta. La tierra queda dura: hay que volver a pasar la azada una vez.</summary>
        public void Clear()
        {
            cropId = null;
            growth = 0; dryHours = 0; wilted = false; dead = false;
            till = Mathf.Min(till, 0.5f);
            Refresh();
        }

        public void Harvest() => Clear();

        void OnHour(int hour)
        {
            bool hasWater = water > 0f;
            if (!Empty && !dead)
            {
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
            }
            if (hasWater) water = Mathf.Max(0, water - DryPerHour * (Empty ? 0.5f : 1f));
            Refresh();
        }

        public void Refresh()
        {
            bool wet = water > 0.15f;
            if (wet != shownWet || Tilled != shownTilled || soil.sharedMaterial == null)
            {
                shownWet = wet; shownTilled = Tilled;
                Color tint = !Tilled ? new Color(0.85f, 0.78f, 0.7f) : wet ? new Color(0.55f, 0.48f, 0.44f) : Color.white;
                soil.sharedMaterial = TexKit.Mat("tierraArada", tint);
                soil.transform.localScale = new Vector3(1, Tilled ? 1f : 0.5f, 1);
            }

            int st = Stage;
            if (st == shownStage && wilted == shownWilted && dead == shownDead && (visual != null || Empty)) return;
            shownStage = st; shownWilted = wilted; shownDead = dead;
            if (visual != null) Destroy(visual.gameObject);
            visual = null;
            if (Empty) return;
            visual = PlantVisuals.Build(cropId, st, wilted || dead, dead, transform).transform;
            visual.localPosition = Vector3.up * 0.07f;
            float h = Mathf.Max(0.6f, Shapes.RenderBounds(visual.gameObject).size.y + 0.1f);
            trigger.center = new Vector3(0, h / 2, 0);
            trigger.size = new Vector3(Size, h, Size);
        }

        // --- IInteractable ---
        public string GetName(UseContext c)
        {
            string hum = Loc.T("humedad") + " " + Mathf.RoundToInt(water * 100) + "%";
            if (Empty)
                return !Tilled ? Loc.T("Tierra a medio carpir") + " (" + Mathf.RoundToInt(till * 100) + "%)" : Loc.T("Surco listo para sembrar") + " · " + hum;
            string n = Loc.T(Crop.name);
            if (dead) return n + " — " + Loc.T("seca (sacala con la azada o con E)");
            if (wilted) return n + " — " + Loc.T("¡marchita! Regala ya");
            if (Ripe) return n + " — " + Loc.T("lista para cosechar");
            return n + " — " + Loc.T("creciendo") + " " + Mathf.RoundToInt(growth / Crop.growHours * 100) + "% · " + hum;
        }

        public string GetPrompt(UseContext c)
        {
            if (!c.hands.IsEmpty) return null;
            if (!Empty && dead) return "[E] Arrancar la planta seca";
            if (Ripe) return "Traé un cajón para cosechar";
            if (Empty && !Tilled) return "Terminá de carpir con la azada";
            if (Empty) return "Sembrá con un sobre de semillas en la mano";
            return null;
        }

        public void Interact(UseContext c)
        {
            if (!Empty && dead && c.hands.IsEmpty) Clear();
        }
    }
}
