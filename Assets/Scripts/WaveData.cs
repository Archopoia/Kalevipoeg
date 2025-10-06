using UnityEngine;

namespace GameJamKalev
{
    [System.Serializable]
    public class EnemySpawnData
    {
    [Header("Enemy Configuration")]
    public GameObject enemyPrefab;
    public int spawnCount = 1;
    public float spawnDelay = 0.5f; // Delay between spawning this enemy type.

    [Header("Spawn Timing")]
    public float startDelay = 0f; // Delay before this enemy type starts spawning
    public bool spawnInSequence = true; // If false, spawns randomly throughout the wave
    }

    [CreateAssetMenu(fileName = "New Wave Data", menuName = "Wave System/Wave Data")]
    public class WaveData : ScriptableObject
    {
        [Header("Wave Information")]
        public string waveName = "Wave";
        public int waveNumber = 1;

        [Header("Enemy Spawns")]
        public EnemySpawnData[] enemySpawnData = new EnemySpawnData[1];

        [Header("Wave Settings")]
        public float waveDuration = 30f; // Maximum time for this wave
        public float timeBetweenEnemies = 0.5f; // Base delay between any enemy spawns

        [Header("Wave Completion")]
        public bool requiresAllEnemiesDead = true; // If false, wave completes after duration
        public int maxEnemiesAlive = 10; // Maximum enemies that can be alive at once
    }
}
