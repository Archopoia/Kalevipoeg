using UnityEngine;

/// <summary>
/// Handles music transitions in the main game scene.
/// Integrates with EnhancedWaveManager to play appropriate music for day/night cycles.
/// </summary>
public class GameMusicController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the EnhancedWaveManager")]
    public EnhancedWaveManager waveManager;

    [Header("Settings")]
    [Tooltip("Enable debug logs")]
    public bool enableDebugLogs = false;

    private bool isInitialized = false;

    void Start()
    {
        InitializeController();
    }

    void InitializeController()
    {
        // Find WaveManager if not assigned
        if (waveManager == null)
        {
            waveManager = FindFirstObjectByType<EnhancedWaveManager>();
        }

        // Ensure MusicManager exists - if not, create it
        if (MusicManager.Instance == null)
        {
            Debug.LogWarning("GameMusicController: MusicManager not found! Creating one automatically...");
            CreateMusicManager();
        }

        // Subscribe to wave manager events
        if (waveManager != null)
        {
            waveManager.OnDayStarted += OnDayStarted;
            waveManager.OnNightStarted += OnNightStarted;

            if (enableDebugLogs)
            {
                Debug.Log("GameMusicController: Subscribed to wave manager events");
            }
        }
        else
        {
            Debug.LogWarning("GameMusicController: EnhancedWaveManager not found! Music transitions will not work automatically.");
        }

        isInitialized = true;

        if (enableDebugLogs)
        {
            Debug.Log("GameMusicController: Initialized successfully");
        }
    }

    void CreateMusicManager()
    {
        // Create MusicManager GameObject
        GameObject musicManagerObj = new GameObject("MusicManager");
        musicManagerObj.AddComponent<MusicManager>();

        Debug.Log("GameMusicController: MusicManager created automatically. Please configure music clips in the Inspector.");
    }

    void OnDayStarted()
    {
        if (!isInitialized) return;

        if (enableDebugLogs)
        {
            Debug.Log("GameMusicController: Day started, switching to day music");
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.OnDayStarted();
        }
    }

    void OnNightStarted()
    {
        if (!isInitialized) return;

        if (enableDebugLogs)
        {
            Debug.Log("GameMusicController: Night started, switching to night music");
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.OnNightStarted();
        }
    }

    #region Public Methods

    /// <summary>
    /// Manually trigger day music (useful for testing)
    /// </summary>
    [ContextMenu("Test Day Music")]
    public void TestDayMusic()
    {
        if (enableDebugLogs)
        {
            Debug.Log("GameMusicController: Testing day music");
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.OnDayStarted();
        }
    }

    /// <summary>
    /// Manually trigger night music (useful for testing)
    /// </summary>
    [ContextMenu("Test Night Music")]
    public void TestNightMusic()
    {
        if (enableDebugLogs)
        {
            Debug.Log("GameMusicController: Testing night music");
        }

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.OnNightStarted();
        }
    }

    /// <summary>
    /// Force initialize the controller
    /// </summary>
    public void ForceInitialize()
    {
        InitializeController();
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Test Music Manager")]
    public void TestMusicManager()
    {
        if (MusicManager.Instance != null)
        {
            Debug.Log("GameMusicController: MusicManager found and working");
        }
        else
        {
            Debug.LogError("GameMusicController: MusicManager not found!");
        }
    }

    [ContextMenu("Test Wave Manager Connection")]
    public void TestWaveManagerConnection()
    {
        if (waveManager != null)
        {
            Debug.Log($"GameMusicController: WaveManager found - Current Wave: {waveManager.GetCurrentWave()}, Is Night: {waveManager.IsNight()}");
        }
        else
        {
            Debug.LogError("GameMusicController: WaveManager not found!");
        }
    }

    #endregion

    void OnDestroy()
    {
        // Unsubscribe from events
        if (waveManager != null)
        {
            waveManager.OnDayStarted -= OnDayStarted;
            waveManager.OnNightStarted -= OnNightStarted;
        }
    }
}
