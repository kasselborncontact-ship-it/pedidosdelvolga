using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace LaTranquera
{
    /// <summary>
    /// Todas las teclas del juego en un solo lugar.
    /// Usa el "Input System" nuevo de Unity (viene instalado en Unity 6).
    /// Si querés cambiar una tecla, cambiala acá y listo.
    /// </summary>
    public static class GameInput
    {
        /// <summary>Cuando hay una pantalla abierta (computadora, pausa) el jugador no se mueve.</summary>
        public static bool Blocked;

        static Keyboard K => Keyboard.current;
        static Mouse M => Mouse.current;

        public static Vector2 Move
        {
            get
            {
                if (Blocked || K == null) return Vector2.zero;
                float x = (K.dKey.isPressed ? 1 : 0) - (K.aKey.isPressed ? 1 : 0);
                float y = (K.wKey.isPressed ? 1 : 0) - (K.sKey.isPressed ? 1 : 0);
                return new Vector2(x, y);
            }
        }

        public static Vector2 Look => (Blocked || M == null) ? Vector2.zero : M.delta.ReadValue();
        public static bool Run => !Blocked && K != null && K.leftShiftKey.isPressed;
        public static bool Jump => Pressed(K?.spaceKey);

        /// <summary>E: agarrar / usar lo que mirás.</summary>
        public static bool Interact => Pressed(K?.eKey);
        /// <summary>G: soltar o apoyar lo que tenés en las manos.</summary>
        public static bool Drop => Pressed(K?.gKey);
        /// <summary>F: abrir una caja.</summary>
        public static bool Open => Pressed(K?.fKey);
        /// <summary>Clic izquierdo: usar lo que tenés en la mano sobre lo que mirás.</summary>
        public static bool UsePressed => !Blocked && M != null && M.leftButton.wasPressedThisFrame;

        /// <summary>Rueda del mouse: -1, 0 o 1.</summary>
        public static int Scroll
        {
            get
            {
                if (Blocked || M == null) return 0;
                float v = M.scroll.ReadValue().y;
                return v > 0.01f ? 1 : (v < -0.01f ? -1 : 0);
            }
        }

        // Estas funcionan aunque haya una pantalla abierta
        public static bool Pause => K != null && K.escapeKey.wasPressedThisFrame;
        public static bool QuickSave => K != null && K.f5Key.wasPressedThisFrame;
        public static bool DebugSkipTime => K != null && K.f8Key.wasPressedThisFrame;

        static bool Pressed(KeyControl k) => !Blocked && k != null && k.wasPressedThisFrame;
    }
}
