using System.Collections;
using UnityEngine;

public class Farmables : MonoBehaviour
{
    public float health = 50f;
    public Animation ShakeAnimation;
    public ParticleSystem HitEffect;
    public AudioSource audioSource;
    public AudioClip hitSound;
    public AudioClip destroySound;

    public bool isTree = false;
    public bool isStone = false;

    Inventory inventory;

    public void TakeDamage(float amount)
    {
        Debug.Log($"Farmables.TakeDamage: Taking {amount} damage. Current health: {health}");

        // Play hit sound if available
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
        else
        {
            Debug.LogWarning($"Farmables: audioSource or hitSound is null on {gameObject.name}");
        }

        // Play hit effect if available
        if (HitEffect != null)
        {
            HitEffect.Play();
        }
        else
        {
            Debug.LogWarning($"Farmables: HitEffect is null on {gameObject.name}");
        }

        // Play shake animation if available
        if (ShakeAnimation != null)
        {
            ShakeAnimation.Play();
        }
        else
        {
            Debug.LogWarning($"Farmables: ShakeAnimation is null on {gameObject.name}");
        }

        health -= amount;
        Debug.Log($"Farmables.TakeDamage: New health: {health}");

        if (isStone)
        {
            Inventory.stone += 1;
            Debug.Log($"Farmables: Added 1 stone. Total stone: {Inventory.stone}");
        }
        if (isTree)
        {
            Inventory.wood += 1;
            Debug.Log($"Farmables: Added 1 wood. Total wood: {Inventory.wood}");
        }

        if (health <= 0f)
        {
            Debug.Log($"Farmables: Health depleted, destroying {gameObject.name}");
            Die();
        }
    }

    void Die()
    {
        // Play deletion sound based on resource type
        if (AudioManager.Instance != null)
        {
            if (isTree)
            {
                AudioManager.Instance.PlayDeletionSound(true); // Tree deleted
            }
            else if (isStone)
            {
                AudioManager.Instance.PlayDeletionSound(false); // Stone deleted
            }
        }

        Tile parentTile = transform.parent?.GetComponent<Tile>();
        if (parentTile != null)
        {
            parentTile.OnResourceDestroyed();
        }

        Destroy(gameObject);
    }
}