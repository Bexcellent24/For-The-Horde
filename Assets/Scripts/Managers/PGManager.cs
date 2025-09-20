using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class PGManager : MonoBehaviour
{
    [Header("References")]
    public MapGenerator mapGenerator;
    public NavMeshSurface navMeshSurface;

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
            StartCoroutine(EnableAgentsNextFrame());
        }
    }
    
    private IEnumerator EnableAgentsNextFrame()
    {
        yield return null; // wait one frame after baking

        NavMeshAgent[] agents = FindObjectsOfType<NavMeshAgent>();

        foreach (var agent in agents)
        {
            agent.enabled = true;
        }
    }
}