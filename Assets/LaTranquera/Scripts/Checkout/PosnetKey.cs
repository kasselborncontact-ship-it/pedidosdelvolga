using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LaTranquera
{
    /// <summary>Una tecla del posnet.</summary>
    public class PosnetKey : MonoBehaviour, IInteractable
    {
        public string key;
        public Posnet posnet;
        public string GetName(UseContext c) => Loc.T("Posnet") + " — " + (key == "BORRAR" ? "Borrar" : key);
        public string GetPrompt(UseContext c) => c.hands.IsEmpty ? "[E] Apretar" : null;
        public void Interact(UseContext c) { if (c.hands.IsEmpty) posnet.Press(key); }
    }
}
