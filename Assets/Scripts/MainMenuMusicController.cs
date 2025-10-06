using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles music and UI interactions in the main menu scene.
/// Integrates with MusicManager to control the music sequence.
/// </summary>
public class MainMenuMusicController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Start button that triggers the transition")]
    public Button startButton;

    [Tooltip("Any other buttons that should be disabled during transition")]
    public Button[] otherButtons;

    [Header("Transition Settings")]
    [Tooltip("Delay before starting transition after button press")]
    public float transitionDelay = 0.5f;

    [Tooltip("Enable debug logs")]
    public bool enableDebugLogs = false;

    private bool isTransitioning = false;

    void Start()
    {
        // Ensure MusicManager exists - if not, create it
        if (MusicManager.Instance == null)
        {
            Debug.LogWarning("MainMenuMusicController: MusicManager not found! Creating one automatically...");
            CreateMusicManager();
        }

        // Setup button events
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        else
        {
            Debug.LogWarning("MainMenuMusicController: Start button not assigned!");
        }

        // Disable other buttons during transition
        SetupOtherButtons();

        if (enableDebugLogs)
        {
            Debug.Log("MainMenuMusicController: Initialized successfully");
        }
    }

    void CreateMusicManager()
    {
        // Create MusicManager GameObject
        GameObject musicManagerObj = new GameObject("MusicManager");
        musicManagerObj.AddComponent<MusicManager>();

        Debug.Log("MainMenuMusicController: MusicManager created automatically. Please configure music clips in the Inspector.");
    }

    void SetupOtherButtons()
    {
        if (otherButtons != null)
        {
            foreach (Button button in otherButtons)
            {
                if (button != null)
                {
                    // Add listener to disable during transition
                    button.onClick.AddListener(() => {
                        if (isTransitioning)
                        {
                            if (enableDebugLogs)
                            {
                                Debug.Log("MainMenuMusicController: Button clicked during transition, ignoring");
                            }
                        }
                    });
                }
            }
        }
    }

    void OnStartButtonClicked()
    {
        if (isTransitioning)
        {
            if (enableDebugLogs)
            {
                Debug.Log("MainMenuMusicController: Start button clicked during transition, ignoring");
            }
            return;
        }

        if (enableDebugLogs)
        {
            Debug.Log("MainMenuMusicController: Start button clicked, starting transition");
        }

        StartTransition();
    }

    void StartTransition()
    {
        isTransitioning = true;

        // Disable all buttons
        DisableAllButtons();

        // Start transition after delay
        Invoke(nameof(TriggerMusicTransition), transitionDelay);
    }

    void DisableAllButtons()
    {
        if (startButton != null)
        {
            startButton.interactable = false;
        }

        if (otherButtons != null)
        {
            foreach (Button button in otherButtons)
            {
                if (button != null)
                {
                    button.interactable = false;
                }
            }
        }
    }

    void TriggerMusicTransition()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.OnStartButtonPressed();

            if (enableDebugLogs)
            {
                Debug.Log("MainMenuMusicController: Triggered music transition");
            }
        }
        else
        {
            Debug.LogError("MainMenuMusicController: MusicManager not found during transition!");
        }
    }

    #region Public Methods

    /// <summary>
    /// Manually trigger the start sequence (useful for testing)
    /// </summary>
    [ContextMenu("Test Start Sequence")]
    public void TestStartSequence()
    {
        if (enableDebugLogs)
        {
            Debug.Log("MainMenuMusicController: Testing start sequence");
        }

        StartTransition();
    }

    /// <summary>
    /// Enable all buttons (useful for testing)
    /// </summary>
    public void EnableAllButtons()
    {
        if (startButton != null)
        {
            startButton.interactable = true;
        }

        if (otherButtons != null)
        {
            foreach (Button button in otherButtons)
            {
                if (button != null)
                {
                    button.interactable = true;
                }
            }
        }
    }

    /// <summary>
    /// Reset the controller state
    /// </summary>
    public void ResetController()
    {
        isTransitioning = false;
        EnableAllButtons();

        if (enableDebugLogs)
        {
            Debug.Log("MainMenuMusicController: Controller reset");
        }
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Test Music Manager")]
    public void TestMusicManager()
    {
        if (MusicManager.Instance != null)
        {
            Debug.Log("MainMenuMusicController: MusicManager found and working");
            MusicManager.Instance.TestMainMenuMusic();
        }
        else
        {
            Debug.LogError("MainMenuMusicController: MusicManager not found!");
        }
    }

    #endregion
}
