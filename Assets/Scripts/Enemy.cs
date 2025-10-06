using UnityEngine;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;
    public float detectionRange = 3f; // How close to detect the town
    public float attackRange = 1.5f; // How close to attack the town
    public float attackInterval = 3f; // Attack every 3 seconds
    public float surroundRadius = 2f; // Distance from town center to surround
    public float avoidanceRadius = 1.5f; // Radius for collision avoidance
    public float avoidanceForce = 3f; // Strength of avoidance force

    [Header("Health")]
    public float maxHealth = 2f;
    public float currentHealth;

    [Header("Visual Effects")]
    public float damageFlashDuration = 0.3f; // Shorter duration for better gameplay
    public Color damageFlashColor = Color.red;
    public GameObject fireballPrefab; // Reference to the Fireball particle effect prefab

    private Queue<Tile> path;
    private Tile currentTarget;
    private Town townTarget;
    private bool hasDetectedTown = false;
    private bool isAttackingTown = false;
    private float lastAttackTime = 0f;
    private Vector3 surroundPosition;
    private bool hasAssignedSurroundPosition = false;
    private static List<Enemy> allEnemies = new List<Enemy>();
    private int enemyId;
    private static int nextEnemyId = 1;
    private float previousDistanceToTarget = float.MaxValue;

    // Visual feedback variables - NEW SYSTEM
    private Renderer[] allRenderers; // Store all renderers in the enemy hierarchy
    private Color[] originalColors; // Store original colors for each renderer
    private bool isFlashing = false;
    private Coroutine flashCoroutine;

    void Awake()
    {
        // Assign unique ID to this enemy
        enemyId = nextEnemyId++;

        // Initialize health
        currentHealth = maxHealth;

        // Setup visual feedback
        SetupVisualFeedback();

        // Add this enemy to the global list
        if (!allEnemies.Contains(this))
            allEnemies.Add(this);
    }

    void OnDestroy()
    {
        // Stop any flashing coroutine
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        // Remove this enemy from the global list
        allEnemies.Remove(this);
    }

    // UPDATED Init: takes both road path AND town reference
    public void Init(List<Tile> roadPath, Town town)
    {
        path = new Queue<Tile>(roadPath);
        townTarget = town;
        NextTarget();
    }

    void Update()
    {
        if (townTarget == null) return;

        // Check if we can detect the town
        float distanceToTown = Vector3.Distance(transform.position, townTarget.transform.position);

        if (!hasDetectedTown && distanceToTown <= detectionRange)
        {
            hasDetectedTown = true;
        }

        if (hasDetectedTown)
        {
            // Assign a surround position if not already assigned
            if (!hasAssignedSurroundPosition)
            {
                surroundPosition = FindSurroundPosition();
                hasAssignedSurroundPosition = true;
            }

            // Move towards surround position with collision avoidance
            float distanceToSurroundPos = Vector3.Distance(transform.position, surroundPosition);

            if (distanceToSurroundPos > 0.5f) // Not at surround position yet
            {
                Vector3 targetPos = surroundPosition;
                targetPos.y = transform.position.y; // Keep enemy at same height

                // Apply collision avoidance (stronger when surrounding)
                Vector3 avoidanceVector = CalculateAvoidance();
                Vector3 finalDirection = (targetPos - transform.position).normalized;

                // Apply stronger avoidance when surrounding the town
                if (avoidanceVector.magnitude > 0.1f)
                {
                    finalDirection += avoidanceVector * 0.3f; // Reduced avoidance force
                }

                finalDirection.y = 0; // Keep movement on horizontal plane
                finalDirection.Normalize();

                // Ensure we always move forward, even with avoidance
                if (finalDirection.magnitude < 0.1f)
                {
                    finalDirection = (targetPos - transform.position).normalized;
                    finalDirection.y = 0;
                }

                transform.position += finalDirection * speed * Time.deltaTime;
            }
            else
            {
                // At surround position - start attacking
                if (!isAttackingTown)
                {
                    isAttackingTown = true;
                }

                AttackTown();
            }
        }
        else
        {
            // Still following the path - NO avoidance during path following
            if (currentTarget == null)
            {
                return;
            }

            Vector3 targetPos = currentTarget.transform.position;
            targetPos.y = transform.position.y; // Keep enemy at same height

            float distanceToTarget = Vector3.Distance(transform.position, targetPos);

            // Simple direct movement to target - no avoidance during path following
            Vector3 direction = (targetPos - transform.position).normalized;
            direction.y = 0; // Keep movement on horizontal plane

            transform.position += direction * speed * Time.deltaTime;

            // Check if we've reached the waypoint or are getting further away (oscillation detection)
            if (distanceToTarget < 0.5f || (distanceToTarget > previousDistanceToTarget && previousDistanceToTarget < 1.0f))
            {
                NextTarget();
                previousDistanceToTarget = float.MaxValue; // Reset for next waypoint
            }
            else
            {
                previousDistanceToTarget = distanceToTarget;
            }
        }
    }

    void AttackTown()
    {
        if (Time.time - lastAttackTime >= attackInterval)
        {
            if (townTarget != null)
            {
                // Spawn Fireball particle effect
                if (fireballPrefab != null)
                {
                    // Calculate direction from enemy to town
                    Vector3 directionToTown = (townTarget.transform.position - transform.position).normalized;

                    // Spawn fireball at enemy position, aimed at town
                    GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.LookRotation(directionToTown));

                    // Auto-destroy the fireball after a short time (adjust as needed)
                    Destroy(fireball, 2f);
                }
                else
                {
                    Debug.LogWarning($"Enemy {gameObject.name}: Fireball prefab is not assigned!");
                }

                townTarget.TakeDamage(1);
                lastAttackTime = Time.time;
            }
        }
    }

    Vector3 CalculateAvoidance()
    {
        Vector3 avoidanceVector = Vector3.zero;
        int nearbyCount = 0;

        foreach (Enemy otherEnemy in allEnemies)
        {
            if (otherEnemy == this) continue;

            float distance = Vector3.Distance(transform.position, otherEnemy.transform.position);

            // Only avoid if very close (reduced radius to prevent unnecessary stopping)
            if (distance < avoidanceRadius * 0.7f) // Reduced threshold
            {
                nearbyCount++;
                // Calculate avoidance force
                Vector3 awayFromOther = (transform.position - otherEnemy.transform.position).normalized;
                float avoidanceStrength = (avoidanceRadius * 0.7f - distance) / (avoidanceRadius * 0.7f);
                Vector3 force = awayFromOther * avoidanceStrength * avoidanceForce;
                avoidanceVector += force;

            }
        }

        return avoidanceVector;
    }

    Vector3 FindSurroundPosition()
    {
        Vector3 townCenter = townTarget.transform.position;

        // Find the direction from town to this enemy
        Vector3 directionToEnemy = (transform.position - townCenter).normalized;

        // If enemy is too close to town center, use a random direction
        if (directionToEnemy.magnitude < 0.1f)
        {
            directionToEnemy = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                0,
                UnityEngine.Random.Range(-1f, 1f)
            ).normalized;
        }

        // Calculate surround position at the specified radius
        Vector3 surroundPos = townCenter + directionToEnemy * surroundRadius;
        surroundPos.y = transform.position.y; // Keep same height

        // Try to find a position that's not too close to other enemies
        int attempts = 0;
        while (attempts < 10)
        {
            bool tooCloseToOther = false;

            foreach (Enemy otherEnemy in allEnemies)
            {
                if (otherEnemy == this || !otherEnemy.hasAssignedSurroundPosition) continue;

                float distanceToOther = Vector3.Distance(surroundPos, otherEnemy.surroundPosition);
                if (distanceToOther < 1.5f) // Minimum distance between surround positions
                {
                    tooCloseToOther = true;
                    break;
                }
            }

            if (!tooCloseToOther) break;

            // Try a different position
            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            surroundPos = townCenter + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * surroundRadius;
            surroundPos.y = transform.position.y;
            attempts++;
        }

        return surroundPos;
    }

    void NextTarget()
    {
        if (path.Count > 0)
        {
            currentTarget = path.Dequeue();
        }
        else
        {
            // Finished following path - will now rely on detection system
            currentTarget = null;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        // Play enemy hit sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyHitSound();
        }

        // Trigger visual feedback
        FlashOnDamage();

        if (currentHealth <= 0)
        {
            // Don't destroy immediately - let the flash complete first
            StartCoroutine(DelayedDeath());
        }
    }

    System.Collections.IEnumerator DelayedDeath()
    {
        // Wait for any ongoing flash to complete
        if (isFlashing)
        {
            yield return new WaitUntil(() => !isFlashing);
        }

        // Play enemy death sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyDeathSound();
        }

        // Add a small delay to ensure flash is visible
        yield return new WaitForSeconds(0.1f);

        // Now destroy the enemy
        Destroy(gameObject);
    }

    void SetupVisualFeedback()
    {
        // Find ALL renderers in the entire enemy hierarchy
        allRenderers = GetComponentsInChildren<Renderer>(true); // Include inactive renderers

        if (allRenderers.Length == 0)
        {
            Debug.LogError($"Enemy {gameObject.name}: No Renderers found! This will prevent damage flashing.");
            return;
        }

        // Store original colors for each renderer
        originalColors = new Color[allRenderers.Length];
        for (int i = 0; i < allRenderers.Length; i++)
        {
            if (allRenderers[i] != null && allRenderers[i].material != null)
            {
                // Try to get the original color using multiple methods
                Color originalColor = Color.white; // Default fallback

                if (allRenderers[i].material.HasProperty("_Color"))
                {
                    originalColor = allRenderers[i].material.GetColor("_Color");
                }
                else if (allRenderers[i].material.HasProperty("_BaseColor"))
                {
                    originalColor = allRenderers[i].material.GetColor("_BaseColor");
                }
                else if (allRenderers[i].material.HasProperty("_MainColor"))
                {
                    originalColor = allRenderers[i].material.GetColor("_MainColor");
                }
                else
                {
                    originalColor = allRenderers[i].material.color;
                }

                originalColors[i] = originalColor;
            }
            else
            {
                originalColors[i] = Color.white;
            }
        }
    }

    void FlashOnDamage()
    {
        if (allRenderers == null || allRenderers.Length == 0)
        {
            return;
        }

        if (isFlashing)
        {
            return;
        }

        // Stop any existing flash coroutine
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        // Start new flash coroutine
        flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    System.Collections.IEnumerator FlashCoroutine()
    {
        isFlashing = true;

        // Create material instances for each renderer to avoid affecting other enemies
        Material[] flashMaterials = new Material[allRenderers.Length];

        for (int i = 0; i < allRenderers.Length; i++)
        {
            if (allRenderers[i] != null && allRenderers[i].material != null)
            {
                // Create a new material instance
                flashMaterials[i] = new Material(allRenderers[i].material);
                allRenderers[i].material = flashMaterials[i];
            }
        }

        // Create a pulsing flash effect
        float flashTime = 0f;
        float pulseSpeed = 10f; // How fast the pulse is

        while (flashTime < damageFlashDuration)
        {
            // Calculate pulse intensity (0 to 1)
            float pulseIntensity = Mathf.Abs(Mathf.Sin(flashTime * pulseSpeed));

            // Blend between original color and damage color based on pulse intensity
            for (int i = 0; i < allRenderers.Length; i++)
            {
                if (allRenderers[i] != null && flashMaterials[i] != null)
                {
                    Color currentColor = Color.Lerp(originalColors[i], damageFlashColor, pulseIntensity);

                    // Apply the pulsed color
                    if (flashMaterials[i].HasProperty("_Color"))
                    {
                        flashMaterials[i].SetColor("_Color", currentColor);
                    }
                    else if (flashMaterials[i].HasProperty("_BaseColor"))
                    {
                        flashMaterials[i].SetColor("_BaseColor", currentColor);
                    }
                    else if (flashMaterials[i].HasProperty("_MainColor"))
                    {
                        flashMaterials[i].SetColor("_MainColor", currentColor);
                    }
                    else
                    {
                        flashMaterials[i].color = currentColor;
                    }
                }
            }

            flashTime += Time.deltaTime;
            yield return null; // Wait one frame
        }

        // Restore original colors
        for (int i = 0; i < allRenderers.Length; i++)
        {
            if (allRenderers[i] != null && flashMaterials[i] != null)
            {
                // Restore original color using the same method
                if (flashMaterials[i].HasProperty("_Color"))
                {
                    flashMaterials[i].SetColor("_Color", originalColors[i]);
                }
                else if (flashMaterials[i].HasProperty("_BaseColor"))
                {
                    flashMaterials[i].SetColor("_BaseColor", originalColors[i]);
                }
                else if (flashMaterials[i].HasProperty("_MainColor"))
                {
                    flashMaterials[i].SetColor("_MainColor", originalColors[i]);
                }
                else
                {
                    flashMaterials[i].color = originalColors[i];
                }
            }
        }

        isFlashing = false;
        flashCoroutine = null;
    }
}
