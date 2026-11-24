using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Puntos por donde caminan los clientes (sin NavMesh, para que sea simple).
    /// Podés mover estos puntos en la escena si cambiás el almacén de lugar.
    /// </summary>
    public class StoreNav : MonoBehaviour
    {
        public static StoreNav I;
        public Transform spawnWest, spawnEast;   // donde aparecen, sobre la banquina
        public Transform curbWest, curbEast;     // donde dejan la ruta y entran al estacionamiento
        public Transform parking;
        public Transform doorOutside, doorInside;

        void Awake() => I = this;

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            foreach (var t in new[] { spawnWest, curbWest, parking, doorOutside, doorInside, curbEast, spawnEast })
                if (t != null) Gizmos.DrawWireSphere(t.position, 0.3f);
        }
    }
}
