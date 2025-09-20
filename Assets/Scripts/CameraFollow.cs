using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // assign your main zombie here

    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0, 15, -10); // default top-down offset
    public float followSpeed = 5f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minZoom = 5f;
    public float maxZoom = 25f;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Follow target position
        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * followSpeed);

        // Keep fixed rotation (looking downwards)
        transform.rotation = Quaternion.Euler(60f, 0f, 0f);

        // Zoom in/out with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            offset.y = Mathf.Clamp(offset.y - scroll * zoomSpeed, minZoom, maxZoom);
        }
    }
}