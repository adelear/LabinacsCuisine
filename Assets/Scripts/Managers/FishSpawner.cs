using System.Collections;
using TMPro; 
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public GameObject[] fishPrefabs;
    public GameObject[] endingDialogues; // 1 to 5, 0-1 is Worse, 1-2 is Bad, 2-3 is Average, 3-4 is Good, 4-5 is Best
    private float initialSpawnIntervalMin = 5f;
    private float initialSpawnIntervalMax = 15f;
    private float additionalSpawnIntervalMin = 1f;
    private float additionalSpawnIntervalMax = 8f;
    private int maxFishNum = 40;
    public int currentFishNum = 0;
    private float spawnTimer = 0f;
    private float totalTimeElapsed = 0f;
    private float currentSpawnInterval;
    private float timeToStopSpawning = 300f; 
    private float gameTime = 390f; 

    private float timeSinceLastAdjustment = 0f;
    private const float timeBetweenAdjustments = 120f; 

    float minXBound = -12.2f; 
    float maxXBound = 7.2f;
    float minZBound = 5f;
    float maxZBound = 10f;

    bool anyFishCanBeCooked;

    [SerializeField] TMP_Text timerText; 

    public AudioClip[] gameEndJingles; // 0-4 worst to best

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
        anyFishCanBeCooked = true; 
    }

    private void Update()
    {
        if (GameManager.Instance.GetGameState() != GameManager.GameState.GAME) return;
        if (totalTimeElapsed <= gameTime)
        {
            totalTimeElapsed += Time.deltaTime;

            if (currentFishNum < maxFishNum)
            {
                spawnTimer += Time.deltaTime;

                if (spawnTimer >= currentSpawnInterval && totalTimeElapsed < timeToStopSpawning)
                {
                    SpawnFish();
                    spawnTimer = 0f;
                }

                if (totalTimeElapsed >= 120 && currentSpawnInterval != additionalSpawnIntervalMax && totalTimeElapsed < timeToStopSpawning)
                {
                    AdjustSpawnInterval();
                }  
            }

            

            float remainingTime = Mathf.Max(0f, gameTime - totalTimeElapsed);

            int hours = Mathf.FloorToInt(remainingTime / 3600);
            int minutes = Mathf.FloorToInt((remainingTime % 3600) / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);

            timerText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);

            //Debug.Log(formattedTime); 
            if (totalTimeElapsed > timeToStopSpawning)
            {
                bool anyFishCanBeCooked = false; // Flag to track if any fish can be cooked

                GameObject[] fishes = GameObject.FindGameObjectsWithTag("Fish");
                foreach (GameObject fish in fishes)
                {
                    Fish fishScript = fish.GetComponent<Fish>();
                    if (fishScript.canCook)
                    {
                        anyFishCanBeCooked = true;
                        break; 
                    }
                }
                if (!anyFishCanBeCooked)
                {
                    GameEnd();
                }
            }
        }
        if (totalTimeElapsed >= gameTime)
        {
            GameEnd();
        }
    }

    private void GameEnd()
    {
        AudioSource levelMusic = GameObject.Find("LevelMusic").GetComponent<AudioSource>();
        levelMusic.Stop();
        GameObject[] fishes = GameObject.FindGameObjectsWithTag("Fish");
        foreach (GameObject fish in fishes) Destroy(fish);

        float averageRating = GameManager.Instance.AverageRating;

        if (averageRating >= 0f && averageRating < 1f)
        {
            endingDialogues[0].SetActive(true);
            AudioManager.Instance.PlayOneShot(gameEndJingles[0], false);
        }
        else if (averageRating >= 1f && averageRating < 2f)
        {
            endingDialogues[1].SetActive(true);
            AudioManager.Instance.PlayOneShot(gameEndJingles[1], false);
        }
        else if (averageRating >= 2f && averageRating < 3f)
        {
            endingDialogues[2].SetActive(true);
            AudioManager.Instance.PlayOneShot(gameEndJingles[2], false);
        }
        else if (averageRating >= 3f && averageRating <= 4f)
        {
            endingDialogues[3].SetActive(true);
            AudioManager.Instance.PlayOneShot(gameEndJingles[3], false);
        }
        else if (averageRating >= 4f && averageRating <= 5f)
        {
            endingDialogues[4].SetActive(true);
            AudioManager.Instance.PlayOneShot(gameEndJingles[4], false);
        }
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
