using System;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Audio
{
    [Serializable]
    public class AudioClipsPlayer
    {
        [SerializeField] private AudioClip[] _clips = Array.Empty<AudioClip>();

        [Range(0f, 2f)]
        [SerializeField] private float _meanPitch = 1f;

        [Range(0f, 1f)]
        [SerializeField] private float _pitchVariance;

        [Range(0f, 1f)]
        [SerializeField] private float _volume = 1f;

        private static Stack<AudioSource> s_pooledAudioSources = new Stack<AudioSource>();
        private static HashSet<AudioSource> s_activeAudioSources = new HashSet<AudioSource>();

        public void Update()
        {
            foreach (var activeAudioSource in s_activeAudioSources
                         .Where(activeAudioSource => !activeAudioSource.isPlaying)
                         .ToArray())
            {
                s_activeAudioSources.Remove(activeAudioSource);
                s_pooledAudioSources.Push(activeAudioSource);
            }
        }

        public float Pitch
        {
            get => s_activeAudioSources.FirstOrDefault()?.pitch ?? _meanPitch;
            set
            {
                foreach (var activeAudioSource in s_activeAudioSources)
                {
                    activeAudioSource.pitch = value;
                }
            }
        }

        public void Play()
        {
            Play(_volume, _meanPitch);
        }

        public void Play(float volume)
        {
            Play(volume, _meanPitch);
        }

        public void PlayPitched(float pitch)
        {
            Play(_volume, pitch);
        }

        public void Play(float volume, float pitch)
        {
            var source = GetSource();
            s_activeAudioSources.Add(source);
            ApplySettings(source, volume, pitch);
            source.Play();
        }

        private void ApplySettings(AudioSource source, float volume, float pitch)
        {
            source.clip = _clips.Length > 0 ? _clips[Random.Range(0, _clips.Length)] : null;
            source.pitch = pitch + Random.Range(-_pitchVariance, _pitchVariance);
            source.volume = volume;
        }

        private static AudioSource GetSource() =>
            s_pooledAudioSources.Count > 0
                ? s_pooledAudioSources.Pop()
                : MakeNewSource();

        private static AudioSource MakeNewSource()
        {
            var gameObject = new GameObject("Audio Source");
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.hideFlags = HideFlags.HideAndDontSave;

            return audioSource;
        }
    }
}