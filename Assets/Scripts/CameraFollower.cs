using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    public Transform m_FollowTarget; // The car or target to follow

    [Range(1, 10)]
    public float m_cameraSpeed = 5f; // Speed for smoothing
    private float m_cameraZoomSpeed = 10f; // Speed for zooming
    private float minZoomDistance = 5f; // Minimum zoom distance
    private float maxZoomDistance = 20f; // Maximum zoom distance

    private Vector3 initialOffset; // Initial offset between camera and target
    private Quaternion initialCameraRotation;
    private float currentZoom; // Current zoom level
    private ZoomMode currentZoomMode = ZoomMode.Middle; // Current zoom mode
    private bool isReversing = false; // Whether the car is reversing
    private float reverseTimer = 0f; // Timer to track reversing duration
    private float reverseThreshold = 0.25f; // Time threshold to switch the camera when reversing
    
    private Rigidbody targetRigidbody; // Reference to the target's rigidbody
    [Header("Reverse Settings")]
    [SerializeField] private float reverseSpeedThreshold = -0.5f; // Speed threshold to determine reverse movement
    [SerializeField] private bool shouldReverse = false; // Whether the camera should reverse
    
    private enum ZoomMode
    {
        Lowest,
        Middle,
        Highest
    }

    void Start()
    {
        initialOffset = transform.position - m_FollowTarget.position;
        initialCameraRotation = transform.rotation;
        currentZoom = (minZoomDistance + maxZoomDistance) / 2f; // Start at the middle zoom
        
        // Get the Rigidbody component from the target
        targetRigidbody = m_FollowTarget.GetComponent<Rigidbody>();
        if (targetRigidbody == null)
        {
            Debug.LogError("No Rigidbody found on follow target!");
        }
    }

    void Update()
    {
        // Handle mode switching
        if (Input.GetKeyDown(KeyCode.C)) // 'C' for camera settings toggle
        {
            SwitchZoomMode();
        }

        // Check if actually moving backwards
        if (targetRigidbody != null)
        {
            // Project the velocity onto the car's forward direction
            float forwardSpeed = Vector3.Dot(targetRigidbody.linearVelocity, m_FollowTarget.forward);
            
            // Check if moving backwards and S key is pressed
            if (forwardSpeed < reverseSpeedThreshold && Input.GetKey(KeyCode.S))
            {
                reverseTimer += Time.deltaTime;
                if (reverseTimer > reverseThreshold)
                {
                    isReversing = true;
                }
            }
            else
            {
                reverseTimer = 0f;
                isReversing = false;
            }
        }
    }

    void FixedUpdate()
    {
        // Handle zooming
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentZoom -= scroll * m_cameraZoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoomDistance, maxZoomDistance);

        // Adjust offset based on zoom level and reversing state
        Vector3 zoomedOffset = initialOffset.normalized * currentZoom;

        if (isReversing && shouldReverse)
        {
            // Flip the offset to look behind while maintaining the initial rotation angle
            Vector3 flippedOffset = new Vector3(-initialOffset.x, initialOffset.y, -initialOffset.z);
            zoomedOffset = flippedOffset.normalized * currentZoom;
        }

        Vector3 targetPosition = m_FollowTarget.position + m_FollowTarget.rotation * zoomedOffset;
        Quaternion targetRotation = isReversing && shouldReverse
            ? Quaternion.LookRotation(-m_FollowTarget.forward, Vector3.up) * initialCameraRotation
            : m_FollowTarget.rotation * initialCameraRotation;

        transform.position = Vector3.Lerp(transform.position, targetPosition, m_cameraSpeed * Time.fixedDeltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, m_cameraSpeed * Time.fixedDeltaTime);
    }

    private void SwitchZoomMode()
    {
        switch (currentZoomMode)
        {
            case ZoomMode.Lowest:
                currentZoomMode = ZoomMode.Middle;
                currentZoom = (minZoomDistance + maxZoomDistance) / 2f;
                break;
            case ZoomMode.Middle:
                currentZoomMode = ZoomMode.Highest;
                currentZoom = maxZoomDistance;
                break;
            case ZoomMode.Highest:
                currentZoomMode = ZoomMode.Lowest;
                currentZoom = minZoomDistance;
                break;
        }
    }
}