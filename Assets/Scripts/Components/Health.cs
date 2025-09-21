using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    public delegate void OnDeath();
    public event OnDeath onDeath;

    public delegate void OnDamage(float damage, float currentHealth, float maxHealth);
    public event OnDamage onDamage;

    void Start()
    {
        // If not initialized via Init(), use maxHealth
        if (currentHealth <= 0)
            currentHealth = maxHealth;
    }

    public void Init(float health)
    {
        maxHealth = health;
        currentHealth = health;
    }

    public void TakeDamage(float amount, Vector3 damageSource = default)
    {
        // For player, check if we should redirect damage
        if (CompareTag("Player"))
        {
            DamageRedirector redirector = GetComponent<DamageRedirector>();
            if (redirector != null)
            {
                // Let the redirector handle the damage
                redirector.ProcessDamage(amount, damageSource);
                return; // Don't apply damage to player directly
            }
        }

        // Apply damage normally for non-players or players without redirector
        ApplyDamage(amount);
    }
    
    public void ApplyDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(0f, currentHealth); // Ensure health doesn't go negative
        
        // Notify damage listeners
        onDamage?.Invoke(amount, currentHealth, maxHealth);
        
        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        onDeath?.Invoke();
        
        if (CompareTag("Player"))
        {
            GameManager.Instance.LoseGame();
        }
        else if (CompareTag("Zombie"))
        {
            // Notify horde manager that a zombie died
            if (HordeManager.Instance != null)
            {
                Zombie zombieComponent = GetComponent<Zombie>();
                if (zombieComponent != null)
                {
                    HordeManager.Instance.RemoveFromHorde(zombieComponent);
                }
            }
        }

        Destroy(gameObject);
    }

    public float GetHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => maxHealth > 0 ? currentHealth / maxHealth : 0f;
    public bool IsAlive() => currentHealth > 0f;

    /// <summary>
    /// Heal the entity
    /// </summary>
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    /// <summary>
    /// Set health to a specific value (useful for debugging or special events)
    /// </summary>
    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0f, maxHealth);
        if (currentHealth <= 0f)
            Die();
    }
}