using UnityEngine;

public class Health : MonoBehaviour
{
    private float currentHealth;

    public delegate void OnDeath();
    public event OnDeath onDeath;

    public void Init(float health)
    {
        currentHealth = health;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
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
        Destroy(gameObject);
    }

    public float GetHealth() => currentHealth;
}