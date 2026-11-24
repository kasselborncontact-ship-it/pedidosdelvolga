using System.Collections;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>La camioneta vieja del proveedor (una F-100 celeste): llega, descarga y se va.</summary>
    public class DeliveryTruck : MonoBehaviour
    {
        public float speed = 9f;

        public void Run(DeliveryManager dm, DeliveryManager.Order order)
        {
            VehicleVisuals.Build(VehicleVisuals.Kind.F100, transform, new Color(0.55f, 0.75f, 0.85f));
            StartCoroutine(Drive(dm, order));
        }

        IEnumerator Drive(DeliveryManager dm, DeliveryManager.Order order)
        {
            if (dm.truckRoute == null || dm.truckRoute.Length == 0) { dm.Unload(order); Destroy(gameObject); yield break; }
            transform.position = dm.truckRoute[0].position;
            for (int i = 1; i < dm.truckRoute.Length; i++)
                yield return DriveTo(dm.truckRoute[i].position, i == dm.truckRoute.Length - 1);
            yield return new WaitForSeconds(1.5f);
            dm.Unload(order);
            Sfx.Play("drop");
            yield return new WaitForSeconds(2f);
            foreach (var t in dm.truckExit) yield return DriveTo(t.position, false);
            Destroy(gameObject);
        }

        IEnumerator DriveTo(Vector3 target, bool brake)
        {
            while ((transform.position - target).sqrMagnitude > 0.04f)
            {
                var to = target - transform.position;
                float sp = brake ? Mathf.Clamp(to.magnitude, 1.5f, speed) : speed;
                transform.position = Vector3.MoveTowards(transform.position, target, sp * Time.deltaTime);
                to.y = 0;
                if (to.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(to), 4f * Time.deltaTime);
                yield return null;
            }
        }
    }
}
