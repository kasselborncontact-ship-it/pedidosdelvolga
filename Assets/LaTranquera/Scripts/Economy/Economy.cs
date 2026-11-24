using System;
using System.Collections.Generic;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// La plata. Hay dos lugares:
    ///  - Banco: acá entran las ventas con tarjeta y de acá se pagan proveedor, mejoras y gastos.
    ///  - Caja: los billetes del cajón (ver CashDrawer). Al dormir se deposita lo que sobra del cambio.
    /// </summary>
    public class Economy : MonoBehaviour
    {
        public static Economy I;

        public int bank = 40000;
        [Header("Gastos diarios")]
        public int luzBase = 1800;
        public int luzPorHeladera = 900;

        public static event Action Changed;

        [Serializable]
        public class DayStats { public int sales; public int cashSales; public int cardSales; public int expenses; public int customers; public int angry; }
        public DayStats today = new DayStats();
        public DayStats yesterday = new DayStats();

        void Awake() => I = this;
        void OnEnable() => TimeManager.NewDay += OnNewDay;
        void OnDisable() => TimeManager.NewDay -= OnNewDay;

        public int CashInDrawer => CashDrawer.I != null ? CashDrawer.I.Total : 0;

        public bool CanAfford(int amount) => bank >= amount;

        public bool TrySpend(int amount, string reason)
        {
            if (bank < amount) { HUD.Toast("No te alcanza la plata del banco (" + Money.Fmt(bank) + ")"); Sfx.Play("error"); return false; }
            bank -= amount;
            today.expenses += amount;
            Changed?.Invoke();
            return true;
        }

        /// <summary>Gastos obligatorios (la luz): se cobran aunque quede en negativo.</summary>
        public void ForceExpense(int amount, string reason)
        {
            bank -= amount;
            today.expenses += amount;
            HUD.Toast(reason + ": -" + Money.Fmt(amount));
            Changed?.Invoke();
        }

        public void Deposit(int amount)
        {
            bank += amount;
            Changed?.Invoke();
        }

        public void RecordSale(int amount, bool card)
        {
            today.sales += amount;
            if (card) today.cardSales += amount; else today.cashSales += amount;
            today.customers++;
            Changed?.Invoke();
        }

        public static void NotifyChanged() => Changed?.Invoke();

        void OnNewDay(int day)
        {
            int fridges = 0;
            foreach (var s in Shelf.All) if (s.isFridge) fridges++;
            ForceExpense(luzBase + fridges * luzPorHeladera, "Factura de luz");
        }

        /// <summary>Lo llama la cama al despertar: guarda el resumen del día y empieza uno nuevo.</summary>
        public void CloseDay()
        {
            yesterday = today;
            today = new DayStats();
        }

        /// <summary>Pasa al banco los billetes grandes del cajón (deja cambio chico).</summary>
        public int DepositDrawer()
        {
            if (CashDrawer.I == null) return 0;
            int moved = CashDrawer.I.TakeForDeposit();
            bank += moved;
            Changed?.Invoke();
            return moved;
        }
    }

    /// <summary>Precios de venta que pone el jugador (si no, se usa el sugerido).</summary>
    public static class PriceBook
    {
        static readonly Dictionary<string, int> prices = new Dictionary<string, int>();
        public static event Action Changed;

        public static int Get(string pid)
        {
            if (prices.TryGetValue(pid, out int p)) return p;
            return ProductDB.Get(pid)?.refPrice ?? 0;
        }

        public static void Set(string pid, int price)
        {
            prices[pid] = Mathf.Clamp(price, 100, 999900);
            Changed?.Invoke();
        }

        public static void Clear() { prices.Clear(); Changed?.Invoke(); }

        /// <summary>Precio / referencia. 1 = justo. 1,3 = 30% más caro.</summary>
        public static float Ratio(string pid)
        {
            var d = ProductDB.Get(pid);
            return d == null ? 1f : (float)Get(pid) / d.refPrice;
        }

        public static Dictionary<string, int> All => prices;
    }
}
