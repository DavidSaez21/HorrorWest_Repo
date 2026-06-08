using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnInfo
{
    public GameObject enemyPrefab;
    public int count;
}

[System.Serializable]
public class Wave
{
    public string waveName = "Wave";
    public List<EnemySpawnInfo> enemies;
    public float timeLimit = 60f;
    public float timeBetweenSpawns = 0.5f;
}

public class WaveManager : MonoBehaviour
{
    [Header("Waves")]
    [SerializeField] private List<Wave> waves;

    [Header("Spawn Area")]
    [SerializeField] private Transform spawnAreaCenter;
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private float minSpawnDistance = 4f;

    [Header("Wave Transition")]
    [SerializeField] private float timeBetweenWaves = 3f;

    [Header("Auto Start")]
    [SerializeField] private bool autoStart = true;   // false para el bar/cárcel

    public event System.Action<int> OnWaveStarted;
    public event System.Action OnAllWavesCompleted;

    private int currentWaveIndex = 0;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isSpawning = false;
    private bool levelCompleted = false;
    private Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (autoStart)
            StartWaves();
    }

    public void ResetWaves()
    {
        StopAllCoroutines();
        currentWaveIndex = 0;
        activeEnemies.Clear();
        isSpawning = false;
        levelCompleted = false;
    }

    // Llamar desde BarEntrance/JailEntrance al entrar
    public void StartWaves()
    {
        if (levelCompleted || isSpawning) return;
        StartCoroutine(StartWave(currentWaveIndex));
    }

    private IEnumerator StartWave(int index)
    {
        if (index >= waves.Count)
        {
            levelCompleted = true;
            OnAllWavesCompleted?.Invoke();
            yield break;
        }

        Wave wave = waves[index];
        OnWaveStarted?.Invoke(index);
        isSpawning = true;

        foreach (EnemySpawnInfo spawnInfo in wave.enemies)
        {
            for (int i = 0; i < spawnInfo.count; i++)
            {
                SpawnEnemy(spawnInfo.enemyPrefab);
                yield return new WaitForSeconds(wave.timeBetweenSpawns);
            }
        }

        isSpawning = false;

        float timer = 0f;
        while (timer < wave.timeLimit)
        {
            timer += Time.deltaTime;
            activeEnemies.RemoveAll(e => e == null);
            if (activeEnemies.Count == 0) break;
            yield return null;
        }

        currentWaveIndex++;

        if (currentWaveIndex < waves.Count)
        {
            yield return new WaitForSeconds(timeBetweenWaves);
            StartCoroutine(StartWave(currentWaveIndex));
        }
        else
        {
            levelCompleted = true;
            OnAllWavesCompleted?.Invoke();
        }
    }

    private void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null) return;
        Vector2 spawnPos = GetValidSpawnPosition();
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        activeEnemies.Add(enemy);
    }

    private Vector2 GetValidSpawnPosition()
    {
        Vector2 center = spawnAreaCenter != null ? (Vector2)spawnAreaCenter.position : Vector2.zero;
        for (int i = 0; i < 10; i++)
        {
            Vector2 randomPos = center + Random.insideUnitCircle * spawnRadius;
            if (player == null) return randomPos;
            if (Vector2.Distance(randomPos, player.position) >= minSpawnDistance)
                return randomPos;
        }
        return center + Random.insideUnitCircle * spawnRadius;
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnAreaCenter == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spawnAreaCenter.position, spawnRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnAreaCenter.position, minSpawnDistance);
    }
}