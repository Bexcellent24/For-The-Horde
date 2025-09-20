using UnityEngine;
using UnityEngine.AI;

public class EnemyActor : MonoBehaviour
{
    public EnemyData data;

    private Health health;
    private NavMeshAgent agent;
    private Attacker attacker;
    private Perception perception;

    void Awake()
    {
        health = GetComponent<Health>();
        agent = GetComponent<NavMeshAgent>();
        attacker = GetComponent<Attacker>();
        perception = GetComponent<Perception>();

        if (data != null)
        {
            health?.Init(data.maxHealth);
            attacker?.Init(data.fireRate, data.attackRange, data.bulletPrefab, data.bulletSpeed, data.bulletDamage);
            perception?.Init(data.viewRadius, data.viewAngle, data.hearingRadius);
            agent.speed = data.moveSpeed;
        }
    }
}