using UnityEngine;

namespace LaTranquera
{
    /// <summary>Hace girar un objeto sobre su eje vertical (ventilador de techo, molino).</summary>
    public class Spin : MonoBehaviour
    {
        public float degreesPerSecond = 220f;
        public Vector3 axis = Vector3.up;
        void Update() => transform.Rotate(axis, degreesPerSecond * Time.deltaTime, Space.Self);
    }
}
