using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Centralized audio manager for the game. Handles all audio playback and provides easy inspector configuration.
/// This is a singleton that can be accessed from anywhere in the game.
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [Tooltip("Main audio source for general sound effects")]
    public AudioSource sfxSource;

    [Tooltip("Audio source for ambient/environmental sounds")]
    public AudioSource ambientSource;

    [Tooltip("Audio source for music (Note: MusicManager handles music separately)")]
    public AudioSource musicSource;

    [Header("Gathering Sounds")]
    [Tooltip("Sound played when gathering wood from trees")]
    public AudioClip gatherTreeSound;

    [Tooltip("Sound played when gathering stone")]
    public AudioClip gatherStoneSound;

    [Tooltip("Sound played when a tree is completely destroyed")]
    public AudioClip treeDeletedSound;

    [Tooltip("Sound played when stone is completely destroyed")]
    public AudioClip stoneDeletedSound;

    [Header("Player Movement Sounds")]
    [Tooltip("Multiple walking sounds that will be randomly selected")]
    public AudioClip[] playerWalkingSounds = new AudioClip[4];

    [Tooltip("Volume for player walking sounds")]
    [Range(0f, 1f)]
    public float playerWalkingVolume = 0.7f;

    [Header("Tower Sounds")]
    [Tooltip("Sound played when a tower is built")]
    public AudioClip towerBuiltSound;

    [Tooltip("Sound played when a tower is upgraded")]
    public AudioClip towerUpgradedSound;

    [Tooltip("Multiple shooting sounds that will be randomly selected")]
    public AudioClip[] towerShootSounds = new AudioClip[4];

    [Tooltip("Volume for tower shooting sounds")]
    [Range(0f, 1f)]
    public float towerShootVolume = 0.7f;

    [Header("Combat Sounds")]
    [Tooltip("Sound played when an enemy is hit")]
    public AudioClip enemyHitSound;

    [Tooltip("Multiple death sounds that will be randomly selected")]
    public AudioClip[] enemyDeathSounds = new AudioClip[3];

    [Tooltip("Volume for enemy death sounds")]
    [Range(0f, 1f)]
    public float enemyDeathVolume = 0.8f;

    [Tooltip("Sound played when the village/town is hit")]
    public AudioClip villageHitSound;

    [Header("Enemy Movement Sounds")]
    [Tooltip("Walking sounds for Enemy Type 1")]
    public AudioClip[] enemy1WalkingSounds = new AudioClip[3];

    [Tooltip("Walking sounds for Enemy Type 2")]
    public AudioClip[] enemy2WalkingSounds = new AudioClip[3];

    [Tooltip("Walking sounds for Enemy Type 3")]
    public AudioClip[] enemy3WalkingSounds = new AudioClip[3];

    [Tooltip("Volume for enemy walking sounds")]
    [Range(0f, 1f)]
    public float enemyWalkingVolume = 0.5f;

    [Header("Enemy Ambient Sounds")]
    [Tooltip("Ambient/idle sound for Enemy Type 1")]
    public AudioClip enemy1AmbientSound;

    [Tooltip("Ambient/idle sound for Enemy Type 2")]
    public AudioClip enemy2AmbientSound;

    [Tooltip("Ambient/idle sound for Enemy Type 3")]
    public AudioClip enemy3AmbientSound;

    [Tooltip("Volume for enemy ambient sounds")]
    [Range(0f, 1f)]
    public float enemyAmbientVolume = 0.4f;

    [Header("Game Events")]
    [Tooltip("Sound played when WaveTrigger is crossed")]
    public AudioClip waveTriggerCrossedSound;

    [Tooltip("Sound played when day cycle begins")]
    public AudioClip dayCycleSound;

    [Tooltip("Sound played when night cycle begins")]
    public AudioClip nightCycleSound;

    [Tooltip("Sound played when game over occurs")]
    public AudioClip gameOverSound;

    [Header("Audio Settings")]
    [Tooltip("Master volume for all sound effects")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;

    [Tooltip("Volume for ambient sounds")]
    [Range(0f, 1f)]
    public float ambientVolume = 0.8f;

    [Tooltip("Volume for music")]
    [Range(0f, 1f)]
    public float musicVolume = 0.6f;

    // Singleton instance
    public static AudioManager Instance { get; private set; }

    // Random audio players for cycling through sounds
    private RandomAudioPlayer playerWalkingPlayer;
    private RandomAudioPlayer enemy1WalkingPlayer;
    private RandomAudioPlayer enemy2WalkingPlayer;
    private RandomAudioPlayer enemy3WalkingPlayer;
    private RandomAudioPlayer towerShootPlayer;
    private RandomAudioPlayer enemyDeathPlayer;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
            InitializeRandomPlayers();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeAudioSources()
    {
        // Create audio sources if they don't exist
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.playOnAwake = false;
            ambientSource.loop = true;
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }
    }

    void InitializeRandomPlayers()
    {
        // Initialize random audio players for cycling sounds
        playerWalkingPlayer = new RandomAudioPlayer(playerWalkingSounds, sfxSource, playerWalkingVolume);
        enemy1WalkingPlayer = new RandomAudioPlayer(enemy1WalkingSounds, sfxSource, enemyWalkingVolume);
        enemy2WalkingPlayer = new RandomAudioPlayer(enemy2WalkingSounds, sfxSource, enemyWalkingVolume);
        enemy3WalkingPlayer = new RandomAudioPlayer(enemy3WalkingSounds, sfxSource, enemyWalkingVolume);
        towerShootPlayer = new RandomAudioPlayer(towerShootSounds, sfxSource, towerShootVolume);
        enemyDeathPlayer = new RandomAudioPlayer(enemyDeathSounds, sfxSource, enemyDeathVolume);
    }

    void Update()
    {
        // Update volumes based on settings
        sfxSource.volume = masterVolume;
        ambientSource.volume = ambientVolume * masterVolume;
        musicSource.volume = musicVolume * masterVolume;
    }

    #region Public Audio Methods

    /// <summary>
    /// Play a gathering sound (tree or stone)
    /// </summary>
    public void PlayGatherSound(bool isTree)
    {
        AudioClip clip = isTree ? gatherTreeSound : gatherStoneSound;
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Play a deletion sound (tree or stone)
    /// </summary>
    public void PlayDeletionSound(bool isTree)
    {
        AudioClip clip = isTree ? treeDeletedSound : stoneDeletedSound;
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Play a random player walking sound
    /// </summary>
    public void PlayPlayerWalkingSound()
    {
        playerWalkingPlayer?.PlayRandomSound();
    }

    /// <summary>
    /// Play a random enemy walking sound for the specified enemy type
    /// </summary>
    public void PlayEnemyWalkingSound(int enemyType)
    {
        switch (enemyType)
        {
            case 1:
                enemy1WalkingPlayer?.PlayRandomSound();
                break;
            case 2:
                enemy2WalkingPlayer?.PlayRandomSound();
                break;
            case 3:
                enemy3WalkingPlayer?.PlayRandomSound();
                break;
            default:
                Debug.LogWarning($"AudioManager: Unknown enemy type {enemyType}");
                break;
        }
    }

    /// <summary>
    /// Play an enemy ambient sound for the specified enemy type
    /// </summary>
    public void PlayEnemyAmbientSound(int enemyType)
    {
        AudioClip clipToPlay = null;

        switch (enemyType)
        {
            case 1:
                clipToPlay = enemy1AmbientSound;
                break;
            case 2:
                clipToPlay = enemy2AmbientSound;
                break;
            case 3:
                clipToPlay = enemy3AmbientSound;
                break;
            default:
                Debug.LogWarning($"AudioManager: Unknown enemy type {enemyType} for ambient sound");
                return;
        }

        if (clipToPlay != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clipToPlay, enemyAmbientVolume);
        }
    }

    /// <summary>
    /// Play tower built sound
    /// </summary>
    public void PlayTowerBuiltSound()
    {
        if (towerBuiltSound != null)
        {
            sfxSource.PlayOneShot(towerBuiltSound);
        }
    }

    /// <summary>
    /// Play tower upgraded sound
    /// </summary>
    public void PlayTowerUpgradedSound()
    {
        if (towerUpgradedSound != null)
        {
            sfxSource.PlayOneShot(towerUpgradedSound);
        }
    }

    /// <summary>
    /// Play a random tower shoot sound
    /// </summary>
    public void PlayTowerShootSound()
    {
        towerShootPlayer?.PlayRandomSound();
    }

    /// <summary>
    /// Play enemy hit sound
    /// </summary>
    public void PlayEnemyHitSound()
    {
        if (enemyHitSound != null)
        {
            sfxSource.PlayOneShot(enemyHitSound);
        }
    }

    /// <summary>
    /// Play a random enemy death sound
    /// </summary>
    public void PlayEnemyDeathSound()
    {
        enemyDeathPlayer?.PlayRandomSound();
    }

    /// <summary>
    /// Play village hit sound
    /// </summary>
    public void PlayVillageHitSound()
    {
        if (villageHitSound != null)
        {
            sfxSource.PlayOneShot(villageHitSound);
        }
    }

    /// <summary>
    /// Play wave trigger crossed sound
    /// </summary>
    public void PlayWaveTriggerCrossedSound()
    {
        if (waveTriggerCrossedSound != null)
        {
            sfxSource.PlayOneShot(waveTriggerCrossedSound);
        }
    }

    /// <summary>
    /// Play day cycle sound
    /// </summary>
    public void PlayDayCycleSound()
    {
        if (dayCycleSound != null)
        {
            sfxSource.PlayOneShot(dayCycleSound);
        }
    }

    /// <summary>
    /// Play night cycle sound
    /// </summary>
    public void PlayNightCycleSound()
    {
        if (nightCycleSound != null)
        {
            sfxSource.PlayOneShot(nightCycleSound);
        }
    }

    /// <summary>
    /// Play game over sound
    /// </summary>
    public void PlayGameOverSound()
    {
        if (gameOverSound != null)
        {
            sfxSource.PlayOneShot(gameOverSound);
        }
    }

    /// <summary>
    /// Play any custom audio clip
    /// </summary>
    public void PlayCustomSound(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Test All Sounds")]
    public void TestAllSounds()
    {
        Debug.Log("AudioManager: Testing all sounds...");

        PlayGatherSound(true);  // Tree
        Invoke(nameof(TestGatherStone), 0.5f);
        Invoke(nameof(TestTreeDeleted), 1f);
        Invoke(nameof(TestStoneDeleted), 1.5f);
        Invoke(nameof(TestPlayerWalking), 2f);
        Invoke(nameof(TestTowerBuilt), 2.5f);
        Invoke(nameof(TestTowerUpgraded), 3f);
        Invoke(nameof(TestTowerShoot), 3.2f);
        Invoke(nameof(TestEnemyHit), 3.5f);
        Invoke(nameof(TestEnemyDeath), 3.7f);
        Invoke(nameof(TestVillageHit), 4f);
        Invoke(nameof(TestEnemyWalking), 4.5f);
        Invoke(nameof(TestEnemyAmbient), 4.8f);
        Invoke(nameof(TestWaveTrigger), 5f);
        Invoke(nameof(TestDayCycle), 5.5f);
        Invoke(nameof(TestNightCycle), 6f);
        Invoke(nameof(TestGameOver), 6.5f);
    }

    private void TestGatherStone() => PlayGatherSound(false);
    private void TestTreeDeleted() => PlayDeletionSound(true);
    private void TestStoneDeleted() => PlayDeletionSound(false);
    private void TestPlayerWalking() => PlayPlayerWalkingSound();
    private void TestTowerBuilt() => PlayTowerBuiltSound();
    private void TestTowerUpgraded() => PlayTowerUpgradedSound();
    private void TestTowerShoot() => PlayTowerShootSound();
    private void TestEnemyHit() => PlayEnemyHitSound();
    private void TestEnemyDeath() => PlayEnemyDeathSound();
    private void TestVillageHit() => PlayVillageHitSound();
    private void TestEnemyWalking() => PlayEnemyWalkingSound(1);
    private void TestEnemyAmbient() => PlayEnemyAmbientSound(1);
    private void TestWaveTrigger() => PlayWaveTriggerCrossedSound();
    private void TestDayCycle() => PlayDayCycleSound();
    private void TestNightCycle() => PlayNightCycleSound();
    private void TestGameOver() => PlayGameOverSound();

    #endregion
}
