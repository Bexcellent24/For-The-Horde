using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    [SerializeField] private GameObject clickMarkerPrefab;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f); // visual ray in Scene view
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log("Destination: " + hit.point + " Hit: " + hit.collider.name);
                agent.SetDestination(hit.point);
                
                if (clickMarkerPrefab != null)
                {
                    // Spawn marker slightly above ground so it doesn’t Z-fight
                    Vector3 pos = hit.point + Vector3.up * 0.01f;
                    Instantiate(clickMarkerPrefab, pos, Quaternion.identity);
                }
            }
            else
            {
                Debug.Log("No hit detected");
            }
        }

    }
}