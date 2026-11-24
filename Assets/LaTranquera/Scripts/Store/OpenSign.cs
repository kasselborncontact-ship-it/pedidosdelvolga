using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LaTranquera
{
    /// <summary>Cartel de ABIERTO / CERRADO colgado en la vidriera. E para darlo vuelta.</summary>
    public class OpenSign : MonoBehaviour, IInteractable
    {
        public Text frontText, backText;
        public Renderer board;

        void OnEnable() => StoreManager.Changed += Refresh;
        void OnDisable() => StoreManager.Changed -= Refresh;
        void Start() => Refresh();

        void Refresh()
        {
            bool open = StoreManager.I != null && StoreManager.I.isOpen;
            string t = Loc.T(open ? "ABIERTO" : "CERRADO");
            Color c = open ? new Color(0.1f, 0.55f, 0.15f) : new Color(0.75f, 0.1f, 0.1f);
            if (frontText) { frontText.text = t; frontText.color = c; }
            if (backText) { backText.text = t; backText.color = c; }
        }

        public string GetName(UseContext c) => Loc.T("Cartel") + ": " + Loc.T(StoreManager.I.isOpen ? "ABIERTO" : "CERRADO");
        public string GetPrompt(UseContext c) => StoreManager.I.isOpen ? "[E] Dar vuelta: CERRADO" : "[E] Dar vuelta: ABIERTO";

        public void Interact(UseContext c)
        {
            StoreManager.I.SetOpen(!StoreManager.I.isOpen);
            Sfx.Play("ok", 0.4f);
            if (StoreManager.I.isOpen) HUD.Toast("¡Almacén abierto! Los clientes van a empezar a llegar.");
        }
    }
}
