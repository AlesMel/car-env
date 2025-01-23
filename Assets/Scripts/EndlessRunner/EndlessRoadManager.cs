using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndlessRoadManager : MonoBehaviour
{
    public GameObject[] roadPrefabs; // Assign your road prefab in the Inspector
    public Transform player; // Reference to the player's car (assign in Inspector)
    public int numberOfSegments = 10; // Number of road segments to keep active
    private float segmentLength; // Length of each road segment

    private Queue<GameObject> activeSegments = new Queue<GameObject>();
    private Vector3 nextSpawnPosition;
    private int spawned_index;
    private int rand_ind;

    [Header("Obstacles")]
    public GameObject[] obstacles;
    public GameObject endGate;
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
        Vector3 roadPrefabBounds = roadPrefabs[0].GetComponent<MeshRenderer>().bounds.size;
        segmentLength = roadPrefabBounds.z;

        nextSpawnPosition = Vector3.zero;
        nextSpawnPosition.z -= segmentLength;
        for (int i = 0; i < numberOfSegments; i++)
            SpawnSegment();


        roadWidth = (roadPrefabBounds.x - 2 * curbSizeX) / 2;
        leftLaneX  = -roadWidth + roadWidth / 2;
        rightLaneX =  roadWidth - roadWidth / 2;

        SpawnObstaclePool();
        SpawnCollectiblePool();

        originalInterval = spawnInterval;
    }

    void FixedUpdate()
    {
        if (player.position.z > (nextSpawnPosition.z - (numberOfSegments - 2) * segmentLength))
        {
            SpawnSegment();
            RemoveOldSegment();
        }

        RepositionObstaclesBehindPlayer();
        RepositionCollectiblesBehindPlayer();

        distanceText.text = player.position.z.ToString("0");
    }

    private int CaculateProgressiveSpawnInterval()
    {
        float progress = player.position.z / 1000f;
        return (int)(originalInterval - progress);
    }


    private void SpawnObstaclePool()
    {
        float startZ = player.position.z + 50f;
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
        float currentZ = player.position.z + 30f;

        for (int i = 0; i < numberOfCollectibles; i++)
        {
            int randomIndex = Random.Range(0, collectibles.Length);
            GameObject collectible = Instantiate(collectibles[randomIndex]);
            activeCollectibles.Enqueue(collectible);

            Collectible collScript = collectible.GetComponent<Collectible>();
            
            if (collScript != null)
            {
                collScript.manager = this;
            }

            float laneX = (Random.Range(0, 2) == 0) ? leftLaneX : rightLaneX;
            laneX = FindSafeLaneX(laneX, currentZ);
            collectible.transform.position = new Vector3(laneX, collectibleYOffset, currentZ);
            currentZ += collectibleSpawnInterval;
        }
    }


    private void RepositionObstaclesBehindPlayer()
    {
        if (activeObstacles.Count == 0) return;

        GameObject oldestObstacle = activeObstacles.Peek();

        if (player.position.z > oldestObstacle.transform.position.z + 30f) 
        {
            spawnInterval = CaculateProgressiveSpawnInterval();
            activeObstacles.Dequeue();
            float newZ = GetFarthestObstacleZ() + spawnInterval; 

            float laneX = Random.Range(0, 2) == 0 ? leftLaneX : rightLaneX;

            oldestObstacle.transform.position = new Vector3(laneX, 0, newZ);

            activeObstacles.Enqueue(oldestObstacle);
        }
    }

    private void RepositionCollectiblesBehindPlayer()
    {
        if (activeCollectibles.Count == 0) return;

        GameObject oldestCollectible = activeCollectibles.Peek();

        if (!oldestCollectible.activeSelf || player.position.z > oldestCollectible.transform.position.z + 15f)
        {
            activeCollectibles.Dequeue();

            float newZ = GetFarthestCollectibleZ() + collectibleSpawnInterval;

            float laneX = Random.Range(0, 2) == 0 ? leftLaneX : rightLaneX;
            laneX = FindSafeLaneX(laneX, newZ);

            oldestCollectible.transform.position = new Vector3(laneX, collectibleYOffset, newZ);
            oldestCollectible.SetActive(true);
            activeCollectibles.Enqueue(oldestCollectible);

        }
    }

    private float GetFarthestCollectibleZ()
    {
        float maxZ = float.MinValue;
        foreach (var collectible in activeCollectibles)
        {
            float z = collectible.transform.position.z;
            if (z > maxZ) maxZ = z;
        }
        return maxZ;
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

    
    void SpawnSegment()
    {
        do
        {
            rand_ind = Random.Range(0, roadPrefabs.Length);
        } while (rand_ind == spawned_index);
        
        spawned_index = rand_ind;

        GameObject newSegment = Instantiate(roadPrefabs[rand_ind], nextSpawnPosition, Quaternion.identity);
        activeSegments.Enqueue(newSegment);
        nextSpawnPosition.z += segmentLength;

        // Randomize the rotation of the road segment either 0 or 180 degrees
        newSegment.transform.Rotate(Vector3.up, Random.Range(0, 2) * 180);
    }

    void RemoveOldSegment()
    {
        if (activeSegments.Count > numberOfSegments)
        {
            GameObject oldSegment = activeSegments.Dequeue();
            Vector3 oldSegmentPosition = oldSegment.transform.position;

            endGate.transform.position = new Vector3(0, 0, oldSegmentPosition.z + 10f);

            Destroy(oldSegment);
        }
    }

    public void GameOver()
    {
        Debug.Log("Game Over!");
    }
}
