using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject roadPrefab; // Assign your road prefab in the Inspector
    public GameObject startGatePrefab;
    public GameObject goalGatePrefab;

    public int numberOfSegments = 10; // Number of road segments to keep active
    private float segmentLength; // Length of each road segment

    private Queue<GameObject> activeSegments = new Queue<GameObject>();
    private Vector3 nextSpawnPosition;
    private int spawned_index;
    private int rand_ind;

    [Header("Obstacles")]
    public GameObject[] obstacles;
    [Range(10f, 100f)]
    public float spawnInterval = 30f; // Distance between obstacle spawns
    private float originalInterval;
    public bool progressiveSpawn = true; // Increase spawn interval over time


    [Header("Collectibles Settings")]
    public GameObject[] collectibles;
    public int numberOfCollectibles = 20; 
    public float collectibleSpawnInterval = 25f; // how far apart collectibles are
    private Queue<GameObject> activeCollectibles = new Queue<GameObject>();
    private float collectibleYOffset = 1f;

    [Header("UI Elements")]
    public Text scoreText;
    public Text distanceText;

    [Header("Spawner Settings")]
    public int numberOfObstacles = 20; // Number of obstacles to keep-active
    public float roadWidth; // Calculated road width minus curbs
    private float leftLaneX; // Center X position of the left lane
    private float rightLaneX; // Center X position of the right lane
    private float curbSizeX = 1.7f; // Width of the curbs on each side
    private float lastSpawnZ; // Tracks the last Z position for spawning    
    private Queue<GameObject> activeObstacles = new Queue<GameObject>();

    void Start()
    {
        Vector3 roadPrefabBounds = roadPrefab.GetComponent<MeshRenderer>().bounds.size;
        segmentLength = roadPrefabBounds.z;

        roadWidth = (roadPrefabBounds.x - 2 * curbSizeX) / 2;
        leftLaneX  = -roadWidth + roadWidth / 2;
        rightLaneX =  roadWidth - roadWidth / 2;

        // SpawnCollectiblePool();

        originalInterval = spawnInterval;
    }

    void FixedUpdate()
    {
        // distanceText.text = player.position.z.ToString("0");
    }


    private void SpawnObstacles()
    {
        float startZ = startGatePrefab.transform.position.z + 50f;
        float currentZ = startZ;
        for (int i = 0; i < numberOfObstacles; i++)
        {
            int randomIndex = Random.Range(0, obstacles.Length);
            GameObject obstacle = Instantiate(obstacles[randomIndex]);
            float laneX = Random.Range(0, 2) == 0 ? leftLaneX : rightLaneX;

            obstacle.transform.position = new Vector3(laneX, 0, currentZ);

            currentZ += spawnInterval; 

            activeObstacles.Enqueue(obstacle);
        }
    }

    private float FindSafeLaneX(float chosenLaneX, float spawnZ)
    {
        if (activeObstacles.Count == 0) return chosenLaneX;
        float safeDistance = 15f;
        foreach (var obstacleObj in activeObstacles)
        {
            Vector3 obsPos = obstacleObj.transform.position;
            if (Mathf.Abs(obsPos.x - chosenLaneX) < 0.1f && Mathf.Abs(obsPos.z - spawnZ) < safeDistance)
            {
                if (Mathf.Approximately(chosenLaneX, leftLaneX))
                    return rightLaneX;
                else
                    return leftLaneX;
            }
        }
        return chosenLaneX;
    }


    private void SpawnCollectiblePool()
    {
        float currentZ = startGatePrefab.transform.position.z + 30f;

        for (int i = 0; i < numberOfCollectibles; i++)
        {
            int randomIndex = Random.Range(0, collectibles.Length);
            GameObject collectible = Instantiate(collectibles[randomIndex]);
            activeCollectibles.Enqueue(collectible);

            Collectible collScript = collectible.GetComponent<Collectible>();
            
            // if (collScript != null)
            // {
            //     collScript.manager = this;
            // }

            float laneX = (Random.Range(0, 2) == 0) ? leftLaneX : rightLaneX;
            laneX = FindSafeLaneX(laneX, currentZ);
            collectible.transform.position = new Vector3(laneX, collectibleYOffset, currentZ);
            currentZ += collectibleSpawnInterval;
        }
    }

    public void OnCollectibleTriggered(GameObject collectible)
    {
        collectible.SetActive(false);
        scoreText.text = (int.Parse(scoreText.text) + 1).ToString();
    }

    private float GetFarthestObstacleZ()
    {
        float maxZ = float.MinValue;
        foreach (var obstacle in activeObstacles)
        {
            float z = obstacle.transform.position.z;
            if (z > maxZ)
                maxZ = z;
        }
        return maxZ;
    }

    public void GameOver()
    {
        Debug.Log("Game Over!");
    }
}
