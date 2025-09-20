using UnityEngine;
using System;
using System.Collections.Generic;

public class Perception : MonoBehaviour
{
    [Header("Vision Settings")]
    [SerializeField] private float _viewRadius = 10f;
    [SerializeField] private float _viewAngle = 120f;
    [SerializeField] private float _hearingRadius = 5f;
    
    [Header("Target Persistence")]
    [SerializeField] private float targetLossTime = 2f; // Time before losing target completely
    [SerializeField] private float targetSwitchDistance = 3f; // Distance threshold to switch to closer target
    
    public float ViewRadius => _viewRadius;
    public float ViewAngle => _viewAngle;
    public float HearingRadius => _hearingRadius;
    
    [Header("Targeting")]
    public LayerMask targetLayer; // zombies/player
    public LayerMask obstacleLayer; // walls/objects blocking vision
    
    [SerializeField] private Transform _currentTarget;
    private float _targetLostTime = 0f;
    private Vector3 _lastKnownTargetPosition;

    public Transform currentTarget 
    { 
        get => _currentTarget; 
        private set => _currentTarget = value; 
    }

    public Vector3 lastKnownTargetPosition => _lastKnownTargetPosition;
    public bool hasLostTarget => _currentTarget == null && _targetLostTime > 0f;

    public event Action<Transform> OnTargetAcquired;
    public event Action OnTargetLost;

    public void Init(float viewRadius, float viewAngle, float hearingRadius)
    {
        _viewRadius = viewRadius;
        _viewAngle = viewAngle;
        _hearingRadius = hearingRadius;
    }

    void Update()
    {
        UpdateVision();
        HandleTargetPersistence();
    }

    private void UpdateVision()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, _viewRadius, targetLayer);
        Transform bestTarget = null;
        float bestScore = 0f;

        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue; // Skip self
            
            Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);

            // Only consider target if within view cone
            if (angle <= _viewAngle / 2f)
            {
                // Check line of sight with offset to avoid ground collision
                Vector3 rayStart = transform.position + Vector3.up * 0.5f;
                Vector3 targetPos = hit.transform.position + Vector3.up * 0.5f;
                Vector3 rayDir = (targetPos - rayStart).normalized;
                float rayDistance = Vector3.Distance(rayStart, targetPos);

                if (!Physics.Raycast(rayStart, rayDir, rayDistance, obstacleLayer))
                {
                    float distance = Vector3.Distance(transform.position, hit.transform.position);
                    
                    // Score based on distance and angle (closer and more centered = higher score)
                    float distanceScore = 1f - (distance / _viewRadius);
                    float angleScore = 1f - (angle / (_viewAngle / 2f));
                    float score = (distanceScore + angleScore) * 0.5f;
                    
                    // Prefer current target slightly to avoid flickering
                    if (hit.transform == _currentTarget)
                        score += 0.1f;
                    
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestTarget = hit.transform;
                    }
                }
            }
        }

        // Only switch targets if the new target is significantly better or we have no target
        if (bestTarget != null)
        {
            if (_currentTarget == null || 
                bestTarget == _currentTarget ||
                Vector3.Distance(transform.position, bestTarget.position) < 
                Vector3.Distance(transform.position, _currentTarget.position) - targetSwitchDistance)
            {
                SetTarget(bestTarget);
            }
        }
        else if (_currentTarget != null)
        {
            // Start losing target
            if (_targetLostTime == 0f)
            {
                _lastKnownTargetPosition = _currentTarget.position;
                _targetLostTime = Time.time;
            }
        }
    }

    private void HandleTargetPersistence()
    {
        // Clear target after target loss time
        if (_targetLostTime > 0f && Time.time - _targetLostTime > targetLossTime)
        {
            SetTarget(null);
            _targetLostTime = 0f;
        }
    }

    /// <summary>
    /// Called externally to simulate hearing a sound (bullet, explosion, etc.)
    /// </summary>
    public void HearSound(Vector3 position, float intensity = 1f)
    {
        float dist = Vector3.Distance(transform.position, position);
        if (dist <= _hearingRadius * intensity)
        {
            // Only react if we don't have a current target or the sound is very close
            if (_currentTarget == null || dist < _hearingRadius * 0.3f)
            {
                Vector3 dir = (position - transform.position).normalized;
                transform.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.z));
                
                // Store the heard position as last known target position
                _lastKnownTargetPosition = position;
            }
        }
    }

    private void SetTarget(Transform target)
    {
        if (target != _currentTarget)
        {
            Transform previousTarget = _currentTarget;
            _currentTarget = target;
            
            if (target != null)
            {
                _targetLostTime = 0f;
                _lastKnownTargetPosition = target.position;
                OnTargetAcquired?.Invoke(_currentTarget);
            }
            else if (previousTarget != null)
            {
                OnTargetLost?.Invoke();
            }
        }
    }

    // Helper method to check if a specific target type is visible
    public bool CanSeeTargetWithTag(string tag)
    {
        return _currentTarget != null && _currentTarget.CompareTag(tag);
    }

    void OnDrawGizmosSelected()
    {
        // Draw view radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _viewRadius);
        
        // Draw hearing radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _hearingRadius);
        
        // Draw view cone
        Gizmos.color = Color.red;
        Vector3 leftBoundary = Quaternion.Euler(0, -_viewAngle / 2, 0) * transform.forward * _viewRadius;
        Vector3 rightBoundary = Quaternion.Euler(0, _viewAngle / 2, 0) * transform.forward * _viewRadius;
        
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        
        // Draw current target
        if (_currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, _currentTarget.position);
        }
        
        // Draw last known position
        if (_targetLostTime > 0f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_lastKnownTargetPosition, 1f);
        }
    }
}