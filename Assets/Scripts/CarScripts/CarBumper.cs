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

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Goal"))
        {
            Debug.Log("Goal reached!");
        }
        else
        {
            Debug.Log("Obstacle hit!");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Obstacle"))
        {
            Debug.Log("Obstacle hit!");
            manager.GameOver();
        }
    }
}
