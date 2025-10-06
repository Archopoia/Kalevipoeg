using UnityEngine;
using System;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public int width = 15;
    public int height = 15;

    [Header("Land Tile Prefabs")]
    public List<GameObject> landTilePrefabs = new List<GameObject>();

    [Header("Road Tile Prefabs")]
    public List<GameObject> roadTilePrefabs = new List<GameObject>();

    [Header("Resource Tile Spawn Chances")]
    [Range(0f, 1f)]
    public float treeTileSpawnChance = 0.3f;
    [Range(0f, 1f)]
    public float rockTileSpawnChance = 0.3f;

    [Header("Grid Settings")]
    public float tileSpacing = 1f;

    [Header("Grid Position")]
    public Vector3 gridOffset = Vector3.zero;
    public bool useCustomPosition = false;

    [HideInInspector] public Tile[,] grid;

    private float tileSize;
    private float finalSpacing;

    // Cached prefab lists for performance
    private List<GameObject> validLandPrefabs;
    private List<GameObject> validRoadPrefabs;
    private List<GameObject> treePrefabs;
    private List<GameObject> rockPrefabs;
    private List<GameObject> regularPrefabs;

    // Event to notify others when grid is ready
    public event Action<GridManager> OnGridGenerated;

    void Start()
    {
        // Cache prefab lists once at startup
        CachePrefabLists();

        // Calculate tile size once
        CalculateTileSize();

        // Generate grid
        GenerateGrid();

        // Notify listeners
        OnGridGenerated?.Invoke(this);
    }

    void CachePrefabLists()
    {
        // Cache valid land prefabs
        validLandPrefabs = new List<GameObject>();
        treePrefabs = new List<GameObject>();
        rockPrefabs = new List<GameObject>();
        regularPrefabs = new List<GameObject>();

        foreach (var prefab in landTilePrefabs)
        {
            if (prefab != null)
            {
                validLandPrefabs.Add(prefab);

                string prefabName = prefab.name.ToLower();
                if (prefabName.Contains("tree"))
                {
                    treePrefabs.Add(prefab);
                }
                else if (prefabName.Contains("rock"))
                {
                    rockPrefabs.Add(prefab);
                }
                else
                {
                    regularPrefabs.Add(prefab);
                }
            }
        }

        // Cache valid road prefabs
        validRoadPrefabs = new List<GameObject>();
        foreach (var prefab in roadTilePrefabs)
        {
            if (prefab != null)
            {
                validRoadPrefabs.Add(prefab);
            }
        }
    }

    void CalculateTileSize()
    {
        if (validLandPrefabs.Count > 0)
        {
            Renderer rend = validLandPrefabs[0].GetComponent<Renderer>();
            tileSize = rend.bounds.size.x;
            finalSpacing = tileSize * tileSpacing;
        }
        else
        {
            Debug.LogError("GridManager: No valid land tile prefabs found!");
        }
    }

    void GenerateGrid()
    {
        grid = new Tile[width, height];

        // Calculate starting position
        Vector3 startPosition = useCustomPosition ? gridOffset : transform.position;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 spawnPos = startPosition + new Vector3(x * finalSpacing, 0, z * finalSpacing);

                GameObject tileObj = Instantiate(GetRandomLandTile(), spawnPos, Quaternion.identity);
                tileObj.transform.parent = transform;

                Tile tile = tileObj.GetComponent<Tile>();
                if (tile == null) tile = tileObj.AddComponent<Tile>();

                tile.gridX = x;
                tile.gridZ = z;
                tile.isRoad = false;

                grid[x, z] = tile;
            }
        }
    }

    GameObject GetRandomLandTile()
    {
        // Fast weighted selection
        float randomValue = UnityEngine.Random.Range(0f, 1f);

        // Check for tree spawn first
        if (treePrefabs.Count > 0 && randomValue < treeTileSpawnChance)
        {
            return treePrefabs[UnityEngine.Random.Range(0, treePrefabs.Count)];
        }

        // Check for rock spawn
        if (rockPrefabs.Count > 0 && randomValue < (treeTileSpawnChance + rockTileSpawnChance))
        {
            return rockPrefabs[UnityEngine.Random.Range(0, rockPrefabs.Count)];
        }

        // Spawn regular tile
        if (regularPrefabs.Count > 0)
        {
            return regularPrefabs[UnityEngine.Random.Range(0, regularPrefabs.Count)];
        }

        // Fallback to any valid land prefab
        return validLandPrefabs[UnityEngine.Random.Range(0, validLandPrefabs.Count)];
    }

    GameObject GetRandomRoadTile()
    {
        return validRoadPrefabs[UnityEngine.Random.Range(0, validRoadPrefabs.Count)];
    }

    public void SetTileAsRoad(int x, int z)
    {
        if (x < 0 || x >= width || z < 0 || z >= height) return;

        Tile currentTile = grid[x, z];
        if (currentTile.isRoad) return;

        Vector3 position = currentTile.transform.position;
        Destroy(currentTile.gameObject);

        GameObject roadTileObj = Instantiate(GetRandomRoadTile(), position, Quaternion.identity);
        roadTileObj.transform.parent = transform;

        Tile roadTile = roadTileObj.GetComponent<Tile>();
        if (roadTile == null) roadTile = roadTileObj.AddComponent<Tile>();

        roadTile.gridX = x;
        roadTile.gridZ = z;
        roadTile.isRoad = true;

        grid[x, z] = roadTile;
    }

    public void SetTileAsLand(int x, int z)
    {
        if (x < 0 || x >= width || z < 0 || z >= height) return;

        Tile currentTile = grid[x, z];
        if (!currentTile.isRoad) return;

        Vector3 position = currentTile.transform.position;
        Destroy(currentTile.gameObject);

        GameObject landTileObj = Instantiate(GetRandomLandTile(), position, Quaternion.identity);
        landTileObj.transform.parent = transform;

        Tile landTile = landTileObj.GetComponent<Tile>();
        if (landTile == null) landTile = landTileObj.AddComponent<Tile>();

        landTile.gridX = x;
        landTile.gridZ = z;
        landTile.isRoad = false;

        grid[x, z] = landTile;
    }

    // Debug method to show tile categorization
    [ContextMenu("Debug Tile Categories")]
    public void DebugTileCategories()
    {
        Debug.Log($"Tree Tiles ({treePrefabs.Count}): {string.Join(", ", treePrefabs.ConvertAll(p => p.name))}");
        Debug.Log($"Rock Tiles ({rockPrefabs.Count}): {string.Join(", ", rockPrefabs.ConvertAll(p => p.name))}");
        Debug.Log($"Regular Tiles ({regularPrefabs.Count}): {string.Join(", ", regularPrefabs.ConvertAll(p => p.name))}");
        Debug.Log($"Tree Spawn Chance: {treeTileSpawnChance * 100}%");
        Debug.Log($"Rock Spawn Chance: {rockTileSpawnChance * 100}%");
        Debug.Log($"Regular Spawn Chance: {(1f - treeTileSpawnChance - rockTileSpawnChance) * 100}%");
    }

    // Method to regenerate grid at new position
    [ContextMenu("Regenerate Grid")]
    public void RegenerateGrid()
    {
        // Clear existing grid
        if (grid != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    if (grid[x, z] != null)
                    {
                        DestroyImmediate(grid[x, z].gameObject);
                    }
                }
            }
        }

        // Generate new grid
        GenerateGrid();

        // Notify listeners
        OnGridGenerated?.Invoke(this);
    }

    // Method to get grid bounds for visualization
    public Bounds GetGridBounds()
    {
        Vector3 startPosition = useCustomPosition ? gridOffset : transform.position;
        Vector3 center = startPosition + new Vector3((width - 1) * finalSpacing * 0.5f, 0, (height - 1) * finalSpacing * 0.5f);
        Vector3 size = new Vector3(width * finalSpacing, 1f, height * finalSpacing);
        return new Bounds(center, size);
    }

    // Public methods for other scripts to get grid information
    public Vector3 GetGridStartPosition()
    {
        return useCustomPosition ? gridOffset : transform.position;
    }

    public float GetTileSpacing()
    {
        return finalSpacing;
    }

    void OnDrawGizmosSelected()
    {
        if (finalSpacing > 0)
        {
            // Draw grid bounds
            Gizmos.color = Color.yellow;
            Bounds bounds = GetGridBounds();
            Gizmos.DrawWireCube(bounds.center, bounds.size);

            // Draw starting position
            Gizmos.color = Color.red;
            Vector3 startPosition = useCustomPosition ? gridOffset : transform.position;
            Gizmos.DrawWireSphere(startPosition, 0.5f);
        }
    }
}