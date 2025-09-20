using UnityEngine;

public class VisionConeRenderer : MonoBehaviour
{
    [Header("Cone Settings")]
    public Material coneMaterial;       // Assign your premade transparent material here
    public int segments = 20;           // Smoothness of the cone

    private Perception perception;
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    void Start()
    {
        // Get perception from parent instead of self
        perception = GetComponentInParent<Perception>();

        mesh = new Mesh();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        meshFilter.mesh = mesh;

        if (coneMaterial != null)
        {
            meshRenderer.material = coneMaterial;
        }
        else
        {
            Debug.LogWarning("VisionConeRenderer: No material assigned!");
        }
    }


    void Update()
    {
        DrawCone();
    }

    private void DrawCone()
    {
        if (perception == null) return;

        float radius = perception.ViewRadius;
        float angle = perception.ViewAngle;

        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero; // origin at enemy position

        float halfAngle = angle / 2f;
        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = -halfAngle + (angle / segments) * i;
            float rad = currentAngle * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * radius;
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        // Align cone with enemy
        meshRenderer.transform.position = transform.position;
        meshRenderer.transform.rotation = transform.rotation;
    }
}
