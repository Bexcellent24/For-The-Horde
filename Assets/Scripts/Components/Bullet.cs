using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float _speed = 10f;       
    private float _damage = 20f;     
    private Transform target;
    private float lifeTimer = 0f;
    
    [Header("Settings")]
    public float lifeTime = 2f; // Bullet destroys itself after 2 seconds
    public float hitDistance = 0.5f; // Distance to register a hit
    
    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;

    public void SetTarget(Transform t, float bulletSpeed, float bulletDamage)
    {
        target = t;
        _speed = bulletSpeed;
        _damage = bulletDamage;
        lifeTimer = 0f; // Reset timer when target is set
    }

    void Update()
    {
        // Increment life timer
        lifeTimer += Time.deltaTime;
        
        // Destroy bullet if it's been alive too long
        if (lifeTimer >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        // Destroy if target is gone
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Move towards target
        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * (_speed * Time.deltaTime);

        // Check for hit
        if (Vector3.Distance(transform.position, target.position) < hitDistance)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {
        Health h = target.GetComponent<Health>();
        if (h != null)
        {
            // Pass bullet position as damage source for redirection logic
            h.TakeDamage(_damage, transform.position);
        }
        
        // Spawn hit effect
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
        
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        // Draw hit detection sphere
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitDistance);
        
        // Draw line to target
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}