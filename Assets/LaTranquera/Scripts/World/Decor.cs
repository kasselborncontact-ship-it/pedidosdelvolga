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
        public string sound = "ok";

        public string GetName(UseContext c) => Loc.T(displayName);
        public string GetPrompt(UseContext c) => string.IsNullOrEmpty(action) ? null : action;

        public void Interact(UseContext c)
        {
            if (toggleLight != null) toggleLight.enabled = !toggleLight.enabled;
            if (!string.IsNullOrEmpty(message)) HUD.Toast(message);
            if (!string.IsNullOrEmpty(action)) Sfx.Play(sound, 0.4f);
        }
    }
}
