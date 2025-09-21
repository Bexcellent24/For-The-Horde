using UnityEngine;
using UnityEngine.AI;

public class AttackerAnimatorController : MonoBehaviour
{ 
    private Animator anim; 
    private NavMeshAgent agent;

    void Awake() 
    {
        anim = GetComponent<Animator>();
        agent = GetComponentInParent<NavMeshAgent>();
    }

    void Update() {
            float speed = agent.velocity.magnitude;
            anim.SetFloat("Speed", speed);
    }
}
