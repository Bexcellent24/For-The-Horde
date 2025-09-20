using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Perception))]
public class EnemyWander : MonoBehaviour
{
    [Header("Wander Settings")]
    public float wanderRadius = 5f;
    public float wanderWaitTime = 2f;
    public float wanderMinDistance = 1f;
    public float maxHeightDifference = 2f;
    
    [Header("Behavior Settings")]
    public bool pauseWhenTargetAcquired = true;
    public bool investigateLastKnownPosition = true;
    public float investigationTime = 3f;
    
    private NavMeshAgent agent;
    private Perception perception;
    private Coroutine wanderCoroutine;
    private bool isInvestigating = false;
    private Vector3 originalPosition;
    private float originalSpeed;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        perception = GetComponent<Perception>();
        originalPosition = transform.position;
        originalSpeed = agent.speed;
        
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

    void OnEnable()
    {
        if (wanderCoroutine == null)
            wanderCoroutine = StartCoroutine(WanderLoop());
    }

    void OnDisable()
    {
        if (wanderCoroutine != null)
        {
            StopCoroutine(wanderCoroutine);
            wanderCoroutine = null;
        }
    }

    private void OnTargetAcquired(Transform target)
    {
        if (pauseWhenTargetAcquired)
        {
            isInvestigating = false;
            agent.speed = originalSpeed; // Resume normal speed when target found
        }
    }

    private void OnTargetLost()
    {
        if (investigateLastKnownPosition && perception.hasLostTarget)
        {
            StartCoroutine(InvestigatePosition(perception.lastKnownTargetPosition));
        }
    }

    private IEnumerator InvestigatePosition(Vector3 position)
    {
        if (position == Vector3.zero) yield break;
        
        isInvestigating = true;
        agent.speed = originalSpeed * 1.2f; // Move slightly faster when investigating
        
        // Move to last known position
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            
            // Wait until we reach the position or timeout
            float timer = 0f;
            while (!agent.pathPending && agent.remainingDistance > agent.stoppingDistance && timer < 10f)
            {
                timer += Time.deltaTime;
                yield return null;
                
                // Stop investigating if we acquire a new target
                if (perception.currentTarget != null)
                {
                    isInvestigating = false;
                    agent.speed = originalSpeed;
                    yield break;
                }
            }
            
            // Look around at the investigation point
            Vector3 originalForward = transform.forward;
            float lookTimer = 0f;
            
            while (lookTimer < investigationTime && perception.currentTarget == null)
            {
                // Slowly rotate to look around
                float rotationProgress = lookTimer / investigationTime;
                float angle = rotationProgress * 360f;
                Vector3 lookDirection = Quaternion.Euler(0, angle, 0) * originalForward;
                transform.rotation = Quaternion.LookRotation(lookDirection);
                
                lookTimer += Time.deltaTime;
                yield return null;
            }
        }
        
        isInvestigating = false;
        agent.speed = originalSpeed;
    }

    private IEnumerator WanderLoop()
    {
        // Wait until the agent is on a valid NavMesh
        while (!agent.isOnNavMesh)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        while (true)
        {
            // Only wander if no current target and not investigating
            if (perception.currentTarget == null && !isInvestigating)
            {
                Vector3 wanderPos = GetRandomNavMeshPosition(transform.position, wanderRadius);

                if (wanderPos != Vector3.zero)
                {
                    agent.SetDestination(wanderPos);

                    // Wait until agent reaches destination or finds a target
                    float timeoutTimer = 0f;
                    float maxWanderTime = (wanderRadius * 2f) / agent.speed + 2f; // Reasonable timeout
                    
                    while (!agent.pathPending && 
                           agent.remainingDistance > agent.stoppingDistance && 
                           timeoutTimer < maxWanderTime)
                    {
                        timeoutTimer += Time.deltaTime;
                        yield return null;

                        // Stop wandering if target acquired or start investigating
                        if (perception.currentTarget != null || isInvestigating)
                            break;
                            
                        // Handle case where agent gets stuck
                        if (agent.velocity.magnitude < 0.1f && timeoutTimer > 2f)
                        {
                            break; // Exit and try a new destination
                        }
                    }

                    // Wait at the destination if we reached it and no target
                    if (perception.currentTarget == null && !isInvestigating)
                    {
                        float waitTimer = 0f;
                        while (waitTimer < wanderWaitTime && 
                               perception.currentTarget == null && 
                               !isInvestigating)
                        {
                            waitTimer += Time.deltaTime;
                            yield return null;
                        }
                    }
                }
                else
                {
                    // Couldn't find wander position, wait a bit before trying again
                    yield return new WaitForSeconds(1f);
                }
            }
            else
            {
                // Wait while we have a target or are investigating
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    private Vector3 GetRandomNavMeshPosition(Vector3 origin, float radius)
    {
        const int maxAttempts = 15;
        
        for (int i = 0; i < maxAttempts; i++)
        {
            // Generate random position within radius
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            
            // Ensure minimum distance from current position
            if (randomCircle.magnitude < wanderMinDistance)
                randomCircle = randomCircle.normalized * wanderMinDistance;
                
            Vector3 candidate = origin + new Vector3(randomCircle.x, 0f, randomCircle.y);

            // Try to find valid NavMesh position
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, radius * 0.5f, NavMesh.AllAreas))
            {
                // Check height difference
                if (Mathf.Abs(hit.position.y - origin.y) <= maxHeightDifference)
                {
                    // Ensure the position is actually reachable
                    NavMeshPath path = new NavMeshPath();
                    if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                    {
                        return hit.position;
                    }
                }
            }
        }

        // Fallback: try positions closer to current location
        for (int i = 0; i < 5; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * (radius * 0.5f);
            Vector3 candidate = origin + new Vector3(randomCircle.x, 0f, randomCircle.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return Vector3.zero; // Failed to find valid position
    }

    // Public method to force a new wander destination (useful for external systems)
    public void ForceNewWanderDestination()
    {
        if (!isInvestigating && perception.currentTarget == null)
        {
            Vector3 newPos = GetRandomNavMeshPosition(transform.position, wanderRadius);
            if (newPos != Vector3.zero)
            {
                agent.SetDestination(newPos);
            }
        }
    }

    // Public method to return to original spawn position
    public void ReturnToOrigin()
    {
        if (NavMesh.SamplePosition(originalPosition, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw wander radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
        
        // Draw minimum wander distance
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, wanderMinDistance);
        
        // Draw investigation point if investigating
        if (isInvestigating && perception.hasLostTarget)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(perception.lastKnownTargetPosition, 1f);
            Gizmos.DrawLine(transform.position, perception.lastKnownTargetPosition);
        }
        
        // Draw original position
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(originalPosition, Vector3.one * 0.5f);
    }
}