using UnityEngine;
using UnityEngine.InputSystem;

public class PauseGame : MonoBehaviour
{

    public GameObject pauseMenu;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    private bool wasPausePressedLastFrame = false;

    private void Start()
    {
        // Auto-find PlayerInputHandler if not assigned
        if (playerInputHandler == null)
        {
            playerInputHandler = FindFirstObjectByType<PlayerInputHandler>();
            if (playerInputHandler != null)
            {
                Debug.Log($"PauseGame: Auto-found PlayerInputHandler on {playerInputHandler.gameObject.name}");
            }
            else
            {
                Debug.LogError("PauseGame: PlayerInputHandler not found!");
            }
        }

        if (pauseMenu == null)
        {
            Debug.LogError("PauseGame: pauseMenu GameObject is not assigned!");
        }
    }

    private void Update()
    {
        // Detect pause input using edge detection (only trigger once when pressed)
        if (playerInputHandler != null)
        {
            bool isPausePressedNow = playerInputHandler.PauseMenuPressed;

            // Only trigger when key goes from NOT pressed to pressed
            if (isPausePressedNow && !wasPausePressedLastFrame)
            {
                Debug.Log("PauseGame: Escape key detected, toggling pause menu");
                TogglePause();
            }

            wasPausePressedLastFrame = isPausePressedNow;
        }
    }

    private void TogglePause()
    {
        if (!pauseMenu.activeSelf)
        {
            Debug.Log("PauseGame: Opening pause menu");
            Pause();
        }
        else
        {
            Debug.Log("PauseGame: Closing pause menu");
            Continue();
        }
    }
    public void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable player input actions when paused
        if (playerInputHandler != null)
        {
            playerInputHandler.DisablePlayerActions();
            Debug.Log("PauseGame: Player actions disabled");
        }

        Debug.Log("PauseGame: Game paused, cursor visible");
    }

    public void Continue()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Re-enable player input actions when resumed
        if (playerInputHandler != null)
        {
            playerInputHandler.EnablePlayerActions();
            Debug.Log("PauseGame: Player actions enabled");
        }

        Debug.Log("PauseGame: Game resumed, cursor locked");
    }

   public void Quit()

    {
        Time.timeScale = 1;
        Application.Quit();
    }
}
