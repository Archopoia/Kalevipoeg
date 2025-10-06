using UnityEngine;

public class Tile : MonoBehaviour
{
    public int gridX;
    public int gridZ;
    public bool isRoad = false;
    public bool isOccupied = false;
    public bool hasTower = false;
    public GameObject towerInstance = null;

    private Renderer rend;
    private Color originalColor;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color; // Store the original color
    }

    public void SetRoad()
    {
        isRoad = true;
        originalColor = Color.gray;
        rend.material.color = Color.gray;
    }

    public void SetBuildable()
    {
        isRoad = false;
        originalColor = Color.green;
        rend.material.color = Color.green;
    }

    public void SetTown()
    {
        originalColor = Color.blue;
        rend.material.color = Color.blue;
    }

    public void SetSpawn()
    {
        originalColor = Color.red;
        rend.material.color = Color.red;
    }

    public bool CanPlaceTower()
    {
        bool canPlace = !isRoad && !isOccupied && !hasTower && !HasResource();

        // Debug logging for tower placement validation - only log when validation fails
        if (!canPlace && Time.frameCount % 60 == 0) // Log occasionally to avoid spam
        {
            Debug.LogWarning($"Tile ({gridX}, {gridZ}) CanPlaceTower FAILED:");
            Debug.LogWarning($"  - !isRoad: {!isRoad} (isRoad: {isRoad})");
            Debug.LogWarning($"  - !isOccupied: {!isOccupied} (isOccupied: {isOccupied})");
            Debug.LogWarning($"  - !hasTower: {!hasTower} (hasTower: {hasTower})");
            Debug.LogWarning($"  - !HasResource(): {!HasResource()} (HasResource: {HasResource()})");
            Debug.LogWarning($"  - Final result: {canPlace}");
        }

        return canPlace;
    }

    public bool CanPlaceTowerWithResources(int woodCost, int stoneCost)
    {
        bool canPlaceTile = CanPlaceTower();
        bool hasWood = Inventory.wood >= woodCost;
        bool hasStone = Inventory.stone >= stoneCost;
        bool finalResult = canPlaceTile && hasWood && hasStone;

        // Debug logging for resource validation - only log when validation fails
        if (!finalResult && Time.frameCount % 60 == 0) // Log occasionally to avoid spam
        {
            Debug.LogWarning($"Tile ({gridX}, {gridZ}) CanPlaceTowerWithResources FAILED:");
            Debug.LogWarning($"  - CanPlaceTower: {canPlaceTile}");
            Debug.LogWarning($"  - HasWood ({Inventory.wood}/{woodCost}): {hasWood}");
            Debug.LogWarning($"  - HasStone ({Inventory.stone}/{stoneCost}): {hasStone}");
            Debug.LogWarning($"  - Final result: {finalResult}");
        }

        return finalResult;
    }

    public bool HasResource()
    {
        bool hasResource = false;
        string resourceInfo = "";

        // Check for child objects named "Stone" or Farmables components that are actual resources
        foreach (Transform child in transform)
        {
            if (child.name == "Stone")
            {
                hasResource = true;
                resourceInfo += $"Stone ({child.name}), ";
            }

            // Check for Farmables components that are actual resources (not decorative grass)
            Farmables farmable = child.GetComponent<Farmables>();
            if (farmable != null && (farmable.isTree || farmable.isStone))
            {
                hasResource = true;
                string type = farmable.isTree ? "Tree" : "Stone";
                resourceInfo += $"Farmable {type} ({child.name}), ";
            }
        }

        // Debug logging for resource detection
        if (Time.frameCount % 60 == 0 && hasResource) // Only log when resources are found to avoid spam
        {
            Debug.Log($"Tile ({gridX}, {gridZ}) HasResource: {hasResource} - Found: {resourceInfo.TrimEnd(' ', ',')}");
        }

        return hasResource;
    }

    public void OnResourceDestroyed()
    {
        // Called when a resource on this tile is destroyed
        // This will allow tower placement if no other resources remain
        bool canPlaceNow = CanPlaceTower();
        Debug.Log($"Tile ({gridX}, {gridZ}): Resource destroyed. Can place tower now: {canPlaceNow}");
    }

    public string GetPlacementBlockReason()
    {
        if (isRoad) return "This tile is a road - towers cannot be placed on roads";
        if (isOccupied) return "This tile is occupied by another structure";
        if (hasTower) return "A tower is already placed on this tile";
        if (HasResource()) return "This tile has resources (trees/stones) - gather them first before placing a tower";
        return "Unknown reason - tile appears to be valid for tower placement";
    }

    [ContextMenu("Debug Tile Contents")]
    public void DebugTileContents()
    {
        Debug.Log($"Tile ({gridX}, {gridZ}) Contents:");
        Debug.Log($"  - isRoad: {isRoad}");
        Debug.Log($"  - isOccupied: {isOccupied}");
        Debug.Log($"  - hasTower: {hasTower}");
        Debug.Log($"  - HasResource(): {HasResource()}");
        Debug.Log($"  - CanPlaceTower(): {CanPlaceTower()}");

        Debug.Log("  Child objects:");
        foreach (Transform child in transform)
        {
            Farmables farmable = child.GetComponent<Farmables>();
            string farmableInfo = farmable != null ? $" (Farmables: isTree={farmable.isTree}, isStone={farmable.isStone})" : "";
            Debug.Log($"    - {child.name}{farmableInfo}");
        }
    }

    public void PlaceTower(GameObject towerPrefab)
    {
        Debug.Log($"Tile ({gridX}, {gridZ}): PlaceTower called with prefab: {(towerPrefab != null ? towerPrefab.name : "null")}");

        if (CanPlaceTower())
        {
            Debug.Log($"Tile ({gridX}, {gridZ}): Placing tower...");
            hasTower = true;
            isOccupied = true;
            towerInstance = Instantiate(towerPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            towerInstance.transform.SetParent(transform);

            // Keep the tile highlighted to show it's occupied
            rend.material.color = Color.yellow;
            Debug.Log($"Tile ({gridX}, {gridZ}): Tower placed successfully! Tower instance: {(towerInstance != null ? towerInstance.name : "null")}");
        }
        else
        {
            Debug.LogError($"Tile ({gridX}, {gridZ}): Cannot place tower - validation failed!");
            Debug.LogError($"Tile ({gridX}, {gridZ}): CanPlaceTower: {CanPlaceTower()}, isRoad: {isRoad}, isOccupied: {isOccupied}, hasTower: {hasTower}, HasResource: {HasResource()}");
        }
    }

    public void HighlightForTower()
    {
        if (CanPlaceTower())
        {
            rend.material.color = Color.green; // Highlight in green for valid placement
        }
        else
        {
            rend.material.color = Color.red; // Highlight in red for invalid placement
        }
    }

    public void ClearHighlight()
    {
        if (!hasTower) // Only clear highlight if there's no tower
        {
            rend.material.color = originalColor; // Restore original color
        }
        // If hasTower is true, keep the yellow highlight to show it's occupied
    }
}

