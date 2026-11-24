using System.Text;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Va en la cámara del jugador. Cada frame tira un rayo desde el centro de la pantalla,
    /// ve qué estás mirando y reparte las teclas:
    ///   E  = usar / agarrar lo que mirás
    ///   Clic izquierdo = usar lo que tenés en la mano sobre lo que mirás
    ///   G  = soltar / apoyar
    ///   F  = abrir (cajas)
    /// </summary>
    public class Interactor : MonoBehaviour
    {
        public Camera cam;
        public PlayerHands hands;
        public float reach = 3f;

        public static string CurrentName { get; private set; }
        public static string CurrentPrompt { get; private set; }

        readonly UseContext ctx = new UseContext();
        readonly StringBuilder sb = new StringBuilder();
        readonly RaycastHit[] hits = new RaycastHit[16];
        readonly Collider[] near = new Collider[16];

        /// <summary>
        /// Elige qué estás mirando. En vez de quedarse con lo primero que toca el rayo, prueba lo que hay
        /// en la línea de mira (hasta la primera pared) y se queda con lo primero que se PUEDE usar.
        /// Además, si con las manos vacías apuntás cerca de algo que se agarra, te ayuda a apuntar.
        /// </summary>
        void Pick(Transform t)
        {
            ctx.hasHit = false;
            ctx.target = null;
            int n = Physics.RaycastNonAlloc(t.position, t.forward, hits, reach, ~0, QueryTriggerInteraction.Collide);
            System.Array.Sort(hits, 0, n, HitComparer.I);
            RaycastHit? first = null;
            for (int i = 0; i < n; i++)
            {
                var h = hits[i];
                if (h.collider == null || (hands.Held != null && h.collider.transform.IsChildOf(hands.Held.transform))) continue;
                if (first == null) first = h;
                var it = h.collider.GetComponentInParent<IInteractable>();
                if (it != null && Useful(it, h)) { Set(h, it); return; }
                if (!h.collider.isTrigger) break; // una pared o un objeto sólido tapa lo que está atrás
            }
            if (first.HasValue) Set(first.Value, first.Value.collider.GetComponentInParent<IInteractable>());

            // Ayuda para agarrar: con las manos vacías, busca algo agarrable cerca del punto que mirás
            if (hands.IsEmpty && !(ctx.target is Carryable))
            {
                Vector3 p = ctx.hasHit ? ctx.hit.point : t.position + t.forward * Mathf.Min(reach, 2f);
                int m = Physics.OverlapSphereNonAlloc(p, 0.3f, near, ~0, QueryTriggerInteraction.Ignore);
                Carryable best = null; float bd = float.MaxValue;
                for (int i = 0; i < m; i++)
                {
                    var c = near[i].GetComponentInParent<Carryable>();
                    if (c == null || c.IsHeld) continue;
                    float d = Vector3.Cross(t.forward, c.transform.position - t.position).magnitude; // distancia a la línea de mira
                    if (d < bd && Vector3.Distance(t.position, c.transform.position) <= reach + 0.3f) { bd = d; best = c; }
                }
                if (best != null && (ctx.target == null || ctx.target.GetPrompt(ctx) == null))
                {
                    ctx.target = best;
                    if (!ctx.hasHit) { ctx.hasHit = true; ctx.hit = default; }
                }
            }
        }

        void Set(RaycastHit h, IInteractable it)
        {
            ctx.hasHit = true;
            ctx.hit = h;
            ctx.target = it;
        }

        bool Useful(IInteractable it, RaycastHit h)
        {
            ctx.hasHit = true; ctx.hit = h; ctx.target = it;
            if (!string.IsNullOrEmpty(it.GetPrompt(ctx))) return true;
            if (hands.Held != null && !string.IsNullOrEmpty(hands.Held.GetUsePrompt(ctx))) return true;
            return false;
        }

        class HitComparer : System.Collections.Generic.IComparer<RaycastHit>
        {
            public static readonly HitComparer I = new HitComparer();
            public int Compare(RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance);
        }

        void Update()
        {
            if (GameInput.Blocked)
            {
                CurrentName = null; CurrentPrompt = null;
                return;
            }

            ctx.hands = hands;
            var t = cam.transform;
            Pick(t);
            var openable = ctx.Get<IOpenable>();

            // --- Textos de ayuda ---
            CurrentName = ctx.target?.GetName(ctx);
            sb.Clear();
            string p = ctx.target?.GetPrompt(ctx);
            if (!string.IsNullOrEmpty(p)) sb.AppendLine(Loc.T(p));
            if (openable != null && hands.IsEmpty)
            {
                string op = openable.GetOpenPrompt();
                if (!string.IsNullOrEmpty(op)) sb.AppendLine(Loc.T(op));
            }
            if (!hands.IsEmpty)
            {
                string up = hands.Held.GetUsePrompt(ctx);
                if (!string.IsNullOrEmpty(up)) sb.AppendLine(Loc.T(up));
                sb.AppendLine(Loc.T(PlayerHands.CanPlaceOn(ctx) ? "[G] Apoyar" : "[G] Soltar"));
            }
            CurrentPrompt = sb.ToString();

            // --- Acciones ---
            if (GameInput.Interact && ctx.target != null)
                ctx.target.Interact(ctx);
            else if (GameInput.Open && openable != null && hands.IsEmpty)
                openable.Open();
            else if (GameInput.UsePressed && !hands.IsEmpty)
                hands.Held.Use(ctx);

            if (!hands.IsEmpty) hands.Held.OnHeldUpdate(ctx);

            if (GameInput.Drop && !hands.IsEmpty)
                hands.Drop(ctx);
        }
    }
}
