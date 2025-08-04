using System;
using UnityEngine;

namespace Inventory.Sound
{
    public class SoundService : MonoBehaviour
    {
        public AudioSource soundEffect;
        public AudioSource soundMusic;

        public SoundType[] Sounds;

        public bool isMute = false;
        
        private void Start()
        {
            PlayMusic(global::Sounds.MUSIC);
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
        DOOR,
        SWITCH,
        MUSIC,
        ERROR,
    }