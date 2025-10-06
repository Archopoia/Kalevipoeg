using UnityEngine;

/// <summary>
/// Handles enemy movement audio, specifically walking sounds for different enemy types.
/// Automatically detects when the enemy is moving and plays appropriate sounds.
/// </summary>
public class EnemyMovementAudio : MonoBehaviour
{
    [Header("Enemy Settings")]
    [Tooltip("Enemy type (1, 2, or 3) - determines which walking sounds to use")]
    public int enemyType = 1;

    [Tooltip("Minimum movement speed to trigger walking sounds")]
    public float minMovementSpeed = 0.1f;

    [Tooltip("Time between walking sound plays")]
    public float walkingSoundInterval = 0.8f;

    [Header("Audio Settings")]
    [Tooltip("Volume multiplier for walking sounds")]
    [Range(0f, 1f)]
    public float walkingVolume = 1f;

    [Tooltip("Enable debug logs")]
    public bool enableDebugLogs = false;

    private float lastWalkingSoundTime;
    private bool wasMoving = false;
    private Vector3 lastPosition;
    private Enemy enemyComponent;

    void Start()
    {
        // Get enemy component for movement detection
        enemyComponent = GetComponent<Enemy>();

        // Store initial position
        lastPosition = transform.position;

        // Validate enemy type
        if (enemyType < 1 || enemyType > 3)
        {
            Debug.LogWarning($"EnemyMovementAudio on {gameObject.name}: Invalid enemy type {enemyType}. Using type 1.");
            enemyType = 1;
        }
    }

    void Update()
    {
        // Calculate movement
        Vector3 currentPosition = transform.position;
        Vector3 movement = currentPosition - lastPosition;
        float movementSpeed = movement.magnitude / Time.deltaTime;

        bool isMoving = movementSpeed >= minMovementSpeed;

        // Play walking sound when starting to move or at intervals while moving
        if (isMoving)
        {
            if (!wasMoving || Time.time - lastWalkingSoundTime >= walkingSoundInterval)
            {
                PlayWalkingSound();
                lastWalkingSoundTime = Time.time;
            }
        }

        wasMoving = isMoving;
        lastPosition = currentPosition;
    }

    /// <summary>
    /// Play a walking sound through the AudioManager
    /// </summary>
    private void PlayWalkingSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyWalkingSound(enemyType);

            if (enableDebugLogs)
            {
                Debug.Log($"EnemyMovementAudio on {gameObject.name}: Playing walking sound for enemy type {enemyType}");
            }
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
                Debug.Log($"EnemyMovementAudio on {gameObject.name}: Enemy type changed to {enemyType}");
            }
        }
        else
        {
            Debug.LogWarning($"EnemyMovementAudio on {gameObject.name}: Invalid enemy type {newType}. Must be 1, 2, or 3.");
        }
    }

    /// <summary>
    /// Force play a walking sound (useful for testing)
    /// </summary>
    [ContextMenu("Test Walking Sound")]
    public void TestWalkingSound()
    {
        PlayWalkingSound();
    }

    /// <summary>
    /// Update walking sound settings
    /// </summary>
    public void UpdateWalkingSettings(float interval, float volume)
    {
        walkingSoundInterval = interval;
        walkingVolume = volume;
    }
}
