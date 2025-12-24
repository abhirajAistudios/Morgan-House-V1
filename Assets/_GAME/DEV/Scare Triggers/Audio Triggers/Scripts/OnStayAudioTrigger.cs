using System.Collections;
using UnityEngine;

public class OnStayAudioTrigger : MonoBehaviour
{
    [SerializeField]
    private float fadeDuration;   // Time (in seconds) for fading audio in/out
    
    private GameObject player;    // Reference to the Player GameObject
    private AudioSource audio;    // Reference to this object's AudioSource
    private bool isPlaying = false; // Tracks if the trigger audio is currently playing

    private void Start()
    {
        player = FindAnyObjectByType<PlayerController>().gameObject;
        audio = GetComponent<AudioSource>();
        
        // Start with audio muted
        audio.volume = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Start playing the audio immediately
            audio.Play();
            
            // Begin fading in the audio to full volume (1f)
            StartCoroutine(Fade(true, fadeDuration, 1f));
            isPlaying = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Begin fading out the audio back to 0
            StartCoroutine(Fade(true, fadeDuration, 0f));
            isPlaying = false;
        }
    }

    private void Update()
    {
        // Ensure audio is managed properly depending on pause state & player visibility
        UpdateSound();
    }
    
    /// Handles fading audio in/out smoothly.
    private IEnumerator Fade(bool fade, float duration, float targetVolume)
    {
        // If this is a fade-in, wait until the clip is almost finished before starting fade-out
        if (!fade)
        {
            double lengthOfSource = (double)audio.clip.samples / audio.clip.frequency; // Clip length in seconds
            yield return new WaitForSecondsRealtime((float)lengthOfSource - duration);
        }

        float time = 0f;
        float startVol = audio.volume;

        // Smoothly adjust the volume over the given duration
        while (time < duration)
        {
            time += Time.deltaTime;
            audio.volume = Mathf.Lerp(startVol, targetVolume, time / duration);
            yield return null;
        }
    }
    
    /// Controls whether the audio should play or stop based on game state & player visibility.
    private void UpdateSound()
    {
        if (Time.timeScale != 0.0f) // Game is running (not paused)
        {
            if (isPlaying && player.activeInHierarchy)
            {
                // Resume playback if it was stopped
                if (!audio.isPlaying)
                    audio.Play();
            }
            else if (!player.activeInHierarchy)
            {
                // Stop audio completely if player object is inactive
                if (audio.isPlaying)
                    audio.Stop();
            }
        }
        else // Game is paused
        {
            // Stop audio during pause
            if (audio.isPlaying)
                audio.Stop();
        }
    }
}