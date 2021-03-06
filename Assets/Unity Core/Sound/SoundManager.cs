﻿using Util;
using System;
using Service;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Sound
{
    sealed public class SoundManager : MonoBehaviour, SoundService, UpdateReceiver
    {
        [Serializable]
        private class BaseSoundEntry<T>
        {
            [SerializeField]
            public T type;

            [SerializeField]
            public string resource;
        }

        [Serializable]
        private class SoundEntry : BaseSoundEntry<SoundType> {}

        [Serializable]
        private class MusicEntry : BaseSoundEntry<MusicType> {}

        [SerializeField]
        private int initialChannelCount;

        [SerializeField]
        private List<SoundEntry> defaultSounds;

        [SerializeField]
        private List<MusicEntry> defaultMusic;

        private Dictionary<SoundType, string> soundLookup;
        private Dictionary<MusicType, string> musicLookup;
        private ObjectCache<string, AudioClip> soundCache;

        private readonly List<AudioSource> freeChannels = new List<AudioSource>();
        private readonly List<AudioSource> activeChannels = new List<AudioSource>();

        private AudioSource musicChannel;

        public bool SoundMuted { get; private set; }
        public bool MusicMuted { get; private set; }
        public bool MusicPlaying { get { return musicChannel.isPlaying; } }
        public int SoundsPlaying { get { return activeChannels.Count; } }

        public void Start()
        {
            soundCache = new ObjectCache<string, AudioClip>
            {
                OnMissingKey = LoadClipCallback,
            };

            while (freeChannels.Count < initialChannelCount)
            {
                freeChannels.Add(CreateChannel());
            }

            musicChannel = CreateChannel();

            GlobalState.Instance.Services.SetInstance<SoundService>(this);

            GlobalState.EventService.Persistent.AddEventHandler<PlaySoundEvent>(OnPlaySound);
            GlobalState.EventService.Persistent.AddEventHandler<PlayMusicEvent>(OnPlayMusic);

            var settings = GlobalState.User.settings;
            SoundMuted = !settings.sfxOn;
            MusicMuted = !settings.musicOn;

            settings.AddListener(OnSettingsChanged);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void OnUpdate()
        {
            var index = 0;

            while (index < activeChannels.Count)
            {
                if (!activeChannels[index].isPlaying)
                {
                    var channel = activeChannels[index];

                    channel.Stop();
                    channel.clip = null;

                    freeChannels.Add(channel);
                    activeChannels.RemoveAt(index);
                }
                else
                {
                    index++;
                }
            }

            if (activeChannels.Count == 0)
            {
                GlobalState.UpdateService.Updates.Remove(this);
            }
        }

        public void PlaySound(AudioClip clip)
        {
            if (!SoundMuted)
            {
                PlayClip(clip);
            }
        }

        public void PlaySound(SoundType type)
        {
            PlaySound(GetSoundByType(type));
        }

        public void PlayMusic(AudioClip clip, bool loop)
        {
            StopMusic();

            musicChannel.clip = clip;
            musicChannel.loop = loop;

            RestartMusic();
        }

        public void PlayMusic(MusicType type, bool loop)
        {
            PlayMusic(GetMusicByType(type), loop);
        }

        public void StopMusic()
        {
            musicChannel.Stop();
        }

        public void RestartMusic()
        {
            if (!MusicMuted)
            {
                musicChannel.Play();
            }
        }

        public void PreloadSound(SoundType type)
        {
            soundLookup = soundLookup ?? BuildLookup<SoundEntry, SoundType>(defaultSounds);
            LoadClipAsync(soundLookup, type);
        }

        public void PreloadMusic(MusicType type)
        {
            musicLookup = musicLookup ?? BuildLookup<MusicEntry, MusicType>(defaultMusic);
            LoadClipAsync(musicLookup, type);
        }

        public AudioClip GetSoundByType(SoundType type)
        {
            soundLookup = soundLookup ?? BuildLookup<SoundEntry, SoundType>(defaultSounds);
            return LoadClipByType(soundLookup, type);
        }

        public AudioClip GetMusicByType(MusicType type)
        {
            musicLookup = musicLookup ?? BuildLookup<MusicEntry, MusicType>(defaultMusic);
            return LoadClipByType(musicLookup, type);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StopChannels(c => true);
            freeChannels.AddRange(activeChannels);
            activeChannels.Clear();

            soundCache.Clear();
        }

        private Dictionary<U, string> BuildLookup<T, U>(IEnumerable<T> entries) where T : BaseSoundEntry<U>
        {
            var result = new Dictionary<U, string>();

            foreach (var entry in entries)
            {
                result.Add(entry.type, entry.resource);
            }

            return result;
        }

        private AudioClip LoadClipByType<T>(Dictionary<T, string> lookup, T type)
        {
            return lookup.ContainsKey(type) ? soundCache.Get(lookup[type]) : null;
        }

        private void LoadClipAsync<T>(Dictionary<T, string> lookup, T type)
        {
            string name;

            if (lookup.TryGetValue(type, out name))
            {
                if (!soundCache.Contains(name))
                {
                    GlobalState.AssetService.LoadAssetAsync<AudioClip>(name, c => soundCache.Add(name, c));
                }
            }
        }

        private AudioSource CreateChannel()
        {
            var channel = gameObject.AddComponent<AudioSource>();

            channel.playOnAwake = false;

            return channel;
        }

        private AudioSource PlayClip(AudioClip clip)
        {
            var channel = GetFreeChannel();

            channel.clip = clip;
            channel.loop = false;
            channel.Play();

            activeChannels.Add(channel);

            if (activeChannels.Count == 1)
            {
                GlobalState.UpdateService.Updates.Add(this);
            }

            return channel;
        }

        private AudioSource GetFreeChannel()
        {
            if (freeChannels.Count == 0)
            {
                freeChannels.Add(CreateChannel());
            }

            var channel = freeChannels[0];
            freeChannels.RemoveAt(0);

            return channel;
        }

        private void OnPlaySound(PlaySoundEvent gameEvent)
        {
            PlaySound(gameEvent.clip);
        }

        private void OnPlayMusic(PlayMusicEvent gameEvent)
        {
            PlayMusic(gameEvent.clip, gameEvent.loop);
        }

        private void StopChannels(Func<AudioSource, bool> predicate)
        {
            foreach (var channel in activeChannels.Where(predicate))
            {
                channel.Stop();
            }
        }

        private void OnSettingsChanged(Observable target)
        {
            var settings = target as State.Settings;
            bool restartMusic = (MusicMuted && settings.musicOn);

            if (!SoundMuted && !settings.sfxOn)
            {
                StopChannels(c => c != musicChannel);
            }

            if (!MusicMuted && !settings.musicOn)
            {
                StopMusic();
            }

            SoundMuted = !settings.sfxOn;
            MusicMuted = !settings.musicOn;

            if (restartMusic)
            {
                RestartMusic();
            }
        }

        private AudioClip LoadClipCallback(string path)
        {
            return GlobalState.AssetService.LoadAsset<AudioClip>(path);
        }
    }
}
