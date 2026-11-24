using System.Collections.Generic;
using System.Globalization;

namespace LaTranquera
{
    /// <summary>
    /// Traducciones. El juego está escrito en español y el texto en español es la "clave".
    /// Para traducir al inglés, agregá la frase en la tabla "En" y poné Loc.Language = "en".
    /// Para frases con datos usá {0}, {1}: Loc.F("Faltan {0}", plata).
    /// </summary>
    public static class Loc
    {
        public static string Language = "es";

        static readonly Dictionary<string, string> En = new Dictionary<string, string>
        {
            { "[E] Agarrar", "[E] Pick up" },
            { "[G] Soltar", "[G] Drop" },
            { "ABIERTO", "OPEN" },
            { "CERRADO", "CLOSED" },
            { "Día {0}", "Day {0}" },
            { "Banco", "Bank" },
            { "Caja", "Till" },
            { "En las manos: {0}", "Holding: {0}" },
            { "Nada", "Nothing" },
            { "Cajón de madera", "Wooden crate" },
            { "Lechuga", "Lettuce" },
            { "Tomate", "Tomato" },
            { "Atado de zanahorias", "Carrot bunch" },
            { "Azada", "Hoe" },
            { "Regadera", "Watering can" },
            { "Etiquetadora", "Price labeler" },
            { "Estantería", "Shelf" },
            { "Heladera", "Fridge" },
            { "Partida guardada", "Game saved" },
            // ...agregá más acá
        };

        public static string T(string es)
        {
            if (Language == "es" || string.IsNullOrEmpty(es)) return es;
            return En.TryGetValue(es, out var t) ? t : es;
        }

        public static string F(string es, params object[] args) => string.Format(T(es), args);
    }

    /// <summary>Formato de plata argentino: $ 12.500</summary>
    public static class Money
    {
        public static string Fmt(int pesos)
        {
            string s = System.Math.Abs(pesos).ToString("N0", CultureInfo.InvariantCulture).Replace(",", ".");
            return (pesos < 0 ? "-$ " : "$ ") + s;
        }
    }
}
