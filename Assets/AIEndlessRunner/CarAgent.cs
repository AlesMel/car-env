using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.UIElements;

public class CarAgent : Agent
{
    private LidarSensor lidarSensor;

    public CarController carController;
    public Transform goalTransform;  // Reference to Goal in Unity Editor

    private Vector3 defaultPosition;
    private float startDistance;

    void Start()
    {
        defaultPosition = carController.transform.position;
        startDistance = Vector3.Distance(defaultPosition, goalTransform.position);
        lidarSensor = GetComponent<LidarSensor>();
    }

    public override void OnEpisodeBegin()
    {
        carController.ResetCar(defaultPosition);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 carVelocity = carController.GetVelocity();
        float distanceToGoal = Vector3.Distance(carController.transform.position, goalTransform.position);
        lidarSensor.DoRaycasts();
        sensor.AddObservation(carVelocity);
        sensor.AddObservation(distanceToGoal);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        carController.HandleInput(actions.ContinuousActions[0], actions.ContinuousActions[1]);


        Vector3 goalDirection = (goalTransform.position - defaultPosition).normalized;
        float progress = Vector3.Dot(transform.position - defaultPosition, goalDirection);
        float totalDistance = Vector3.Distance(defaultPosition, goalTransform.position);
        float distanceReward = Mathf.Clamp01(progress / totalDistance);

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Obstacle"))
        {
            AddReward(-1.0f);
            EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Goal"))
        {
            AddReward(2.0f);
            EndEpisode();
        }
    }
}
