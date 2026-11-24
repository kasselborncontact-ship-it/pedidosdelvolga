using System.Collections;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>Hace que un vehículo recorra una lista de puntos (ida), espere, y recorra otra (vuelta).</summary>
    public class RouteDriver : MonoBehaviour
    {
        public float speed = 9f;
        public bool Busy { get; private set; }

        public IEnumerator Drive(Transform[] points, bool brakeAtEnd)
        {
            Busy = true;
            for (int i = 0; i < points.Length; i++)
                if (points[i] != null) yield return DriveTo(points[i].position, brakeAtEnd && i == points.Length - 1);
            Busy = false;
        }

        public IEnumerator DriveTo(Vector3 target, bool brake)
        {
            while ((transform.position - target).sqrMagnitude > 0.09f)
            {
                var to = target - transform.position;
                float sp = brake ? Mathf.Clamp(to.magnitude * 1.2f, 1.5f, speed) : speed;
                to.y = 0;
                if (to.sqrMagnitude > 0.01f)
                {
                    var look = Quaternion.LookRotation(to);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, look, 90f * Time.deltaTime * Mathf.Max(1f, sp / 4f));
                }
                // avanza en la dirección en la que mira (dobla como un auto, no de costado)
                float align = Mathf.Clamp01(Vector3.Dot(transform.forward, to.normalized));
                transform.position = Vector3.MoveTowards(transform.position, target, sp * Mathf.Max(0.25f, align) * Time.deltaTime);
                yield return null;
            }
        }
    }
}
