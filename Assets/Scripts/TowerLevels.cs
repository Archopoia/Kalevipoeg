using UnityEngine;

[System.Serializable]
public class TowerLevel
{
    [Header("Tower Level Stats")]
    public int level = 1;
    public float range = 20f; // Starting range for level 1
    public float damage = 1f;
    public float fireRate = 1f; // Attacks per second
    public float projectileSpeed = 10f;

    [Header("Visual")]
    public GameObject towerPrefab;
    public GameObject previewPrefab; // Semi-transparent preview version

    [Header("Upgrade Costs")]
    public int woodCost = 3;
    public int stoneCost = 3;

    [Header("Description")]
    public string description = "Tower Level";

    public TowerLevel(int level, float range, float damage, float fireRate, float projectileSpeed,
                     GameObject towerPrefab, GameObject previewPrefab, int woodCost, int stoneCost, string description)
    {
        this.level = level;
        this.range = range;
        this.damage = damage;
        this.fireRate = fireRate;
        this.projectileSpeed = projectileSpeed;
        this.towerPrefab = towerPrefab;
        this.previewPrefab = previewPrefab;
        this.woodCost = woodCost;
        this.stoneCost = stoneCost;
        this.description = description;
    }
}

public class TowerLevels : MonoBehaviour
{
    [Header("Tower Level Configuration")]
    public TowerLevel[] towerLevels = new TowerLevel[3];

    [Header("Range Progression")]
    public float baseRange = 20f;
    public float rangeIncreasePerLevel = 10f;

    void Awake()
    {
        // Only initialize default tower levels if completely unconfigured
        if (towerLevels == null || towerLevels.Length == 0)
        {
            Debug.Log("TowerLevels: No tower levels configured, initializing defaults");
            InitializeDefaultLevels();
        }
        else
        {
            Debug.Log($"TowerLevels: Found {towerLevels.Length} configured tower levels");
        }
    }

    void InitializeDefaultLevels()
    {
        towerLevels = new TowerLevel[3];

        // Level 1 - Basic Tower
        towerLevels[0] = new TowerLevel(
            level: 1,
            range: baseRange,
            damage: 1f,
            fireRate: 1f,
            projectileSpeed: 10f,
            towerPrefab: null, // Will be set in inspector
            previewPrefab: null, // Will be set in inspector
            woodCost: 3, // Default cost - can be changed in inspector
            stoneCost: 0, // Default cost - can be changed in inspector
            description: "Basic Tower - Level 1"
        );

        // Level 2 - Improved Tower
        towerLevels[1] = new TowerLevel(
            level: 2,
            range: baseRange + rangeIncreasePerLevel,
            damage: 2f,
            fireRate: 1.2f,
            projectileSpeed: 12f,
            towerPrefab: null, // Will be set in inspector
            previewPrefab: null, // Will be set in inspector
            woodCost: 3, // Default cost - can be changed in inspector
            stoneCost: 3, // Default cost - can be changed in inspector
            description: "Improved Tower - Level 2"
        );

        // Level 3 - Advanced Tower
        towerLevels[2] = new TowerLevel(
            level: 3,
            range: baseRange + (rangeIncreasePerLevel * 2),
            damage: 4f,
            fireRate: 1.5f,
            projectileSpeed: 15f,
            towerPrefab: null, // Will be set in inspector
            previewPrefab: null, // Will be set in inspector
            woodCost: 0, // Default cost - can be changed in inspector
            stoneCost: 9, // Default cost - can be changed in inspector
            description: "Advanced Tower - Level 3"
        );
    }

    public TowerLevel GetTowerLevel(int level)
    {
        if (level >= 1 && level <= towerLevels.Length)
        {
            TowerLevel result = towerLevels[level - 1];
            return result;
        }
        Debug.LogError($"TowerLevels: Requested level {level} is out of range (1-{towerLevels.Length})");
        return null;
    }

    public TowerLevel GetNextTowerLevel(int currentLevel)
    {
        if (currentLevel < towerLevels.Length)
        {
            TowerLevel result = towerLevels[currentLevel];
            return result;
        }
        return null;
    }

    public bool CanUpgradeTower(int currentLevel)
    {
        return currentLevel < towerLevels.Length;
    }

    public int GetMaxLevel()
    {
        return towerLevels.Length;
    }

    [ContextMenu("Debug Tower Costs")]
    public void DebugTowerCosts()
    {
        Debug.Log("=== TOWER COSTS DEBUG ===");
        for (int i = 0; i < towerLevels.Length; i++)
        {
            if (towerLevels[i] != null)
            {
                TowerLevel level = towerLevels[i];
                Debug.Log($"Level {level.level}: Wood={level.woodCost}, Stone={level.stoneCost}");
            }
            else
            {
                Debug.LogWarning($"Level {i + 1}: NULL");
            }
        }
        Debug.Log("=== END TOWER COSTS DEBUG ===");
    }
}