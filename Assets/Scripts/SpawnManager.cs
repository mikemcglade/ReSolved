using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] enemyPrefabs; 
    private float spawnRangeX = 16;
    private float spawnPosZ = 20;
    private float startDelay = 2;
    private float spawnInterval = 2.5f;


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

            Vector3 spawnPos = new Vector3(Random.Range(-spawnRangeX, spawnRangeX), 1.1f, spawnPosZ);
            int enemyIndex = Random.Range(0, enemyPrefabs.Length);
            Instantiate(enemyPrefabs[enemyIndex], spawnPos, enemyPrefabs[enemyIndex].transform.rotation);

    }
}
