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
    public float timeLimit = 60f;           // Segundos antes de forzar la siguiente ronda
    public float timeBetweenSpawns = 0.5f;  // Tiempo entre cada spawn dentro de la ronda
}

public class WaveManager : MonoBehaviour
{
    [Header("Waves")]
    [SerializeField] private List<Wave> waves;

    [Header("Spawn Area")]
    [SerializeField] private Transform spawnAreaCenter;     // Centro del área de spawn
    [SerializeField] private float spawnRadius = 10f;       // Radio del área de spawn
    [SerializeField] private float minSpawnDistance = 4f;   // Distancia mínima al player para no spawnear encima

    [Header("Wave Transition")]
    [SerializeField] private float timeBetweenWaves = 3f;   // Segundos entre rondas

    // Eventos para conectar con UI o animaciones si se necesita
    public event System.Action<int> OnWaveStarted;          // Índice de la ronda
    public event System.Action OnAllWavesCompleted;         // Todas las rondas completadas

    private int currentWaveIndex = 0;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isSpawning = false;
    private bool levelCompleted = false;
    private Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
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

        // Spawna todos los enemigos de la ronda con delay entre ellos
        foreach (EnemySpawnInfo spawnInfo in wave.enemies)
        {
            for (int i = 0; i < spawnInfo.count; i++)
            {
                SpawnEnemy(spawnInfo.enemyPrefab);
                yield return new WaitForSeconds(wave.timeBetweenSpawns);
            }
        }

        isSpawning = false;

        // Espera a que se limpie la ronda o al timer
        float timer = 0f;
        while (timer < wave.timeLimit)
        {
            timer += Time.deltaTime;

            // Limpia enemigos destruidos de la lista
            activeEnemies.RemoveAll(e => e == null);

            // Si no quedan enemigos vivos, pasa a la siguiente ronda antes del timer
            if (activeEnemies.Count == 0)
                break;

            yield return null;
        }

        // Transición a la siguiente ronda
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
            Debug.Log("¡Nivel completado!");
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

        // Intenta encontrar una posición válida alejada del player
        for (int i = 0; i < 10; i++)
        {
            Vector2 randomPos = center + Random.insideUnitCircle * spawnRadius;

            if (player == null) return randomPos;

            float distToPlayer = Vector2.Distance(randomPos, player.position);
            if (distToPlayer >= minSpawnDistance)
                return randomPos;
        }

        // Si no encuentra posición válida tras 10 intentos, devuelve una aleatoria
        return center + Random.insideUnitCircle * spawnRadius;
    }

    // Gizmos para ver el área de spawn en el editor
    private void OnDrawGizmosSelected()
    {
        if (spawnAreaCenter == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spawnAreaCenter.position, spawnRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnAreaCenter.position, minSpawnDistance);
    }
}