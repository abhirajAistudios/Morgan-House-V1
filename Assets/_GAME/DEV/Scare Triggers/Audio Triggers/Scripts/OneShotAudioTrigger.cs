using System.Collections.Generic;
using UnityEngine;

/// Plays one or more audio clips once when the player enters the trigger zone.
public class OneShotAudioTrigger : MonoBehaviour
{
    private AudioSource audio;            // Reference to the AudioSource component
    
    public List<AudioClip> soundClip;     // List of sound clips to play on trigger

    private void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Play each clip in the list once (in sequence, overlapping if multiple exist)
            foreach (var clip in soundClip)
            {
                audio.PlayOneShot(clip); 
            }
        }
    }
}