using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Objeto decorativo con una pequeña función: muestra su nombre y, con E, un mensajito
    /// (por ejemplo: la pava y el mate, la radio, la cocina a leña). Opcionalmente prende/apaga una luz.
    /// </summary>
    public class Decor : MonoBehaviour, IInteractable
    {
        public string displayName = "Objeto";
        public string action = "";       // ej.: "[E] Cebarse unos mates"
        public string message = "";      // lo que aparece al usarlo
        public Light toggleLight;        // si tiene, E prende/apaga esta luz
        public GameObject toggleObject;  // si tiene, E lo muestra/esconde (por ejemplo, el fuego)
        public string actionOff = "";    // texto de la acción cuando el objeto está prendido
        public string messageOff = "";
        public string sound = "ok";

        public string GetName(UseContext c) => Loc.T(displayName);
        bool On => toggleObject != null && toggleObject.activeSelf;

        public string GetPrompt(UseContext c)
        {
            if (On && !string.IsNullOrEmpty(actionOff)) return actionOff;
            return string.IsNullOrEmpty(action) ? null : action;
        }

        public void Interact(UseContext c)
        {
            bool wasOn = On;
            if (toggleLight != null) toggleLight.enabled = !toggleLight.enabled;
            if (toggleObject != null) toggleObject.SetActive(!toggleObject.activeSelf);
            string msg = wasOn && !string.IsNullOrEmpty(messageOff) ? messageOff : message;
            if (!string.IsNullOrEmpty(msg)) HUD.Toast(msg);
            if (!string.IsNullOrEmpty(action)) Sfx.Play(sound, 0.4f);
        }
    }
}
