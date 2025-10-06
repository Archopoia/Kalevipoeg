using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float speed = 10f;
    private float damage = 1f;
    private float lifetime = 5f; // Auto-destroy after this time

    private Enemy target;
    private Vector3 targetPosition;
    private bool hasTarget = false;

    // Public properties for Tower to set
    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }

    public float Damage
    {
        get { return damage; }
        set { damage = value; }
    }

    void Start()
    {
        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (hasTarget)
        {
            // Move straight towards the target position (no tracking)
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            // Check if we've reached the target position
            float distanceToPosition = Vector3.Distance(transform.position, targetPosition);
            if (distanceToPosition < 0.5f)
            {
                // Check if target still exists and is close enough to hit
                if (target != null)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
                    if (distanceToTarget < 1.0f) // Hit if target is within 1 unit
                    {
                        HitTarget();
                    }
                    else
                    {
                        // Target moved away, destroy projectile
                        Destroy(gameObject);
                    }
                }
                else
                {
                    // Target was destroyed, destroy projectile
                    Destroy(gameObject);
                }
            }
        }
    }

    public void SetTarget(Enemy enemy)
    {
        target = enemy;
        targetPosition = enemy.transform.position;
        hasTarget = true;

        // Look at target
        if (target != null)
        {
            Vector3 lookDirection = target.transform.position - transform.position;
            lookDirection.y = 0; // Keep projectile level
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }

    void HitTarget()
    {
        if (target != null)
        {
            target.TakeDamage(damage);
        }
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && enemy == target)
        {
            HitTarget();
        }
    }
}
