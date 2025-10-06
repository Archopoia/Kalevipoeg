using UnityEngine;

/// <summary>
/// Example script showing how to customize tiles in the grid manager
/// You can attach this to any GameObject to test tile customization
/// </summary>
public class TileCustomizer : MonoBehaviour
{
    public GridManager gridManager;

    void Start()
    {
        // Wait for grid to be generated
        if (gridManager != null)
        {
            gridManager.OnGridGenerated += OnGridReady;
        }
    }

    void OnGridReady(GridManager gm)
    {
        // Example: Create a custom path pattern
        CreateCustomPath();
    }

    void CreateCustomPath()
    {
        if (gridManager == null) return;

        // Example: Create an L-shaped path
        // Vertical part of L
        for (int z = 0; z < gridManager.height; z++)
        {
            gridManager.SetTileAsRoad(7, z); // Column 7
        }

        // Horizontal part of L
        for (int x = 7; x < gridManager.width; x++)
        {
            gridManager.SetTileAsRoad(x, 7); // Row 7
        }
    }

    // Method to create a straight path (like the original system)
    public void CreateStraightPath(int column)
    {
        if (gridManager == null) return;

        for (int z = 0; z < gridManager.height; z++)
        {
            gridManager.SetTileAsRoad(column, z);
        }
    }

    // Method to create a random path
    public void CreateRandomPath()
    {
        if (gridManager == null) return;

        int startX = Random.Range(0, gridManager.width);
        int startZ = Random.Range(0, gridManager.height);

        // Create a random zigzag path
        int currentX = startX;
        int currentZ = startZ;

        for (int i = 0; i < 10; i++) // Create 10 road tiles
        {
            gridManager.SetTileAsRoad(currentX, currentZ);

            // Randomly move in a direction
            int direction = Random.Range(0, 4);
            switch (direction)
            {
                case 0: currentX++; break; // Right
                case 1: currentX--; break; // Left
                case 2: currentZ++; break; // Up
                case 3: currentZ--; break; // Down
            }

            // Keep within bounds
            currentX = Mathf.Clamp(currentX, 0, gridManager.width - 1);
            currentZ = Mathf.Clamp(currentZ, 0, gridManager.height - 1);
        }
    }
}


