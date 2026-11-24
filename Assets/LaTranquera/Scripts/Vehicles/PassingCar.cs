using UnityEngine;

namespace LaTranquera
{
    /// <summary>Un auto que sigue derecho por la ruta y desaparece al final.</summary>
    public class PassingCar : MonoBehaviour
    {
        public float speed = 18f;
        public float limit = 265f;

        void Update()
        {
            transform.position += transform.forward * speed * Time.deltaTime;
            if (Mathf.Abs(transform.position.x) > limit) Destroy(gameObject);
        }
    }
}
