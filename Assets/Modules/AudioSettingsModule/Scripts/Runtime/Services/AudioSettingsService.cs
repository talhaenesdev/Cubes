using System;
using UnityEngine;

namespace Modules.AudioSettingsModule.Services
{
    public class AudioSettingsService : IAudioSettingsService
    {
        private const string MasterKey = "audio.master";
        private const string MusicKey = "audio.music";

        public float Master { get; private set; }
        public float Music { get; private set; }
        public event Action Changed;

        public AudioSettingsService()
        {
            Master = PlayerPrefs.GetFloat(MasterKey, 1f);
            Music = PlayerPrefs.GetFloat(MusicKey, 0.8f);
            AudioListener.volume = Master;
        }

        public void SetMaster(float value)
        {
            Master = Mathf.Clamp01(value);
            AudioListener.volume = Master;
            PlayerPrefs.SetFloat(MasterKey, Master);
            Changed?.Invoke();
        }

        public void SetMusic(float value)
        {
            Music = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MusicKey, Music);
            Changed?.Invoke();
        }
    }
}
