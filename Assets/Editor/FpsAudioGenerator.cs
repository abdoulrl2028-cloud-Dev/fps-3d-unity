#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace FPS.EditorTools
{
    /// <summary>
    /// Generates real WAV audio assets (ambient music, combat music and gunshot SFX)
    /// procedurally so the game has actual sound without third party files.
    /// </summary>
    public static class FpsAudioGenerator
    {
        private const int SampleRate = 22050;

        [MenuItem("FPS/Generate Audio Assets")]
        public static void GenerateAll()
        {
            EnsureFolder("Assets/Audio/Music");
            EnsureFolder("Assets/Audio/SFX");
            EnsureFolder("Assets/Audio/Ambient");

            WriteWav("Assets/Audio/Music/Ambient_Loop.wav", CreateAmbientLoop(8f));
            WriteWav("Assets/Audio/Music/Combat_Loop.wav", CreateCombatLoop(4f));

            WriteWav("Assets/Audio/SFX/Gunshot_Pistol.wav", CreateGunshot(0.12f, 420f, 0.35f));
            WriteWav("Assets/Audio/SFX/Gunshot_Rifle.wav", CreateGunshot(0.18f, 260f, 0.5f));
            WriteWav("Assets/Audio/SFX/Gunshot_Shotgun.wav", CreateGunshot(0.4f, 140f, 0.75f));
            WriteWav("Assets/Audio/SFX/Reload.wav", CreateGunshot(0.15f, 30f, 0.15f));
            WriteWav("Assets/Audio/SFX/Enemy_Attack.wav", CreateEnemyAttack(0.3f));

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            Debug.Log("[FPS] Audio assets generated.");
        }

        public static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace('\\', '/');
                string folder = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }

        private static void WriteWav(string path, float[] samples)
        {
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }
            File.WriteAllBytes(path, BuildWav(samples));
            AssetDatabase.ImportAsset(path, ImportAssetOptions.Default);
        }

private static byte[] BuildWav(float[] samples)
        {
            byte[] result;
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                int dataSize = samples.Length * 2;
                int byteRate = SampleRate * 2;

                w.Write("RIFF".ToCharArray());

                w.Write("WAVE".ToCharArray());

                w.Write("fmt ".ToCharArray());
                w.Write(16);
                w.Write((short)1);
                w.Write((short)1);
                w.Write(SampleRate);
                w.Write(byteRate);
                w.Write((short)2);
                w.Write((short)16);

                w.Write("data".ToCharArray());
                w.Write(dataSize);

                for (int i = 0; i < samples.Length; i++)
                {
                    short v = (short)Mathf.Clamp(Mathf.RoundToInt(samples[i] * 32767f), short.MinValue, short.MaxValue);
                    w.Write(v);
                }

                w.Flush();
                result = ms.ToArray();
            }
            return result;
        }

        private static float[] CreateAmbientLoop(float seconds)
        {
            int n = (int)(SampleRate * seconds);
            float[] outSamples = new float[n];
            float[] chords = { 0f, 7f, 10f, 12f, 19f, 24f };

            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate;
                float v = 0f;
                for (int c = 0; c < chords.Length; c++)
                {
                    float freq = 82.41f * Mathf.Pow(2f, chords[c] / 12f);
                    float envelope = Mathf.Sin(Mathf.PI * i / n);
                    v += Mathf.Sin(2f * Mathf.PI * freq * t) * 0.08f * envelope;
                    v += Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.03f * envelope;
                }
                outSamples[i] = Mathf.Clamp(v, -1f, 1f) * 0.55f;
            }
            FadeInOut(outSamples);
            return outSamples;
        }

        private static float[] CreateCombatLoop(float seconds)
        {
            int n = (int)(SampleRate * seconds);
            float[] outSamples = new float[n];
            float[] notes = { 0f, 3f, 7f, 10f, 3f, 7f };

            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate;
                float v = 0f;
                int chordIdx = (int)(t * 1.5f) % notes.Length;
                float baseF = 55f * Mathf.Pow(2f, notes[chordIdx] / 12f);

                v += Mathf.Sin(2f * Mathf.PI * baseF * t) * 0.25f;
                v += Mathf.Sin(2f * Mathf.PI * baseF * 2f * t) * 0.12f;
                v += Mathf.Sin(2f * Mathf.PI * baseF * 3f * t) * 0.06f;

                float pulse = (Mathf.Sin(2f * Mathf.PI * 3f * t) > 0.85f) ? 0.15f : 0f;
                v += pulse * Mathf.Sin(2f * Mathf.PI * 60f * t);

                outSamples[i] = Mathf.Clamp(v, -1f, 1f);
            }
            FadeInOut(outSamples);
            for (int i = 0; i < n; i++)
                outSamples[i] *= 0.85f;
            return outSamples;
        }

        private static float[] CreateGunshot(float seconds, float baseFreq, float gain)
        {
            int n = (int)(SampleRate * seconds);
            float[] outSamples = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate;
                float decay = Mathf.Exp(-t * 22f);
                float noise = 2f * (RandomX(i * 3 + 11) - 0.5f);
                float tone = Mathf.Sin(2f * Mathf.PI * baseFreq * t) * Mathf.Exp(-t * 18f);
                outSamples[i] = (noise * 0.85f + tone * 0.15f) * decay * gain;
            }
            return outSamples;
        }

        private static float[] CreateEnemyAttack(float seconds)
        {
            int n = (int)(SampleRate * seconds);
            float[] outSamples = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate;
                float decay = Mathf.Exp(-t * 8f);
                float sweep = Mathf.Sin(2f * Mathf.PI * (450f + 850f * t) * t);
                float noise = 2f * (RandomX(i * 7 + 3) - 0.5f);
                outSamples[i] = (sweep * 0.6f + noise * 0.4f) * decay * 0.4f;
            }
            return outSamples;
        }

        private static float RandomX(int seed)
        {
            seed = (seed ^ 61) ^ (seed >> 16);
            seed *= 9;
            seed ^= seed >> 4;
            seed *= 0x27d4eb2d;
            seed ^= seed >> 15;
            uint u = (uint)seed;
            return (u & 0x7fffffff) / (float)0x7fffffff;
        }

        private static void FadeInOut(float[] samples)
        {
            int fade = samples.Length / 16;
            for (int i = 0; i < fade; i++)
            {
                float k = (float)i / fade;
                samples[i] *= k;
                samples[samples.Length - 1 - i] *= k;
            }
        }
    }
}
#endif