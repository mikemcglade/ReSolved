using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public LayerMask wallLayer; // Assign your wall layer in Inspector
    
    private float spawnRangeX = 16;
    private float spawnPosZ = 20;
    private float startDelay = 2;
    private float spawnInterval = 2.5f;
    private int maxSpawnAttempts = 10; // Safety limit to prevent infinite loops


    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("spawnRandomEnemy", startDelay, spawnInterval);
    }

    // Update is called once per frame
    void Update()
    {
    
    }
    void spawnRandomEnemy()
    {
        Vector3 spawnPos = Vector3.zero;
        bool validPosition = false;
        int attempts = 0;

        // Try to find a valid spawn position (not inside a wall)
        while (!validPosition && attempts < maxSpawnAttempts)
        {
            spawnPos = new Vector3(Random.Range(-spawnRangeX, spawnRangeX), 1.1f, spawnPosZ);
            
            // Check if this position overlaps with a wall
            if (!Physics.CheckSphere(spawnPos, 0.5f, wallLayer))
            {
                validPosition = true;
            }
            
            attempts++;
        }

        // Only spawn if we found a valid position
        if (validPosition)
        {
            int enemyIndex = Random.Range(0, enemyPrefabs.Length);
            Instantiate(enemyPrefabs[enemyIndex], spawnPos, enemyPrefabs[enemyIndex].transform.rotation);
        }
        else
        {
            Debug.LogWarning("Could not find valid spawn position after " + maxSpawnAttempts + " attempts");
        }
    }
}
