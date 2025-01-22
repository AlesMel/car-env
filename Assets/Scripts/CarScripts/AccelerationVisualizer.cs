using UnityEngine;

public class AccelerationVisualizer : MonoBehaviour
{
    public GameObject accelerationArrowPrefab; // Assign your arrow prefab in the inspector
    public Transform arrowParent;              // Optionally parent the arrow to the car

    private GameObject accelerationArrowInstance;

    // References to the car's Rigidbody and previous velocity for computing acceleration
    private Rigidbody rb;
    private Vector3 previousVelocity;

    // A scaling factor to visualize acceleration better
    public float arrowScaleFactor = 0.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        previousVelocity = rb.linearVelocity;

        if (accelerationArrowPrefab != null)
        {
            accelerationArrowInstance = Instantiate(accelerationArrowPrefab, transform.position, Quaternion.identity, arrowParent);
            Debug.Log("Acceleration arrow instantiated.");
        }
        else
        {
            Debug.LogError("No acceleration arrow prefab assigned!");
        }
    }


    void FixedUpdate()
    {
        // Compute the acceleration vector (world space).
        Vector3 accelerationVector = (rb.linearVelocity - previousVelocity) / Time.fixedDeltaTime;
        previousVelocity = rb.linearVelocity;
        Debug.Log("Acceleration magnitude: " + accelerationVector.magnitude);

        // Update the arrow if it exists.
        if (accelerationArrowInstance != null)
        {
            // Position the arrow at the car's position.
            accelerationArrowInstance.transform.position = transform.position;

            // If the acceleration is nearly zero, we might not want to rotate the arrow.
            if (accelerationVector.sqrMagnitude > 0.001f)
            {
                // Rotate the arrow to point in the direction of the acceleration.
                accelerationArrowInstance.transform.rotation = Quaternion.LookRotation(accelerationVector);
            }

            // Scale the arrow based on the acceleration magnitude.
            // Assuming the arrow's local scale on the Z axis represents its length.
            float arrowLength = accelerationVector.magnitude * arrowScaleFactor;
            Vector3 newScale = accelerationArrowInstance.transform.localScale;
            newScale.z = arrowLength;
            newScale.y = 5f; // Set a fixed width for the arrow
            accelerationArrowInstance.transform.localScale = newScale;
        }
    }
}
