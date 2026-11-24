using System;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Reloj del juego. Por defecto 1 segundo real = 1 minuto de juego
    /// (un día de 6:00 a 24:00 dura 18 minutos reales). Cambialo en "minutosPorSegundo".
    /// Avisa a los demás sistemas cuando pasa cada hora y cuando empieza un día nuevo.
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager I;

        public float minutosPorSegundo = 1f;
        /// <summary>Minutos totales desde el comienzo de la partida (día 1 a las 0:00 = 0).</summary>
        public double totalMinutes = 6 * 60;
        public bool paused;

        public static event Action<int> HourPassed;   // hora del día (0-23)
        public static event Action<int> NewDay;       // número de día

        public int Day => (int)(totalMinutes / 1440) + 1;
        public float MinuteOfDay => (float)(totalMinutes % 1440);
        public float Hour => MinuteOfDay / 60f;
        public string Clock
        {
            get
            {
                int m = (int)MinuteOfDay;
                return (m / 60).ToString("00") + ":" + (m % 60).ToString("00");
            }
        }

        void Awake() => I = this;

        void Update()
        {
            if (paused) return;
            Advance(Time.deltaTime * minutosPorSegundo);
        }

        /// <summary>Avanza el reloj y dispara los eventos de cada hora que pasa.</summary>
        public void Advance(double minutes)
        {
            if (minutes <= 0) return;
            double before = totalMinutes;
            totalMinutes += minutes;
            long h0 = (long)(before / 60), h1 = (long)(totalMinutes / 60);
            for (long h = h0 + 1; h <= h1; h++)
            {
                int hourOfDay = (int)(h % 24);
                if (hourOfDay == 0) NewDay?.Invoke((int)(h / 24) + 1);
                HourPassed?.Invoke(hourOfDay);
            }
        }

        /// <summary>Minutos hasta la próxima vez que el reloj marque esa hora.</summary>
        public double MinutesUntil(float hourOfDay)
        {
            double target = hourOfDay * 60;
            double now = MinuteOfDay;
            double diff = target - now;
            if (diff <= 0) diff += 1440;
            return diff;
        }

        public static string FormatClock(double total)
        {
            int m = (int)(total % 1440);
            return "Día " + ((int)(total / 1440) + 1) + " " + (m / 60).ToString("00") + ":" + (m % 60).ToString("00");
        }
    }
}
