using System.Collections;
using UnityEngine;

public class PlagueEffect : MonoBehaviour
{
    private float _damagePerSecond;
    private float _duration;
    private GameObject _vfxInstance;

    [SerializeField] private GameObject plagueVFXPrefab;

    public void Init(float dps, float duration, GameObject vfxPrefab = null, Transform spawnPoint = null)
    {
        _damagePerSecond = dps;
        _duration = duration;

        if (vfxPrefab != null)
            plagueVFXPrefab = vfxPrefab;

        Transform anchor = spawnPoint != null ? spawnPoint : transform;

        if (plagueVFXPrefab != null)
            _vfxInstance = Instantiate(plagueVFXPrefab, anchor.position, Quaternion.identity, anchor);

        StartCoroutine(PlagueRoutine());
    }

    private IEnumerator PlagueRoutine()
    {
        float elapsed = 0f;
        EnemyBase enemy = GetComponent<EnemyBase>();

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            enemy?.TakeDamageSilent(_damagePerSecond * Time.deltaTime);
            yield return null;
        }

        if (_vfxInstance != null)
            Destroy(_vfxInstance);

        Destroy(this);
    }
}