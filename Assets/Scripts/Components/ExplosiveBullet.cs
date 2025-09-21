using UnityEngine;

public class ExplosiveBullet : Bullet
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private LayerMask damageLayers;

    [Header("Explosion Effects")]
    [SerializeField] private GameObject explosionEffectPrefab; // big boom

    protected override void HitTarget()
    {
        // AOE damage
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, damageLayers);
        foreach (Collider hit in hits)
        {
            Health h = hit.GetComponent<Health>();
            if (h != null)
            {
                h.TakeDamage(GetDamage(), transform.position);
            }
        }
        
        // Spawn explosion effect
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}