using UnityEngine;
using System.Collections.Generic;

public class ObjectPooler : MonoBehaviour
{
    [Header("Shell Settings")]
    public GameObject shellPrefab;
    public int poolSize = 10;

    [Header("Spawn Area")]
    public Transform spawnAreaTop;
    public Transform spawnAreaBottom;
    public Transform playerTransform; // Reference to the player
    public float playerProximityRange = 5f;

    [Header("Spawn Timing")]
    public float[] spawnIntervals = { 0.5f, 1f, 1.5f, 2f }; // Add different intervals here

    private List<GameObject> shellPool = new List<GameObject>();
    private float timer;
    private float currentSpawnInterval;

    void Start()
    {
        // Populate the pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(shellPrefab);
            obj.SetActive(false);
            shellPool.Add(obj);
        }

        // Randomize the initial spawn interval
        currentSpawnInterval = GetRandomSpawnInterval();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= currentSpawnInterval)
        {
            SpawnShell();
            timer = 0f;
            currentSpawnInterval = GetRandomSpawnInterval(); // Get a new random interval
        }
    }

    void SpawnShell()
    {
        foreach (GameObject shell in shellPool)
        {
            if (!shell.activeInHierarchy)
            {
                // 🎯 Calculate spawn position near the player
                float playerX = playerTransform.position.x;
                float minX = Mathf.Max(spawnAreaTop.position.x, playerX - playerProximityRange);
                float maxX = Mathf.Min(spawnAreaBottom.position.x, playerX + playerProximityRange);

                float spawnX = Random.Range(playerX, maxX);

                Vector2 spawnPos = new Vector2(spawnX, spawnAreaTop.position.y);
                shell.transform.position = spawnPos;

                shell.SetActive(true);

                // Reactivate its Rigidbody2D in case it was deactivated
                Rigidbody2D rb = shell.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.simulated = true;

                return;
            }
        }
        Debug.LogWarning("No available shells in pool.");
    }

    float GetRandomSpawnInterval()
    {
        if (spawnIntervals.Length == 0)
        {
            Debug.LogWarning("Spawn intervals array is empty! Defaulting to 1 second.");
            return 1f;
        }

        int randomIndex = Random.Range(0, spawnIntervals.Length);
        return spawnIntervals[randomIndex];
    }
}
