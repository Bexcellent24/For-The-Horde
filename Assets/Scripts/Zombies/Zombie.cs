using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform followTarget;

    public float followRadius = 3f;  // how far behind the player zombies cluster
    private Vector3 offset;          // unique offset for this zombie

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (followTarget == null) return;

        Vector3 targetPos = followTarget.position - followTarget.forward * 1.5f; // base behind player
        Vector3 desiredPos = targetPos + offset;

        agent.SetDestination(desiredPos);
    }

    public void SetFollowTarget(Transform target)
    {
        followTarget = target;

        // Generate positions in an arc behind the player (180 degrees)
        float randomAngle = Random.Range(90f, 270f); // 90° to 270° = behind the player
        float randomDistance = Random.Range(0.5f, followRadius);
    
        float radians = randomAngle * Mathf.Deg2Rad;
        float x = Mathf.Cos(radians) * randomDistance;
        float z = Mathf.Sin(radians) * randomDistance;
    
        offset = new Vector3(x, 0, z);
    }
}