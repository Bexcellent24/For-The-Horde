using UnityEngine;
using UnityEngine.AI;

public class EnemyActor : MonoBehaviour
{
    public EnemyData data;

    private Health health;
    private NavMeshAgent agent;
    private Attacker attacker;
    private Perception perception;
    
    private float currentViewAngle;
    private float currentSpeed;

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

            // keep a runtime copy of perception values
            currentViewAngle = data.viewAngle;
            perception?.Init(data.viewRadius, currentViewAngle, data.hearingRadius);
            
            currentSpeed = data.moveSpeed;
            agent.speed = currentSpeed;
        }
    }

    public void EditSpeed(float speedIncrease)
    {
        if (data != null)
        {
            currentSpeed += speedIncrease;
            agent.speed = currentSpeed;
        }
    }

    public void EditPerception(float perceptionDecrease)
    {
        currentViewAngle -= perceptionDecrease;
        Debug.Log($"{name} Edit Perception: new percep = " + currentViewAngle);
        perception?.Init(data.viewRadius, currentViewAngle, data.hearingRadius);
    }
}