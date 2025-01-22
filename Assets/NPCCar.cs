using UnityEngine;
using System.Collections.Generic;

public class NPCCar : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float waypointThreshold = 0.5f;
    [SerializeField] private float lookAheadDistance = 5f; // New parameter for smoother turns
    [SerializeField] private bool loopPath = true;
    
    private List<Transform> waypoints = new List<Transform>();
    private int currentWaypointIndex = 0;
    private Transform currentWaypoint;
    private Vector3 previousPosition;
    
    void Start()
    {
        Transform waypointsParent = GameObject.Find("Waypoints").transform;
        foreach (Transform waypoint in waypointsParent)
        {
            waypoints.Add(waypoint);
        }
        
        if (waypoints.Count > 0)
        {
            currentWaypoint = waypoints[0];
            previousPosition = transform.position;
        }
        else
        {
            Debug.LogError("No waypoints found for NPC car!");
        }
    }

    Vector3 GetLookAheadPoint()
    {
        Vector3 currentToWaypoint = currentWaypoint.position - transform.position;
        float distanceToWaypoint = currentToWaypoint.magnitude;

        if (distanceToWaypoint < lookAheadDistance)
        {
            int nextIndex = (currentWaypointIndex + 1) % waypoints.Count;
            Vector3 nextWaypointPos = waypoints[nextIndex].position;
            
            float blend = distanceToWaypoint / lookAheadDistance;
            return Vector3.Lerp(nextWaypointPos, currentWaypoint.position, blend);
        }

        return currentWaypoint.position;
    }
    
    void FixedUpdate() 
    {
        Vector3 lookAheadPoint = GetLookAheadPoint();
        Vector3 targetDirection = lookAheadPoint - transform.position;
        targetDirection.y = 0;
        
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
        
        if (targetDirection != Vector3.zero) 
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        float distanceToWaypoint = Vector3.Distance(transform.position, currentWaypoint.position);
        if (distanceToWaypoint < waypointThreshold) 
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Count)
            {
                if (loopPath)
                    currentWaypointIndex = 0;
                else
                    currentWaypointIndex = waypoints.Count - 1;
            }
            currentWaypoint = waypoints[currentWaypointIndex];
        }
    }
    
    void OnDrawGizmos()
    {
        if (waypoints.Count == 0) return;
        
        // Draw waypoints and connections
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;
            
            Gizmos.DrawSphere(waypoints[i].position, 0.5f);
            
            if (i < waypoints.Count - 1 && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
            else if (loopPath && waypoints[0] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
            }
        }
        
        // Draw look-ahead point in play mode
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(GetLookAheadPoint(), 0.3f);
        }
    }
}