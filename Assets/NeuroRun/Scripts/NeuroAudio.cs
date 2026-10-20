using UnityEngine;

namespace NeuroRun
{
    public class NeuroAudio : MonoBehaviour
    {
        public enum Cue { Collect, Correct, Wrong, Damage, Dash, Win, Click }
        public AudioClip music;
        public AudioClip[] effects;
        public bool Muted { get; private set; }
        AudioSource soundtrack, fx;
        void Awake()
        {
            soundtrack = gameObject.AddComponent<AudioSource>();
            soundtrack.clip = music; soundtrack.loop = true; soundtrack.volume = .19f;
            fx = gameObject.AddComponent<AudioSource>(); fx.volume = .45f;
            Muted = PlayerPrefs.GetInt("NeuroRun.Muted", 0) == 1;
            ApplyMute();
        }
        // O primeiro clique/submit libera o áudio também no navegador.
        public void StartMusic() { if (!soundtrack.isPlaying) soundtrack.Play(); }
        public void Play(Cue cue)
        { int i = (int)cue; if (i < effects.Length && effects[i] != null) fx.PlayOneShot(effects[i]); }
        public void ToggleMute()
        { Muted = !Muted; ApplyMute(); PlayerPrefs.SetInt("NeuroRun.Muted", Muted ? 1 : 0); PlayerPrefs.Save(); }
        void ApplyMute() { soundtrack.mute = Muted; fx.mute = Muted; }
    }
}
