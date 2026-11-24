using System.Collections.Generic;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Cantero de la huerta (1,2 m x 3 m, 6 plantas).
    /// Ciclo: carpir con la azada -> sembrar con el sobre -> regar con la regadera -> esperar -> cosechar con un cajón.
    /// La humedad baja con las horas. Si se seca, las plantas se marchitan y después se mueren.
    /// </summary>
    public class GardenBed : MonoBehaviour, IInteractable
    {
        public string bedId = "cantero_1";
        public string displayName = "Cantero";
        public bool locked;
        public int unlockPrice = 20000;

        [Header("Estado (se guarda)")]
        public bool tilled;
        [Range(0, 1)] public float tillProgress;
        [Range(0, 1)] public float water;

        public static readonly List<GardenBed> All = new List<GardenBed>();
        public Plant[] plants;

        const float SizeX = 1.2f, SizeZ = 3f;
        /// <summary>Cuánta humedad pierde por hora (1 = se seca en 24 h).</summary>
        public float dryPerHour = 1f / 24f;

        GameObject flatSoil, tilledSoil, lockedVisual, border;
        Renderer[] tilledRenderers;

        void Awake()
        {
            if (transform.Find("Visual") == null) Build();
            Cache();
            RefreshVisual();
        }

        void OnEnable() { All.Add(this); TimeManager.HourPassed += OnHour; }
        void OnDisable() { All.Remove(this); TimeManager.HourPassed -= OnHour; }

        /// <summary>Arma el cantero con formas simples. Lo llama el constructor de escena o Awake.</summary>
        public void Build()
        {
            var vis = Shapes.Empty(transform, "Visual", Vector3.zero).transform;
            border = Shapes.Empty(vis, "Borde", Vector3.zero);
            var b = border.transform;
            Shapes.Box(b, "TablaN", new Vector3(0, 0, SizeZ / 2), new Vector3(SizeX + 0.1f, 0.2f, 0.05f), Mats.Madera, false);
            Shapes.Box(b, "TablaS", new Vector3(0, 0, -SizeZ / 2), new Vector3(SizeX + 0.1f, 0.2f, 0.05f), Mats.Madera, false);
            Shapes.Box(b, "TablaE", new Vector3(SizeX / 2, 0, 0), new Vector3(0.05f, 0.2f, SizeZ), Mats.Madera, false);
            Shapes.Box(b, "TablaO", new Vector3(-SizeX / 2, 0, 0), new Vector3(0.05f, 0.2f, SizeZ), Mats.Madera, false);

            flatSoil = Shapes.Box(vis, "TierraSinCarpir", Vector3.zero, new Vector3(SizeX, 0.14f, SizeZ), new Color(0.40f, 0.34f, 0.22f), false);
            tilledSoil = Shapes.Empty(vis, "TierraCarpida", Vector3.zero);
            Shapes.Box(tilledSoil.transform, "Base", Vector3.zero, new Vector3(SizeX, 0.12f, SizeZ), Mats.Tierra, false);
            for (int i = 0; i < 2; i++)
                Shapes.Box(tilledSoil.transform, "Surco", new Vector3((i - 0.5f) * 0.6f, 0.12f, 0), new Vector3(0.38f, 0.06f, SizeZ - 0.1f), Mats.Tierra, false);

            lockedVisual = Shapes.Empty(vis, "Bloqueado", Vector3.zero);
            Shapes.Box(lockedVisual.transform, "Estaca", new Vector3(0, 0, -SizeZ / 2 - 0.3f), new Vector3(0.06f, 0.9f, 0.06f), Mats.Madera, false);
            Shapes.Box(lockedVisual.transform, "Tabla", new Vector3(0, 0.6f, -SizeZ / 2 - 0.33f), new Vector3(0.6f, 0.35f, 0.03f), Mats.MaderaClara, false);
            WorldLabel.Create(lockedVisual.transform, "EN VENTA\n" + Money.Fmt(unlockPrice), new Vector3(0, 0.78f, -SizeZ / 2 - 0.35f),
                new Vector2(0.58f, 0.33f), 60, Color.black, Vector3.back);
            // Estacas en las esquinas para marcar el terreno
            foreach (var corner in new[] { new Vector3(-1, 0, -1), new Vector3(1, 0, -1), new Vector3(-1, 0, 1), new Vector3(1, 0, 1) })
                Shapes.Box(lockedVisual.transform, "Esquina", new Vector3(corner.x * SizeX / 2, 0, corner.z * SizeZ / 2), new Vector3(0.04f, 0.4f, 0.04f), Mats.MaderaClara, false);

            var bc = gameObject.GetComponent<BoxCollider>();
            if (bc == null) bc = gameObject.AddComponent<BoxCollider>();
            bc.center = new Vector3(0, 0.08f, 0);
            bc.size = new Vector3(SizeX + 0.1f, 0.16f, SizeZ + 0.05f);

            // 6 lugares para plantas (2 columnas x 3 filas)
            int n = 0;
            for (int z = 0; z < 3; z++)
                for (int x = 0; x < 2; x++)
                {
                    var spot = Shapes.Empty(transform, "Lugar_" + (n++), new Vector3((x - 0.5f) * 0.6f, 0.15f, (z - 1) * 1f));
                    var t = spot.AddComponent<BoxCollider>();
                    t.isTrigger = true;
                    t.center = new Vector3(0, 0.45f, 0);
                    t.size = new Vector3(0.55f, 0.9f, 0.95f);
                    spot.AddComponent<Plant>();
                }
        }

        void Cache()
        {
            var vis = transform.Find("Visual");
            border = vis.Find("Borde").gameObject;
            flatSoil = vis.Find("TierraSinCarpir").gameObject;
            tilledSoil = vis.Find("TierraCarpida").gameObject;
            lockedVisual = vis.Find("Bloqueado").gameObject;
            tilledRenderers = tilledSoil.GetComponentsInChildren<Renderer>();
            plants = GetComponentsInChildren<Plant>(true);
            foreach (var p in plants) p.bed = this;
        }

        public void RefreshVisual()
        {
            if (border == null) return;
            border.SetActive(!locked);
            lockedVisual.SetActive(locked);
            flatSoil.SetActive(!locked && !tilled);
            tilledSoil.SetActive(!locked && tilled);
            var m = Mats.Get(water > 0.15f ? Mats.TierraMojada : Mats.Tierra);
            if (tilledRenderers != null) foreach (var r in tilledRenderers) r.sharedMaterial = m;
            if (plants != null) foreach (var p in plants) p.gameObject.SetActive(!locked);
        }

        public void Unlock()
        {
            locked = false;
            RefreshVisual();
        }

        public void Till(float amount)
        {
            if (tilled) return;
            tillProgress = Mathf.Clamp01(tillProgress + amount);
            if (tillProgress >= 0.999f)
            {
                tilled = true;
                HUD.Toast("Cantero carpido. ¡A sembrar!");
            }
            RefreshVisual();
        }

        public void AddWater(float amount)
        {
            water = Mathf.Clamp01(water + amount);
            foreach (var p in plants) p.OnWatered();
            RefreshVisual();
        }

        public Plant FirstEmpty()
        {
            foreach (var p in plants) if (p.Empty) return p;
            return null;
        }

        public Plant NearestEmpty(Vector3 worldPoint)
        {
            Plant best = null;
            float bd = float.MaxValue;
            foreach (var p in plants)
            {
                if (!p.Empty) continue;
                float d = (p.transform.position - worldPoint).sqrMagnitude;
                if (d < bd) { bd = d; best = p; }
            }
            return best;
        }

        /// <summary>Cuando se cosechó la última planta, la tierra queda dura y hay que volver a carpir.</summary>
        public void OnPlantHarvested()
        {
            foreach (var p in plants) if (!p.Empty) return;
            tilled = false;
            tillProgress = 0;
            RefreshVisual();
        }

        void OnHour(int hour)
        {
            if (locked) return;
            bool hasWater = water > 0f;
            foreach (var p in plants) p.TickHour(hasWater);
            if (hasWater)
            {
                bool anyPlant = false;
                foreach (var p in plants) if (!p.Empty) anyPlant = true;
                water = Mathf.Max(0, water - dryPerHour * (anyPlant ? 1f : 0.5f));
                RefreshVisual();
            }
        }

        // --- IInteractable ---
        public string GetName(UseContext c)
        {
            if (locked) return Loc.T("Terreno para cantero") + " — " + Loc.T("se compra en la computadora del almacén") + " (" + Money.Fmt(unlockPrice) + ")";
            string state = !tilled ? Loc.T("sin carpir") : Loc.T("carpido");
            return Loc.T(displayName) + " — " + state + ", " + Loc.T("humedad") + " " + Mathf.RoundToInt(water * 100) + "%";
        }

        public string GetPrompt(UseContext c)
        {
            if (locked || !c.hands.IsEmpty) return null;
            if (!tilled) return "Usá la azada para carpir (está en el galpón)";
            if (FirstEmpty() != null) return "Sembrá con un sobre de semillas en la mano";
            return null;
        }

        public void Interact(UseContext c) { }
    }
}
