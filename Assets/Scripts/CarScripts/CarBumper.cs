using Unity.VisualScripting;
using UnityEngine;

public class CarBumper : MonoBehaviour
{

    private Rigidbody m_Rigidbody; // Rigidbody component of the car
    private MeshCollider m_carCollider; // Mesh collider of the car

    [SerializeField]
    private EndlessRoadManager manager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }
}
