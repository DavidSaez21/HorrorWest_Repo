using System.Collections;
using UnityEngine;

/// <summary>
/// Zona de veneno que deja el miniboss al cargar.
/// Ralentiza al player mientras está dentro y desaparece tras X segundos.
/// </summary>
public class PlagueTrail : MonoBehaviour
{
    [SerializeField] private float duration = 3f;
    [SerializeField] private float slowMultiplier = 0.5f;   // 0.5 = la mitad de velocidad
    [SerializeField] private float damagePerSecond = 5f;

    private bool _playerInside = false;

    private void Start()
    {
        StartCoroutine(DestroyAfterDuration());
    }

    private void Update()
    {
        if (_playerInside)
        {
            PlayerHealth ph = PlayerManager.Instance?.Health;
            ph?.TakeDamage(damagePerSecond * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = true;

        // Ralentiza al player
        PlayerManager.Instance?.Movement.SetAimSpeedMultiplier(slowMultiplier);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = false;

        // Restaura velocidad normal
        PlayerManager.Instance?.Movement.SetAimSpeedMultiplier(1f);
    }

    private IEnumerator DestroyAfterDuration()
    {
        yield return new WaitForSeconds(duration);

        // Si el player sigue dentro al destruirse, restaura velocidad
        if (_playerInside)
            PlayerManager.Instance?.Movement.SetAimSpeedMultiplier(1f);

        Destroy(gameObject);
    }
}