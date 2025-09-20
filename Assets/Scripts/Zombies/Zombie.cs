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

        // random offset in a circle
        Vector2 randomCircle = Random.insideUnitCircle * followRadius;
        offset = new Vector3(randomCircle.x, 0, randomCircle.y);
    }
}