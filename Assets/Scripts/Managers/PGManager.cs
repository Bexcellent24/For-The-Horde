using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class PGManager : MonoBehaviour
{
    [Header("References")]
    public MapGenerator mapGenerator;
    public NavMeshSurface navMeshSurface;
    public EnemySpawner enemySpawner;
    
    [Header("Player")]
    public Transform player;
    public float playerNavSampleRadius = 2f;

    void Start()
    {
        if (mapGenerator != null)
            mapGenerator.OnMapGenerated += HandleGenerationComplete;
        
        // Kick off generation
        mapGenerator.Generate();
    }

    private void HandleGenerationComplete()
    {
        Debug.Log("PGManager: Map generation complete, baking NavMesh...");
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            
            float maxAttempts = 50;
            float radius = playerNavSampleRadius; // how far from original start position to search
            float maxHeightDiff = 3f;
            bool found = false;

            Vector3 startPos = player.position;

            for (int i = 0; i < maxAttempts; i++)
            {
                // Random offset within radius
                Vector2 offset2D = Random.insideUnitCircle * radius;
                Vector3 candidate = startPos + new Vector3(offset2D.x, 0f, offset2D.y);

                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                {
                    if (Mathf.Abs(hit.position.y - startPos.y) <= maxHeightDiff)
                    {
                        player.position = hit.position + Vector3.up * 0.5f;
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
                Debug.LogWarning("Failed to find a valid NavMesh position for the player near the start.");


            StartCoroutine(SpawnEnemiesNextFrame());
            StartCoroutine(EnableAgentsNextFrame());
            
        }
    }
    
    private IEnumerator SpawnEnemiesNextFrame()
    {
        yield return null;
        
        EnemySpawnPoint[] spawnPoints = FindObjectsOfType<EnemySpawnPoint>();
        List<Transform> spawnTransforms = new List<Transform>();
        foreach (var sp in spawnPoints)
            spawnTransforms.Add(sp.transform);
        
        enemySpawner.spawnPoints = spawnTransforms;
        enemySpawner.SpawnEnemies();
    }
    
    private IEnumerator EnableAgentsNextFrame()
    {
        yield return null; 

        NavMeshAgent[] agents = FindObjectsOfType<NavMeshAgent>();

        foreach (var agent in agents)
        {
            agent.enabled = true;
        }
    }
}