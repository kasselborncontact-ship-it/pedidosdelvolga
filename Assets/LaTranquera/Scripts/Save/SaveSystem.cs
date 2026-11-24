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

            foreach (var sp in SoilPatch.All)
                d.patches.Add(new PatchSave { pos = sp.transform.position, till = sp.till, water = sp.water, crop = sp.cropId,
                    growth = sp.growth, dry = sp.dryHours, wilted = sp.wilted, dead = sp.dead });
            d.upgrades.AddRange(Upgrades.Owned);

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

            Upgrades.Reset();
            foreach (var u in d.upgrades) Upgrades.Give(u);
            for (int i = SoilPatch.All.Count - 1; i >= 0; i--) Object.Destroy(SoilPatch.All[i].gameObject);
            foreach (var ps in d.patches)
            {
                var sp = SoilPatch.Create(ps.pos);
                sp.till = ps.till; sp.water = ps.water;
                sp.cropId = string.IsNullOrEmpty(ps.crop) ? null : ps.crop;
                sp.growth = ps.growth; sp.dryHours = ps.dry; sp.wilted = ps.wilted; sp.dead = ps.dead;
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
