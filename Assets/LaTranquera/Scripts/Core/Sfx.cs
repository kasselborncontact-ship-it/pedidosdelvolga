using System.Collections.Generic;
using UnityEngine;

namespace LaTranquera
{
    /// <summary>
    /// Sonidos simples generados por código (bip del lector, error, agua...).
    /// Para sonidos reales: poné archivos .wav en Assets/LaTranquera/Resources/Sfx/ con el mismo
    /// nombre (por ejemplo "beep.wav") y se usan automáticamente en lugar de los generados.
    /// </summary>
    public static class Sfx
    {
        static AudioSource src;
        static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

        public static void Play(string name, float volume = 0.6f)
        {
            if (!Application.isPlaying) return;
            if (src == null)
            {
                var go = new GameObject("_Sonidos");
                Object.DontDestroyOnLoad(go);
                src = go.AddComponent<AudioSource>();
                src.spatialBlend = 0f;
            }
            src.PlayOneShot(GetClip(name), volume);
        }

        static AudioClip GetClip(string name)
        {
            if (clips.TryGetValue(name, out var c)) return c;
            c = Resources.Load<AudioClip>("Sfx/" + name);
            if (c == null)
            {
                switch (name)
                {
                    case "beep": c = Tone(name, 1800, 0.11f, 0); break;
                    case "error": c = Tone(name, 180, 0.35f, 1); break;
                    case "ok": c = Tone(name, 900, 0.18f, 0, 1350); break;
                    case "cash": c = Tone(name, 1300, 0.25f, 0, 2100); break;
                    case "pick": c = Tone(name, 420, 0.06f, 0); break;
                    case "drop": c = Tone(name, 260, 0.08f, 0); break;
                    case "water": c = Noise(name, 0.6f); break;
                    case "dig": c = Noise(name, 0.18f); break;
                    case "key": c = Tone(name, 2400, 0.04f, 0); break;
                    default: c = Tone(name, 600, 0.1f, 0); break;
                }
            }
            clips[name] = c;
            return c;
        }

        static AudioClip Tone(string name, float freq, float seconds, int wave, float freq2 = 0)
        {
            int rate = 44100, n = (int)(rate * seconds);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / rate;
                float f = (freq2 > 0 && i > n / 2) ? freq2 : freq;
                float s = Mathf.Sin(2 * Mathf.PI * f * t);
                if (wave == 1) s = Mathf.Sign(s) * 0.5f;
                float env = Mathf.Clamp01(Mathf.Min(i / (rate * 0.005f), (n - i) / (rate * 0.02f)));
                data[i] = s * env * 0.5f;
            }
            var clip = AudioClip.Create(name, n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Noise(string name, float seconds)
        {
            int rate = 22050, n = (int)(rate * seconds);
            var data = new float[n];
            var rnd = new System.Random(1);
            float prev = 0;
            for (int i = 0; i < n; i++)
            {
                float w = (float)(rnd.NextDouble() * 2 - 1);
                prev = prev * 0.7f + w * 0.3f;
                float env = Mathf.Clamp01(Mathf.Min(i / (rate * 0.03f), (n - i) / (rate * 0.1f)));
                data[i] = prev * env * 0.6f;
            }
            var clip = AudioClip.Create(name, n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
