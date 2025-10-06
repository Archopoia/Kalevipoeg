using UnityEngine;

public class Town : MonoBehaviour
{
    public int health = 10;

    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log("Town health: " + health);

        // Play village hit sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayVillageHitSound();
        }

        if (health <= 0)
        {
            Debug.Log("Town destroyed! Game Over!");
            // Don't destroy the town object immediately - let the WaveManager handle game over
            // The WaveManager will check for health <= 0 and trigger game over properly
        }
    }
}
