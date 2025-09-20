using UnityEngine;
using System;

public class Perception : MonoBehaviour
{
    
    private float _viewRadius = 10f;
    private float _viewAngle = 120f;
    private float _hearingRadius = 5f;
    
    public float ViewRadius => _viewRadius;
    public float ViewAngle => _viewAngle;
    public float HearingRadius => _hearingRadius;
    
    [Header("Targeting")]
    public LayerMask targetLayer; // zombies/player
    public LayerMask obstacleLayer; // walls/objects blocking vision
    public Transform currentTarget { get; private set; }

    public event Action<Transform> OnTargetAcquired;

    public void Init(float viewRadius, float viewAngle, float hearingRadius)
    {
        _viewRadius = viewRadius;
        _viewAngle = viewAngle;
        _hearingRadius = hearingRadius;
    }
    void Update()
    {
        UpdateVision();
    }

    /// <summary>
    /// Check vision cone for targets.
    /// </summary>
    private void UpdateVision()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, _viewRadius, targetLayer);
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);

            // Only consider target if within view cone
            if (angle <= _viewAngle / 2f)
            {
                // Check line of sight
                if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, dirToTarget, out RaycastHit obstacle, _viewRadius, obstacleLayer))
                {
                    float dist = Vector3.Distance(transform.position, hit.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = hit.transform;
                    }
                }
            }
        }

        SetTarget(nearest);
    }

    /// <summary>
    /// Called externally to simulate hearing a sound (bullet, explosion, etc.)
    /// </summary>
    public void HearSound(Vector3 position)
    {
        // Only acquire new target if not currently tracking one
        if (currentTarget == null)
        {
            float dist = Vector3.Distance(transform.position, position);
            if (dist <= _hearingRadius)
            {
                Vector3 dir = (position - transform.position).normalized;
                transform.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.z));
                currentTarget = null; // optional: could aim at position, but not assign until seen
            }
        }
    }

    private void SetTarget(Transform target)
    {
        if (target != currentTarget)
        {
            currentTarget = target;
            OnTargetAcquired?.Invoke(currentTarget);
        }
    }
}
