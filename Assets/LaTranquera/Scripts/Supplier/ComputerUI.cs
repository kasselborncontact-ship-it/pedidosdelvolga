using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace LaTranquera
{
    /// <summary>
    /// La computadora del almacén. Pestañas:
    ///  - Proveedor: catálogo con foto, precio y cantidad; carrito con total; confirmar pedido.
    ///  - Precios: precio de venta de cada producto (comparado con el sugerido).
    ///  - Mejoras: comprar más canteros (más adelante: corrales, animales, máquinas).
    ///  - Caja y banco: plata, depósito de la caja y resumen del día.
    /// Toda la pantalla se arma por código al empezar.
    /// </summary>
    public class ComputerUI : MonoBehaviour
    {
        public static ComputerUI I;
        public bool IsOpen { get; private set; }

        Canvas canvas;
        RectTransform root, content;
        readonly Dictionary<string, int> cart = new Dictionary<string, int>();
        readonly Dictionary<string, Text> qtyTexts = new Dictionary<string, Text>();
        Text cartText, totalText;
        string tab = "proveedor";

        static readonly Color Bg = new Color(0.93f, 0.93f, 0.9f, 1f);
        static readonly Color Dark = new Color(0.16f, 0.2f, 0.26f, 1f);
        static readonly Color Accent = new Color(0.2f, 0.5f, 0.3f, 1f);
        static readonly Color Red = new Color(0.7f, 0.2f, 0.2f, 1f);
        static readonly Color Blue = new Color(0.2f, 0.35f, 0.6f, 1f);

        void Awake()
        {
            I = this;
            var go = new GameObject("PantallaComputadora", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            UIKit.Panel(go.transform, "Oscuro", new Color(0, 0, 0, 0.55f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            root = UIKit.Panel(go.transform, "Ventana", Bg, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-780, -470), new Vector2(780, 470)).rectTransform;
            canvas.gameObject.SetActive(false);
        }

        public void Open()
        {
            IsOpen = true;
            canvas.gameObject.SetActive(true);
            GameInput.Blocked = true;
            PlayerController.LockCursor(false);
            Rebuild();
        }

        public void Close()
        {
            IsOpen = false;
            canvas.gameObject.SetActive(false);
            GameInput.Blocked = false;
            PlayerController.LockCursor(true);
        }

        // ------------------------------------------------------------------
        void Rebuild()
        {
            Shapes.DestroyChildren(root);
            qtyTexts.Clear();
            // Barra de arriba
            var bar = UIKit.Panel(root, "Barra", Dark, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -70), Vector2.zero);
            UIKit.LabelAt(bar.transform, "  Computadora del almacén", 26, Color.white, Vector2.zero, new Vector2(0.3f, 1), Vector2.zero, Vector2.zero);
            var tabs = UIKit.Rect(bar.transform, "Pestañas", new Vector2(0.3f, 0), new Vector2(0.92f, 1), new Vector2(0, 8), new Vector2(0, -8));
            UIKit.HList(tabs, 8);
            AddTab(tabs, "proveedor", "Proveedor");
            AddTab(tabs, "precios", "Precios");
            AddTab(tabs, "mejoras", "Mejoras");
            AddTab(tabs, "banco", "Caja y banco");
            var close = UIKit.Button(bar.transform, "X", Red, Close, 28);
            var cr = (RectTransform)close.transform;
            cr.anchorMin = new Vector2(1, 0); cr.anchorMax = new Vector2(1, 1); cr.offsetMin = new Vector2(-70, 8); cr.offsetMax = new Vector2(-8, -8);

            content = UIKit.Rect(root, "Contenido", Vector2.zero, Vector2.one, new Vector2(16, 16), new Vector2(-16, -86));
            switch (tab)
            {
                case "proveedor": BuildSupplier(); break;
                case "precios": BuildPrices(); break;
                case "mejoras": BuildUpgrades(); break;
                case "banco": BuildBank(); break;
            }
        }

        void AddTab(Transform parent, string id, string label)
        {
            var b = UIKit.Button(parent, label, tab == id ? Accent : new Color(0.3f, 0.35f, 0.42f), () => { tab = id; Rebuild(); }, 22);
            UIKit.Size(b, 210, -1);
        }

        // ------------------------- PROVEEDOR -------------------------
        void BuildSupplier()
        {
            var grid = UIKit.ScrollArea(content, Vector2.zero, new Vector2(1, 1), Vector2.zero, new Vector2(-420, 0));
            var gl = grid.gameObject.AddComponent<GridLayoutGroup>();
            gl.cellSize = new Vector2(250, 318);
            gl.spacing = new Vector2(12, 12);
            gl.padding = new RectOffset(10, 10, 10, 10);

            foreach (var e in SupplierCatalog.All)
            {
                var card = UIKit.Panel(grid, "Tarjeta_" + e.id, Color.white, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
                var icon = UIKit.Rect(card.transform, "Foto", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(-75, -160), new Vector2(75, -10));
                var raw = icon.gameObject.AddComponent<RawImage>();
                raw.texture = IconFor(e);
                UIKit.LabelAt(card.transform, e.name, 20, Color.black, new Vector2(0, 1), new Vector2(1, 1), new Vector2(8, -205), new Vector2(-8, -160), TextAnchor.MiddleCenter);
                UIKit.LabelAt(card.transform, e.detail, 16, new Color(0.35f, 0.35f, 0.35f), new Vector2(0, 1), new Vector2(1, 1), new Vector2(8, -230), new Vector2(-8, -205), TextAnchor.MiddleCenter);
                UIKit.LabelAt(card.transform, Money.Fmt(e.price), 24, Accent, new Vector2(0, 1), new Vector2(1, 1), new Vector2(8, -262), new Vector2(-8, -230), TextAnchor.MiddleCenter);
                var row = UIKit.Rect(card.transform, "Cantidad", new Vector2(0, 0), new Vector2(1, 0), new Vector2(30, 8), new Vector2(-30, 50));
                UIKit.HList(row, 6);
                string id = e.id;
                UIKit.Size(UIKit.Button(row, "-", Red, () => ChangeQty(id, -1), 28), 50, -1);
                var qbg = UIKit.Panel(row, "Num", new Color(0.92f, 0.92f, 0.92f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                UIKit.Size(qbg, 70, -1);
                var qt = UIKit.Label(qbg.transform, Qty(id).ToString(), 24, Color.black, TextAnchor.MiddleCenter);
                qtyTexts[id] = qt;
                UIKit.Size(UIKit.Button(row, "+", Accent, () => ChangeQty(id, 1), 28), 50, -1);
            }

            // Carrito
            var side = UIKit.Panel(content, "Carrito", Color.white, new Vector2(1, 0), new Vector2(1, 1), new Vector2(-405, 0), Vector2.zero);
            UIKit.LabelAt(side.transform, "Carrito", 28, Dark, new Vector2(0, 1), new Vector2(1, 1), new Vector2(16, -50), new Vector2(-16, -8));
            cartText = UIKit.LabelAt(side.transform, "", 19, Color.black, new Vector2(0, 0), new Vector2(1, 1), new Vector2(16, 190), new Vector2(-16, -60), TextAnchor.UpperLeft);
            totalText = UIKit.LabelAt(side.transform, "", 22, Color.black, new Vector2(0, 0), new Vector2(1, 0), new Vector2(16, 80), new Vector2(-16, 185), TextAnchor.UpperLeft);
            var buttons = UIKit.Rect(side.transform, "Botones", new Vector2(0, 0), new Vector2(1, 0), new Vector2(16, 12), new Vector2(-16, 70));
            UIKit.HList(buttons, 10).childForceExpandWidth = true;
            UIKit.Button(buttons, "Vaciar", Red, () => { cart.Clear(); Rebuild(); });
            UIKit.Button(buttons, "Confirmar pedido", Accent, Confirm);
            RefreshCart();
        }

        Texture IconFor(CatalogEntry e)
        {
            switch (e.kind)
            {
                case "seeds": return IconRenderer.Get(e.id, t => ProductVisuals.SeedPacket(e.refId, t));
                case "crate": return IconRenderer.Get(e.id, t => ProductVisuals.Crate(t));
                default:
                    return IconRenderer.Get(e.id, t =>
                    {
                        // la caja abierta con 3 unidades adelante, para que se vea qué trae
                        var g = new GameObject("Foto");
                        g.transform.SetParent(t, false);
                        ProductVisuals.CardboardBox(g.transform, true);
                        var def = ProductDB.Get(e.refId);
                        for (int i = 0; i < 3; i++)
                        {
                            var v = ProductVisuals.Build(e.refId, g.transform);
                            v.transform.localPosition = new Vector3((i - 1) * (def.size.x + 0.02f), 0, -0.32f);
                        }
                        var inside = ProductVisuals.Build(e.refId, g.transform);
                        inside.transform.localPosition = new Vector3(0, 0.12f, 0);
                        return g;
                    });
            }
        }

        int Qty(string id) => cart.TryGetValue(id, out int q) ? q : 0;

        void ChangeQty(string id, int d)
        {
            int q = Mathf.Clamp(Qty(id) + d, 0, 20);
            if (q == 0) cart.Remove(id); else cart[id] = q;
            if (qtyTexts.TryGetValue(id, out var t)) t.text = q.ToString();
            Sfx.Play("key", 0.4f);
            RefreshCart();
        }

        int CartTotal()
        {
            int total = 0;
            foreach (var kv in cart) total += SupplierCatalog.Get(kv.Key).price * kv.Value;
            return total;
        }

        void RefreshCart()
        {
            if (cartText == null) return;
            var sb = new StringBuilder();
            if (cart.Count == 0) sb.AppendLine("(vacío)\n\nTocá + en los productos para agregarlos.");
            foreach (var kv in cart)
            {
                var e = SupplierCatalog.Get(kv.Key);
                sb.AppendLine(kv.Value + " x " + e.name + "\n      " + Money.Fmt(e.price * kv.Value));
            }
            cartText.text = sb.ToString();
            int total = CartTotal();
            totalText.text = "TOTAL: " + Money.Fmt(total) + "\nEn el banco: " + Money.Fmt(Economy.I.bank) +
                             (total > Economy.I.bank ? "\n<color=#b22>No te alcanza</color>" : "\nLlega en ~1 h 30 min a la puerta del galpón");
            totalText.supportRichText = true;
        }

        void Confirm()
        {
            if (cart.Count == 0) { Sfx.Play("error"); return; }
            int total = CartTotal();
            if (!Economy.I.TrySpend(total, "Pedido al proveedor")) { RefreshCart(); return; }
            var lines = new List<DeliveryManager.OrderLine>();
            foreach (var kv in cart) lines.Add(new DeliveryManager.OrderLine { entryId = kv.Key, qty = kv.Value });
            DeliveryManager.I.PlaceOrder(lines);
            cart.Clear();
            Sfx.Play("ok");
            Rebuild();
        }

        // ------------------------- PRECIOS -------------------------
        void BuildPrices()
        {
            UIKit.LabelAt(content, "Precio de venta de cada producto. Si ponés mucho más caro que el sugerido, los clientes no compran y se quejan.",
                20, Dark, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -40), Vector2.zero);
            var list = UIKit.ScrollArea(content, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0, -48));
            UIKit.VList(list, 6);
            foreach (var p in ProductDB.All)
            {
                var row = UIKit.Panel(list, "Fila_" + p.id, Color.white, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
                UIKit.Size(row, -1, 64);
                UIKit.HList(row.transform, 12, 6);
                var ic = new GameObject("Foto", typeof(RectTransform)).AddComponent<RawImage>();
                ic.transform.SetParent(row.transform, false);
                string pid = p.id;
                ic.texture = IconRenderer.Get("prod_" + pid, t => ProductVisuals.Build(pid, t));
                UIKit.Size(ic, 52, -1);
                var name = UIKit.Label(row.transform, p.name, 22, Color.black); UIKit.Size(name, 330, -1);
                var sug = UIKit.Label(row.transform, "Sugerido: " + Money.Fmt(p.refPrice) + (p.cost > 0 ? "   (costo " + Money.Fmt(p.cost) + ")" : ""), 18, new Color(0.35f, 0.35f, 0.35f));
                UIKit.Size(sug, 420, -1);
                int price = PriceBook.Get(pid);
                float ratio = PriceBook.Ratio(pid);
                Color pc = ratio > 1.35f ? Red : ratio > 1.05f ? new Color(0.8f, 0.5f, 0.1f) : Accent;
                var cur = UIKit.Label(row.transform, Money.Fmt(price), 26, pc, TextAnchor.MiddleCenter); UIKit.Size(cur, 160, -1);
                UIKit.Size(UIKit.Button(row.transform, "-100", Red, () => { PriceBook.Set(pid, PriceBook.Get(pid) - 100); Rebuild(); }), 90, -1);
                UIKit.Size(UIKit.Button(row.transform, "+100", Accent, () => { PriceBook.Set(pid, PriceBook.Get(pid) + 100); Rebuild(); }), 90, -1);
                UIKit.Size(UIKit.Button(row.transform, "Sugerido", Blue, () => { PriceBook.Set(pid, ProductDB.Get(pid).refPrice); Rebuild(); }, 18), 120, -1);
            }
        }

        // ------------------------- MEJORAS -------------------------
        void BuildUpgrades()
        {
            var list = UIKit.ScrollArea(content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            UIKit.VList(list, 8);
            int n = 0;
            foreach (var bed in GardenBed.All)
            {
                if (!bed.locked) continue;
                n++;
                var b = bed;
                var row = UIKit.Panel(list, "Mejora", Color.white, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
                UIKit.Size(row, -1, 70);
                UIKit.HList(row.transform, 12, 8);
                var l = UIKit.Label(row.transform, "Cantero extra para la huerta (" + b.bedId + ")", 22, Color.black); UIKit.Size(l, 700, -1);
                var pl = UIKit.Label(row.transform, Money.Fmt(b.unlockPrice), 24, Accent); UIKit.Size(pl, 200, -1);
                UIKit.Size(UIKit.Button(row.transform, "Comprar", Accent, () =>
                {
                    if (Economy.I.TrySpend(b.unlockPrice, "Cantero")) { b.Unlock(); Sfx.Play("ok"); HUD.Toast("¡Nuevo cantero listo en la huerta!"); }
                    Rebuild();
                }), 180, -1);
            }
            if (n == 0) UIKit.Size(UIKit.Label(list, "Ya compraste todos los canteros.", 22, Dark), -1, 50);
            foreach (var soon in new[] { "Corral y gallinero (Fase 2)", "Ternera, ternero, lechón y pollitos (Fase 2)", "Cámara de frío y sierra (Fase 3)", "Ordeñadora y tanque de leche (Fase 4)", "Repartidor con camioneta (Fase 5)" })
                UIKit.Size(UIKit.Label(list, "Próximamente: " + soon, 20, new Color(0.5f, 0.5f, 0.5f)), -1, 40);
        }

        // ------------------------- BANCO -------------------------
        void BuildBank()
        {
            var e = Economy.I;
            var sb = new StringBuilder();
            sb.AppendLine("Plata en el banco: " + Money.Fmt(e.bank));
            sb.AppendLine("Plata en el cajón de la caja: " + Money.Fmt(e.CashInDrawer));
            sb.AppendLine();
            sb.AppendLine("HOY: ventas " + Money.Fmt(e.today.sales) + " (efectivo " + Money.Fmt(e.today.cashSales) + ", tarjeta " + Money.Fmt(e.today.cardSales) + ")");
            sb.AppendLine("      clientes atendidos: " + e.today.customers + " · se fueron sin comprar o enojados: " + e.today.angry);
            sb.AppendLine("      gastos: " + Money.Fmt(e.today.expenses));
            sb.AppendLine("AYER: ventas " + Money.Fmt(e.yesterday.sales) + " · gastos " + Money.Fmt(e.yesterday.expenses));
            sb.AppendLine();
            sb.AppendLine("Gastos fijos por día: luz " + Money.Fmt(e.luzBase) + " + " + Money.Fmt(e.luzPorHeladera) + " por heladera.");
            sb.AppendLine();
            sb.AppendLine("Pedidos en camino:");
            if (DeliveryManager.I.pending.Count == 0) sb.AppendLine("   ninguno");
            foreach (var o in DeliveryManager.I.pending)
            {
                int items = 0;
                foreach (var l in o.lines) items += l.qty;
                sb.AppendLine("   " + items + " bultos — llega " + TimeManager.FormatClock(o.arriveAt));
            }
            UIKit.LabelAt(content, sb.ToString(), 24, Color.black, Vector2.zero, Vector2.one, new Vector2(10, 90), new Vector2(-10, -10), TextAnchor.UpperLeft);
            var btn = UIKit.Button(content, "Depositar la caja en el banco (deja cambio chico)", Blue, () =>
            {
                int m = Economy.I.DepositDrawer();
                HUD.Toast("Depositaste " + Money.Fmt(m));
                Rebuild();
            });
            var r = (RectTransform)btn.transform;
            r.anchorMin = new Vector2(0, 0); r.anchorMax = new Vector2(0, 0); r.offsetMin = new Vector2(10, 10); r.offsetMax = new Vector2(620, 70);
        }
    }
}
