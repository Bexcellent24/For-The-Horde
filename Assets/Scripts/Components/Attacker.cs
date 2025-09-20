using UnityEngine;

public class Attacker : MonoBehaviour
{
    private float fireCooldown = 0f;

    private float _fireRate;
    private float _attackRange;

    // Bullet
    private GameObject _bullet;
    private float _bulletSpeed;
    private float _bulletDamage;

    [Header("Fire Settings")]
    public Transform firePoint;

    private Perception perception;

    public void Init(float fireRate, float attackRange, GameObject bulletPrefab, float bulletSpeed, float bulletDamage)
    {
        _fireRate = fireRate;
        _attackRange = attackRange;
        _bullet = bulletPrefab;
        _bulletSpeed = bulletSpeed;
        _bulletDamage = bulletDamage;

        perception = GetComponent<Perception>();
    }

    void Update()
    {
        fireCooldown -= Time.deltaTime;
        if (perception == null) return;

        Transform target = perception.currentTarget;

        // Check if the target is the player
        if (target != null && target.CompareTag("Player"))
        {
            // Only attack the player if the horde is empty
            if (HordeManager.Instance.GetHordeCount() > 0)
            {
                // Look for a zombie instead
                target = FindNearestZombie();
            }
        }

        if (target != null)
        {
            float dist = Vector3.Distance(transform.position, target.position);
            if (dist <= _attackRange && fireCooldown <= 0f)
            {
                Shoot(target);
                fireCooldown = 1f / _fireRate;
            }
        }
    }
    
    private Transform FindNearestZombie()
    {
        Zombie[] zombies = FindObjectsOfType<Zombie>();
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var z in zombies)
        {
            float dist = Vector3.Distance(transform.position, z.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = z.transform;
            }
        }

        return nearest;
    }


    private void Shoot(Transform target)
    {
        if (_bullet == null || target == null) return;

        // Rotate enemy to face target
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0f;
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);

        // Spawn bullet
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        GameObject b = Instantiate(_bullet, spawnPos, Quaternion.identity);
        b.transform.LookAt(target);

        Bullet bullet = b.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetTarget(target, _bulletSpeed, _bulletDamage);
        }

        // Notify nearby enemies of the sound
        Collider[] nearby = Physics.OverlapSphere(transform.position, perception.HearingRadius);
        foreach (var col in nearby)
        {
            Perception p = col.GetComponent<Perception>();
            if (p != null && p != perception)
                p.HearSound(transform.position);
        }
    }
}
