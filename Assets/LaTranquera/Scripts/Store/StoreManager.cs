using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LaTranquera
{
    /// <summary>Estado del almacén: abierto o cerrado, y qué hay a la venta.</summary>
    public class StoreManager : MonoBehaviour
    {
        public static StoreManager I;
        public bool isOpen;
        public static event Action Changed;

        void Awake() => I = this;

        public void SetOpen(bool open)
        {
            isOpen = open;
            Changed?.Invoke();
        }

        public Shelf FindShelfWith(string pid)
        {
            foreach (var s in Shelf.All) if (s.Count(pid) > 0) return s;
            return null;
        }

        public List<string> StockedProducts()
        {
            var set = new HashSet<string>();
            foreach (var s in Shelf.All) set.UnionWith(s.Products());
            return new List<string>(set);
        }
    }
}
