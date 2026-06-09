using System.Collections;
using UnityEngine;

public class PlagueEffect : MonoBehaviour
{
    private float _damagePerSecond;
    private float _duration;
    private GameObject _vfxInstance;

    [SerializeField] private GameObject plagueVFXPrefab;

    public void Init(float dps, float duration, GameObject vfxPrefab = null)
    {
        _damagePerSecond = dps;
        _duration = duration;

        if (vfxPrefab != null)
            plagueVFXPrefab = vfxPrefab;

        // Instancia el VFX encima del enemigo
        if (plagueVFXPrefab != null)
            _vfxInstance = Instantiate(plagueVFXPrefab, transform.position, Quaternion.identity, transform);

        StartCoroutine(PlagueRoutine());
    }

    private IEnumerator PlagueRoutine()
    {
        float elapsed = 0f;
        IDamageable target = GetComponent<IDamageable>();

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            target?.TakeDamage(_damagePerSecond * Time.deltaTime);
            yield return null;
        }

        // Destruye el VFX al terminar
        if (_vfxInstance != null)
            Destroy(_vfxInstance);

        Destroy(this);
    }
}