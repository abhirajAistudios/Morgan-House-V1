using System;
using UnityEngine;

    public class SoundService : GenericMonoSingleton<SoundService>
    {
        public AudioSource soundEffect;
        public AudioSource soundMusic;

        public SoundType[] Sounds;

        public bool isMute = false;
        
        private AudioListener audioListener;
        
        private void Start()
        {
            audioListener = GetComponent<AudioListener>();
            PlayMusic(global::Sounds.MAINMENUBGM);
        }

        public void Mute(bool status)
        {
            isMute = status;
        }

        public void PlayMusic(Sounds sound)
        {
            if (isMute)
                return;


            AudioClip clip = getSoundClip(sound);
            if (clip != null)
            {
                soundMusic.clip = clip;
                soundMusic.Play();
            }
            else
            {
                Debug.LogError("Clip not found for sound type: +  sound");
            }
        }

        public void Play(Sounds sound)
        {
            if (isMute)
                return;

            AudioClip clip = getSoundClip(sound);
            if (clip != null)
            {
                soundEffect.PlayOneShot(clip);
            }
            else
            {
                Debug.LogError("Clip not found for sound type: " + sound);
            }
        }

        private AudioClip getSoundClip(Sounds sound)
        {
            SoundType item = Array.Find(Sounds, i => i.soundType == sound);
            if (item != null)
                return item.soundClip;
            return null;
        }

        public void DisableAudioListener()
        {
            audioListener.enabled = false;
        }

        public void EnableAudioListener()
        {
            audioListener.enabled = true;
        }
    }

[Serializable]
    public class SoundType
    {
        public Sounds soundType;
        public AudioClip soundClip;
    }

    public enum Sounds
    {
        OBJECT,
        SWITCH,
        DOORUNLOCK,
        DOORLOCK,
        DOOROPEN,
        DOORCLOSE,
        LOCKPICKOBJ,
        LOCKPICKSOLVED,
        FLASHLIGHTOBJ,
        FLASHLIGHTBATTERYLOW,
        FLASHLIGHTRECHARGED,
        KEYOBJ,
        FUSEOBJ,
        FUSEINSERTED,
        FUSEPUZZLESOLVED,
        DIALPUZZLEOBJ,
        DIALINSERTED,
        DIALPUZZLESOLVED,
        BATTERYOBJ,
        LETTER,
        PHOTO,
        MAINMENUBGM,
        PAUSEMENUBGM,
        INGAMEBGM,
        LOADINGSCREENBGM,
        DIALROTATE,
    }