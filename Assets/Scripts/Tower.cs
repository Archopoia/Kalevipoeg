using UnityEngine;
using System.Collections.Generic;

public class Tower : MonoBehaviour
{
    [Header("Tower Stats")]
    public float range = 5f;
    public float damage = 1f;
    public float fireRate = 1f; // Attacks per second
    public float projectileSpeed = 10f;

    [Header("Tower Level")]
    public int currentLevel = 1;
    public int maxLevel = 3;

    [Header("Visual")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Player Feedback")]
    public float playerDetectionRange = 8f; // How close player needs to be to see range
    public bool showRangeToPlayer = false;

    [Header("Materials")]
    public Material rangeIndicatorMaterial;

    private float lastFireTime = 0f;
    private Enemy currentTarget = null;
    private Transform playerTransform;
    private GridManager gridManager;
    private List<GameObject> rangeIndicatorCubes = new List<GameObject>();
    private TowerLevels towerLevels;

    void Start()
    {
        // Find player and grid manager
        playerTransform = FindFirstObjectByType<FirstPersonController>()?.transform;
        gridManager = FindFirstObjectByType<GridManager>();
        towerLevels = FindFirstObjectByType<TowerLevels>();

        // Auto-assign range indicator material if not set
        if (rangeIndicatorMaterial == null)
        {
            rangeIndicatorMaterial = Resources.Load<Material>("RangeIndicatorMaterial");
            if (rangeIndicatorMaterial == null)
            {
                // Try to find it in the Materials folder
                rangeIndicatorMaterial = Resources.Load<Material>("Materials/RangeIndicatorMaterial");
            }
            if (rangeIndicatorMaterial != null)
            {
                Debug.Log("Tower: Auto-assigned RangeIndicatorMaterial");
                EnsureMaterialTransparency(rangeIndicatorMaterial);
            }
            else
            {
                Debug.LogWarning("Tower: Could not find RangeIndicatorMaterial. Please assign it manually in the inspector.");
            }
        }
        else
        {
            // Ensure assigned material is set up for transparency
            EnsureMaterialTransparency(rangeIndicatorMaterial);
        }
    }

    void Update()
    {
        // Check if player is nearby to show range
        CheckPlayerRange();

        // Find the closest enemy in range
        Enemy closestEnemy = FindClosestEnemyInRange();

        if (closestEnemy != null)
        {
            currentTarget = closestEnemy;

            // Fire at the target (without rotating the tower)
            if (Time.time - lastFireTime >= 1f / fireRate)
            {
                FireAtTarget(closestEnemy);
                lastFireTime = Time.time;
            }
        }
        else
        {
            currentTarget = null;
        }
    }

    Enemy FindClosestEnemyInRange()
    {
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Enemy closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (Enemy enemy in allEnemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance <= range && distance < closestDistance)
            {
                closestEnemy = enemy;
                closestDistance = distance;
            }
        }

        return closestEnemy;
    }

    void FireAtTarget(Enemy target)
    {
        if (target != null && projectilePrefab != null)
        {
            // Play tower shoot sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayTowerShootSound();
            }

            // Create projectile
            Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;
            GameObject projectileObj = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Damage = damage;
                projectile.Speed = projectileSpeed;
                projectile.SetTarget(target);
            }
        }
    }

    void CheckPlayerRange()
    {
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            bool shouldShowRange = distanceToPlayer <= playerDetectionRange;

            if (shouldShowRange != showRangeToPlayer)
            {
                showRangeToPlayer = shouldShowRange;
                UpdateTileRangeIndicator();
            }
        }
    }

    void UpdateTileRangeIndicator()
    {
        if (gridManager == null) return;

        // Clear existing indicators
        ClearRangeIndicators();

        if (showRangeToPlayer)
        {
            CreateTileRangeIndicators();
        }
    }

    void ClearRangeIndicators()
    {
        foreach (GameObject cube in rangeIndicatorCubes)
        {
            if (cube != null)
            {
                DestroyImmediate(cube);
            }
        }
        rangeIndicatorCubes.Clear();
    }

    void CreateTileRangeIndicators()
    {
        if (gridManager == null || gridManager.grid == null) return;

        // Use the correct tile size and grid positioning
        float tileSize = gridManager.GetTileSpacing();
        Vector3 gridStartPosition = gridManager.GetGridStartPosition();

        // Get tower's grid position relative to grid start
        Vector3 towerPos = transform.position;
        Vector3 relativeTowerPos = towerPos - gridStartPosition;
        int towerGridX = Mathf.RoundToInt(relativeTowerPos.x / tileSize);
        int towerGridZ = Mathf.RoundToInt(relativeTowerPos.z / tileSize);

        // Create diamond pattern
        int rangeInTiles = Mathf.RoundToInt(range / tileSize) + 1; // Add 1 to extend range by one tile

        for (int x = -rangeInTiles; x <= rangeInTiles; x++)
        {
            for (int z = -rangeInTiles; z <= rangeInTiles; z++)
            {
                // Diamond pattern: |x| + |z| <= rangeInTiles
                if (Mathf.Abs(x) + Mathf.Abs(z) <= rangeInTiles)
                {
                    int targetGridX = towerGridX + x;
                    int targetGridZ = towerGridZ + z;

                    // Calculate world position for the range indicator
                    Vector3 cubePos = gridStartPosition + new Vector3(targetGridX * tileSize, 0.1f, targetGridZ * tileSize);

                    // Create transparent red cube
                    GameObject rangeCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    rangeCube.name = "RangeIndicatorCube";
                    rangeCube.transform.position = cubePos;
                    rangeCube.transform.localScale = new Vector3(tileSize * 0.9f, 3f, tileSize * 0.9f);

                    // Remove collider
                    Collider col = rangeCube.GetComponent<Collider>();
                    if (col != null) DestroyImmediate(col);

                    // Set range indicator material
                    Renderer renderer = rangeCube.GetComponent<Renderer>();
                    if (rangeIndicatorMaterial != null)
                    {
                        renderer.material = rangeIndicatorMaterial;
                    }
                    else
                    {
                        Debug.LogWarning("Tower: RangeIndicatorMaterial not assigned! Using default material.");
                        // Fallback to a transparent URP material
                        Material fallbackMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                        fallbackMat.SetFloat("_Surface", 1); // Transparent
                        fallbackMat.SetFloat("_Blend", 0); // Alpha
                        fallbackMat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        fallbackMat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        fallbackMat.SetFloat("_ZWrite", 0);
                        fallbackMat.SetFloat("_AlphaClip", 0);
                        fallbackMat.color = new Color(1f, 0f, 0f, 0.3f);
                        fallbackMat.renderQueue = 3000;
                        renderer.material = fallbackMat;
                    }

                    rangeIndicatorCubes.Add(rangeCube);
                }
            }
        }
    }

    void EnsureMaterialTransparency(Material mat)
    {
        if (mat == null) return;

        // Check if material uses URP shader
        if (mat.shader.name.Contains("Universal Render Pipeline"))
        {
            // Set up for transparency
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetFloat("_Blend", 0); // Alpha blending
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0);
            mat.SetFloat("_AlphaClip", 0);
            mat.renderQueue = 3000;

            // Ensure alpha is set
            Color color = mat.color;
            if (color.a >= 1f)
            {
                color.a = 0.3f; // Set to semi-transparent
                mat.color = color;
            }

            Debug.Log("Tower: Configured material for transparency");
        }
    }

    void OnDestroy()
    {
        // Clean up range indicators when tower is destroyed
        ClearRangeIndicators();
    }

    public bool CanUpgrade()
    {
        if (towerLevels == null)
        {
            return false;
        }

        return towerLevels.CanUpgradeTower(currentLevel);
    }

    public TowerLevel GetNextLevel()
    {
        if (towerLevels != null)
        {
            TowerLevel result = towerLevels.GetNextTowerLevel(currentLevel);
            return result;
        }
        return null;
    }


    public TowerLevel GetCurrentLevelData()
    {
        if (towerLevels != null)
        {
            return towerLevels.GetTowerLevel(currentLevel);
        }
        return null;
    }

    void OnDrawGizmosSelected()
    {
        // Draw range circle in scene view
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
