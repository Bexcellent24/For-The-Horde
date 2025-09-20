using UnityEngine;


public class DamageRedirector : MonoBehaviour
{
    [Header("Damage Redirection")]
    public bool redirectDamageWhenHordeExists = true;
    public bool showRedirectionEffects = true;
    
    private Health playerHealth;

    void Awake()
    {
        playerHealth = GetComponent<Health>();
        if (playerHealth == null)
        {
            Debug.LogError($"DamageRedirector on {name} requires a Health component!");
        }
    }


    public void ProcessDamage(float damage, Vector3 damageSource = default)
    {
        // Check if we should redirect damage to zombies
        if (redirectDamageWhenHordeExists && ShouldRedirectDamage())
        {
            Transform closestZombie = FindClosestZombie();
            if (closestZombie != null)
            {
                Health zombieHealth = closestZombie.GetComponent<Health>();
                if (zombieHealth != null)
                {
                    Debug.Log($"Damage redirected from Player to closest zombie: {closestZombie.name}");
                    
                    // Apply damage directly to zombie (bypass redirection)
                    zombieHealth.ApplyDamage(damage);
                    
                    // Show redirection effects
                    if (showRedirectionEffects)
                    {
                        ShowDamageRedirectionEffect(closestZombie.position);
                    }
                    
                    return;
                }
            }
        }

        // Apply damage to player normally
        if (playerHealth != null)
        {
            playerHealth.ApplyDamage(damage);
        }
    }

    private bool ShouldRedirectDamage()
    {
        return HordeManager.Instance != null && HordeManager.Instance.GetHordeCount() > 0;
    }

    private Transform FindClosestZombie()
    {
        Zombie[] allZombies = FindObjectsOfType<Zombie>();
        Transform closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (var zombie in allZombies)
        {
            if (zombie == null || !zombie.gameObject.activeInHierarchy) continue;
            
            Health zombieHealth = zombie.GetComponent<Health>();
            if (zombieHealth == null) continue;

            float distance = Vector3.Distance(transform.position, zombie.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = zombie.transform;
            }
        }

        return closest;
    }

    private void ShowDamageRedirectionEffect(Vector3 zombiePos)
    {
        // Visual feedback that damage was redirected
        Debug.DrawLine(transform.position, zombiePos, Color.red, 1f);
        Debug.Log($"Damage redirected to closest zombie!");
    }

    void OnDrawGizmosSelected()
    {
        // Draw line to closest zombie if horde exists
        if (Application.isPlaying && ShouldRedirectDamage())
        {
            Transform closestZombie = FindClosestZombie();
            if (closestZombie != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, closestZombie.position);
                
                // Draw a sphere at the closest zombie
                Gizmos.DrawWireSphere(closestZombie.position, 1f);
            }
        }
    }
}