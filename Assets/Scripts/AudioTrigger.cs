using UnityEngine;

/// <summary>
/// Simple component that triggers audio events based on Unity events.
/// Attach this to any GameObject to easily add audio functionality.
/// </summary>
public class AudioTrigger : MonoBehaviour
{
    [Header("Audio Events")]
    [Tooltip("Sound to play when this object is enabled")]
    public AudioClip onEnableSound;

    [Tooltip("Sound to play when this object is disabled")]
    public AudioClip onDisableSound;

    [Tooltip("Sound to play when this object starts")]
    public AudioClip onStartSound;

    [Tooltip("Sound to play when this object is destroyed")]
    public AudioClip onDestroySound;

    [Header("Trigger Settings")]
    [Tooltip("Volume multiplier for sounds played by this trigger")]
    [Range(0f, 1f)]
    public float volumeMultiplier = 1f;

    [Tooltip("Delay before playing onStart sound")]
    public float startDelay = 0f;

    [Tooltip("Delay before playing onDestroy sound")]
    public float destroyDelay = 0f;

    [Header("Debug")]
    [Tooltip("Enable debug logs for this audio trigger")]
    public bool enableDebugLogs = false;

    void Start()
    {
        if (onStartSound != null)
        {
            if (startDelay > 0)
            {
                Invoke(nameof(PlayStartSound), startDelay);
            }
            else
            {
                PlayStartSound();
            }
        }
    }

    void OnEnable()
    {
        if (onEnableSound != null)
        {
            PlaySound(onEnableSound);
        }
    }

    void OnDisable()
    {
        if (onDisableSound != null)
        {
            PlaySound(onDisableSound);
        }
    }

    void OnDestroy()
    {
        if (onDestroySound != null)
        {
            if (destroyDelay > 0)
            {
                // Use a coroutine for delayed destroy sound
                StartCoroutine(PlayDestroySoundDelayed());
            }
            else
            {
                PlaySound(onDestroySound);
            }
        }
    }

    private System.Collections.IEnumerator PlayDestroySoundDelayed()
    {
        yield return new WaitForSeconds(destroyDelay);
        PlaySound(onDestroySound);
    }

    private void PlayStartSound()
    {
        PlaySound(onStartSound);
    }

    /// <summary>
    /// Play a specific audio clip
    /// </summary>
    public void PlaySound(AudioClip clip)
    {
        if (clip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCustomSound(clip, volumeMultiplier);

            if (enableDebugLogs)
            {
                Debug.Log($"AudioTrigger on {gameObject.name}: Playing sound {clip.name}");
            }
        }
    }

    /// <summary>
    /// Play a custom sound with specific volume
    /// </summary>
    public void PlayCustomSound(AudioClip clip, float volume = 1f)
    {
        if (clip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCustomSound(clip, volume * volumeMultiplier);

            if (enableDebugLogs)
            {
                Debug.Log($"AudioTrigger on {gameObject.name}: Playing custom sound {clip.name} at volume {volume}");
            }
        }
    }

    #region Public Methods for Common Game Events

    /// <summary>
    /// Play gather tree sound
    /// </summary>
    public void PlayGatherTreeSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGatherSound(true);
        }
    }

    /// <summary>
    /// Play gather stone sound
    /// </summary>
    public void PlayGatherStoneSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGatherSound(false);
        }
    }

    /// <summary>
    /// Play tree deleted sound
    /// </summary>
    public void PlayTreeDeletedSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDeletionSound(true);
        }
    }

    /// <summary>
    /// Play stone deleted sound
    /// </summary>
    public void PlayStoneDeletedSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDeletionSound(false);
        }
    }

    /// <summary>
    /// Play player walking sound
    /// </summary>
    public void PlayPlayerWalkingSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerWalkingSound();
        }
    }

    /// <summary>
    /// Play tower built sound
    /// </summary>
    public void PlayTowerBuiltSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTowerBuiltSound();
        }
    }

    /// <summary>
    /// Play tower upgraded sound
    /// </summary>
    public void PlayTowerUpgradedSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTowerUpgradedSound();
        }
    }

    /// <summary>
    /// Play tower shoot sound
    /// </summary>
    public void PlayTowerShootSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTowerShootSound();
        }
    }

    /// <summary>
    /// Play enemy hit sound
    /// </summary>
    public void PlayEnemyHitSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyHitSound();
        }
    }

    /// <summary>
    /// Play village hit sound
    /// </summary>
    public void PlayVillageHitSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayVillageHitSound();
        }
    }

    /// <summary>
    /// Play enemy walking sound for specific enemy type
    /// </summary>
    public void PlayEnemyWalkingSound(int enemyType)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyWalkingSound(enemyType);
        }
    }

    /// <summary>
    /// Play wave trigger crossed sound
    /// </summary>
    public void PlayWaveTriggerCrossedSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWaveTriggerCrossedSound();
        }
    }

    /// <summary>
    /// Play day cycle sound
    /// </summary>
    public void PlayDayCycleSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDayCycleSound();
        }
    }

    /// <summary>
    /// Play night cycle sound
    /// </summary>
    public void PlayNightCycleSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayNightCycleSound();
        }
    }

    /// <summary>
    /// Play game over sound
    /// </summary>
    public void PlayGameOverSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOverSound();
        }
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Test All Audio Events")]
    public void TestAllAudioEvents()
    {
        Debug.Log($"AudioTrigger on {gameObject.name}: Testing all audio events...");

        PlayGatherTreeSound();
        Invoke(nameof(TestGatherStone), 0.5f);
        Invoke(nameof(TestTreeDeleted), 1f);
        Invoke(nameof(TestStoneDeleted), 1.5f);
        Invoke(nameof(TestPlayerWalking), 2f);
        Invoke(nameof(TestTowerBuilt), 2.5f);
        Invoke(nameof(TestTowerUpgraded), 3f);
        Invoke(nameof(TestTowerShoot), 3.2f);
        Invoke(nameof(TestEnemyHit), 3.5f);
        Invoke(nameof(TestVillageHit), 4f);
        Invoke(nameof(TestEnemyWalking), 4.5f);
        Invoke(nameof(TestWaveTrigger), 5f);
        Invoke(nameof(TestDayCycle), 5.5f);
        Invoke(nameof(TestNightCycle), 6f);
        Invoke(nameof(TestGameOver), 6.5f);
    }

    private void TestGatherStone() => PlayGatherStoneSound();
    private void TestTreeDeleted() => PlayTreeDeletedSound();
    private void TestStoneDeleted() => PlayStoneDeletedSound();
    private void TestPlayerWalking() => PlayPlayerWalkingSound();
    private void TestTowerBuilt() => PlayTowerBuiltSound();
    private void TestTowerUpgraded() => PlayTowerUpgradedSound();
    private void TestTowerShoot() => PlayTowerShootSound();
    private void TestEnemyHit() => PlayEnemyHitSound();
    private void TestVillageHit() => PlayVillageHitSound();
    private void TestEnemyWalking() => PlayEnemyWalkingSound(1);
    private void TestWaveTrigger() => PlayWaveTriggerCrossedSound();
    private void TestDayCycle() => PlayDayCycleSound();
    private void TestNightCycle() => PlayNightCycleSound();
    private void TestGameOver() => PlayGameOverSound();

    #endregion
}
