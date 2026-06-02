using System.Collections;
using UnityEngine;

public class PlagueTrail : MonoBehaviour
{
    [SerializeField] private float duration = 3f;
    [SerializeField] private float slowMultiplier = 0.5f;
    [SerializeField] private float damagePerSecond = 5f;
    [SerializeField] private float slowDuration = 3f;   // El slow dura X segundos tras salir

    private void Start()
    {
        StartCoroutine(DestroyAfterDuration());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        StopAllCoroutines();
        StartCoroutine(DamageAndSlowRoutine(other));
        StartCoroutine(DestroyAfterDuration());
    }

    private IEnumerator DamageAndSlowRoutine(Collider2D playerCol)
    {
        // Aplica slow
        PlayerManager.Instance?.Movement.SetAimSpeedMultiplier(slowMultiplier);

        float elapsed = 0f;

        // Hace daño mientras el player esté dentro o hasta que se destruya
        while (playerCol != null && elapsed < duration)
        {
            // Comprueba si el player sigue dentro
            Collider2D overlap = Physics2D.OverlapCircle(
                transform.position,
                GetComponent<CircleCollider2D>()?.radius ?? 0.5f,
                LayerMask.GetMask("Player")
            );

            if (overlap != null)
                PlayerManager.Instance?.Health.TakeDamage(damagePerSecond * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Mantiene el slow X segundos después de salir
        yield return new WaitForSeconds(slowDuration);
        PlayerManager.Instance?.Movement.SetAimSpeedMultiplier(1f);
    }

    private IEnumerator DestroyAfterDuration()
    {
        yield return new WaitForSeconds(duration);
        PlayerManager.Instance?.Movement.SetAimSpeedMultiplier(1f);
        Destroy(gameObject);
    }
}