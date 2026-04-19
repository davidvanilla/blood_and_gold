
using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{

    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private GameObject shooterPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int spawnCount = 10;
    [SerializeField] private float restTime = 5f;
    private int aliveZombieCount;
    private bool isResting;

    private void OnEnable()
    {
        GameEvents.OnZombieDied += HandleZombieDead;
    }

    void Start()
    {
        aliveZombieCount = spawnCount;
        SpawnZombies();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void HandleZombieDead()
    {
        aliveZombieCount--;
        Debug.Log($"zombie dead {aliveZombieCount} remaining");
        if (aliveZombieCount <= 0 && !isResting)
        {
            StartCoroutine(RestThenSpawn());
        }
    }
    private IEnumerator  RestThenSpawn()
    {
        isResting = true;
        Debug.Log($"Wave cleared! Resting for {restTime} seconds...");
        yield return new WaitForSeconds(restTime);
        SpawnZombies();
        isResting = false;
    }
    private void SpawnZombies()
    {
        Debug.Log($"spawning zombie -- {spawnCount}");
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
            var enemyObject = Instantiate(zombiePrefab, spawn.position, spawn.rotation);
            enemyObject.SetActive(true);

        }
        for (int i = 0; i < spawnCount; i++)
        {
            Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
            var enemyObject = Instantiate(shooterPrefab, spawn.position, spawn.rotation);
            enemyObject.SetActive(true);

        }
        
        aliveZombieCount = spawnCount*2;
    }
}
