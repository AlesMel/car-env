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
    public float accelerationLerpSpeed = 5f;
    public float steeringLerpSpeed = 0.5f;
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
    private float currentBrakeForce;
    private float currentMotorTorque;
    private float currentCarSpeed;

    private Vector3 previousVelocity;

    public enum DriveType
    {
        FWD,
        RWD,
        AWD
    }

    [Header("Drive Type")]
    public DriveType driveType = DriveType.FWD;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        previousVelocity = rb.linearVelocity; // Initialize previous velocity
    }

    void FixedUpdate()
    {
        HandleInput(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        HandleMotor();
        HandleSteering();
        HandleBraking();
        UpdateWheels();
        DisplaySpeed();
        DisplayAcceleration();
    }

    private void HandleInput(float steering, float acceleration)
    {
        horizontalInput = steering;
        verticalInput = acceleration;
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    private void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;
    }   

    private void HandleSteering()
    {
        // float targetSteerAngle = maxSteerAngle * horizontalInput;
        // frontLeftWheelCollider.steerAngle = Mathf.Lerp(frontLeftWheelCollider.steerAngle, targetSteerAngle, Time.fixedDeltaTime * steeringSpeed);
        // frontRightWheelCollider.steerAngle = Mathf.Lerp(frontRightWheelCollider.steerAngle, targetSteerAngle, Time.fixedDeltaTime * steeringSpeed);
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
}
