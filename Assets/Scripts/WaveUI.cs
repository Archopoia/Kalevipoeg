using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI enemiesText;
    public TextMeshProUGUI timeOfDayText;
    public TextMeshProUGUI gameOverText; // Game over message
    public GameObject wavePanel;

    [Header("Settings")]
    public float updateInterval = 0.1f;

    private EnhancedWaveManager waveManager;
    private float updateTimer = 0f;

    void Start()
    {
        // Hide game over text initially
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        // Find EnhancedWaveManager
        waveManager = FindFirstObjectByType<EnhancedWaveManager>();
        if (waveManager == null)
        {
            Debug.LogError("WaveUI: EnhancedWaveManager not found!");
            return;
        }

        // Subscribe to events
        waveManager.OnWaveStarted += OnWaveStarted;
        waveManager.OnWaveCompleted += OnWaveCompleted;
        waveManager.OnDayStarted += OnDayStarted;
        waveManager.OnNightStarted += OnNightStarted;
        waveManager.OnGameOver += OnGameOver;
        waveManager.OnVictory += OnVictory;

        // Wait a frame for WaveManager to initialize, then update UI
        StartCoroutine(InitializeUI());
    }

    System.Collections.IEnumerator InitializeUI()
    {
        // Wait one frame for WaveManager to complete its Start() method
        yield return null;

        UpdateUI();
    }

    void Update()
    {
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            UpdateUI();
            updateTimer = 0f;
        }
    }

    void UpdateUI()
    {
        if (waveManager == null) return;

        // Check if game is over
        if (waveManager.IsGameOver())
        {
            if (gameOverText != null)
            {
                gameOverText.text = "GAME OVER!\nTown Destroyed!";
                gameOverText.color = Color.red;
                gameOverText.gameObject.SetActive(true);
            }
            return;
        }

        // Check if victory achieved
        if (waveManager.IsVictory())
        {
            if (gameOverText != null)
            {
                gameOverText.text = "Success! You've defended your town";
                gameOverText.color = Color.green;
                gameOverText.gameObject.SetActive(true);
            }
            return;
        }

        // Update wave text
        if (waveText != null)
        {
            int currentWave = waveManager.GetCurrentWave();
            int displayWave = currentWave + 1;

            // Show 0 if no waves have started yet, otherwise show the wave number
            if (displayWave <= 0)
            {
                waveText.text = "Wave: 0";
            }
            else
            {
                waveText.text = $"Wave: {displayWave}";
            }
        }

        // Update enemies text
        if (enemiesText != null)
        {
            int enemiesAlive = waveManager.GetEnemiesAlive();
            int enemiesSpawned = waveManager.GetEnemiesSpawnedThisWave();

            if (waveManager.IsNight())
            {
                enemiesText.text = $"Enemies: {enemiesAlive} / {enemiesSpawned}";
            }
            else
            {
                enemiesText.text = "No Enemies";
            }
        }

        // Update time of day text
        if (timeOfDayText != null)
        {
            timeOfDayText.text = waveManager.IsNight() ? "NIGHT" : "DAY";
            timeOfDayText.color = waveManager.IsNight() ? Color.red : Color.yellow;
        }
    }

    void OnWaveStarted(int waveNumber)
    {
        // Show wave panel if available
        if (wavePanel != null)
        {
            wavePanel.SetActive(true);
        }
    }

    void OnWaveCompleted(int waveNumber)
    {
    }

    void OnDayStarted()
    {
    }

    void OnNightStarted()
    {
    }

    void OnGameOver()
    {
        Debug.Log("WaveUI: OnGameOver() called - showing game over text!");

        // Hide wave panel and show game over
        if (wavePanel != null)
        {
            wavePanel.SetActive(false);
            Debug.Log("WaveUI: Wave panel hidden");
        }

        if (gameOverText != null)
        {
            gameOverText.text = "GAME OVER!\nTown Destroyed!";
            gameOverText.color = Color.red;
            gameOverText.gameObject.SetActive(true);
            Debug.Log("WaveUI: Game over text activated and displayed");
        }
        else
        {
            Debug.LogError("WaveUI: gameOverText is null! Make sure it's assigned in the inspector.");
        }

        // Stop updating the UI
        enabled = false;
        Debug.Log("WaveUI: UI updates disabled");
    }

    void OnVictory()
    {
        Debug.Log("WaveUI: OnVictory() called - showing victory text!");

        // Hide wave panel and show victory
        if (wavePanel != null)
        {
            wavePanel.SetActive(false);
            Debug.Log("WaveUI: Wave panel hidden");
        }

        if (gameOverText != null)
        {
            gameOverText.text = "Success! You've defended your town";
            gameOverText.color = Color.green;
            gameOverText.gameObject.SetActive(true);
            Debug.Log("WaveUI: Victory text activated and displayed");
        }
        else
        {
            Debug.LogError("WaveUI: gameOverText is null! Make sure it's assigned in the inspector.");
        }

        // Stop updating the UI
        enabled = false;
        Debug.Log("WaveUI: UI updates disabled");
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (waveManager != null)
        {
            waveManager.OnWaveStarted -= OnWaveStarted;
            waveManager.OnWaveCompleted -= OnWaveCompleted;
            waveManager.OnDayStarted -= OnDayStarted;
            waveManager.OnNightStarted -= OnNightStarted;
            waveManager.OnGameOver -= OnGameOver;
            waveManager.OnVictory -= OnVictory;
        }
    }
}
