using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LaTranquera
{
    /// <summary>
    /// Gancho o lugar fijo de una herramienta. E con la herramienta en la mano para dejarla.
    /// </summary>
    public class ToolHook : MonoBehaviour, IInteractable
    {
        public string acceptsKind = "hoe";
        public string label = "Lugar de la azada";

        public string GetName(UseContext c) => Loc.T(label);
        public string GetPrompt(UseContext c)
        {
            if (c.Held != null && c.Held.kind == acceptsKind) return "[E] Dejar " + Loc.T(c.Held.displayName) + " en su lugar";
            return null;
        }
        public void Interact(UseContext c)
        {
            if (c.Held == null || c.Held.kind != acceptsKind) return;
            var t = c.hands.Release();
            t.FreezeAt(transform.position, transform.rotation);
            Sfx.Play("drop", 0.3f);
        }
    }
}
