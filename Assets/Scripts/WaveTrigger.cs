using UnityEngine;

public class WaveTrigger : MonoBehaviour
{
    [Header("Wave Manager Reference")]
    public EnhancedWaveManager waveManager;

    [Header("Trigger Settings")]
    public bool hasTriggered = false;

    [Header("Debug Settings")]
    public bool enableDebugLogs = true;

    void Start()
    {
        // Find the wave manager if not assigned
        if (waveManager == null)
        {
            waveManager = FindFirstObjectByType<EnhancedWaveManager>();
        }

        if (waveManager == null)
        {
            Debug.LogError("WaveTrigger: No EnhancedWaveManager found in scene!");
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log("WaveTrigger: EnhancedWaveManager found and assigned!");
        }

        // Debug collider setup
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("WaveTrigger: No Collider component found! Please add a Collider component.");
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log($"WaveTrigger: Collider found - IsTrigger: {col.isTrigger}, Type: {col.GetType().Name}");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (enableDebugLogs)
            Debug.Log($"WaveTrigger: Something entered trigger! Object: {other.name}, Tag: {other.tag}");

        // Check if the player entered the trigger
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            Debug.Log("WaveTrigger: Player crossed the line! Starting wave system...");

            // Play wave trigger crossed sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayWaveTriggerCrossedSound();
            }

            // Start the wave system
            if (waveManager != null)
            {
                waveManager.ForceStartDay();
            }
            else
            {
                Debug.LogError("WaveTrigger: WaveManager is null! Cannot start wave system.");
            }
        }
        else if (enableDebugLogs)
        {
            if (hasTriggered)
                Debug.Log("WaveTrigger: Trigger already activated, ignoring.");
            if (!other.CompareTag("Player"))
                Debug.LogWarning($"WaveTrigger: Object {other.name} with tag '{other.tag}' entered trigger, but it's not the Player!");
        }
    }

    // Optional: Reset trigger when player exits
    void OnTriggerExit(Collider other)
    {
        if (enableDebugLogs)
            Debug.Log($"WaveTrigger: Something exited trigger! Object: {other.name}, Tag: {other.tag}");

        if (other.CompareTag("Player"))
        {
            // hasTriggered = false;
        }
    }

    // Debug method to test the trigger manually
    [ContextMenu("Test Trigger Manually")]
    public void TestTriggerManually()
    {
        Debug.Log("WaveTrigger: Testing trigger manually...");
        if (waveManager != null)
        {
            hasTriggered = true;
            waveManager.ForceStartDay();
            Debug.Log("WaveTrigger: Manual trigger test successful!");
        }
        else
        {
            Debug.LogError("WaveTrigger: Cannot test - WaveManager is null!");
        }
    }
}