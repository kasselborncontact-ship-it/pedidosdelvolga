using UnityEngine;

namespace LaTranquera
{
    /// <summary>Puerta que se abre y se cierra con E. La hoja gira sobre su bisagra (el objeto "leaf").</summary>
    public class Door : MonoBehaviour, IInteractable
    {
        public Transform leaf;
        public float openAngle = 100f;
        public bool open = true;
        public string displayName = "Puerta";
        float current;

        void Start() => current = open ? openAngle : 0;

        public string GetName(UseContext c) => Loc.T(displayName);
        public string GetPrompt(UseContext c) => open ? "[E] Cerrar" : "[E] Abrir";
        public void Interact(UseContext c) { open = !open; Sfx.Play("drop", 0.3f); }

        void Update()
        {
            if (leaf == null) return;
            current = Mathf.MoveTowards(current, open ? openAngle : 0, 160f * Time.deltaTime);
            leaf.localRotation = Quaternion.Euler(0, current, 0);
        }
    }
}
