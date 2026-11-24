using System.Collections.Generic;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Un terreno (lote) para sembrar. Si es tuyo, podés carpir y plantar en CUALQUIER lugar adentro.
    /// Los que no son tuyos tienen un cartel EN VENTA y se compran en la computadora (pestaña Mejoras).
    /// El terreno es un rectángulo en el piso: centro = posición del objeto, tamaño = size (X por Z).
    /// </summary>
    public class LandPlot : MonoBehaviour
    {
        public string plotId = "lote_1";
        public string displayName = "Lote 1";
        public Vector2 size = new Vector2(14, 16);
        public bool owned;
        public int price;
        /// <summary>Cartel de venta y estacas (se esconden al comprarlo).</summary>
        public GameObject forSaleVisual;
        /// <summary>El piso del lote (pasto si no es tuyo, tierra arada si es tuyo).</summary>
        public Renderer ground;

        public static readonly List<LandPlot> All = new List<LandPlot>();

        void OnEnable() => All.Add(this);
        void OnDisable() => All.Remove(this);
        void Start() => Refresh();

        public bool Contains(Vector3 p, float margin = 0.3f)
        {
            var l = p - transform.position;
            return Mathf.Abs(l.x) <= size.x / 2 - margin && Mathf.Abs(l.z) <= size.y / 2 - margin;
        }

        public static LandPlot At(Vector3 p)
        {
            foreach (var l in All) if (l.Contains(p, 0f)) return l;
            return null;
        }

        public static bool CanFarm(Vector3 p)
        {
            foreach (var l in All) if (l.owned && l.Contains(p)) return true;
            return false;
        }

        public static LandPlot Find(string id) => All.Find(l => l.plotId == id);

        public void SetOwned(bool o)
        {
            owned = o;
            Refresh();
        }

        public void Refresh()
        {
            if (forSaleVisual != null) forSaleVisual.SetActive(!owned);
            if (ground != null) ground.sharedMaterial = owned ? TexKit.Mat("tierra", new Color(0.95f, 0.9f, 0.85f)) : TexKit.Mat("pasto", Color.white);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = owned ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.1f, new Vector3(size.x, 0.2f, size.y));
        }
    }
}
