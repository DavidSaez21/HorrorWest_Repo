using System.Collections;
using UnityEngine;

/// <summary>
/// Se añade dinámicamente al enemigo al impactarle una bala con peste.
/// Aplica daño progresivo durante X segundos y luego se destruye.
/// </summary>
public class PlagueEffect : MonoBehaviour
{
    private float damagePerSecond;
    private float duration;
    private float tickInterval = 0.5f;  // Daño cada 0.5 segundos

    public void Init(float damagePerSecond, float duration)
    {
        this.damagePerSecond = damagePerSecond;
        this.duration = duration;
        StartCoroutine(PlagueCoroutine());
    }

    private IEnumerator PlagueCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;

            if (TryGetComponent(out IDamageable target))
                target.TakeDamage(damagePerSecond * tickInterval);
        }

        Destroy(this);
    }
}