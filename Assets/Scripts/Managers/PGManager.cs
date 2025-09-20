using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

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
        StartCoroutine(ValidateAndMovePlayer());
    }
}

private IEnumerator ValidateAndMovePlayer()
{
    // Wait a frame for NavMesh to fully initialize
    yield return null;
    
    float maxAttempts = 50;
    float radius = playerNavSampleRadius;
    float maxHeightDiff = 3f;
    float moveSpeed = 10f; // Speed for smooth movement
    
    Vector3 startPos = player.position;
    Vector3 targetPosition = startPos;
    bool needsToMove = false;

    // First, check if current position is valid with a more generous search radius
    if (NavMesh.SamplePosition(startPos, out NavMeshHit startHit, 3f, NavMesh.AllAreas))
    {
        float heightDiff = Mathf.Abs(startHit.position.y - startPos.y);
        float horizontalDist = Vector3.Distance(new Vector3(startHit.position.x, 0, startHit.position.z), 
                                              new Vector3(startPos.x, 0, startPos.z));
        
        // Check if position is valid (reasonable height difference and close horizontally)
        if (heightDiff <= maxHeightDiff && horizontalDist <= 2f)
        {
            Debug.Log("Player is already in a valid NavMesh position.");
            // Still set target to the exact NavMesh position for consistency
            targetPosition = startHit.position + Vector3.up * 0.5f;
            
            // Only move if there's a significant difference
            if (Vector3.Distance(startPos, targetPosition) > 0.1f)
            {
                needsToMove = true;
            }
        }
        else
        {
            needsToMove = true;
            targetPosition = startHit.position + Vector3.up * 0.5f;
            Debug.Log($"Player position adjusted to nearest valid NavMesh point. Height diff: {heightDiff}, Horizontal dist: {horizontalDist}");
        }
    }
    else
    {
        // Current position is not valid, find a new one
        needsToMove = true;
        bool found = false;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 offset2D = Random.insideUnitCircle * radius;
            Vector3 candidate = startPos + new Vector3(offset2D.x, 0f, offset2D.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                if (Mathf.Abs(hit.position.y - startPos.y) <= maxHeightDiff)
                {
                    targetPosition = hit.position + Vector3.up * 0.5f;
                    found = true;
                    Debug.Log($"Found valid NavMesh position for player at {targetPosition}");
                    break;
                }
            }
        }

        if (!found)
        {
            Debug.LogWarning("Failed to find a valid NavMesh position for the player. Using original position.");
            targetPosition = startPos;
            needsToMove = false;
        }
    }

    // Smoothly move player to target position if needed
    if (needsToMove && Vector3.Distance(player.position, targetPosition) > 0.1f)
    {
        yield return StartCoroutine(SmoothMovePlayer(player.position, targetPosition, moveSpeed));
    }
    else if (needsToMove)
    {
        player.position = targetPosition;
    }

    // Continue with enemy spawning
    StartCoroutine(SpawnEnemiesNextFrame());
    StartCoroutine(EnableAgentsNextFrame());
}

    private IEnumerator SmoothMovePlayer(Vector3 startPos, Vector3 endPos, float speed)
    {
        float journeyLength = Vector3.Distance(startPos, endPos);
        float journeyTime = journeyLength / speed;
        float elapsedTime = 0f;

        Debug.Log($"Smoothly moving player from {startPos} to {endPos} over {journeyTime:F2} seconds");

        while (elapsedTime < journeyTime)
        {
            elapsedTime += Time.deltaTime;
            float fractionOfJourney = elapsedTime / journeyTime;
            
            // Use smooth step for easing
            fractionOfJourney = Mathf.SmoothStep(0f, 1f, fractionOfJourney);
            
            player.position = Vector3.Lerp(startPos, endPos, fractionOfJourney);
            yield return null;
        }

        // Ensure we end up exactly at the target position
        player.position = endPos;
        Debug.Log("Player movement complete");
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