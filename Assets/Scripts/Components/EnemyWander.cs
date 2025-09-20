using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Perception))]
public class EnemyWander : MonoBehaviour
{
    [Header("Wander Settings")]
    public float wanderRadius = 5f;          // max distance from current position
    public float wanderWaitTime = 2f;        // time to wait at each point
    public float wanderMinDistance = 1f;     // min distance from current pos
    public float maxHeightDifference = 2f; // max height difference allowed

    private NavMeshAgent agent;
    private Perception perception;
    private Coroutine wanderCoroutine;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        perception = GetComponent<Perception>();
    }

    void OnEnable()
    {
        wanderCoroutine = StartCoroutine(WanderLoop());
    }

    void OnDisable()
    {
        if (wanderCoroutine != null)
            StopCoroutine(wanderCoroutine);
    }

    private IEnumerator WanderLoop()
    {
        // Wait until the agent is on a valid NavMesh
        while (!agent.isOnNavMesh)
            yield return null;
        
        while (true)
        {
            // Only wander if no current target
            if (perception.currentTarget == null)
            {
                Vector3 wanderPos = GetRandomNavMeshPosition(transform.position, wanderRadius);

                if (wanderPos != Vector3.zero)
                {
                    agent.SetDestination(wanderPos);

                    // Wait until agent reaches destination or timeout
                    float timer = 0f;
                    while (!agent.pathPending && agent.remainingDistance > agent.stoppingDistance && timer < 10f)
                    {
                        timer += Time.deltaTime;
                        yield return null;

                        // If a target is acquired mid-wander, stop moving
                        if (perception.currentTarget != null)
                            break;
                    }

                    // Wait at the spot
                    float waitTimer = 0f;
                    while (waitTimer < wanderWaitTime && perception.currentTarget == null)
                    {
                        waitTimer += Time.deltaTime;
                        yield return null;
                    }
                }
            }
            else
            {
                // pause wandering if there is a target
                yield return null;
            }
        }
    }

    private Vector3 GetRandomNavMeshPosition(Vector3 origin, float radius)
    {
        for (int i = 0; i < 10; i++) // try 10 times
        {
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 candidate = origin + new Vector3(randomCircle.x, 0f, randomCircle.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                if (Mathf.Abs(hit.position.y - origin.y) <= maxHeightDifference)
                    return hit.position;
            }
        }

        return Vector3.zero; // failed to find a valid position
    }
}
