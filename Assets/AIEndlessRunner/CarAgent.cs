using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.UIElements;

public class CarAgent : Agent
{

    public LidarSensor lidar;
    public CarController carController;
    private Vector3 defaultPosition;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        defaultPosition = carController.transform.position;
    }

    public override void OnEpisodeBegin()
    {
        carController.ResetCar(defaultPosition);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float[] lidarReadings = lidar.GetDistances();
        Vector3 carVelocity = carController.GetVelocity();
        sensor.AddObservation(lidarReadings);
        sensor.AddObservation(carVelocity);
        // for (int i = 0; i < lidarReadings.Length; i++)
        // {
        //     Debug.Log(lidarReadings[i]);
        // }
        // Debug.Log("Velocity: " + carVelocity);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Debug.Log("Action received" + actions.ContinuousActions[0] + " " + actions.ContinuousActions[1]);
        carController.HandleInput(actions.ContinuousActions[0], actions.ContinuousActions[1]);
    }

    public void AddRewards()
    {
        Debug.Log("Reward added");
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}
