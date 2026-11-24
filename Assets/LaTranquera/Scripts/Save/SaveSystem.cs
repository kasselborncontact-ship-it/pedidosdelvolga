using System.IO;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Guarda y carga la partida en un archivo JSON.
    /// En Windows queda en: C:\Users\TU_USUARIO\AppData\LocalLow\(empresa)\(juego)\partida.json
    /// (se ve en la consola de Unity al guardar). Más adelante se puede sincronizar con Steam Cloud.
    /// </summary>
    public static class SaveSystem
    {
        public static string PathFile => Path.Combine(Application.persistentDataPath, "partida.json");
        public static bool HasSave => File.Exists(PathFile);

        public static void Delete()
        {
            if (HasSave) File.Delete(PathFile);
        }

        public static void Save()
        {
            var d = new SaveData();
            d.totalMinutes = TimeManager.I.totalMinutes;
            d.bank = Economy.I.bank;
            d.today = Economy.I.today;
            d.yesterday = Economy.I.yesterday;
            if (CashDrawer.I != null) d.drawer = (int[])CashDrawer.I.counts.Clone();
            d.storeOpen = StoreManager.I.isOpen;
            var p = PlayerController.I.transform;
            d.playerPos = p.position;
            d.playerYaw = p.eulerAngles.y;

            foreach (var kv in PriceBook.All) d.prices.Add(new PriceSave { id = kv.Key, price = kv.Value });

            foreach (var b in GardenBed.All)
            {
                var bs = new BedSave { id = b.bedId, locked = b.locked, tilled = b.tilled, till = b.tillProgress, water = b.water };
                foreach (var pl in b.plants)
                    bs.plants.Add(new PlantSave { crop = pl.cropId, growth = pl.growth, dry = pl.dryHours, wilted = pl.wilted, dead = pl.dead });
                d.beds.Add(bs);
            }
            // Los canteros bloqueados están desactivados a medias; buscamos todos igual
            foreach (var b in Object.FindObjectsByType<GardenBed>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (d.beds.Exists(x => x.id == b.bedId)) continue;
                d.beds.Add(new BedSave { id = b.bedId, locked = b.locked });
            }

            foreach (var s in Shelf.All)
            {
                var ss = new ShelfSave { id = s.shelfId };
                foreach (var slot in s.slots) ss.slots.Add(new Shelf.Slot { productId = slot.productId, count = slot.count });
                d.shelves.Add(ss);
            }

            foreach (var c in Carryable.All)
            {
                if (c == null) continue;
                d.objects.Add(new ObjSave
                {
                    kind = c.kind,
                    pos = c.transform.position,
                    rot = c.transform.eulerAngles,
                    frozen = c.frozen && !c.IsHeld,
                    extra = c.SaveExtra()
                });
            }

            d.orders = DeliveryManager.I.pending;

            File.WriteAllText(PathFile, JsonUtility.ToJson(d, true));
            Debug.Log("Partida guardada en " + PathFile);
        }

        public static bool Load()
        {
            if (!HasSave) return false;
            SaveData d;
            try { d = JsonUtility.FromJson<SaveData>(File.ReadAllText(PathFile)); }
            catch (System.Exception e) { Debug.LogWarning("No se pudo leer la partida: " + e.Message); return false; }
            if (d == null) return false;

            TimeManager.I.totalMinutes = d.totalMinutes;
            Economy.I.bank = d.bank;
            Economy.I.today = d.today ?? new Economy.DayStats();
            Economy.I.yesterday = d.yesterday ?? new Economy.DayStats();
            if (CashDrawer.I != null && d.drawer != null) CashDrawer.I.SetCounts(d.drawer);
            StoreManager.I.SetOpen(d.storeOpen);
            PlayerController.I.Teleport(d.playerPos + Vector3.up * 0.1f, d.playerYaw);

            PriceBook.Clear();
            foreach (var pr in d.prices) PriceBook.Set(pr.id, pr.price);

            foreach (var b in Object.FindObjectsByType<GardenBed>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var bs = d.beds.Find(x => x.id == b.bedId);
                if (bs == null) continue;
                b.locked = bs.locked; b.tilled = bs.tilled; b.tillProgress = bs.till; b.water = bs.water;
                for (int i = 0; i < b.plants.Length && i < bs.plants.Count; i++)
                {
                    var ps = bs.plants[i];
                    var pl = b.plants[i];
                    pl.cropId = string.IsNullOrEmpty(ps.crop) ? null : ps.crop;
                    pl.growth = ps.growth; pl.dryHours = ps.dry; pl.wilted = ps.wilted; pl.dead = ps.dead;
                    pl.Refresh();
                }
                b.RefreshVisual();
            }

            foreach (var s in Shelf.All)
            {
                var ss = d.shelves.Find(x => x.id == s.shelfId);
                if (ss != null) s.LoadSlots(ss.slots);
            }

            // Borra los objetos sueltos de la escena y crea los guardados
            for (int i = Carryable.All.Count - 1; i >= 0; i--) Object.Destroy(Carryable.All[i].gameObject);
            foreach (var o in d.objects)
            {
                var c = WorldFactory.Create(o.kind, o.extra, o.pos + Vector3.up * 0.02f, Quaternion.Euler(o.rot));
                if (c != null && o.frozen) c.FreezeAt(o.pos, Quaternion.Euler(o.rot));
            }

            DeliveryManager.I.pending = d.orders ?? new System.Collections.Generic.List<DeliveryManager.Order>();
            Economy.NotifyChanged();
            return true;
        }
    }
}
