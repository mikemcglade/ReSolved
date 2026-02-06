using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DripSpawner : MonoBehaviour
{
    public GameObject dripPrefab;
    public float spawnInterval = 5f;
    public float spawnRangeX = 10f;
    public float spawnRangeZ = 10f;

    private void Start()
    {
        StartCoroutine(SpawnDripsRoutine());
    }

    private IEnumerator SpawnDripsRoutine()
    {
        while (true)
        {
            SpawnDrip();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnDrip()
   {
    // Generate random offsets within the specified range
    float randomX = Random.Range(-spawnRangeX, spawnRangeX);
    float randomZ = Random.Range(-spawnRangeZ, spawnRangeZ);

    // Calculate the spawn position relative to THIS spawner's position
    Vector3 spawnPosition = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

    // Instantiate the drip prefab at the calculated position
    Instantiate(dripPrefab, spawnPosition, Quaternion.identity);
}
}
