using UnityEngine;

public class Civilian : MonoBehaviour
{
    [SerializeField] private float turnRadius = 2f;

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
                TurnIntoZombie();
                break;
            }
        }
    }

    private void TurnIntoZombie()
    {
        Vector3 pos = transform.position;
        HordeManager.Instance.AddZombie(pos);
        GameManager.Instance.AddBrain();
        Destroy(gameObject);
    }
}