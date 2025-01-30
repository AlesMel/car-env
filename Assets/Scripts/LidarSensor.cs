using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LidarSensor : MonoBehaviour
{
    public float maxDistance = 100f;
    [Range(0, 180)] public int verticalFOV = 30;
    [Range(0, 180)] public int horizontalFOV = 30;
    [Range(1, 20)] public int verticalStep = 8;
    [Range(1, 20)] public int horizontalStep = 8;

    public bool drawLines = true;
    public bool drawSpheres = true;
    public float sphereRadius = 0.1f;
    [SerializeField] private Color nearColor = Color.red;
    [SerializeField] private Color farColor = Color.green;
    
    public string[] tagsToDetect;

    private struct LidarRay
    {
        public float vertical;
        public float horizontal;
        public Vector3 localDirection;
    }

    private List<LidarRay> lidarRays;
    private float[] distances;
    private HashSet<string> tagSet;
    public bool returnNormalized = true;
    public bool debugLogs = false;

    private void OnValidate()
    {
        if (tagsToDetect != null) tagSet = new HashSet<string>(tagsToDetect);
        else tagSet = new HashSet<string>();
        RebuildRays();
    }

    private void Awake()
    {
        RebuildRays();
    }

    private void RebuildRays()
    {
        lidarRays = new List<LidarRay>();
        for (int p = -verticalFOV; p <= verticalFOV; p += verticalStep)
        {
            for (int y = -horizontalFOV; y <= horizontalFOV; y += horizontalStep)
            {
                LidarRay ray;
                ray.vertical = p;
                ray.horizontal = y;
                ray.localDirection = CalculateLocalDirection(p, y);
                lidarRays.Add(ray);
            }
        }
        distances = new float[lidarRays.Count];
    }

    private void FixedUpdate()
    {
        if (!Application.isPlaying) return;
        DoRaycasts();
    }

    private void DoRaycasts()
    {
        Vector3 origin = transform.position;
        Quaternion rotation = transform.rotation;
        for (int i = 0; i < lidarRays.Count; i++)
        {
            Vector3 dir = rotation * lidarRays[i].localDirection;
            if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDistance))
            {
                if (tagSet.Contains(hit.collider.tag)) 
                {
                    distances[i] = hit.distance;
                    if (returnNormalized) distances[i] /= maxDistance;
                }
                else distances[i] = -1f;
            }
            else distances[i] = -1f;
        }
        if (debugLogs)
        {
            for (int i = 0; i < distances.Length; i++)
            {
                Debug.Log($"Distance {i}: {distances[i]}");
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (lidarRays == null || distances == null) RebuildRays();
        if (!Application.isPlaying) DoRaycasts();
        
        Vector3 origin = transform.position;
        Quaternion rotation = transform.rotation;

        for (int i = 0; i < lidarRays.Count; i++)
        {
            float distance = distances[i];
            if (returnNormalized) distance *= maxDistance;
            Vector3 dir = rotation * lidarRays[i].localDirection;
            if (distance > 0f)
            {
                float fraction = distance / maxDistance;
                Gizmos.color = Color.Lerp(nearColor, farColor, fraction);
                if (drawSpheres) Gizmos.DrawSphere(origin + dir * distance, sphereRadius);
                if (drawLines) Gizmos.DrawLine(origin, origin + dir * distance);
            }
            else
            {
                if (drawLines)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(origin, origin + dir * maxDistance);
                }
            }
        }
    }

    private Vector3 CalculateLocalDirection(float p, float y)
    {
        float pitchRad = Mathf.Deg2Rad * p;
        float yawRad = Mathf.Deg2Rad * y;
        float x = Mathf.Cos(pitchRad) * Mathf.Cos(yawRad);
        float z = Mathf.Cos(pitchRad) * Mathf.Sin(yawRad);
        float yy = Mathf.Sin(pitchRad);
        return new Vector3(x, yy, z).normalized;
    }

    public float[] GetDistances() => distances;
    public int RayCount => lidarRays?.Count ?? 0;
}
