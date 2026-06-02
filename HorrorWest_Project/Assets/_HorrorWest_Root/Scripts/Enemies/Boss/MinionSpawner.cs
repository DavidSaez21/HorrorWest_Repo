using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns enemy minions at the south entrance of La Iglesia.
/// Reuses existing enemy prefabs — zero extra enemy code needed.
///
/// Setup:
///   • Assign enemyPrefabs (drag your existing enemy prefabs here).
///   • Assign spawnPoint (an empty Transform at the south door).
///   • maxMinionsPerSpawn and spawnStagger are configurable from the Inspector
///     or overridden by ChurchBoss via the public fields.
/// </summary>
public class MinionSpawner : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────

    [Header("References")]
    [Tooltip("Array of enemy prefabs from previous levels. One is chosen at random per spawn slot.")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [Tooltip("Empty Transform at the south entrance where minions appear.")]
    [SerializeField] private Transform spawnPoint;

    [Header("Spawn Config")]
    [Tooltip("How many minions to spawn per invocation (configurable without recompile).")]
    [SerializeField] private int maxMinionsPerSpawn = 2;
    [Tooltip("Seconds between each individual minion appearing (staggered entry).")]
    [SerializeField] private float spawnStagger = 0.3f;

    [Header("Game Feel")]
    [Tooltip("Optional VFX prefab instantiated at the spawn point when a minion appears.")]
    [SerializeField] private GameObject spawnFxPrefab;

    // Set by ChurchBoss from ChurchBossData (overrides Inspector values if assigned)
    [HideInInspector] public float staggerOverride = -1f; // -1 = use Inspector value

    // ── Runtime ───────────────────────────────────────────────────────────────

    private readonly List<GameObject> _livingMinions = new();
    private Action _onComplete;
    private Coroutine _spawnCoroutine;

    // ── Public API ────────────────────────────────────────────────────────────

    public void Execute(Action onComplete)
    {
        _onComplete = onComplete;
        if (_spawnCoroutine != null) StopCoroutine(_spawnCoroutine);
        _spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    /// <summary>Returns true if any spawned minion is still alive.</summary>
    public bool HasLivingMinions()
    {
        // Remove destroyed entries first
        _livingMinions.RemoveAll(go => go == null);
        return _livingMinions.Count > 0;
    }

    // ── Coroutine ─────────────────────────────────────────────────────────────

    private IEnumerator SpawnRoutine()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0 || spawnPoint == null)
        {
            Debug.LogWarning("[MinionSpawner] No enemy prefabs or spawn point assigned.");
            _onComplete?.Invoke();
            yield break;
        }

        float stagger = staggerOverride > 0f ? staggerOverride : spawnStagger;

        for (int i = 0; i < maxMinionsPerSpawn; i++)
        {
            SpawnOne();
            if (i < maxMinionsPerSpawn - 1)
                yield return new WaitForSeconds(stagger);
        }

        // Notify the action queue that the spawn wave is "done"
        // (minions persist; the queue uses HasLivingMinions to suppress re-spawns)
        _onComplete?.Invoke();
        _spawnCoroutine = null;
    }

    private void SpawnOne()
    {
        // Pick a random prefab
        int idx = UnityEngine.Random.Range(0, enemyPrefabs.Length);
        GameObject prefab = enemyPrefabs[idx];
        if (prefab == null) return;

        // Slight random offset so minions don't stack exactly
        Vector3 offset = new Vector3(
            UnityEngine.Random.Range(-0.4f, 0.4f),
            UnityEngine.Random.Range(-0.4f, 0.4f),
            0f);

        GameObject minion = Instantiate(prefab, spawnPoint.position + offset, Quaternion.identity);
        _livingMinions.Add(minion);

        // Optional spawn VFX
        if (spawnFxPrefab != null)
            Instantiate(spawnFxPrefab, spawnPoint.position + offset, Quaternion.identity);
    }
}
