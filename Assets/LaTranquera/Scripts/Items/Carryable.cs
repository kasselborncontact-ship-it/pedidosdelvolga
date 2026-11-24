using System.Collections.Generic;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Base de todo lo que se puede llevar en las manos: cajones, cajas, productos, herramientas.
    /// Tiene física (Rigidbody): si lo soltás, cae; si lo apoyás, queda ahí.
    /// </summary>
    public class Carryable : MonoBehaviour, IInteractable
    {
        /// <summary>Tipo para el guardado ("crate", "box", "product", "seeds", "hoe", "can", "labeler", "bucket").</summary>
        public string kind = "item";
        public string displayName = "Objeto";
        public Vector3 holdOffset = new Vector3(0.3f, -0.35f, 0.65f);
        public Vector3 holdEuler = Vector3.zero;
        public float mass = 1f;

        /// <summary>Si está colgado en un gancho o quieto a propósito (no tiene gravedad).</summary>
        public bool frozen;

        public static readonly List<Carryable> All = new List<Carryable>();

        [HideInInspector] public Rigidbody rb;
        public bool IsHeld { get; private set; }

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.maxDepenetrationVelocity = 1.5f;  // si queda encimado con otra cosa, se separa suave (no "salta")
            rb.linearDamping = 0.2f;
            rb.angularDamping = 1.5f;
            rb.isKinematic = frozen;
        }

        protected virtual void Start()
        {
            rb.mass = mass; // el Setup de cada objeto puede haber cambiado la masa
        }

        protected virtual void OnEnable() => All.Add(this);
        protected virtual void OnDisable() => All.Remove(this);

        public virtual string GetName(UseContext c) => Loc.T(displayName);
        public virtual string GetPrompt(UseContext c) => c.hands.IsEmpty ? "[E] Agarrar" : null;
        public virtual void Interact(UseContext c)
        {
            if (c.hands.IsEmpty) c.hands.Pick(this);
        }

        /// <summary>Texto del clic izquierdo cuando lo tenés en la mano.</summary>
        public virtual string GetUsePrompt(UseContext c) => null;
        /// <summary>Clic izquierdo con esto en la mano.</summary>
        public virtual void Use(UseContext c) { }
        /// <summary>Se llama todos los frames mientras lo tenés en la mano (para la rueda del mouse, etc.).</summary>
        public virtual void OnHeldUpdate(UseContext c) { }

        public void SetHeld(bool held, Transform anchor)
        {
            IsHeld = held;
            foreach (var col in GetComponentsInChildren<Collider>()) col.enabled = !held;
            if (held)
            {
                frozen = false;
                rb.isKinematic = true;
                transform.SetParent(anchor, false);
                transform.localPosition = holdOffset;
                transform.localRotation = Quaternion.Euler(holdEuler);
            }
            else
            {
                transform.SetParent(null, true);
                rb.isKinematic = frozen;
                if (!frozen) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
            }
        }

        /// <summary>Lo deja quieto en un lugar (gancho de herramientas).</summary>
        public void FreezeAt(Vector3 pos, Quaternion rot)
        {
            frozen = true;
            rb.isKinematic = true;
            transform.SetPositionAndRotation(pos, rot);
        }

        // --- Guardado: cada tipo guarda sus datos extra como texto ---
        public virtual string SaveExtra() => "";
        public virtual void LoadExtra(string data) { }

        /// <summary>Agrega un BoxCollider que envuelve todo lo visible.</summary>
        public void FitCollider()
        {
            var b = Shapes.RenderBounds(gameObject);
            var bc = GetComponent<BoxCollider>();
            if (bc == null) bc = gameObject.AddComponent<BoxCollider>();
            bc.center = transform.InverseTransformPoint(b.center);
            bc.size = new Vector3(b.size.x / transform.lossyScale.x, b.size.y / transform.lossyScale.y, b.size.z / transform.lossyScale.z);
        }
    }
}
