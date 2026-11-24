using System.Collections;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>La camioneta vieja del proveedor (una F-100 celeste): entra por el camino de tierra, descarga en el depósito y se va.</summary>
    public class DeliveryTruck : MonoBehaviour
    {
        public void Run(DeliveryManager dm, DeliveryManager.Order order)
        {
            VehicleVisuals.Build(VehicleVisuals.Kind.F100, transform, new Color(0.55f, 0.75f, 0.85f), new Color(0.95f, 0.95f, 0.92f));
            StartCoroutine(Drive(dm, order));
        }

        IEnumerator Drive(DeliveryManager dm, DeliveryManager.Order order)
        {
            if (dm.truckRoute == null || dm.truckRoute.Length < 2) { dm.Unload(order); Destroy(gameObject); yield break; }
            var drv = gameObject.AddComponent<RouteDriver>();
            transform.position = dm.truckRoute[0].position;
            transform.rotation = Quaternion.LookRotation(dm.truckRoute[1].position - dm.truckRoute[0].position);
            yield return drv.Drive(dm.truckRoute, true);
            yield return new WaitForSeconds(1.5f);
            dm.Unload(order);
            Sfx.Play("drop");
            yield return new WaitForSeconds(2f);
            yield return drv.Drive(dm.truckExit, false);
            Destroy(gameObject);
        }
    }
}
