using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CarController : MonoBehaviour
{
    [Header("Car Settings")]
    public float maxSteerAngle = 30f;
    public float steeringSpeed = 5f;
    public float motorForce = 1500f;
    public float brakeForce = 3000f;
    public float maxSpeed = 120f;

    [Header("Wheel Transforms")]
    [SerializeField]
    private Transform frontLeftWheelTransform;
    [SerializeField]
    private Transform frontRightWheelTransform;
    [SerializeField]
    private Transform rearLeftWheelTransform;
    [SerializeField]
    private Transform rearRightWheelTransform;

    [Header("Wheel Colliders")]
    [SerializeField]
    private WheelCollider frontLeftWheelCollider;
    [SerializeField]
    private WheelCollider frontRightWheelCollider;
    [SerializeField]
    private WheelCollider rearLeftWheelCollider;
    [SerializeField]
    private WheelCollider rearRightWheelCollider;

    [Header("UI Elements")]
    public Text speedText;
    public Text accelerationText;

    private Rigidbody rb;

    private float horizontalInput;
    private float verticalInput;
    private bool isBreaking;
    private float currentSteerAngle;
    private float currentMotorTorque;

    private Vector3 previousVelocity;

    [Header("Drive Type")]
    public bool canReverse = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        previousVelocity = rb.linearVelocity; // Initialize previous velocity
    }

    void FixedUpdate()
    {
        // HandleInput(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        HandleMotor();
        HandleSteering();
        HandleBraking();
        UpdateWheels();
        DisplaySpeed();
        DisplayAcceleration();
    }

    public void HandleInput(float steering, float acceleration)
    {
        horizontalInput = steering;
        verticalInput = acceleration;
        if (!canReverse) {
            isBreaking = verticalInput < 0;
        } else {
            isBreaking = verticalInput < 0 && Vector3.Dot(rb.linearVelocity, transform.forward) > 0;
        }
    }

    private void HandleMotor()
    {
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        float speedRatio = forwardSpeed / (maxSpeed / 3.6f);
        speedRatio = Mathf.Clamp01(speedRatio);

        currentMotorTorque = verticalInput * motorForce * (1 - speedRatio);

        frontLeftWheelCollider.motorTorque = currentMotorTorque;
        frontRightWheelCollider.motorTorque = currentMotorTorque;
        rearLeftWheelCollider.motorTorque = 0;
        rearRightWheelCollider.motorTorque = 0;
    } 

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }


    private void HandleBraking()
    {
        float brakingForce = isBreaking ? brakeForce : 0f;  
        frontLeftWheelCollider.brakeTorque = brakingForce;
        frontRightWheelCollider.brakeTorque = brakingForce;
        rearLeftWheelCollider.brakeTorque = brakingForce;
        rearRightWheelCollider.brakeTorque = brakingForce;
    }


    private void UpdateWheels()
    {
        UpdateSingleWheelPose(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheelPose(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheelPose(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateSingleWheelPose(rearRightWheelCollider, rearRightWheelTransform);
    }

    private void UpdateSingleWheelPose(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);

        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    private void DisplaySpeed()
    {
        float currentSpeed = rb.linearVelocity.magnitude * 3.6f; // Convert to km/h
        if (speedText != null)
        {
            float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
            if (forwardSpeed < 0)
                speedText.text = currentSpeed.ToString("0");
            else
                speedText.text = Mathf.Abs(currentSpeed).ToString("0");
        }
    }

    private void DisplayAcceleration()
    {
        if (accelerationText == null) return;

        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 acceleration = (currentVelocity - previousVelocity) / Time.fixedDeltaTime;

        float accelerationInG = acceleration.magnitude / 9.81f;
        accelerationText.text = accelerationInG.ToString("F2");
        previousVelocity = currentVelocity;
    }

    public Vector3 GetVelocity()
    {
        return rb.linearVelocity / 3.6f;
    }

    public void ResetCar(Vector3 position)
    {
        transform.SetPositionAndRotation(position, Quaternion.identity);
        
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
