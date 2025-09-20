using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Perception))]
public class CivilianFlee : MonoBehaviour
{
    [Header("Flee Settings")]
    public float fleeRadius = 8f;
    public float stopDistance = 1f;
    public float nextFleeDelay = 0.5f;
    public float panicSpeedMultiplier = 1.5f;
    
    [Header("Target Detection")]
    public string[] dangerTags = { "Zombie", "Player" }; // Tags that cause fleeing
    
    private NavMeshAgent agent;
    private Perception perception;
    private Transform threatTarget;
    private float fleeTimer = 0f;
    private float originalSpeed;
    private bool isFleeing = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        perception = GetComponent<Perception>();
        originalSpeed = agent.speed;
        
        perception.OnTargetAcquired += HandleTargetSpotted;
        perception.OnTargetLost += HandleTargetLost;
    }

    void OnDestroy()
    {
        if (perception != null)
        {
            perception.OnTargetAcquired -= HandleTargetSpotted;
            perception.OnTargetLost -= HandleTargetLost;
        }
    }

    void HandleTargetSpotted(Transform target)
    {
        if (target != null && IsDangerous(target))
        {
            StartFleeing(target);
        }
    }
    
    void HandleTargetLost()
    {
        // Continue fleeing for a bit even after losing sight
        if (isFleeing)
        {
            Invoke(nameof(StopFleeingDelayed), 2f);
        }
    }

    private bool IsDangerous(Transform target)
    {
        foreach (string dangerTag in dangerTags)
        {
            if (target.CompareTag(dangerTag))
                return true;
        }
        return false;
    }

    private void StartFleeing(Transform threat)
    {
        threatTarget = threat;
        isFleeing = true;
        agent.speed = originalSpeed * panicSpeedMultiplier;
        
        // Cancel any delayed stop fleeing
        CancelInvoke(nameof(StopFleeingDelayed));
    }

    private void StopFleeingDelayed()
    {
        if (perception.currentTarget == null || !IsDangerous(perception.currentTarget))
        {
            StopFleeing();
        }
    }

    private void StopFleeing()
    {
        isFleeing = false;
        threatTarget = null;
        agent.speed = originalSpeed;
        
        if (agent.hasPath)
            agent.ResetPath();
    }

    void Update()
    {
        // Update threat target from perception
        if (perception.currentTarget != null && IsDangerous(perception.currentTarget))
        {
            threatTarget = perception.currentTarget;
            if (!isFleeing)
                StartFleeing(threatTarget);
        }

        if (isFleeing && threatTarget != null)
        {
            fleeTimer -= Time.deltaTime;

            // Pick a new flee point if reached previous destination or timer elapsed
            if (!agent.hasPath || 
                (agent.remainingDistance <= stopDistance && !agent.pathPending) || 
                fleeTimer <= 0f)
            {
                PickFleePosition();
                fleeTimer = nextFleeDelay;
            }
        }
        else if (isFleeing && threatTarget == null)
        {
            // Use last known position if we lost the threat
            if (perception.hasLostTarget)
            {
                Vector3 lastPos = perception.lastKnownTargetPosition;
                if (lastPos != Vector3.zero)
                {
                    FleeFromPosition(lastPos);
                }
            }
            else
            {
                StopFleeing();
            }
        }
    }

    private void PickFleePosition()
    {
        Vector3 fleeFromPos = threatTarget != null ? threatTarget.position : perception.lastKnownTargetPosition;
        FleeFromPosition(fleeFromPos);
    }

    private void FleeFromPosition(Vector3 dangerPosition)
    {
        const int maxAttempts = 15;
        bool found = false;

        for (int i = 0; i < maxAttempts; i++)
        {
            // Calculate flee direction away from danger
            Vector3 fleeDir = (transform.position - dangerPosition).normalized;
            
            // Add some randomness to avoid predictable movement
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.7f, 0.7f), 
                0f, 
                Random.Range(-0.7f, 0.7f)
            );
            
            // Combine flee direction with random offset
            Vector3 finalDir = (fleeDir + randomOffset).normalized;
            Vector3 candidate = transform.position + finalDir * Random.Range(fleeRadius * 0.5f, fleeRadius);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                // Check if the position is actually further from danger
                float currentDistToDanger = Vector3.Distance(transform.position, dangerPosition);
                float candidateDistToDanger = Vector3.Distance(hit.position, dangerPosition);
                
                if (candidateDistToDanger >= currentDistToDanger * 0.8f && // At least maintain distance
                    Mathf.Abs(hit.position.y - transform.position.y) <= 3f) // Reasonable height difference
                {
                    agent.SetDestination(hit.position);
                    found = true;
                    break;
                }
            }
        }
        
        if (!found)
        {
            // Fallback: try to move directly away
            Vector3 fallbackDir = (transform.position - dangerPosition).normalized;
            Vector3 fallbackPos = transform.position + fallbackDir * fleeRadius * 0.5f;
            
            if (NavMesh.SamplePosition(fallbackPos, out NavMeshHit fallbackHit, 5f, NavMesh.AllAreas))
            {
                agent.SetDestination(fallbackHit.position);
            }
            else
            {
                Debug.LogWarning($"Civilian {name} couldn't find flee position");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (isFleeing && threatTarget != null)
        {
            // Draw flee radius
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, fleeRadius);
            
            // Draw line to threat
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, threatTarget.position);
        }
    }
}