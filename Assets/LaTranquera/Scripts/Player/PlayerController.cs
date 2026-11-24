using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Movimiento en primera persona: WASD para caminar, Shift para correr,
    /// Espacio para saltar y el mouse para mirar.
    /// Va en el objeto "Jugador", que tiene un CharacterController.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController I;

        public Transform cameraPivot;
        public float walkSpeed = 3.2f;
        public float runSpeed = 5.5f;
        public float mouseSensitivity = 0.08f;
        public float gravity = -18f;
        public float jumpHeight = 0.9f;

        CharacterController cc;
        float pitch, vy;

        void Awake()
        {
            I = this;
            cc = GetComponent<CharacterController>();
        }

        void Start() => LockCursor(true);

        public static void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        void Update()
        {
            Vector2 look = GameInput.Look * mouseSensitivity;
            transform.Rotate(0f, look.x, 0f);
            pitch = Mathf.Clamp(pitch - look.y, -85f, 85f);
            cameraPivot.localEulerAngles = new Vector3(pitch, 0f, 0f);

            Vector2 m = GameInput.Move;
            Vector3 dir = transform.right * m.x + transform.forward * m.y;
            if (dir.sqrMagnitude > 1f) dir.Normalize();
            float speed = GameInput.Run ? runSpeed : walkSpeed;

            if (cc.isGrounded)
            {
                vy = -1f;
                if (GameInput.Jump) vy = Mathf.Sqrt(2f * -gravity * jumpHeight);
            }
            vy += gravity * Time.deltaTime;
            cc.Move((dir * speed + Vector3.up * vy) * Time.deltaTime);

            // Si se cae del mapa, vuelve al estacionamiento
            if (transform.position.y < -20f) Teleport(new Vector3(0, 1, 11), 0);
        }

        public void Teleport(Vector3 pos, float yaw)
        {
            cc.enabled = false;
            transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw, 0));
            cc.enabled = true;
            vy = 0;
        }
    }
}
