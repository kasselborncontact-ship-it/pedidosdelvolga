using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Las manos del jugador: puede llevar UNA sola cosa por vez.
    /// No hay inventario: lo que tenés en las manos es todo lo que llevás.
    /// </summary>
    public class PlayerHands : MonoBehaviour
    {
        public static PlayerHands I;

        /// <summary>Punto delante de la cámara donde se ve lo que llevás.</summary>
        public Transform holdAnchor;
        public Camera cam;

        public Carryable Held { get; private set; }
        public bool IsEmpty => Held == null;

        void Awake() => I = this;

        public void Pick(Carryable c)
        {
            if (Held != null || c == null) return;
            Held = c;
            c.SetHeld(true, holdAnchor);
            Sfx.Play("pick", 0.3f);
        }

        /// <summary>Saca el objeto de las manos sin tirarlo (por ejemplo, para colgarlo en un gancho).</summary>
        public Carryable Release()
        {
            var c = Held;
            Held = null;
            if (c != null) c.SetHeld(false, null);
            return c;
        }

        /// <summary>El objeto se "gasta" (por ejemplo, una lechuga que pusiste en la estantería).</summary>
        public void ConsumeHeld()
        {
            var c = Held;
            Held = null;
            if (c != null) Destroy(c.gameObject);
        }

        public static bool CanPlaceOn(UseContext ctx)
        {
            return ctx.hasHit && ctx.hit.distance < 3f && Vector3.Dot(ctx.hit.normal, Vector3.up) > 0.6f;
        }

        static readonly Collider[] buf = new Collider[16];
        static bool Overlapping(Carryable c, Collider[] own)
        {
            foreach (var col in own)
            {
                if (col.isTrigger) continue;
                var b = col.bounds;
                int n = Physics.OverlapBoxNonAlloc(b.center, b.extents * 0.95f, buf, col.transform.rotation, ~0, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < n; i++)
                    if (!buf[i].transform.IsChildOf(c.transform)) return true;
            }
            return false;
        }

        /// <summary>G: si mirás una superficie plana cerca (piso, mesa), lo apoya ahí. Si no, lo suelta.</summary>
        public void Drop(UseContext ctx)
        {
            var c = Release();
            if (c == null) return;
            if (CanPlaceOn(ctx))
            {
                c.transform.rotation = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0);
                c.transform.position = ctx.hit.point + Vector3.up * 0.02f;
                // si queda encimado con otra cosa, lo subimos un poco hasta que entre
                var cols = c.GetComponentsInChildren<Collider>();
                for (int tries = 0; tries < 10 && Overlapping(c, cols); tries++) c.transform.position += Vector3.up * 0.05f;
                Physics.SyncTransforms();
            }
            else
            {
                c.transform.position = cam.transform.position + cam.transform.forward * 0.8f - Vector3.up * 0.2f;
                c.rb.linearVelocity = cam.transform.forward * 1.5f;
            }
            Sfx.Play("drop", 0.3f);
        }
    }
}
