using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject townPrefab;
    public GameObject cavePrefab;
    public GridManager gridManager;
    public float spawnInterval = 5f;

    private float timer;
    public Town townInstance;
    public GameObject caveInstance;
    public List<Tile> roadTiles = new List<Tile>();

    void Awake()
    {
        // Always find the GridManager at runtime to ensure we get the correct active instance
        var allGridManagers = FindObjectsByType<GridManager>(FindObjectsSortMode.None);

        // Use the first active GridManager
        gridManager = FindFirstObjectByType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("EnemySpawner: No active GridManager found in scene!");
            return;
        }

        gridManager.OnGridGenerated += SetupTerritory;
    }

    void Start()
    {
        // Check if grid is already generated and setup territory if needed
        if (gridManager != null && gridManager.grid != null && townInstance == null)
        {
            SetupTerritory(gridManager);
        }

        // Backup: Set up territory after a short delay if it hasn't been set up yet
        Invoke("CheckAndSetupTerritory", 1f);

        // Additional backup: Try again after 2 seconds
        Invoke("ForceSetupTerritory", 2f);
    }

    void CheckAndSetupTerritory()
    {
        if (townInstance == null && gridManager != null && gridManager.grid != null)
        {
            SetupTerritory(gridManager);
        }
    }

    void ForceSetupTerritory()
    {
        // Only setup if no town exists
        if (townInstance == null && gridManager != null && gridManager.grid != null)
        {
            SetupTerritory(gridManager);
        }
    }

    void OnDisable()
    {
        if (gridManager != null)
        {
            gridManager.OnGridGenerated -= SetupTerritory;
        }
    }

    // Removed Update() method - spawning is now controlled entirely by WaveManager
    // The WaveManager calls SpawnEnemy() directly when needed

    void SetupTerritory(GridManager gm)
    {
        int width = gm.width;
        int height = gm.height;

        // Place town in center of 2x2 area (around position 1,1)
        int townX = 1;
        int townZ = 1;

        // Place spawn at top-right corner
        int spawnX = width - 1;
        int spawnZ = height - 1;

        // Create serpentine path
        roadTiles.Clear();
        CreateSerpentinePath(gm, spawnX, spawnZ, townX, townZ);

        // Get the correct tile size and grid offset from GridManager
        float tileSize = gm.GetTileSpacing();
        Vector3 gridStartPosition = gm.GetGridStartPosition();

        // Spawn town centered in 2x2 area (end of path)
        if (townPrefab != null)
        {
            // Calculate center position of 2x2 area
            Vector3 townPos = gridStartPosition + new Vector3((townX + 0.5f) * tileSize, 1, (townZ + 0.5f) * tileSize);
            GameObject townObj = Instantiate(townPrefab, townPos, Quaternion.identity);
            townInstance = townObj.GetComponent<Town>();
        }
        else
        {
            Debug.LogError("EnemySpawner: Town Prefab is null! Please assign it in the Inspector.");
        }

        // Spawn cave at top-right (start of path)
        if (cavePrefab != null)
        {
            Vector3 cavePos = gridStartPosition + new Vector3(spawnX * tileSize, 1, spawnZ * tileSize);
            caveInstance = Instantiate(cavePrefab, cavePos, Quaternion.identity);
        }
        else
        {
            Debug.LogError("EnemySpawner: Cave Prefab is null! Please assign it in the Inspector.");
        }
    }

    void CreateSerpentinePath(GridManager gm, int startX, int startZ, int endX, int endZ)
    {
        int currentX = startX;
        int currentZ = startZ;

        // Add starting point
        gm.SetTileAsRoad(currentX, currentZ);

        if (gm.grid != null && gm.grid[currentX, currentZ] != null)
        {
            roadTiles.Add(gm.grid[currentX, currentZ]);
        }
        else
        {
            Debug.LogError($"EnemySpawner: Failed to add starting tile - grid[{currentX}, {currentZ}] is null!");
        }

        // Create serpentine path
        bool movingRight = startX > endX; // Start by moving toward town

        while (currentX != endX || currentZ != endZ)
        {
            if (movingRight)
            {
                // Move horizontally toward town
                if (currentX > endX)
                {
                    currentX--;
                }
                else if (currentX < endX)
                {
                    currentX++;
                }
                else
                {
                    // Reached target X, switch to vertical movement
                    movingRight = false;
                    continue;
                }
            }
            else
            {
                // Move vertically toward town
                if (currentZ > endZ)
                {
                    currentZ--;
                }
                else if (currentZ < endZ)
                {
                    currentZ++;
                }
                else
                {
                    // Reached target Z, switch to horizontal movement
                    movingRight = true;
                    continue;
                }
            }

            // Add current position to path
            gm.SetTileAsRoad(currentX, currentZ);
            if (gm.grid != null && gm.grid[currentX, currentZ] != null)
            {
                roadTiles.Add(gm.grid[currentX, currentZ]);
            }
            else
            {
                Debug.LogError($"EnemySpawner: Failed to add tile ({currentX}, {currentZ}) to roadTiles - grid tile is null!");
            }

            // Switch direction every few steps for serpentine effect
            if (UnityEngine.Random.Range(0f, 1f) < 0.3f) // 30% chance to change direction
            {
                movingRight = !movingRight;
            }
        }
    }

    // Public method for WaveManager to trigger enemy spawns
    public void SpawnEnemy()
    {
        SpawnEnemy(enemyPrefab);
    }

    // New method to spawn specific enemy prefab
    public void SpawnEnemy(GameObject specificEnemyPrefab)
    {
        if (roadTiles.Count == 0)
        {
            Debug.LogWarning($"EnemySpawner: No road tiles available for spawning on {gameObject.name}!");
            return;
        }

        if (specificEnemyPrefab == null)
        {
            Debug.LogError("EnemySpawner: Enemy Prefab is null! Please assign it in the Inspector.");
            return;
        }

        if (caveInstance == null)
        {
            Debug.LogError("EnemySpawner: Cave instance is null! Cannot spawn enemies.");
            return;
        }

        // Spawn enemies at the cave location
        Vector3 spawnPos = caveInstance.transform.position;
        spawnPos.y = 1; // Ensure enemies spawn above ground

        GameObject enemyObj = Instantiate(specificEnemyPrefab, spawnPos, Quaternion.identity);

        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.Init(new List<Tile>(roadTiles), townInstance);
        }
        else
        {
            Debug.LogError("EnemySpawner: Enemy component not found on enemy prefab!");
        }
    }

    [ContextMenu("Test Road Tile Creation")]
    public void TestRoadTileCreation()
    {
        if (gridManager == null)
        {
            Debug.LogError("EnemySpawner: No GridManager found for testing!");
            return;
        }

        if (gridManager.grid == null)
        {
            Debug.LogError("EnemySpawner: GridManager.grid is null!");
            return;
        }

        Debug.Log("EnemySpawner: Testing road tile creation...");

        // Test setting a tile as road
        int testX = 0;
        int testZ = 0;

        Debug.Log($"EnemySpawner: Testing tile ({testX}, {testZ})");
        Debug.Log($"EnemySpawner: Before - tile is road: {gridManager.grid[testX, testZ].isRoad}");

        gridManager.SetTileAsRoad(testX, testZ);

        Debug.Log($"EnemySpawner: After - tile is road: {gridManager.grid[testX, testZ].isRoad}");
        Debug.Log($"EnemySpawner: Tile object null: {gridManager.grid[testX, testZ] == null}");
    }

    [ContextMenu("Debug EnemySpawner Instances")]
    public void DebugEnemySpawnerInstances()
    {
        EnemySpawner[] allSpawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
        Debug.Log($"EnemySpawner: Found {allSpawners.Length} EnemySpawner instances in scene:");

        for (int i = 0; i < allSpawners.Length; i++)
        {
            EnemySpawner spawner = allSpawners[i];
            Debug.Log($"  Instance {i}: {spawner.gameObject.name} - ID: {spawner.GetInstanceID()}, roadTiles: {spawner.roadTiles.Count}, townInstance: {spawner.townInstance != null}");
        }
    }

}
