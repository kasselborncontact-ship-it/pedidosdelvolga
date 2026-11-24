using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LaTranquera
{
    /// <summary>
    /// El "director" del juego: al empezar carga la partida guardada (o arma una nueva),
    /// maneja la pausa (Esc), el guardado rápido (F5) y el dormir.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager I;
        public NewGameSetup newGameSetup;
        public SleepBed bed;
        bool paused, sleeping;

        void Awake()
        {
            I = this;
            Application.targetFrameRate = 120;
        }

        void Start()
        {
            GameInput.Blocked = false;
            bool loaded = SaveSystem.HasSave && SaveSystem.Load();
            if (!loaded && newGameSetup != null) newGameSetup.Spawn();
            HUD.Toast(loaded ? "Partida cargada" : "¡Bienvenido a La Tranquera! Empezá por el galpón: ahí están la azada y las semillas.");
        }

        void Update()
        {
            if (GameInput.Pause && !sleeping)
            {
                if (ComputerUI.I != null && ComputerUI.I.IsOpen) ComputerUI.I.Close();
                else TogglePause();
            }
            if (GameInput.QuickSave && !sleeping && !paused) { SaveSystem.Save(); HUD.Toast("Partida guardada"); }

            // Si te quedás despierto hasta las 2:00, te dormís solo
            if (!sleeping && TimeManager.I != null && TimeManager.I.Hour >= 2f && TimeManager.I.Hour < 5f) Sleep(true);

#if UNITY_EDITOR || DEBUG
            // Truco de prueba: F8 adelanta 6 horas (las plantas crecen si tienen agua)
            if (GameInput.DebugSkipTime && !sleeping)
            {
                TimeManager.I.Advance(360);
                HUD.Toast("[Prueba] +6 horas");
            }
#endif
        }

        public void TogglePause()
        {
            paused = !paused;
            HUD.I.ShowPause(paused);
            GameInput.Blocked = paused;
            PlayerController.LockCursor(!paused);
            Time.timeScale = paused ? 0f : 1f;
        }

        public void Sleep(bool passedOut)
        {
            if (sleeping) return;
            StartCoroutine(SleepRoutine(passedOut));
        }

        IEnumerator SleepRoutine(bool passedOut)
        {
            sleeping = true;
            GameInput.Blocked = true;
            yield return HUD.I.FadeTo(1f, 1.2f, passedOut ? "Te quedaste dormido de cansancio..." : "Durmiendo...");
            if (CustomerSpawner.I != null) CustomerSpawner.I.ClearAll();

            var tm = TimeManager.I;
            tm.Advance(tm.MinutesUntil(6f));   // pasan las horas: crecen las plantas, se cobra la luz

            int deposited = Economy.I.DepositDrawer();
            var e = Economy.I;
            string summary = "Día " + tm.Day + " — 06:00\n\n" +
                             "Ayer vendiste " + Money.Fmt(e.today.sales) + " a " + e.today.customers + " clientes\n" +
                             "Gastos: " + Money.Fmt(e.today.expenses) + "\n" +
                             (deposited > 0 ? "Depositaste la caja: " + Money.Fmt(deposited) + "\n" : "") +
                             "Banco: " + Money.Fmt(e.bank);
            e.CloseDay();

            if (bed != null && bed.wakeUpPoint != null)
                PlayerController.I.Teleport(bed.wakeUpPoint.position, bed.wakeUpPoint.eulerAngles.y);
            if (PlayerHands.I != null && !PlayerHands.I.IsEmpty && passedOut) { /* lo seguís teniendo en la mano */ }

            SaveSystem.Save();
            yield return HUD.I.FadeTo(1f, 0.1f, summary + "\n\nPartida guardada");
            yield return new WaitForSecondsRealtime(3.5f);
            yield return HUD.I.FadeTo(0f, 1f);
            GameInput.Blocked = false;
            sleeping = false;
        }

        public void NewGame()
        {
            SaveSystem.Delete();
            PriceBook.Clear();
            Upgrades.Reset();
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Quit()
        {
            SaveSystem.Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
