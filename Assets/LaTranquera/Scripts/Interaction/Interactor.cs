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

        void Update()
        {
            if (GameInput.Blocked)
            {
                CurrentName = null; CurrentPrompt = null;
                return;
            }

            ctx.hands = hands;
            var t = cam.transform;
            ctx.hasHit = Physics.Raycast(t.position, t.forward, out ctx.hit, reach, ~0, QueryTriggerInteraction.Collide);
            ctx.target = ctx.hasHit ? ctx.hit.collider.GetComponentInParent<IInteractable>() : null;
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
