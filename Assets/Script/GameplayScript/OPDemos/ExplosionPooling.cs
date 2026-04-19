using UnityEngine;
using System.Collections;

public class PooledExplosionSpawner : MonoBehaviour
{
    [Header("Pooling")]
    public SimpleObjectPool pool;

    [Header("Spawn Timing")]
    public float spawnInterval = 1.5f;

    [Header("Spawn Area (use transforms)")]
    public Transform spawnAreaTopLeft;
    public Transform spawnAreaBottomRight;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            Vector2 spawnPos = GetRandomPosition();
            GameObject obj = pool.GetObject();
            obj.transform.position = spawnPos;

            StartCoroutine(ReturnAfter(obj, 0.2f));
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    Vector2 GetRandomPosition()
    {
        if (spawnAreaTopLeft == null || spawnAreaBottomRight == null)
        {
            Debug.LogWarning("Spawn area transforms not assigned!");
            return transform.position;
        }

        float minX = spawnAreaTopLeft.position.x;
        float maxX = spawnAreaBottomRight.position.x;
        float maxY = spawnAreaTopLeft.position.y;
        float minY = spawnAreaBottomRight.position.y;

        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);
        return new Vector2(x, y);
    }

    IEnumerator ReturnAfter(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        pool.ReturnObject(obj);
    }
}
