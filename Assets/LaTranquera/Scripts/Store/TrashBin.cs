using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LaTranquera
{
    /// <summary>Tacho de basura: clic con una caja vacía en la mano para tirarla.</summary>
    public class TrashBin : MonoBehaviour, IInteractable
    {
        public string GetName(UseContext c) => Loc.T("Tacho de basura");
        public string GetPrompt(UseContext c)
        {
            var box = c.Held as SupplierBox;
            if (box != null) return box.IsEmpty ? "[E] Tirar la caja vacía" : "La caja todavía tiene mercadería";
            return null;
        }
        public void Interact(UseContext c)
        {
            var box = c.Held as SupplierBox;
            if (box != null && box.IsEmpty) { c.hands.ConsumeHeld(); Sfx.Play("drop"); }
        }
    }
}
