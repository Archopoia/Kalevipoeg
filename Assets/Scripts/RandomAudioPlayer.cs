using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles random audio playback with cycling through a list of sounds.
/// Each sound is played once before repeating, ensuring variety.
/// </summary>
public class RandomAudioPlayer
{
    private AudioClip[] audioClips;
    private AudioSource audioSource;
    private float volume;
    private List<int> availableIndices;
    private int lastPlayedIndex = -1;

    /// <summary>
    /// Initialize the random audio player
    /// </summary>
    /// <param name="clips">Array of audio clips to cycle through</param>
    /// <param name="source">Audio source to play sounds from</param>
    /// <param name="vol">Volume multiplier for the sounds</param>
    public RandomAudioPlayer(AudioClip[] clips, AudioSource source, float vol = 1f)
    {
        audioClips = clips;
        audioSource = source;
        volume = vol;

        // Initialize available indices list
        RefreshAvailableIndices();
    }

    /// <summary>
    /// Play a random sound from the available clips
    /// </summary>
    public void PlayRandomSound()
    {
        if (audioClips == null || audioClips.Length == 0 || audioSource == null)
        {
            return;
        }

        // If no available indices, refresh the list
        if (availableIndices.Count == 0)
        {
            RefreshAvailableIndices();
        }

        // Safety check: if still no available indices after refresh, return
        if (availableIndices.Count == 0)
        {
            Debug.LogWarning("RandomAudioPlayer: No valid audio clips available to play");
            return;
        }

        // Pick a random index from available ones
        int randomIndex = Random.Range(0, availableIndices.Count);
        int clipIndex = availableIndices[randomIndex];

        // Remove the selected index from available list
        availableIndices.RemoveAt(randomIndex);

        // Play the sound
        AudioClip clipToPlay = audioClips[clipIndex];
        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay, volume);
        }

        lastPlayedIndex = clipIndex;
    }

    /// <summary>
    /// Refresh the available indices list (called when all sounds have been played)
    /// </summary>
    private void RefreshAvailableIndices()
    {
        availableIndices = new List<int>();

        // Safety check: ensure audioClips is not null
        if (audioClips == null)
        {
            Debug.LogWarning("RandomAudioPlayer: audioClips is null, cannot refresh indices");
            return;
        }

        // Add all valid indices (excluding null clips)
        for (int i = 0; i < audioClips.Length; i++)
        {
            if (audioClips[i] != null)
            {
                availableIndices.Add(i);
            }
        }

        // Log warning if no valid clips found
        if (availableIndices.Count == 0)
        {
            Debug.LogWarning("RandomAudioPlayer: No valid audio clips found in the array. Please assign audio clips in the inspector.");
        }
    }

    /// <summary>
    /// Get the number of available sounds to play
    /// </summary>
    public int GetAvailableCount()
    {
        return availableIndices.Count;
    }

    /// <summary>
    /// Get the total number of sounds in the collection
    /// </summary>
    public int GetTotalCount()
    {
        if (audioClips == null) return 0;

        int count = 0;
        for (int i = 0; i < audioClips.Length; i++)
        {
            if (audioClips[i] != null)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Check if all sounds have been played and the list needs refreshing
    /// </summary>
    public bool NeedsRefresh()
    {
        return availableIndices.Count == 0;
    }

    /// <summary>
    /// Force refresh the available indices (useful for testing)
    /// </summary>
    public void ForceRefresh()
    {
        RefreshAvailableIndices();
    }
}
