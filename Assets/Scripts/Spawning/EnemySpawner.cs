using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class EnemyQuota
{
    public GameObject prefab;
    public int count;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public List<EnemyQuota> enemyQuotas;
    public float spawnRadius = 3f;
    public float maxHeightDifference = 0.5f; 
    public float minSeparation = 1f; 
    public float minPlayerDistance = 10f;
    public Transform playerPos;

    private List<Vector3> usedPositions = new List<Vector3>();
    public List<Transform> spawnPoints;
    
    public void SpawnEnemies()
    {
        usedPositions.Clear();

        foreach (var quota in enemyQuotas)
        {
            int spawned = 0;
            
            int attempts = 0;
            while (spawned < quota.count && attempts < quota.count * 10)
            {
                attempts++;

                // pick a random spawn point
                Transform sp = spawnPoints[Random.Range(0, spawnPoints.Count)];

                // random offset within radius
                Vector2 offset = Random.insideUnitCircle * spawnRadius;
                Vector3 candidate = sp.position + new Vector3(offset.x, 0f, offset.y);
                
                // skip if too close to player
                if (Vector3.Distance(candidate, playerPos.position) < minPlayerDistance)
                    continue;

                // sample nav mesh
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                {
                    // check height difference
                    if (Mathf.Abs(hit.position.y - sp.position.y) > maxHeightDifference)
                        continue;

                    // check separation from other enemies
                    bool tooClose = false;
                    foreach (var used in usedPositions)
                    {
                        if (Vector3.Distance(used, hit.position) < minSeparation)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    if (tooClose) continue;

                    // spawn enemy slightly above the NavMesh to ensure agent is valid
                    float spawnHeightOffset = 1f; // adjust based on agent size
                    Vector3 spawnPos = hit.position + Vector3.up * spawnHeightOffset;

                    GameObject enemy = Instantiate(quota.prefab, spawnPos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));

                    usedPositions.Add(hit.position);
                    
                    spawned++;
                }
            }

            if (spawned < quota.count)
                Debug.LogWarning($"Could only spawn {spawned} / {quota.count} of {quota.prefab.name}");
        }
    }
}
