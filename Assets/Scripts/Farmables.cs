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
        audioSource.PlayOneShot(hitSound);
        HitEffect.Play();
        ShakeAnimation.Play();
        health -= amount;

        if (isStone)
        {
            Inventory.stone += 1;
        }
        if (isTree)
        {
            Inventory.wood += 1;
        }

        if (health <= 0f)
        {
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