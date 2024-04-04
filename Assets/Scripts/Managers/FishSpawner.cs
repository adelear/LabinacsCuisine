using System.Collections;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public GameObject[] fishPrefabs;
    private float initialSpawnIntervalMin = 5f;
    private float initialSpawnIntervalMax = 15f;
    private float additionalSpawnIntervalMin = 1f;
    private float additionalSpawnIntervalMax = 8f;
    private int maxFishNum = 40;
    public int currentFishNum = 0;
    private float spawnTimer = 0f;
    private float totalTimeElapsed = 0f;
    private float currentSpawnInterval;

    private float timeSinceLastAdjustment = 0f;
    private const float timeBetweenAdjustments = 120f; 

    float minXBound = -14.2f; 
    float maxXBound = 7.2f;
    float minZBound = -0.5f;
    float maxZBound = 13f;

    public static FishSpawner Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Another instance of FishSpawner already exists. Destroying this one.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentSpawnInterval = Random.Range(initialSpawnIntervalMin, initialSpawnIntervalMax);
    }

    private void Update()
    {
        // if (GameManager.Instance.GetGameState() != GameManager.GameState.GAME) return; 
        totalTimeElapsed += Time.deltaTime;

        if (currentFishNum < maxFishNum)
        {
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= currentSpawnInterval)
            {
                SpawnFish();
                spawnTimer = 0f;
            }

            if (totalTimeElapsed >= 240f && currentSpawnInterval != additionalSpawnIntervalMax)
            {
                AdjustSpawnInterval();
            }
        }

        // Format the total time elapsed into minutes, seconds, and milliseconds
        int minutes = (int)(totalTimeElapsed / 60);
        int seconds = (int)(totalTimeElapsed % 60);
        int milliseconds = (int)((totalTimeElapsed - Mathf.Floor(totalTimeElapsed)) * 1000); 
        string formattedTime = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);

        //Debug.Log(formattedTime);
    }


    private void SpawnFish()
    {
        float[] cumulativeWeights = new float[fishPrefabs.Length];
        float totalWeight = 0f;
        for (int i = 0; i < fishPrefabs.Length; i++)
        {
            if (i == 0)
                totalWeight += 0.6f;
            else if (i == 1)
                totalWeight += 0.3f;
            else
                totalWeight += 0.1f;

            cumulativeWeights[i] = totalWeight;
        }

        float randomValue = Random.Range(0f, totalWeight);
        GameObject selectedPrefab = null;
        for (int i = 0; i < fishPrefabs.Length; i++)
        {
            if (randomValue <= cumulativeWeights[i])
            {
                selectedPrefab = fishPrefabs[i];
                break;
            }
        }

        if (selectedPrefab != null)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            Collider[] colliders = Physics.OverlapSphere(spawnPosition, 1f);
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Table"))
                {
                    spawnPosition = GetRandomSpawnPosition();
                    break;
                }
            }

            Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
            currentFishNum++;
        }
    }


    private void AdjustSpawnInterval()
    {
        if (totalTimeElapsed >= 240f && currentSpawnInterval != additionalSpawnIntervalMax)
        {
            currentSpawnInterval = Random.Range(additionalSpawnIntervalMin, additionalSpawnIntervalMax);
            Debug.Log("Adjusted spawn interval after 4 minutes");
        }

        if (totalTimeElapsed >= 480f && currentSpawnInterval != 5f)
        {
            currentSpawnInterval = Random.Range(1f, 5f);
            Debug.Log("Adjusted spawn interval to 1-5 after 8 minutes");
        }

        timeSinceLastAdjustment += Time.deltaTime;
        if (timeSinceLastAdjustment >= timeBetweenAdjustments)
        {
            additionalSpawnIntervalMax -= 3f;
            Debug.Log("Decreased max spawn interval by 2");
            timeSinceLastAdjustment = 0f;
        }

        if (additionalSpawnIntervalMax < additionalSpawnIntervalMin)
        {
            additionalSpawnIntervalMax = additionalSpawnIntervalMin;
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float randomX = Random.Range(minXBound, maxXBound);
        float randomZ = Random.Range(minZBound, maxZBound);

        return new Vector3(randomX, 0, randomZ);
    }
}