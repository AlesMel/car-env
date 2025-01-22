using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Prefab References")]
    public GameObject[] obstacles;
    public GameObject[] collectibles;
    public GameObject roadPrefab;
    public Transform player;

    [Header("Spawner Settings")]
    public int preSpawnCount = 10; // Number of obstacles to pre-spawn
    public float spawnInterval = 20f; // Distance between obstacle spawns
    public float roadWidth; // Calculated road width minus curbs
    private float leftLaneX; // Center X position of the left lane
    private float rightLaneX; // Center X position of the right lane
    private float curbSizeX = 1.7f; // Width of the curbs on each side
    private float lastSpawnZ; // Tracks the last Z position for spawning    


    // TODO: Add rotation

    void Start()
    {
        // Calculate lane positions
        roadWidth = (roadPrefab.GetComponent<MeshRenderer>().bounds.size.x - 2 * curbSizeX) / 2;
        leftLaneX = -roadWidth + roadWidth / 2;
        rightLaneX = roadWidth - roadWidth / 2;

        // Pre-spawn obstacles
        PreSpawnObstacles();
        lastSpawnZ = player.position.z + preSpawnCount * spawnInterval; // Set lastSpawnZ after pre-spawning
    }

    void FixedUpdate()
    {
        // Dynamically spawn obstacles as the player progresses
        if (player.position.z > lastSpawnZ - 3 * spawnInterval)
        {
            SpawnObstacle();
            lastSpawnZ += spawnInterval;
        }
        RemoveOldObstacle();
    }

    void PreSpawnObstacles()
    {
        for (int i = 1; i <= preSpawnCount; i++)
        {
            float spawnZ = player.position.z + i * spawnInterval;
            SpawnObstacleAt(spawnZ);
        }
    }

    void SpawnObstacle()
    {
        // Spawn one obstacle dynamically
        SpawnObstacleAt(lastSpawnZ + spawnInterval);
    }

    void SpawnObstacleAt(float zPosition)
    {
        // Select a random obstacle prefab
        int randomIndex = Random.Range(0, obstacles.Length);
        GameObject obstacle = Instantiate(obstacles[randomIndex]);

        // Randomly choose a lane (0 for left, 1 for right)
        float laneX = Random.Range(0, 2) == 0 ? leftLaneX : rightLaneX;

        // Set the obstacle position at the specified Z position
        obstacle.transform.position = new Vector3(laneX, 0, zPosition);
    }

    void RemoveOldObstacle()
    {
        if (transform.childCount > preSpawnCount)
        {
            Destroy(transform.GetChild(0).gameObject);
        }
    }
}
