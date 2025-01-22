using UnityEngine;

public class StaticCarSpawner : MonoBehaviour
{
    private Vector3 m_InitialPosition;
    private Quaternion m_InitialRotation;
    private Rigidbody m_Rigidbody;
    private Collider m_BodyCollider;

    private float respawnDelay = 3f;  // Time before attempting respawn
    private float checkRadius = 2.2f;   // Radius to check for obstacles
    [SerializeField] private LayerMask collisionLayers;  // Layers to check for collision

    private float displaceTimer = 0f;
    private bool isDisplaced = false;

    void Start()
    {
        // Store initial transform
        m_InitialPosition = transform.position;
        m_InitialRotation = transform.rotation;
        
        // Get components
        m_Rigidbody = GetComponent<Rigidbody>();
        m_BodyCollider = GetComponent<Collider>();
    }

    void Update()
    {
        // Check if car has moved from initial position
        if (Vector3.Distance(transform.position, m_InitialPosition) > 0.1f)
        {
            if (!isDisplaced)
            {
                isDisplaced = true;
                displaceTimer = 0f;
            }
            
            if (isDisplaced)
            {
                displaceTimer += Time.deltaTime;
                
                // After delay, check if we can respawn
                if (displaceTimer >= respawnDelay)
                {
                    TryRespawn();
                }
            }
        }
    }

    private void TryRespawn()
    {
        if (IsSpawnPositionClear())
        {
            // Reset position and physics
            transform.position = m_InitialPosition;
            transform.rotation = m_InitialRotation;

            if (m_Rigidbody != null)
            {
                m_Rigidbody.linearVelocity = Vector3.zero;
                m_Rigidbody.angularVelocity = Vector3.zero;
            }

            // Reset state
            isDisplaced = false;
            displaceTimer = 0f;
        }
    }

    private bool IsSpawnPositionClear()
    {
        // Use SphereCast to check if spawn position is clear
        Collider[] colliders = Physics.OverlapSphere(
            m_InitialPosition,
            checkRadius,
            collisionLayers
        );

        foreach (Collider col in colliders)
        {
            // Ignore self and children
            if (col.transform.IsChildOf(transform) || col.transform == transform)
                continue;

            // If we found any other collider, position is not clear
            return false;
        }

        return true;
    }

    // Optional: Visualize the check radius in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}