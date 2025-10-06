using UnityEngine;

/// <summary>
/// Handles enemy ambient/idle audio for different enemy types.
/// Plays a single ambient sound at random intervals while the enemy is alive.
/// </summary>
public class EnemyAmbientAudio : MonoBehaviour
{
    [Header("Enemy Settings")]
    [Tooltip("Enemy type (1, 2, or 3) - determines which ambient sound to use")]
    public int enemyType = 1;

    [Header("Timing Settings")]
    [Tooltip("Minimum time between ambient sounds (seconds)")]
    public float minInterval = 5f;

    [Tooltip("Maximum time between ambient sounds (seconds)")]
    public float maxInterval = 10f;

    [Tooltip("Enable debug logs")]
    public bool enableDebugLogs = false;

    private float nextAmbientSoundTime;
    private Enemy enemyComponent;
    private bool isAlive = true;

    void Start()
    {
        // Get enemy component to check if alive
        enemyComponent = GetComponent<Enemy>();

        // Validate enemy type
        if (enemyType < 1 || enemyType > 3)
        {
            Debug.LogWarning($"EnemyAmbientAudio on {gameObject.name}: Invalid enemy type {enemyType}. Using type 1.");
            enemyType = 1;
        }

        // Set initial ambient sound time
        ScheduleNextAmbientSound();
    }

    void Update()
    {
        // Check if enemy is still alive
        if (enemyComponent != null && enemyComponent.currentHealth <= 0)
        {
            isAlive = false;
            return;
        }

        // Play ambient sound if it's time and enemy is alive
        if (isAlive && Time.time >= nextAmbientSoundTime)
        {
            PlayAmbientSound();
            ScheduleNextAmbientSound();
        }
    }

    /// <summary>
    /// Play an ambient sound through the AudioManager
    /// </summary>
    private void PlayAmbientSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyAmbientSound(enemyType);

            if (enableDebugLogs)
            {
                Debug.Log($"EnemyAmbientAudio on {gameObject.name}: Playing ambient sound for enemy type {enemyType}");
            }
        }
    }

    /// <summary>
    /// Schedule the next ambient sound to play
    /// </summary>
    private void ScheduleNextAmbientSound()
    {
        float randomInterval = Random.Range(minInterval, maxInterval);
        nextAmbientSoundTime = Time.time + randomInterval;

        if (enableDebugLogs)
        {
            Debug.Log($"EnemyAmbientAudio on {gameObject.name}: Next ambient sound scheduled in {randomInterval:F1} seconds");
        }
    }

    /// <summary>
    /// Set the enemy type (useful for dynamic enemy types)
    /// </summary>
    public void SetEnemyType(int newType)
    {
        if (newType >= 1 && newType <= 3)
        {
            enemyType = newType;

            if (enableDebugLogs)
            {
                Debug.Log($"EnemyAmbientAudio on {gameObject.name}: Enemy type changed to {enemyType}");
            }
        }
        else
        {
            Debug.LogWarning($"EnemyAmbientAudio on {gameObject.name}: Invalid enemy type {newType}. Must be 1, 2, or 3.");
        }
    }

    /// <summary>
    /// Update ambient sound timing settings
    /// </summary>
    public void UpdateAmbientSettings(float minInt, float maxInt)
    {
        minInterval = minInt;
        maxInterval = maxInt;
    }

    /// <summary>
    /// Force play an ambient sound (useful for testing)
    /// </summary>
    [ContextMenu("Test Ambient Sound")]
    public void TestAmbientSound()
    {
        PlayAmbientSound();
    }

    /// <summary>
    /// Reset the ambient sound timer
    /// </summary>
    [ContextMenu("Reset Ambient Timer")]
    public void ResetAmbientTimer()
    {
        ScheduleNextAmbientSound();
    }
}
