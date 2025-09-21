using UnityEngine;

public class TurnIntoZombie : MonoBehaviour
{
    [SerializeField] private float turnRadius = 2f;
    [SerializeField] private GameObject bloodSplatterPrefab;
    [SerializeField] private bool triggerEndGame = false;

    private string[] deathSoundNames = { "Die", "Die 2", "Die 3" };
    private float deathSoundChance = 0.5f;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, turnRadius);
    }

    void Update()
    {
        // check for nearby zombies
        Collider[] hits = Physics.OverlapSphere(transform.position, turnRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") || hit.CompareTag("Zombie"))
            {
                Turn();
                break;
            }
        }
    }

    private void Turn()
    {
        Vector3 pos = transform.position;
        HordeManager.Instance.AddZombie(pos);
        GameManager.Instance.AddBrain();

        AudioManager.Instance?.PlaySFX("Bite");
        
        if (bloodSplatterPrefab != null)
        {
            Instantiate(bloodSplatterPrefab, transform.position, Quaternion.identity);
        }
        
        if (Random.value < deathSoundChance && deathSoundNames != null && deathSoundNames.Length > 0)
        {
            string choice = deathSoundNames[Random.Range(0, deathSoundNames.Length)];
            AudioManager.Instance?.PlaySFX(choice);
        }
        
        if (triggerEndGame)
        {
            GameManager.Instance.WinGame();
            Destroy(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
      
    }
}