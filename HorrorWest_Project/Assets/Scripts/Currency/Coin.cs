using System.Collections;
using UnityEngine;

public enum CoinValue { One = 1, Five = 5, Ten = 10 }

public class Coin : MonoBehaviour
{
    [SerializeField] private CoinValue coinValue = CoinValue.One;

    [Header("Drop Animation")]
    [SerializeField] private float jumpHeight = 0.5f;
    [SerializeField] private float jumpDuration = 0.3f;
    [SerializeField] private float landDuration = 0.15f;

    private bool canPickup = false;
    private Collider2D col;

    private void Start()
    {
        col = GetComponent<Collider2D>();
        col.enabled = false;    // No se puede recoger mientras salta
        StartCoroutine(DropAnimation());
    }

    private IEnumerator DropAnimation()
    {
        Vector3 startPos = transform.position;

        // Dirección aleatoria del salto
        Vector2 randomDir = Random.insideUnitCircle.normalized * Random.Range(0.2f, 0.5f);
        Vector3 peakPos = startPos + new Vector3(randomDir.x, jumpHeight, 0f);
        Vector3 landPos = startPos + new Vector3(randomDir.x, 0f, 0f);

        // Sube hasta el pico
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / jumpDuration;
            transform.position = Vector3.Lerp(startPos, peakPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        // Baja hasta el suelo
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / landDuration;
            transform.position = Vector3.Lerp(peakPos, landPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        // Ya en el suelo, se puede recoger
        col.enabled = true;
        canPickup = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canPickup) return;
        if (!other.CompareTag("Player")) return;

        CurrencyManager.Instance.AddCoins((int)coinValue);
        Destroy(gameObject);
    }
}