using UnityEngine;

public class TriggerChecker : MonoBehaviour
{
    [Tooltip("Optional tag to filter which objects to check overlap with")]
    public string targetTag = "Car";

    private void OnTriggerStay(Collider other)
    {
        Transform currentTransform = other.transform;
        while (currentTransform.parent != null)
        {
            if (currentTransform.CompareTag(targetTag))
            {
                break;
            }
            currentTransform = currentTransform.parent;
        }

        if (!currentTransform.CompareTag(targetTag))
            return;

        Collider triggerCollider = GetComponent<Collider>();
        Collider carBodyCollider = other;

        if (triggerCollider == null || carBodyCollider == null)
            return;

        // Calculate bounds in world space
        Bounds triggerBounds = triggerCollider.bounds;
        Bounds carBodyBounds = carBodyCollider.bounds;

        // Calculate intersection
        Vector3 min = Vector3.Max(triggerBounds.min, carBodyBounds.min);
        Vector3 max = Vector3.Min(triggerBounds.max, carBodyBounds.max);

        // Check if there is an intersection
        if (min.x <= max.x && min.y <= max.y && min.z <= max.z)
        {
            // Calculate volumes
            float intersectionVolume = (max.x - min.x) * (max.y - min.y) * (max.z - min.z);
            float carBodyVolume = carBodyBounds.size.x * carBodyBounds.size.y * carBodyBounds.size.z;
            float overlapPercentage = (intersectionVolume / carBodyVolume) * 100f;

            if (overlapPercentage > 90f)
            {
                Debug.Log($"Car {currentTransform.name} body is mostly inside the trigger!");
            }
        }
    }

    
    private void OnDrawGizmos()
    {
        // Visualize the trigger area in the editor
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        }
    }

}