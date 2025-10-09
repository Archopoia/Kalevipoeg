using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class TowerPlacementManager : MonoBehaviour
{
    [Header("Tower Settings")]
    public GameObject towerPrefab;
    public GameObject towerPreviewPrefab; // Semi-transparent preview version

    [Header("Tower Level System")]
    public TowerLevels towerLevels;

    [Header("Tower Stats")]
    // Range will be read from the tower prefab automatically

    [Header("Materials")]
    public Material rangeIndicatorMaterial;

    [Header("Player Reference")]
    public Transform playerTransform;

    [Header("Grid Settings")]
    public float tileSize = 9f; // Should match GridManager tileSize

    [Header("Input Settings")]
    public KeyCode towerToggleKey = KeyCode.T;

    private GridManager gridManager;
    private Tile currentHighlightedTile = null;
    private GameObject currentPreview = null;
    private bool isInTowerMode = false;
    private float actualTileSize;
    private List<GameObject> rangePreviewCubes = new List<GameObject>();
    private Tower currentTowerForUpgrade = null;
    private GameObject hiddenTower = null; // Store reference to hidden tower

    // Convert KeyCode to Input System Key
    private Key GetInputSystemKey(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.T: return Key.T;
            case KeyCode.Space: return Key.Space;
            case KeyCode.Escape: return Key.Escape;
            case KeyCode.Return: return Key.Enter;
            case KeyCode.Backspace: return Key.Backspace;
            case KeyCode.Tab: return Key.Tab;
            case KeyCode.Delete: return Key.Delete;
            case KeyCode.LeftArrow: return Key.LeftArrow;
            case KeyCode.RightArrow: return Key.RightArrow;
            case KeyCode.UpArrow: return Key.UpArrow;
            case KeyCode.DownArrow: return Key.DownArrow;
            case KeyCode.A: return Key.A;
            case KeyCode.B: return Key.B;
            case KeyCode.C: return Key.C;
            case KeyCode.D: return Key.D;
            case KeyCode.E: return Key.E;
            case KeyCode.F: return Key.F;
            case KeyCode.G: return Key.G;
            case KeyCode.H: return Key.H;
            case KeyCode.I: return Key.I;
            case KeyCode.J: return Key.J;
            case KeyCode.K: return Key.K;
            case KeyCode.L: return Key.L;
            case KeyCode.M: return Key.M;
            case KeyCode.N: return Key.N;
            case KeyCode.O: return Key.O;
            case KeyCode.P: return Key.P;
            case KeyCode.Q: return Key.Q;
            case KeyCode.R: return Key.R;
            case KeyCode.S: return Key.S;
            case KeyCode.U: return Key.U;
            case KeyCode.V: return Key.V;
            case KeyCode.W: return Key.W;
            case KeyCode.X: return Key.X;
            case KeyCode.Y: return Key.Y;
            case KeyCode.Z: return Key.Z;
            default: return Key.None;
        }
    }

    void Awake()
    {
    }

    void Start()
    {
        // Find the GridManager
        gridManager = FindFirstObjectByType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("TowerPlacementManager: GridManager not found!");
            return;
        }

        // Find the TowerLevels
        if (towerLevels == null)
        {
            towerLevels = FindFirstObjectByType<TowerLevels>();
            if (towerLevels == null)
            {
                Debug.LogError("TowerPlacementManager: TowerLevels not found!");
                return;
            }
        }

        // Calculate actual tile size from GridManager
        CalculateTileSize();

        // Find player if not assigned
        if (playerTransform == null)
        {
            playerTransform = FindFirstObjectByType<FirstPersonController>()?.transform;
            if (playerTransform == null)
            {
                Debug.LogError("TowerPlacementManager: Player not found! Make sure FirstPersonController exists in scene.");
            }
        }

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
                Debug.Log("TowerPlacementManager: Auto-assigned RangeIndicatorMaterial");
                EnsureMaterialTransparency(rangeIndicatorMaterial);
            }
            else
            {
                Debug.LogWarning("TowerPlacementManager: Could not find RangeIndicatorMaterial. Please assign it manually in the inspector.");
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
        // Use only Input System
        Key inputSystemKey = GetInputSystemKey(towerToggleKey);
        bool newInputT = Keyboard.current != null && Keyboard.current[inputSystemKey].wasPressedThisFrame;

        // Toggle tower mode with T key
        if (newInputT)
        {
            Debug.Log($"TowerPlacementManager: T key pressed!");
            ToggleTowerMode();
        }

        if (isInTowerMode)
        {
            HandleTowerPlacement();
        }
        else
        {
            // Tower mode not active - log occasionally to confirm
            if (Time.frameCount % 300 == 0) // Log every 5 seconds (60fps)
            {
                Debug.Log("TowerPlacementManager: Tower mode is OFF. Press T to enter tower placement mode.");
            }
        }
    }

    void CalculateTileSize()
    {
        // Try to get tile size from GridManager
        actualTileSize = gridManager.GetTileSpacing();

        // If GridManager hasn't calculated it yet, calculate manually
        if (actualTileSize <= 0)
        {
            if (gridManager.landTilePrefabs.Count > 0 && gridManager.landTilePrefabs[0] != null)
            {
                Renderer rend = gridManager.landTilePrefabs[0].GetComponent<Renderer>();
                actualTileSize = rend.bounds.size.x * gridManager.tileSpacing;
            }
            else
            {
                actualTileSize = tileSize; // Fallback to inspector value
            }
        }
    }

    void ToggleTowerMode()
    {
        isInTowerMode = !isInTowerMode;
        Debug.Log($"TowerPlacementManager: Tower mode toggled to: {isInTowerMode}");

        if (!isInTowerMode)
        {
            Debug.Log("TowerPlacementManager: Exiting tower mode, clearing highlights");
            ClearHighlight();
        }
        else
        {
            Debug.Log("TowerPlacementManager: Entering tower mode. Look at a tile and click to place a tower.");
        }
    }

    void HandleTowerPlacement()
    {
        if (gridManager == null || playerTransform == null || towerLevels == null)
        {
            Debug.LogError($"TowerPlacementManager: Missing references! GridManager: {gridManager != null}, Player: {playerTransform != null}, TowerLevels: {towerLevels != null}");
            return;
        }

        // Debug: Check if we're in tower mode (only log occasionally to avoid spam)
        if (isInTowerMode && Time.frameCount % 60 == 0) // Log every second (60fps)
        {
            Debug.Log("TowerPlacementManager: In tower placement mode, looking for tiles...");
        }

        // Get the tile in front of the player
        Tile frontTile = GetTileInFrontOfPlayer();

        if (frontTile != null && Time.frameCount % 60 == 0) // Log tile info occasionally
        {
            Debug.Log($"TowerPlacementManager: Looking at tile ({frontTile.gridX}, {frontTile.gridZ}) - CanPlaceTower: {frontTile.CanPlaceTower()}, HasResource: {frontTile.HasResource()}, isRoad: {frontTile.isRoad}, hasTower: {frontTile.hasTower}");
        }

        // Update highlighting
        if (frontTile != currentHighlightedTile)
        {
            ClearHighlight();
            if (frontTile != null)
            {
                // Check if there's already a tower on this tile
                Tower existingTower = null;
                if (frontTile.hasTower && frontTile.towerInstance != null)
                {
                    existingTower = frontTile.towerInstance.GetComponent<Tower>();
                }

                if (existingTower != null)
                {
                    // Check if tower is already at max level
                    if (existingTower.currentLevel >= existingTower.maxLevel)
                    {
                        // For max level towers, just highlight the tile in red without showing any preview
                        HighlightTileOnly(frontTile);
                    }
                    else
                    {
                        bool canUpgrade = existingTower.CanUpgrade();

                        // Tower exists - check if we can upgrade it
                        if (canUpgrade)
                        {
                            HighlightTowerForUpgrade(frontTile, existingTower);
                        }
                        else
                        {
                            HighlightInvalidTile(frontTile);
                        }
                    }
                }
                else if (HasEnoughResourcesForLevel1())
                {
                    // Only highlight if the tile can actually accept a tower
                    if (frontTile.CanPlaceTower())
                    {
                        HighlightTile(frontTile);
                    }
                    else
                    {
                        HighlightInvalidTile(frontTile);
                    }
                }
                else
                {
                    // Show a visual indicator for invalid tiles too
                    HighlightInvalidTile(frontTile);
                }
            }
        }

        // Handle left click to place/upgrade tower (use only Input System)
        bool newInputClick = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        if (newInputClick && frontTile != null)
        {
            Debug.Log($"TowerPlacementManager: Left click detected!");
            Debug.Log($"TowerPlacementManager: Clicked on tile ({frontTile.gridX}, {frontTile.gridZ})");

            // Check if there's already a tower on this tile
            Tower existingTower = null;
            if (frontTile.hasTower && frontTile.towerInstance != null)
            {
                existingTower = frontTile.towerInstance.GetComponent<Tower>();
                Debug.Log($"TowerPlacementManager: Existing tower found at level {existingTower.currentLevel}");
            }

            if (existingTower != null)
            {
                // Check if tower is already at max level
                if (existingTower.currentLevel >= existingTower.maxLevel)
                {
                    Debug.Log("TowerPlacementManager: Tower is already at max level, cannot upgrade further");
                    return; // Exit early, don't try to upgrade
                }

                bool canUpgrade = existingTower.CanUpgrade();
                bool hasResources = HasEnoughResourcesForUpgrade(existingTower);

                Debug.Log($"TowerPlacementManager: Upgrade check - CanUpgrade: {canUpgrade}, HasResources: {hasResources}");

                if (canUpgrade && hasResources)
                {
                    Debug.Log("TowerPlacementManager: Attempting to upgrade tower...");
                    UpgradeTower(frontTile, existingTower);
                }
                else
                {
                    Debug.Log($"TowerPlacementManager: Cannot upgrade tower - CanUpgrade: {canUpgrade}, HasResources: {hasResources}");
                }
            }
            else if (HasEnoughResourcesForLevel1())
            {
                // Check if tile can actually accept a tower before attempting placement
                if (frontTile.CanPlaceTower())
                {
                    Debug.Log("TowerPlacementManager: Attempting to place new tower...");
                    PlaceTower(frontTile);
                }
                else
                {
                    Debug.LogWarning($"TowerPlacementManager: Cannot place tower on tile ({frontTile.gridX}, {frontTile.gridZ}) - {frontTile.GetPlacementBlockReason()}");
                }
            }
            else
            {
                Debug.Log("TowerPlacementManager: Cannot place tower - insufficient resources");
                Debug.Log($"TowerPlacementManager: Current resources - Wood: {Inventory.wood}, Stone: {Inventory.stone}");
                if (towerLevels != null)
                {
                    TowerLevel level1Data = towerLevels.GetTowerLevel(1);
                    if (level1Data != null)
                    {
                        Debug.Log($"TowerPlacementManager: Level 1 tower costs - Wood: {level1Data.woodCost}, Stone: {level1Data.stoneCost}");
                    }
                }
            }
        }
    }

    Tile GetTileInFrontOfPlayer()
    {
        // Recalculate tile size if it's still 0
        if (actualTileSize <= 0)
        {
            CalculateTileSize();
            Debug.Log($"TowerPlacementManager: Recalculated tile size: {actualTileSize}");
        }

        // Calculate the position in front of the player
        Vector3 playerPos = playerTransform.position;
        Vector3 playerForward = playerTransform.forward;

        // Project forward by actualTileSize to get the tile in front
        Vector3 frontPosition = playerPos + playerForward * actualTileSize;

        // Get the grid start position to account for custom positioning
        Vector3 gridStartPosition = gridManager.GetGridStartPosition();

        // Convert world position to grid coordinates relative to grid start
        Vector3 relativePosition = frontPosition - gridStartPosition;
        int gridX = Mathf.RoundToInt(relativePosition.x / actualTileSize);
        int gridZ = Mathf.RoundToInt(relativePosition.z / actualTileSize);

        // Clamp to grid bounds
        gridX = Mathf.Clamp(gridX, 0, gridManager.width - 1);
        gridZ = Mathf.Clamp(gridZ, 0, gridManager.height - 1);

        // Debug tile detection occasionally
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"TowerPlacementManager: Player pos: {playerPos}, Forward: {playerForward}, Front pos: {frontPosition}");
            Debug.Log($"TowerPlacementManager: Grid start: {gridStartPosition}, Relative: {relativePosition}");
            Debug.Log($"TowerPlacementManager: Calculated grid coords: ({gridX}, {gridZ}), Tile size: {actualTileSize}");
        }

        // Get the tile from the grid
        if (gridManager.grid != null && gridX >= 0 && gridX < gridManager.width && gridZ >= 0 && gridZ < gridManager.height)
        {
            Tile tile = gridManager.grid[gridX, gridZ];
            if (tile == null && Time.frameCount % 60 == 0)
            {
                Debug.LogWarning($"TowerPlacementManager: Tile at ({gridX}, {gridZ}) is null!");
            }
            return tile;
        }
        else if (Time.frameCount % 60 == 0)
        {
            Debug.LogWarning($"TowerPlacementManager: Grid coordinates ({gridX}, {gridZ}) are out of bounds! Grid size: {gridManager.width}x{gridManager.height}");
        }

        return null;
    }

    string GetTileBlockReason(Tile tile)
    {
        if (tile.isRoad) return "Road tile";
        if (tile.isOccupied) return "Tile occupied";
        if (tile.hasTower) return "Tower already placed";
        if (tile.HasResource()) return "Resource present (gather it first)";
        if (!HasEnoughResourcesForLevel1())
        {
            if (towerLevels != null)
            {
                TowerLevel level1Data = towerLevels.GetTowerLevel(1);
                if (level1Data != null)
                {
                    return $"Insufficient resources (need {level1Data.woodCost} wood, {level1Data.stoneCost} stone)";
                }
            }
            return "Insufficient resources";
        }
        return "Unknown reason";
    }

    bool HasEnoughResourcesForLevel1()
    {
        if (towerLevels == null) return false;
        TowerLevel level1Data = towerLevels.GetTowerLevel(1);
        if (level1Data == null) return false;
        return Inventory.wood >= level1Data.woodCost && Inventory.stone >= level1Data.stoneCost;
    }

    bool HasEnoughResourcesForUpgrade(Tower tower)
    {
        if (tower == null)
        {
            return false;
        }

        if (towerLevels == null)
        {
            return false;
        }

        TowerLevel nextLevel = tower.GetNextLevel();
        if (nextLevel == null)
        {
            return false;
        }

        bool hasEnoughWood = Inventory.wood >= nextLevel.woodCost;
        bool hasEnoughStone = Inventory.stone >= nextLevel.stoneCost;

        return hasEnoughWood && hasEnoughStone;
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

        }
    }

    void HighlightTileOnly(Tile tile)
    {
        if (tile == null) return;

        currentHighlightedTile = tile;

        // Highlight tile in red without showing any preview
        Renderer rend = tile.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.red;
        }
    }

    void HighlightInvalidTile(Tile tile)
    {
        if (tile == null) return;

        currentHighlightedTile = tile;

        // Highlight invalid tile in red
        Renderer rend = tile.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.red;
        }

        // Create a red preview cube to show it's invalid
        if (towerPreviewPrefab != null)
        {
            currentPreview = Instantiate(towerPreviewPrefab, tile.transform.position + Vector3.up * 0.5f, Quaternion.identity);
            currentPreview.transform.SetParent(tile.transform);

            // Set red transparency
            Renderer previewRenderer = currentPreview.GetComponent<Renderer>();
            if (previewRenderer != null)
            {
                Material previewMaterial = new Material(previewRenderer.material);
                previewMaterial.color = new Color(1f, 0f, 0f, 0.5f); // Red with 50% opacity
                previewMaterial.SetFloat("_Mode", 3); // Set to transparent mode
                previewMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                previewMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                previewMaterial.SetInt("_ZWrite", 0);
                previewMaterial.DisableKeyword("_ALPHATEST_ON");
                previewMaterial.EnableKeyword("_ALPHABLEND_ON");
                previewMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                previewMaterial.renderQueue = 3000;
                previewRenderer.material = previewMaterial;
            }
        }
    }

    void HighlightTile(Tile tile)
    {
        if (tile == null || towerLevels == null) return;

        Debug.Log($"TowerPlacementManager: Highlighting tile at ({tile.gridX}, {tile.gridZ}) for tower placement");

        currentHighlightedTile = tile;
        tile.HighlightForTower();

        // Get Level 1 tower data for preview
        TowerLevel level1Data = towerLevels.GetTowerLevel(1);
        if (level1Data == null)
        {
            Debug.LogError("TowerPlacementManager: Level 1 tower data not found!");
            return;
        }

        Debug.Log($"TowerPlacementManager: Level 1 data found - range: {level1Data.range}, preview prefab: {(level1Data.previewPrefab != null ? "assigned" : "null")}");

        // Create preview tower using Level 1 preview prefab
        if (level1Data.previewPrefab != null)
        {
            currentPreview = Instantiate(level1Data.previewPrefab, tile.transform.position + Vector3.up * 0.5f, Quaternion.identity);
            currentPreview.transform.SetParent(tile.transform);

            // Set transparency
            Renderer previewRenderer = currentPreview.GetComponent<Renderer>();
            if (previewRenderer != null)
            {
                Material previewMaterial = new Material(previewRenderer.material);
                Color previewColor = previewMaterial.color;
                previewColor.a = 0.5f; // 50% opacity
                previewMaterial.color = previewColor;
                previewMaterial.SetFloat("_Mode", 3); // Set to transparent mode
                previewMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                previewMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                previewMaterial.SetInt("_ZWrite", 0);
                previewMaterial.DisableKeyword("_ALPHATEST_ON");
                previewMaterial.EnableKeyword("_ALPHABLEND_ON");
                previewMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                previewMaterial.renderQueue = 3000;
                previewRenderer.material = previewMaterial;
            }
            Debug.Log("TowerPlacementManager: Preview tower created");
        }
        else
        {
            Debug.LogWarning("TowerPlacementManager: No preview prefab assigned for Level 1!");
        }

        // Create range preview for Level 1
        CreateRangePreviewForLevel(tile, level1Data);
    }

    void ClearHighlight()
    {
        if (currentHighlightedTile != null)
        {
            currentHighlightedTile.ClearHighlight();
            currentHighlightedTile = null;
        }

        if (currentPreview != null)
        {
            DestroyImmediate(currentPreview);
            currentPreview = null;
        }

        // Restore hidden tower if it exists and is still valid
        if (hiddenTower != null)
        {
            // Check if the hidden tower still exists before trying to activate it
            if (hiddenTower != null && !hiddenTower.activeInHierarchy)
            {
                hiddenTower.SetActive(true);
            }
            hiddenTower = null;
        }

        // Clear tower for upgrade reference
        currentTowerForUpgrade = null;

        // Clear range preview
        ClearRangePreview();
    }

    void PlaceTower(Tile tile)
    {
        Debug.Log($"TowerPlacementManager: PlaceTower called for tile ({tile.gridX}, {tile.gridZ})");

        if (tile == null || towerLevels == null)
        {
            Debug.LogError($"TowerPlacementManager: PlaceTower failed - tile: {tile != null}, towerLevels: {towerLevels != null}");
            return;
        }

        // Get Level 1 tower data
        TowerLevel level1Data = towerLevels.GetTowerLevel(1);
        if (level1Data == null)
        {
            Debug.LogError("TowerPlacementManager: Level 1 tower data not found!");
            return;
        }

        Debug.Log($"TowerPlacementManager: Placing Level 1 tower (cost: {level1Data.woodCost} wood, {level1Data.stoneCost} stone)");

        // Check if player has enough resources for Level 1
        if (!tile.CanPlaceTowerWithResources(level1Data.woodCost, level1Data.stoneCost))
        {
            // This should rarely happen now since we check CanPlaceTower() before calling PlaceTower()
            Debug.LogWarning($"TowerPlacementManager: Unexpected validation failure in PlaceTower() - {tile.GetPlacementBlockReason()}");
            return;
        }

        // Consume resources
        if (Inventory.wood >= level1Data.woodCost && Inventory.stone >= level1Data.stoneCost)
        {
            Debug.Log("TowerPlacementManager: Consuming resources and placing tower...");
            Inventory.wood -= level1Data.woodCost;
            Inventory.stone -= level1Data.stoneCost;

            // Place the actual tower using Level 1 prefab
            tile.PlaceTower(level1Data.towerPrefab);
            Debug.Log("TowerPlacementManager: Tower placed successfully!");

            // Play tower built sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayTowerBuiltSound();
            }

            // Clear the preview
            ClearHighlight();
        }
        else
        {
            Debug.LogError("TowerPlacementManager: Failed to consume resources for tower placement!");
        }
    }

    void HighlightTowerForUpgrade(Tile tile, Tower tower)
    {
        if (tile == null || tower == null) return;

        currentHighlightedTile = tile;
        currentTowerForUpgrade = tower;
        tile.HighlightForTower();

        // Hide the current tower
        hiddenTower = tower.gameObject;
        hiddenTower.SetActive(false);

        // Get the next level data
        TowerLevel nextLevel = tower.GetNextLevel();
        if (nextLevel == null)
        {
            Debug.LogError("TowerPlacementManager: Next level data not found!");
            return;
        }

        // Create preview of next level tower
        if (nextLevel.previewPrefab != null)
        {
            currentPreview = Instantiate(nextLevel.previewPrefab, tile.transform.position + Vector3.up * 0.5f, Quaternion.identity);
            currentPreview.transform.SetParent(tile.transform);

            // Set transparency
            Renderer previewRenderer = currentPreview.GetComponent<Renderer>();
            if (previewRenderer != null)
            {
                Material previewMaterial = new Material(previewRenderer.material);
                Color previewColor = previewMaterial.color;
                previewColor.a = 0.5f; // 50% opacity
                previewMaterial.color = previewColor;
                previewMaterial.SetFloat("_Mode", 3); // Set to transparent mode
                previewMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                previewMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                previewMaterial.SetInt("_ZWrite", 0);
                previewMaterial.DisableKeyword("_ALPHATEST_ON");
                previewMaterial.EnableKeyword("_ALPHABLEND_ON");
                previewMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                previewMaterial.renderQueue = 3000;
                previewRenderer.material = previewMaterial;
            }
        }

        // Create range preview for next level
        CreateRangePreviewForUpgrade(tile, nextLevel);
    }

    void UpgradeTower(Tile tile, Tower tower)
    {
        if (tile == null || tower == null) return;

        // Get the next level data
        TowerLevel nextLevel = tower.GetNextLevel();
        if (nextLevel == null)
        {
            Debug.LogError("TowerPlacementManager: Next level data not found!");
            return;
        }

        // Check if player has enough resources
        if (Inventory.wood >= nextLevel.woodCost && Inventory.stone >= nextLevel.stoneCost)
        {
            // Consume resources
            Inventory.wood -= nextLevel.woodCost;
            Inventory.stone -= nextLevel.stoneCost;

            // Upgrade the tower visually and functionally
            if (!UpgradeTowerVisual(tile, tower, nextLevel))
            {
                Debug.LogError("TowerPlacementManager: Failed to upgrade tower!");
                // Refund resources
                Inventory.wood += nextLevel.woodCost;
                Inventory.stone += nextLevel.stoneCost;
            }
            else
            {
                // Play tower upgraded sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayTowerUpgradedSound();
                }

                // Upgrade succeeded - clear the hidden tower reference since it's been replaced
                hiddenTower = null;
            }
        }
        else
        {
            Debug.LogWarning($"TowerPlacementManager: Not enough resources for upgrade! Need {nextLevel.woodCost} wood, {nextLevel.stoneCost} stone");
        }

        // Clear the preview (this will also restore the hidden tower if upgrade failed)
        ClearHighlight();
    }

    bool UpgradeTowerVisual(Tile tile, Tower tower, TowerLevel nextLevel)
    {
        if (tile == null || tower == null || nextLevel == null || nextLevel.towerPrefab == null)
        {
            Debug.LogError("TowerPlacementManager: Invalid parameters for tower visual upgrade!");
            return false;
        }

        // Store the current tower's position and rotation
        Vector3 position = tower.transform.position;
        Quaternion rotation = tower.transform.rotation;
        Transform parent = tower.transform.parent;

        // Destroy the old tower
        DestroyImmediate(tower.gameObject);

        // Create the new tower with the next level prefab
        GameObject newTowerObj = Instantiate(nextLevel.towerPrefab, position, rotation, parent);

        // Get the new tower component and update its stats
        Tower newTower = newTowerObj.GetComponent<Tower>();
        if (newTower != null)
        {
            // Update the tower stats to match the next level
            newTower.currentLevel = nextLevel.level;
            newTower.range = nextLevel.range;
            newTower.damage = nextLevel.damage;
            newTower.fireRate = nextLevel.fireRate;
            newTower.projectileSpeed = nextLevel.projectileSpeed;

            // Update the tile's tower reference
            tile.towerInstance = newTowerObj;

            return true;
        }
        else
        {
            Debug.LogError("TowerPlacementManager: New tower prefab doesn't have Tower component!");
            return false;
        }
    }

    void CreateRangePreviewForLevel(Tile tile, TowerLevel levelData)
    {
        if (gridManager == null || gridManager.grid == null || levelData == null) return;

        float towerRange = levelData.range;
        Debug.Log($"TowerPlacementManager: Creating range preview for new tower with range {towerRange}");

        // Use the correct tile size and grid positioning
        float tileSize = actualTileSize;
        Vector3 gridStartPosition = gridManager.GetGridStartPosition();

        // Get tile's grid position relative to grid start
        Vector3 tilePos = tile.transform.position;
        Vector3 relativeTilePos = tilePos - gridStartPosition;
        int tileGridX = Mathf.RoundToInt(relativeTilePos.x / tileSize);
        int tileGridZ = Mathf.RoundToInt(relativeTilePos.z / tileSize);

        // Create diamond pattern
        int rangeInTiles = Mathf.RoundToInt(towerRange / tileSize) + 1; // Add 1 to extend range by one tile
        Debug.Log($"TowerPlacementManager: Range in tiles: {rangeInTiles}, tile size: {tileSize}");

        for (int x = -rangeInTiles; x <= rangeInTiles; x++)
        {
            for (int z = -rangeInTiles; z <= rangeInTiles; z++)
            {
                // Diamond pattern: |x| + |z| <= rangeInTiles
                if (Mathf.Abs(x) + Mathf.Abs(z) <= rangeInTiles)
                {
                    int targetGridX = tileGridX + x;
                    int targetGridZ = tileGridZ + z;

                    // Calculate world position for the range indicator
                    Vector3 cubePos = gridStartPosition + new Vector3(targetGridX * tileSize, 0.1f, targetGridZ * tileSize);

                    // Create transparent blue cube for new tower preview
                    GameObject rangeCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    rangeCube.name = "NewTowerRangePreviewCube";
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
                        Debug.Log($"TowerPlacementManager: Using assigned range indicator material");
                    }
                    else
                    {
                        Debug.LogWarning("TowerPlacementManager: RangeIndicatorMaterial not assigned! Using default material.");
                        // Fallback to a transparent URP material
                        Material fallbackMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                        fallbackMat.SetFloat("_Surface", 1); // Transparent
                        fallbackMat.SetFloat("_Blend", 0); // Alpha
                        fallbackMat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        fallbackMat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        fallbackMat.SetFloat("_ZWrite", 0);
                        fallbackMat.SetFloat("_AlphaClip", 0);
                        fallbackMat.color = new Color(0f, 0f, 1f, 0.5f); // Blue for new tower preview with higher alpha
                        fallbackMat.renderQueue = 3000;
                        renderer.material = fallbackMat;
                    }

                    rangePreviewCubes.Add(rangeCube);
                }
            }
        }

        Debug.Log($"TowerPlacementManager: Created {rangePreviewCubes.Count} range preview cubes");
    }

    void CreateRangePreviewForUpgrade(Tile tile, TowerLevel nextLevel)
    {
        if (gridManager == null || gridManager.grid == null || nextLevel == null) return;

        float towerRange = nextLevel.range;

        // Use the correct tile size and grid positioning
        float tileSize = actualTileSize;
        Vector3 gridStartPosition = gridManager.GetGridStartPosition();

        // Get tile's grid position relative to grid start
        Vector3 tilePos = tile.transform.position;
        Vector3 relativeTilePos = tilePos - gridStartPosition;
        int tileGridX = Mathf.RoundToInt(relativeTilePos.x / tileSize);
        int tileGridZ = Mathf.RoundToInt(relativeTilePos.z / tileSize);

        // Create diamond pattern
        int rangeInTiles = Mathf.RoundToInt(towerRange / tileSize) + 1; // Add 1 to extend range by one tile

        Debug.Log($"TowerPlacementManager: Creating upgrade range preview at ({tileGridX}, {tileGridZ}) with range {rangeInTiles} tiles");

        for (int x = -rangeInTiles; x <= rangeInTiles; x++)
        {
            for (int z = -rangeInTiles; z <= rangeInTiles; z++)
            {
                // Diamond pattern: |x| + |z| <= rangeInTiles
                if (Mathf.Abs(x) + Mathf.Abs(z) <= rangeInTiles)
                {
                    int targetGridX = tileGridX + x;
                    int targetGridZ = tileGridZ + z;

                    // Calculate world position for the range indicator
                    Vector3 cubePos = gridStartPosition + new Vector3(targetGridX * tileSize, 0.1f, targetGridZ * tileSize);

                    // Create transparent green cube for upgrade preview
                    GameObject rangeCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    rangeCube.name = "UpgradeRangePreviewCube";
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
                        Debug.LogWarning("TowerPlacementManager: RangeIndicatorMaterial not assigned! Using default material.");
                        // Fallback to a transparent URP material
                        Material fallbackMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                        fallbackMat.SetFloat("_Surface", 1); // Transparent
                        fallbackMat.SetFloat("_Blend", 0); // Alpha
                        fallbackMat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        fallbackMat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        fallbackMat.SetFloat("_ZWrite", 0);
                        fallbackMat.SetFloat("_AlphaClip", 0);
                        fallbackMat.color = new Color(0f, 1f, 0f, 0.3f); // Green for upgrade preview
                        fallbackMat.renderQueue = 3000;
                        renderer.material = fallbackMat;
                    }

                    rangePreviewCubes.Add(rangeCube);
                }
            }
        }
    }


    void ClearRangePreview()
    {
        foreach (GameObject cube in rangePreviewCubes)
        {
            if (cube != null)
            {
                DestroyImmediate(cube);
            }
        }
        rangePreviewCubes.Clear();
    }

    // Public method to force exit tower mode (called by WaveManager when day starts)
    public void ForceExitTowerMode()
    {
        if (isInTowerMode)
        {
            isInTowerMode = false;
            ClearHighlight();
        }
    }
}
