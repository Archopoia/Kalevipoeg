using UnityEngine;

/// <summary>
/// Handles player movement audio, specifically walking sounds.
/// Automatically detects when the player is moving and plays appropriate sounds.
/// </summary>
public class PlayerMovementAudio : MonoBehaviour
{
    [Header("Movement Detection")]
    [Tooltip("Minimum movement speed to trigger walking sounds")]
    public float minMovementSpeed = 0.1f;

    [Tooltip("Time between walking sound plays")]
    public float walkingSoundInterval = 0.5f;

    [Tooltip("Reference to the player's movement component")]
    public PlayerInputHandler playerInputHandler;

    [Tooltip("Reference to the FirstPersonController (alternative detection method)")]
    public FirstPersonController firstPersonController;

    [Header("Audio Settings")]
    [Tooltip("Volume multiplier for walking sounds")]
    [Range(0f, 1f)]
    public float walkingVolume = 1f;

    [Tooltip("Enable debug logs")]
    public bool enableDebugLogs = false;

    private float lastWalkingSoundTime;
    private bool wasMoving = false;
    private Vector2 lastMovementInput;
    private Vector3 lastPosition;

    void Start()
    {
        // Find components if not assigned
        if (playerInputHandler == null)
        {
            playerInputHandler = GetComponent<PlayerInputHandler>();
            if (playerInputHandler == null)
            {
                playerInputHandler = FindFirstObjectByType<PlayerInputHandler>();
            }
        }

        if (firstPersonController == null)
        {
            firstPersonController = GetComponent<FirstPersonController>();
            if (firstPersonController == null)
            {
                firstPersonController = FindFirstObjectByType<FirstPersonController>();
            }
        }

        // Store initial position for movement detection
        lastPosition = transform.position;

        if (playerInputHandler == null && firstPersonController == null)
        {
            Debug.LogError("PlayerMovementAudio: No PlayerInputHandler or FirstPersonController found!");
        }
        else
        {
            if (enableDebugLogs)
            {
                Debug.Log("PlayerMovementAudio: Initialized successfully");
            }
        }
    }

    void Update()
    {
        bool isMoving = false;

        // Method 1: Check input from PlayerInputHandler
        if (playerInputHandler != null)
        {
            Vector2 currentMovementInput = playerInputHandler.MovementInput;
            isMoving = currentMovementInput.magnitude >= minMovementSpeed;
        }

        // Method 2: Check actual movement from position change (fallback)
        if (!isMoving)
        {
            Vector3 currentPosition = transform.position;
            float movementDistance = Vector3.Distance(currentPosition, lastPosition);
            float movementSpeed = movementDistance / Time.deltaTime;
            isMoving = movementSpeed >= minMovementSpeed;
            lastPosition = currentPosition;
        }

        // Play walking sound when starting to move or at intervals while moving
        if (isMoving)
        {
            if (!wasMoving || Time.time - lastWalkingSoundTime >= walkingSoundInterval)
            {
                PlayWalkingSound();
                lastWalkingSoundTime = Time.time;

                if (enableDebugLogs)
                {
                    Debug.Log("PlayerMovementAudio: Playing walking sound");
                }
            }
        }

        wasMoving = isMoving;
    }

    /// <summary>
    /// Play a walking sound through the AudioManager
    /// </summary>
    private void PlayWalkingSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerWalkingSound();

            if (enableDebugLogs)
            {
                Debug.Log($"PlayerMovementAudio: Playing walking sound");
            }
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
