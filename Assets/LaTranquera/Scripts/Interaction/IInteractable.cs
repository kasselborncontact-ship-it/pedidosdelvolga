using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Todo lo que se puede mirar y usar con E implementa esta interfaz:
    /// cajones, plantas, estanterías, el cartel, la caja registradora, la cama...
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Nombre que aparece debajo del punto del centro.</summary>
        string GetName(UseContext c);
        /// <summary>Texto de ayuda para la tecla E (o null si no hay nada para hacer).</summary>
        string GetPrompt(UseContext c);
        /// <summary>Se llama al apretar E mirando este objeto.</summary>
        void Interact(UseContext c);
    }

    /// <summary>Cosas que se abren con F (cajas del proveedor).</summary>
    public interface IOpenable
    {
        string GetOpenPrompt();
        void Open();
    }

    /// <summary>
    /// Información de "qué estoy mirando" que se pasa a todos los sistemas.
    /// </summary>
    public class UseContext
    {
        public PlayerHands hands;
        public bool hasHit;
        public RaycastHit hit;
        public IInteractable target;

        public Carryable Held => hands != null ? hands.Held : null;

        /// <summary>Busca un componente del tipo T en lo que estoy mirando (o en sus padres).</summary>
        public T Get<T>() where T : class
        {
            if (!hasHit || hit.collider == null) return null;
            return hit.collider.GetComponentInParent<T>();
        }
    }
}
