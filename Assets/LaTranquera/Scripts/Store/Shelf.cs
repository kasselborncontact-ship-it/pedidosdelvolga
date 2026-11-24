using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LaTranquera
{
    /// <summary>
    /// Estantería o heladera con lugares visibles. Cada lugar tiene UN producto y una
    /// cantidad máxima según su tamaño. Cada lugar muestra una etiqueta con nombre y precio.
    /// El frente de la estantería mira hacia +Z local.
    /// Los lácteos (y más adelante las carnes) solo se pueden poner si isFridge = true.
    /// </summary>
    public class Shelf : MonoBehaviour, IInteractable
    {
        public string shelfId = "estanteria_1";
        public string displayName = "Estantería";
        public bool isFridge;
        public int levels = 4;
        public int slotsPerLevel = 3;
        public float width = 2.4f;
        public float depth = 0.42f;
        public float firstLevelY = 0.12f;
        public float levelSpacing = 0.44f;
        /// <summary>Dónde se para el cliente para agarrar cosas.</summary>
        public Transform customerPoint;

        [Serializable]
        public class Slot
        {
            public string productId;
            public int count;
            [NonSerialized] public Vector3 localPos;
            [NonSerialized] public Transform root;
            [NonSerialized] public Text tag;
        }

        public List<Slot> slots = new List<Slot>();
        public static readonly List<Shelf> All = new List<Shelf>();

        float SlotWidth => width / slotsPerLevel;

        void Awake() => BuildSlots();

        void OnEnable() { All.Add(this); PriceBook.Changed += RefreshAllTags; }
        void OnDisable() { All.Remove(this); PriceBook.Changed -= RefreshAllTags; }

        void BuildSlots()
        {
            var old = slots;
            slots = new List<Slot>();
            var root = transform.Find("Lugares");
            if (root != null) Destroy(root.gameObject);
            root = Shapes.Empty(transform, "Lugares", Vector3.zero).transform;

            for (int l = 0; l < levels; l++)
                for (int s = 0; s < slotsPerLevel; s++)
                {
                    var slot = new Slot();
                    int i = slots.Count;
                    if (i < old.Count) { slot.productId = old[i].productId; slot.count = old[i].count; }
                    float x = (s - (slotsPerLevel - 1) * 0.5f) * SlotWidth;
                    float y = firstLevelY + l * levelSpacing;
                    slot.localPos = new Vector3(x, y, 0);
                    slot.root = Shapes.Empty(root, "Lugar_" + i, slot.localPos).transform;
                    // Collider del lugar: es lo que "mirás" para poner o sacar
                    var bc = slot.root.gameObject.AddComponent<BoxCollider>();
                    bc.center = new Vector3(0, levelSpacing * 0.45f, 0);
                    bc.size = new Vector3(SlotWidth * 0.96f, levelSpacing * 0.9f, depth);
                    slot.tag = WorldLabel.Create(slot.root, "", new Vector3(0, -0.015f, depth * 0.5f + 0.012f),
                        new Vector2(SlotWidth * 0.8f, 0.05f), 18, Color.black, Vector3.forward, new Color(1f, 0.97f, 0.6f));
                    slots.Add(slot);
                    RefreshSlot(slot);
                }
        }

        int Cap(ProductDef d) => d.slotCap;

        public bool Accepts(ProductDef d) => d.storage == Storage.Heladera ? isFridge : true;

        public Slot NearestSlot(Vector3 worldPoint, Func<Slot, bool> filter)
        {
            Slot best = null;
            float bd = float.MaxValue;
            foreach (var s in slots)
            {
                if (filter != null && !filter(s)) continue;
                float d = (s.root.position + Vector3.up * levelSpacing * 0.3f - worldPoint).sqrMagnitude;
                if (d < bd) { bd = d; best = s; }
            }
            return best;
        }

        /// <summary>Pone una unidad en el lugar libre (o con el mismo producto) más cercano a donde mirás.</summary>
        public bool TryAdd(string pid, Vector3 worldPoint, out string reason)
        {
            reason = null;
            var d = ProductDB.Get(pid);
            if (d == null) { reason = "Producto desconocido"; return false; }
            if (!Accepts(d)) { reason = ProductDB.NameOf(pid) + " va en la heladera"; Sfx.Play("error"); return false; }
            // Primero el lugar que estás mirando; si no sirve, el lugar compatible más cercano.
            Func<Slot, bool> fits = x => (x.count == 0 || x.productId == pid) && x.count < Cap(d);
            var slot = NearestSlot(worldPoint, null);
            if (slot == null || !fits(slot)) slot = NearestSlot(worldPoint, fits);
            if (slot == null) { reason = Loc.T(displayName) + " no tiene más lugar para " + ProductDB.NameOf(pid); return false; }
            if (slot.count == 0) slot.productId = pid;
            slot.count++;
            RefreshSlot(slot);
            Sfx.Play("drop", 0.25f);
            return true;
        }

        public int Count(string pid)
        {
            int n = 0;
            foreach (var s in slots) if (s.productId == pid) n += s.count;
            return n;
        }

        /// <summary>Lo usa el cliente para llevarse una unidad.</summary>
        public bool TakeOne(string pid)
        {
            foreach (var s in slots)
                if (s.productId == pid && s.count > 0)
                {
                    s.count--;
                    if (s.count == 0) s.productId = null;
                    RefreshSlot(s);
                    return true;
                }
            return false;
        }

        /// <summary>Devuelve una unidad (cliente que se fue sin comprar).</summary>
        public bool PutBack(string pid)
        {
            var d = ProductDB.Get(pid);
            if (d == null || !Accepts(d)) return false;
            return TryAdd(pid, transform.position, out _);
        }

        public HashSet<string> Products()
        {
            var set = new HashSet<string>();
            foreach (var s in slots) if (s.count > 0) set.Add(s.productId);
            return set;
        }

        void RefreshAllTags()
        {
            foreach (var s in slots) UpdateTag(s);
        }

        void UpdateTag(Slot s)
        {
            if (s.tag == null) return;
            if (s.count > 0) s.tag.text = ProductDB.NameOf(s.productId) + "  " + Money.Fmt(PriceBook.Get(s.productId));
            else s.tag.text = "";
            s.tag.transform.parent.gameObject.SetActive(s.count > 0);
        }

        public void RefreshSlot(Slot s)
        {
            // borra las unidades dibujadas (menos la etiqueta y el collider)
            for (int i = s.root.childCount - 1; i >= 0; i--)
            {
                var ch = s.root.GetChild(i);
                if (ch.name.StartsWith("Vis_")) Destroy(ch.gameObject);
            }
            UpdateTag(s);
            if (s.count <= 0) return;
            var d = ProductDB.Get(s.productId);
            var unit = d.size * 1.06f;
            var area = new Vector3(SlotWidth * 0.92f, levelSpacing * 0.85f, depth * 0.9f);
            var pos = Shapes.Arrange(s.count, unit, area);
            foreach (var p in pos)
            {
                var v = ProductVisuals.Build(s.productId, s.root);
                v.transform.localPosition = p + new Vector3(0, 0.005f, 0);
            }
        }

        public void LoadSlots(List<Slot> data)
        {
            for (int i = 0; i < slots.Count && i < data.Count; i++)
            {
                slots[i].productId = string.IsNullOrEmpty(data[i].productId) ? null : data[i].productId;
                slots[i].count = slots[i].productId == null ? 0 : data[i].count;
                RefreshSlot(slots[i]);
            }
        }

        public void ClearAll()
        {
            foreach (var s in slots) { s.productId = null; s.count = 0; RefreshSlot(s); }
        }

        // --- IInteractable ---
        public string GetName(UseContext c)
        {
            var s = NearestSlot(c.hit.point, null);
            string n = Loc.T(displayName);
            if (s == null || s.count == 0) return n + " — " + Loc.T("lugar vacío");
            var d = ProductDB.Get(s.productId);
            return n + " — " + ProductDB.NameOf(s.productId) + " " + s.count + "/" + Cap(d) + " · " + Money.Fmt(PriceBook.Get(s.productId));
        }

        public string GetPrompt(UseContext c)
        {
            if (!c.hands.IsEmpty) return null;
            var s = NearestSlot(c.hit.point, null);
            return s != null && s.count > 0 ? "[E] Sacar uno" : null;
        }

        public void Interact(UseContext c)
        {
            if (!c.hands.IsEmpty) return;
            var s = NearestSlot(c.hit.point, null);
            if (s == null || s.count == 0) return;
            string pid = s.productId;
            s.count--;
            if (s.count == 0) s.productId = null;
            RefreshSlot(s);
            var item = WorldFactory.Product(pid, c.hit.point, Quaternion.identity);
            c.hands.Pick(item);
        }
    }
}
