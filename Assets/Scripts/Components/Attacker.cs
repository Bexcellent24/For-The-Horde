using UnityEngine;

public class Attacker : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float aimTolerance = 10f; 
    
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
    private Transform lockedTarget; 
    private Animator anim;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }
    
    public void Init(float fireRate, float attackRange, GameObject bulletPrefab, float bulletSpeed, float bulletDamage)
    {
        _fireRate = fireRate;
        _attackRange = attackRange;
        _bullet = bulletPrefab;
        _bulletSpeed = bulletSpeed;
        _bulletDamage = bulletDamage;

        perception = GetComponent<Perception>();
        
        if (perception != null)
        {
            perception.OnTargetAcquired += OnTargetAcquired;
            perception.OnTargetLost += OnTargetLost;
        }
    }

    void OnDestroy()
    {
        if (perception != null)
        {
            perception.OnTargetAcquired -= OnTargetAcquired;
            perception.OnTargetLost -= OnTargetLost;
        }
    }

    private void OnTargetAcquired(Transform target)
    {
        // Let the Update method handle target determination through DetermineBestTarget()
        // This just signals that perception has found something
        Debug.Log($"{name} - Perception acquired target: {target?.name}");
    }

    private void OnTargetLost()
    {
        // Don't immediately lose locked target, keep for a moment
        if (lockedTarget != null)
        {
            // Check if target still exists and is within reasonable range
            if (lockedTarget == null || Vector3.Distance(transform.position, lockedTarget.position) > _attackRange * 1.5f)
            {
                lockedTarget = null;
            }
        }
    }

    void Update()
    {
        fireCooldown -= Time.deltaTime;
        
        if (perception == null) return;

        // Determine the best target based on current situation
        Transform bestTarget = DetermineBestTarget();
        
        // Update locked target if we found a better one
        if (bestTarget != null && bestTarget != lockedTarget)
        {
            lockedTarget = bestTarget;
        }
        else if (bestTarget == null && lockedTarget != null && !IsValidTarget(lockedTarget))
        {
            lockedTarget = null;
        }

        // Attack the locked target if valid
        if (lockedTarget != null && IsValidTarget(lockedTarget))
        {
            float dist = Vector3.Distance(transform.position, lockedTarget.position);
            
            // Debug information
            if (Application.isEditor)
            {
              //  Debug.Log($"{name} - Target: {lockedTarget.name}, Distance: {dist:F2}, AttackRange: {_attackRange}, " +
                        // $"Aimed: {IsAimedAtTarget(lockedTarget)}, Cooldown: {fireCooldown:F2}");
            }
            
            if (dist <= _attackRange)
            {
                // Rotate towards target
                RotateTowardsTarget(lockedTarget);
                
                // Check if we're aimed well enough to shoot
                if (IsAimedAtTarget(lockedTarget) && fireCooldown <= 0f)
                {
                   // Debug.Log($"{name} - SHOOTING at {lockedTarget.name}!");
                    Shoot(lockedTarget);
                    fireCooldown = 1f / _fireRate;
                }
                else
                {
                    // Debug why we're not shooting
                    if (!IsAimedAtTarget(lockedTarget))
                    {
                        Vector3 dirToTarget = (lockedTarget.position - transform.position).normalized;
                        float angle = Vector3.Angle(transform.forward, dirToTarget);
                       // Debug.Log($"{name} - Not aimed properly. Angle: {angle:F2}, Tolerance: {aimTolerance}");
                    }
                    if (fireCooldown > 0f)
                    {
                        //Debug.Log($"{name} - Still on cooldown: {fireCooldown:F2}");
                    }
                }
            }
            else
            {
                //Debug.Log($"{name} - Target out of range. Distance: {dist:F2}, Range: {_attackRange}");
            }
        }
        else
        {
            // Clear locked target if it's no longer valid
            lockedTarget = null;
        }
    }

    private Transform DetermineBestTarget()
    {
        // First priority: If horde exists, target nearest zombie
        if (HordeManager.Instance != null && HordeManager.Instance.GetHordeCount() > 0)
        {
            Transform nearestZombie = FindNearestZombie();
            if (nearestZombie != null)
            {
                return nearestZombie;
            }
        }
        
        // Second priority: Target player if no horde or no zombies found
        Transform player = FindPlayer();
        if (player != null)
        {
            return player;
        }
        
        // Fallback: Use whatever perception system found
        Transform currentPerceptionTarget = perception.currentTarget;
        if (currentPerceptionTarget != null)
        {
            return currentPerceptionTarget;
        }
        
        return null;
    }

    private Transform FindPlayer()
    {
        // First check if current perception target is player
        if (perception.currentTarget != null && perception.currentTarget.CompareTag("Player"))
        {
            return perception.currentTarget;
        }
        
        // Search for player in view range
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distToPlayer = Vector3.Distance(transform.position, player.transform.position);
            
            // Only target player if within perception range
            if (distToPlayer <= perception.ViewRadius)
            {
                // Check if we can see the player (line of sight)
                Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
                Vector3 rayStart = transform.position + Vector3.up * 0.5f;
                
                if (!Physics.Raycast(rayStart, dirToPlayer, distToPlayer, perception.obstacleLayer))
                {
                    // Check if player is within view angle
                    float angle = Vector3.Angle(transform.forward, dirToPlayer);
                    if (angle <= perception.ViewAngle / 2f)
                    {
                        return player.transform;
                    }
                }
            }
        }
        
        return null;
    }

    private bool IsValidTarget(Transform target)
    {
        if (target == null) return false;
        
        // Check if target still exists and has required components
        return target.gameObject.activeInHierarchy;
    }

    private Transform FindNearestZombie()
    {
        Zombie[] zombies = FindObjectsOfType<Zombie>();
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var z in zombies)
        {
            if (z == null || !z.gameObject.activeInHierarchy) continue;
            
            float dist = Vector3.Distance(transform.position, z.transform.position);
            if (dist < minDist && dist <= perception.ViewRadius)
            {
                // Check if we can see this zombie
                Vector3 dirToZombie = (z.transform.position - transform.position).normalized;
                if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, dirToZombie, dist, perception.obstacleLayer))
                {
                    minDist = dist;
                    nearest = z.transform;
                }
            }
        }

        return nearest;
    }

    private void RotateTowardsTarget(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0f; // Keep rotation horizontal

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private bool IsAimedAtTarget(Transform target)
    {
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToTarget);
        return angle <= aimTolerance;
    }

    private void Shoot(Transform target)
    {
        if (_bullet == null || target == null) return;

        // Spawn bullet
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 1f;
        GameObject b = Instantiate(_bullet, spawnPos, Quaternion.identity);
        
        // Aim at target with slight prediction for moving targets
        Vector3 targetPos = target.position;
        if (target.GetComponent<Rigidbody>() != null)
        {
            Vector3 targetVelocity = target.GetComponent<Rigidbody>().linearVelocity;
            float timeToTarget = Vector3.Distance(spawnPos, targetPos) / _bulletSpeed;
            targetPos += targetVelocity * timeToTarget; // Lead the target
        }
        
        b.transform.LookAt(targetPos);

        Bullet bullet = b.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetTarget(target, _bulletSpeed, _bulletDamage);
        }
        
        if (anim != null)
        {
            anim.SetTrigger("Shoot");
        }

        // Notify nearby enemies of the gunshot sound with higher intensity
        NotifyNearbyOfSound(transform.position, 1.5f);
    }

    private void NotifyNearbyOfSound(Vector3 soundPosition, float intensity = 1f)
    {
        Collider[] nearby = Physics.OverlapSphere(soundPosition, perception.HearingRadius * intensity);
        foreach (var col in nearby)
        {
            Perception p = col.GetComponent<Perception>();
            if (p != null && p != perception)
            {
                p.HearSound(soundPosition, intensity);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
        
        // Draw line to locked target
        if (lockedTarget != null)
        {
            float dist = Vector3.Distance(transform.position, lockedTarget.position);
            
            // Color code the line based on status
            if (dist <= _attackRange)
            {
                if (IsAimedAtTarget(lockedTarget) && fireCooldown <= 0f)
                    Gizmos.color = Color.green; // Ready to shoot
                else if (!IsAimedAtTarget(lockedTarget))
                    Gizmos.color = Color.yellow; // Aiming
                else
                    Gizmos.color = Color.cyan; // Cooldown
            }
            else
            {
                Gizmos.color = Color.red; // Out of range
            }
            
            Gizmos.DrawLine(transform.position, lockedTarget.position);
            
            // Draw aiming cone
            Gizmos.color = Color.cyan;
            Vector3 forward = transform.forward * _attackRange;
            Vector3 leftBoundary = Quaternion.Euler(0, -aimTolerance, 0) * forward;
            Vector3 rightBoundary = Quaternion.Euler(0, aimTolerance, 0) * forward;
            
            Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
            Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        }
    }
}