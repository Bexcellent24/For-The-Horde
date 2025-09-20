using UnityEngine;

public class TurnIntoZombie : MonoBehaviour
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
        Destroy(gameObject);
    }
}