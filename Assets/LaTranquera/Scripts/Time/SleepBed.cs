using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// La cama de la casa. Desde las 19:00 se puede dormir: pasa la noche hasta las 6:00,
    /// las plantas crecen (si tenían agua), se cobran los gastos y se guarda la partida.
    /// </summary>
    public class SleepBed : MonoBehaviour, IInteractable
    {
        public float earliestHour = 19f;
        public Transform wakeUpPoint;

        public string GetName(UseContext c) => Loc.T("Cama");

        public string GetPrompt(UseContext c)
        {
            float h = TimeManager.I.Hour;
            bool can = h >= earliestHour || h < 6f;
            return can ? "[E] Dormir hasta mañana (guarda la partida)" : "Podés dormir desde las " + earliestHour + ":00";
        }

        public void Interact(UseContext c)
        {
            float h = TimeManager.I.Hour;
            if (h >= earliestHour || h < 6f) GameManager.I.Sleep(false);
            else HUD.Toast("Todavía es temprano para dormir");
        }
    }
}
