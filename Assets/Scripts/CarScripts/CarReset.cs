using UnityEngine;

public class CarReset : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        // Save the car's starting position and rotation
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    public void ResetCarPosition()
    {
        // Reset the car's position and rotation
        transform.position = startPosition;
        transform.rotation = startRotation;

        // Optionally, reset the velocity if using a Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
