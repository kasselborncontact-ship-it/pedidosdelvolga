using System;
using System.Collections.Generic;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>Todo lo que se guarda en el archivo de partida (JSON).</summary>
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public double totalMinutes;
        public int bank;
        public int[] drawer = new int[7];
        public bool storeOpen;
        public Vector3 playerPos;
        public float playerYaw;
        public List<PriceSave> prices = new List<PriceSave>();
        public List<PatchSave> patches = new List<PatchSave>();
        public List<string> upgrades = new List<string>();
        public List<ShelfSave> shelves = new List<ShelfSave>();
        public List<ObjSave> objects = new List<ObjSave>();
        public List<DeliveryManager.Order> orders = new List<DeliveryManager.Order>();
        public Economy.DayStats today = new Economy.DayStats();
        public Economy.DayStats yesterday = new Economy.DayStats();
    }

    [Serializable] public class PriceSave { public string id; public int price; }

    [Serializable]
    public class PatchSave
    {
        public Vector3 pos;
        public float till, water, growth, dry;
        public string crop;
        public bool wilted, dead;
    }

    [Serializable]
    public class ShelfSave
    {
        public string id;
        public List<Shelf.Slot> slots = new List<Shelf.Slot>();
    }

    [Serializable]
    public class ObjSave
    {
        public string kind;
        public Vector3 pos;
        public Vector3 rot;
        public bool frozen;
        public string extra;
    }
}
