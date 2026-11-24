using UnityEngine;

namespace LaTranquera
{
    /// <summary>La computadora del mostrador: E para usarla (pedidos, precios, mejoras, banco).</summary>
    public class ComputerTerminal : MonoBehaviour, IInteractable
    {
        public string GetName(UseContext c) => Loc.T("Computadora");
        public string GetPrompt(UseContext c) => "[E] Usar (pedidos al proveedor, precios, mejoras)";
        public void Interact(UseContext c) => ComputerUI.I.Open();
    }
}
