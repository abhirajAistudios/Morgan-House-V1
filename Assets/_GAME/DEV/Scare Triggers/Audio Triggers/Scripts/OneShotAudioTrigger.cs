using System.Collections.Generic;
using UnityEngine;

/// Plays one or more audio clips once when the player enters the trigger zone.
public class OneShotAudioTrigger : MonoBehaviour
{
    private AudioSource audio;            // Reference to the AudioSource component
    
    [SerializeField] private List<AudioClip> soundClip;     // List of sound clips to play on trigger
    [SerializeField] private bool playOnce;
    private bool hasPlayed;

    private void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!playOnce)
            {
                PlayAudio();
            } 
            else if (playOnce && !hasPlayed)
            {
                PlayAudio();
                hasPlayed = true;
            }
        }
    }

    private void PlayAudio()
    {
        foreach (var clip in soundClip)
        {
            audio.PlayOneShot(clip); 
        }
    }
}