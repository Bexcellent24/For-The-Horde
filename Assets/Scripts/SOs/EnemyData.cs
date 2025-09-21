using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Data/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Stats")]
    public string enemyName;
    public float maxHealth = 100f;
    public float moveSpeed = 3.5f;

    [Header("Attack Stats")]
    public float attackRange = 10f;
    public float fireRate = 1f;
    public GameObject bulletPrefab;
    public float bulletDamage = 20f;
    public float bulletSpeed = 10f;
    

    [Header("Perception Stats")]
    public float viewRadius;
    public float viewAngle;
    public float hearingRadius;
    
}

