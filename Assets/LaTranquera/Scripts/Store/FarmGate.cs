using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LaTranquera
{
    /// <summary>Tranquera (portón de campo) que se abre y se cierra con E.</summary>
    public class FarmGate : MonoBehaviour, IInteractable
    {
        public Transform leaf;
        public float openAngle = 95f;
        bool open;
        float current;

        public string GetName(UseContext c) => Loc.T("Tranquera");
        public string GetPrompt(UseContext c) => open ? "[E] Cerrar la tranquera" : "[E] Abrir la tranquera";
        public void Interact(UseContext c) { open = !open; Sfx.Play("drop", 0.4f); }

        void Update()
        {
            if (leaf == null) return;
            current = Mathf.MoveTowards(current, open ? openAngle : 0, 90f * Time.deltaTime);
            leaf.localRotation = Quaternion.Euler(0, current, 0);
        }
    }
}
