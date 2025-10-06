using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using GameJamKalev;

public class EnhancedWaveManager : MonoBehaviour
{
    [Header("Wave Configuration")]
    public WaveData[] waveDataArray = new WaveData[3];

    [Header("Day Phase Settings")]
    public float dayDuration = 3f; // How long the day phase lasts (in seconds)

    [Header("Lighting Settings")]
    public Light sunLight;
    public Color dayColor = Color.white;
    public Color nightColor = new Color(0.2f, 0.2f, 0.4f);
    public float dayIntensity = 1f;
    public float nightIntensity = 0.3f;
    public float transitionDuration = 1f;

    [Header("References")]
    public EnemySpawner enemySpawner;
    public Volume postProcessVolume;

    [Header("Game Over Settings")]
    public int townMaxHealth = 10;
    public bool gameOver = false;
    public float gameOverDelay = 2f; // Delay before returning to main menu
    public string mainMenuSceneName = "MainMenu";

    [Header("Victory Settings")]
    public bool victory = false;
    public float victoryDelay = 2f; // Delay before returning to main menu on victory

    [Header("Debug")]
    public bool isNight = false;
    public int currentWave = 0;
    public int enemiesAlive = 0;
    public int enemiesSpawnedThisWave = 0;

    // Events
    public System.Action<int> OnWaveStarted;
    public System.Action<int> OnWaveCompleted;
    public System.Action OnDayStarted;
    public System.Action OnNightStarted;
    public System.Action OnGameOver;
    public System.Action OnVictory;

    private bool isTransitioning = false;
    private Coroutine currentWaveCoroutine;
    private List<Enemy> activeEnemies = new List<Enemy>();
    private bool isDayPhaseActive = false; // Track if we're in day phase
    private WaveData currentWaveData;

    void Start()
    {
        // Find references if not assigned
        if (enemySpawner == null)
        {
            // Find the EnemySpawner that has road tiles set up
            EnemySpawner[] allSpawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
            foreach (EnemySpawner spawner in allSpawners)
            {
                if (spawner.roadTiles.Count > 0)
                {
                    enemySpawner = spawner;
                    break;
                }
            }

            // Fallback to first spawner if none have road tiles yet
            if (enemySpawner == null)
            {
                enemySpawner = FindFirstObjectByType<EnemySpawner>();
            }
        }
        if (sunLight == null)
            sunLight = FindFirstObjectByType<Light>();
        if (postProcessVolume == null)
            postProcessVolume = FindFirstObjectByType<Volume>();

        // Initialize state
        isDayPhaseActive = false;
        isNight = false;
        currentWave = -1; // Start at -1, first wave will be 0 (Wave 1)
        enemiesAlive = 0;
        enemiesSpawnedThisWave = 0;

        // Wait for Town to be created before starting the game
        //StartCoroutine(WaitForTownAndStart());
    }

    System.Collections.IEnumerator WaitForTownAndStart()
    {
        // Wait for Town to be created (with timeout)
        float timeout = 10f; // 10 second timeout
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            Town town = FindFirstObjectByType<Town>();
            if (town != null)
            {
                StartDay();
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null; // Wait one frame
        }

        // Timeout reached - Town still not found
        Debug.LogError("EnhancedWaveManager: Timeout waiting for Town creation! Starting anyway...");
        StartDay();
    }

    void Update()
    {
        // Don't update if game is over or victory achieved
        if (gameOver || victory)
        {
            return;
        }

        // Count alive enemies
        CountAliveEnemies();

        // Check for game over condition
        CheckGameOver();

        // Check for victory condition (wave 3 completed)
        CheckVictory();

        // Check if wave is complete (only if not game over/victory and in night phase)
        if (!gameOver && !victory && isNight && !isDayPhaseActive && enemiesAlive == 0 && enemiesSpawnedThisWave >= GetCurrentWaveEnemyCount())
        {
            CompleteWave();
        }
    }

    void CountAliveEnemies()
    {
        enemiesAlive = 0;
        activeEnemies.Clear();

        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy != null)
            {
                enemiesAlive++;
                activeEnemies.Add(enemy);
            }
        }
    }

    int GetCurrentWaveEnemyCount()
    {
        // Use WaveData system
        if (currentWaveData != null)
        {
            int totalEnemies = 0;
            foreach (var spawnData in currentWaveData.enemySpawnData)
            {
                totalEnemies += spawnData.spawnCount;
            }
            return totalEnemies;
        }
        return 0;
    }

    void CheckGameOver()
    {
        // Don't check for game over during initialization (before first wave starts)
        if (currentWave < 0)
        {
            return;
        }

        // Check if town is destroyed
        Town town = FindFirstObjectByType<Town>();
        if (town == null)
        {
            Debug.LogError("EnhancedWaveManager: Town not found! Game Over triggered.");
            GameOver();
        }
        else if (town.health <= 0)
        {
            Debug.LogError($"EnhancedWaveManager: Town health is {town.health}! Game Over triggered.");
            GameOver();
        }
    }

    void CheckVictory()
    {
        // Check if wave 3 is completed (wave index 2, since we start from 0)
        // Victory occurs when wave 3 is completed and all enemies are defeated
        if (currentWave >= 2 && isNight && !isDayPhaseActive && enemiesAlive == 0 && enemiesSpawnedThisWave >= GetCurrentWaveEnemyCount())
        {
            Victory();
        }
    }

    void GameOver()
    {
        if (gameOver)
        {
            return; // Prevent multiple game over calls
        }

        gameOver = true;

        // Stop all spawning
        if (enemySpawner != null)
        {
            enemySpawner.enabled = false;
        }

        // Stop all coroutines
        StopAllCoroutines();

        // Trigger game over event
        OnGameOver?.Invoke();

        // Play game over sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOverSound();
        }

        // Start coroutine to return to main menu after delay
        StartCoroutine(ReturnToMainMenu());
    }

    void Victory()
    {
        if (victory)
        {
            return; // Prevent multiple victory calls
        }

        victory = true;

        // Stop all spawning
        if (enemySpawner != null)
        {
            enemySpawner.enabled = false;
        }

        // Stop all coroutines
        StopAllCoroutines();

        // Trigger victory event
        OnVictory?.Invoke();

        // Play victory sound (if available)
        if (AudioManager.Instance != null)
        {
            // You can add a victory sound method to AudioManager if needed
            // AudioManager.Instance.PlayVictorySound();
        }

        // Start coroutine to return to main menu after delay
        StartCoroutine(ReturnToMainMenuOnVictory());
    }

    System.Collections.IEnumerator ReturnToMainMenu()
    {
        Debug.Log($"EnhancedWaveManager: Game Over - waiting {gameOverDelay} seconds before returning to main menu");

        // Wait for the specified delay
        yield return new WaitForSeconds(gameOverDelay);

        Debug.Log($"EnhancedWaveManager: Loading scene '{mainMenuSceneName}'");

        // Load the main menu scene
        try
        {
            SceneManager.LoadScene(mainMenuSceneName);
            Debug.Log($"EnhancedWaveManager: Successfully loaded scene '{mainMenuSceneName}'");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"EnhancedWaveManager: Failed to load scene '{mainMenuSceneName}': {e.Message}");
            Debug.LogError("EnhancedWaveManager: Make sure the scene is added to Build Settings!");
        }
    }

    System.Collections.IEnumerator ReturnToMainMenuOnVictory()
    {
        Debug.Log($"EnhancedWaveManager: Victory - waiting {victoryDelay} seconds before returning to main menu");

        // Wait for the specified delay
        yield return new WaitForSeconds(victoryDelay);

        Debug.Log($"EnhancedWaveManager: Loading scene '{mainMenuSceneName}'");

        // Load the main menu scene
        try
        {
            SceneManager.LoadScene(mainMenuSceneName);
            Debug.Log($"EnhancedWaveManager: Successfully loaded scene '{mainMenuSceneName}'");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"EnhancedWaveManager: Failed to load scene '{mainMenuSceneName}': {e.Message}");
            Debug.LogError("EnhancedWaveManager: Make sure the scene is added to Build Settings!");
        }
    }

    public void StartDay()
    {
        if (isTransitioning || gameOver || victory || isDayPhaseActive)
        {
            return;
        }

        isDayPhaseActive = true;
        isNight = false;

        // Cancel any existing invoke calls to prevent overlap
        CancelInvoke(nameof(StartNextWave));

        // Disable enemy spawning
        if (enemySpawner != null)
            enemySpawner.enabled = false;

        // Force exit tower mode to prevent tower destruction issues
        TowerPlacementManager towerManager = FindFirstObjectByType<TowerPlacementManager>();
        if (towerManager != null)
        {
            towerManager.ForceExitTowerMode();
        }

        // Change lighting to day
        StartCoroutine(TransitionToDay());

        // Trigger day event
        OnDayStarted?.Invoke();

        // Play day cycle sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDayCycleSound();
        }

        // Start next wave after day duration
        Invoke(nameof(StartNextWave), dayDuration);
    }

    public void StartNight()
    {
        if (isTransitioning || gameOver || victory)
        {
            return;
        }

        isDayPhaseActive = false; // End day phase
        isNight = true;
        enemiesSpawnedThisWave = 0;

        // Enable enemy spawning
        if (enemySpawner != null)
            enemySpawner.enabled = true;

        // Change lighting to night
        StartCoroutine(TransitionToNight());

        // Trigger night event
        OnNightStarted?.Invoke();

        // Play night cycle sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayNightCycleSound();
        }

        // Start spawning enemies for this wave
        StartWave();
    }

    void StartNextWave()
    {
        currentWave++;

        // Set current wave data
        if (waveDataArray != null && currentWave < waveDataArray.Length)
        {
            currentWaveData = waveDataArray[currentWave];
        }
        else
        {
            Debug.LogWarning($"EnhancedWaveManager: No WaveData found for wave {currentWave + 1}. Using default settings.");
            currentWaveData = null;
        }

        StartNight();
    }

    void StartWave()
    {
        if (currentWaveCoroutine != null)
            StopCoroutine(currentWaveCoroutine);

        currentWaveCoroutine = StartCoroutine(SpawnWaveEnemies());

        // Trigger wave started event
        OnWaveStarted?.Invoke(currentWave);
    }

    IEnumerator SpawnWaveEnemies()
    {
        if (currentWaveData == null)
        {
            Debug.LogError("EnhancedWaveManager: No WaveData assigned for current wave!");
            yield break;
        }

        foreach (var spawnData in currentWaveData.enemySpawnData)
        {
            if (spawnData.enemyPrefab == null)
            {
                Debug.LogWarning($"EnhancedWaveManager: Enemy prefab is null in spawn data for wave {currentWave + 1}");
                continue;
            }

            // Wait for start delay
            if (spawnData.startDelay > 0)
            {
                yield return new WaitForSeconds(spawnData.startDelay);
            }

            // Spawn enemies for this type
            for (int i = 0; i < spawnData.spawnCount; i++)
            {
                if (enemySpawner != null)
                {
                    // Check if this spawner has road tiles, if not, find one that does
                    if (enemySpawner.roadTiles.Count == 0)
                    {
                        EnemySpawner[] allSpawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
                        foreach (EnemySpawner spawner in allSpawners)
                        {
                            if (spawner.roadTiles.Count > 0)
                            {
                                enemySpawner = spawner;
                                break;
                            }
                        }
                    }

                    enemySpawner.SpawnEnemy(spawnData.enemyPrefab);
                    enemiesSpawnedThisWave++;
                }

                // Wait for spawn delay
                yield return new WaitForSeconds(spawnData.spawnDelay);
            }
        }
    }

    void CompleteWave()
    {
        // Trigger wave completed event
        OnWaveCompleted?.Invoke(currentWave);

        // Only start day if we're not already in day phase
        if (!isDayPhaseActive)
        {
            StartDay();
        }
    }

    IEnumerator TransitionToDay()
    {
        isTransitioning = true;
        float elapsedTime = 0f;

        Color startColor = sunLight.color;
        float startIntensity = sunLight.intensity;

        while (elapsedTime < transitionDuration)
        {
            float t = elapsedTime / transitionDuration;
            sunLight.color = Color.Lerp(startColor, dayColor, t);
            sunLight.intensity = Mathf.Lerp(startIntensity, dayIntensity, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        sunLight.color = dayColor;
        sunLight.intensity = dayIntensity;
        isTransitioning = false;
    }

    IEnumerator TransitionToNight()
    {
        isTransitioning = true;
        float elapsedTime = 0f;

        Color startColor = sunLight.color;
        float startIntensity = sunLight.intensity;

        while (elapsedTime < transitionDuration)
        {
            float t = elapsedTime / transitionDuration;
            sunLight.color = Color.Lerp(startColor, nightColor, t);
            sunLight.intensity = Mathf.Lerp(startIntensity, nightIntensity, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        sunLight.color = nightColor;
        sunLight.intensity = nightIntensity;
        isTransitioning = false;
    }

    // Public methods for external access
    public bool IsNight() => isNight;
    public bool IsGameOver() => gameOver;
    public bool IsVictory() => victory;
    public int GetCurrentWave() => currentWave;
    public int GetEnemiesAlive() => enemiesAlive;
    public int GetEnemiesSpawnedThisWave() => enemiesSpawnedThisWave;

    // Debug methods
    [ContextMenu("Force Start Day")]
    public void ForceStartDay()
    {
        StopAllCoroutines();
        StartDay();
    }

    [ContextMenu("Force Start Night")]
    public void ForceStartNight()
    {
        StopAllCoroutines();
        StartNight();
    }

    [ContextMenu("Skip to Next Wave")]
    public void SkipToNextWave()
    {
        if (gameOver) return;

        // Kill all enemies
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }

        CompleteWave();
    }

    [ContextMenu("Force Game Over")]
    public void ForceGameOver()
    {
        GameOver();
    }

    [ContextMenu("Force Victory")]
    public void ForceVictory()
    {
        Victory();
    }

    [ContextMenu("Test Return to Main Menu")]
    public void TestReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    [ContextMenu("Reset Game")]
    public void ResetGame()
    {
        gameOver = false;
        victory = false;
        currentWave = -1; // Start at -1, first wave will be 0 (Wave 1)
        enemiesAlive = 0;
        enemiesSpawnedThisWave = 0;
        isNight = false;
        isDayPhaseActive = false;
        isTransitioning = false;
        currentWaveData = null;

        // Cancel all invokes
        CancelInvoke();

        // Reset town health
        Town town = FindFirstObjectByType<Town>();
        if (town != null)
        {
            town.health = townMaxHealth;
        }

        // Stop all coroutines and start fresh
        StopAllCoroutines();
        StartDay();
    }

    [ContextMenu("Debug EnemySpawner Instances")]
    public void DebugEnemySpawnerInstances()
    {
        EnemySpawner[] allSpawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
        Debug.Log($"EnhancedWaveManager: Found {allSpawners.Length} EnemySpawner instances in scene:");

        for (int i = 0; i < allSpawners.Length; i++)
        {
            EnemySpawner spawner = allSpawners[i];
            Debug.Log($"  Instance {i}: {spawner.gameObject.name} - ID: {spawner.GetInstanceID()}, roadTiles: {spawner.roadTiles.Count}, townInstance: {spawner.townInstance != null}");
        }

        Debug.Log($"EnhancedWaveManager: Currently using EnemySpawner - {enemySpawner?.gameObject.name} - Instance ID: {enemySpawner?.GetInstanceID()}");
    }
}
