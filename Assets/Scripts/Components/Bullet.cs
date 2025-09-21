using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float _speed = 10f;       
    private float _damage = 20f;     
    private Transform target;
    private float lifeTimer = 0f;

    [Header("Settings")]
    public float lifeTime = 2f; 
    public float hitDistance = 0.5f;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;

    public void SetTarget(Transform t, float bulletSpeed, float bulletDamage)
    {
        target = t;
        _speed = bulletSpeed;
        _damage = bulletDamage;
        lifeTimer = 0f;
    }

    void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * (_speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < hitDistance)
        {
            HitTarget();
        }
    }

    protected virtual void HitTarget()
    {
        Health h = target.GetComponent<Health>();
        if (h != null)
        {
            h.TakeDamage(_damage, transform.position);
        }

        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    // Helpers for child classes
    protected float GetDamage() => _damage;
    protected GameObject GetHitEffectPrefab() => hitEffectPrefab;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitDistance);

        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}