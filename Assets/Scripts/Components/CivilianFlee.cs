using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Perception))]
public class CivilianFlee : MonoBehaviour
{
    public float fleeRadius = 5f;
    public float stopDistance = 0.5f;
    public float nextFleeDelay = 1f; // wait before picking a new flee point

    private NavMeshAgent agent;
    private Perception perception;
    private Transform zombieTarget;
    private float fleeTimer = 0f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        perception = GetComponent<Perception>();
        perception.OnTargetAcquired += HandleZombieSpotted;
    }

    void HandleZombieSpotted(Transform zombie)
    {
        // Only react to zombies
        if (zombie != null && zombie.CompareTag("Zombie"))
        {
            zombieTarget = zombie;
        }
    }

    void Update()
    {
        if (zombieTarget != null)
        {
            fleeTimer -= Time.deltaTime;

            // Pick a new flee point if reached previous or timer elapsed
            if (!agent.hasPath || agent.remainingDistance <= stopDistance || fleeTimer <= 0f)
            {
                PickFleePosition();
                fleeTimer = nextFleeDelay;
            }
        }
        else
        {
            // Stop fleeing if no zombie nearby
            if (agent.hasPath)
                agent.ResetPath();
        }
    }

    private void PickFleePosition()
    {
        const int maxAttempts = 10;
        bool found = false;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 fleeDir = (transform.position - zombieTarget.position).normalized;
            Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
            Vector3 candidate = transform.position + fleeDir * fleeRadius + randomOffset;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                if (Mathf.Abs(hit.position.y - transform.position.y) <= 2f)
                {
                    agent.SetDestination(hit.position);
                    found = true;
                    break;
                }
            }
        }
        
        if (!found)
        {
            Debug.Log("No flee Position Found");
        }
    }

}
