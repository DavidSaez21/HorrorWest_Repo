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

    [Header("Imán")]
    [SerializeField] private float magnetSpeed = 15f;

    private bool canPickup = false;
    private Collider2D col;
    private Transform player;
    private bool isBeingMagneted = false;

    private void Start()
    {
        col = GetComponent<Collider2D>();
        col.enabled = false;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        StartCoroutine(DropAnimation());
    }

    private void Update()
    {
        if (isBeingMagneted && player != null)
        {
            Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(direction * magnetSpeed * Time.deltaTime);
        }
    }

    private IEnumerator DropAnimation()
    {
        Vector3 startPos = transform.position;
        Vector2 randomDir = Random.insideUnitCircle.normalized * Random.Range(0.2f, 0.5f);
        Vector3 peakPos = startPos + new Vector3(randomDir.x, jumpHeight, 0f);
        Vector3 landPos = startPos + new Vector3(randomDir.x, 0f, 0f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / jumpDuration;
            transform.position = Vector3.Lerp(startPos, peakPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / landDuration;
            transform.position = Vector3.Lerp(peakPos, landPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        col.enabled = true;
        canPickup = true;
        CheckMagnet();
    }

    public void CheckMagnet()
    {
        if (PlayerManager.Instance?.Stats != null && PlayerManager.Instance.Stats.hasCoinMagnet)
        {
            isBeingMagneted = true;
            col.enabled = true;
            canPickup = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canPickup) return;
        if (!other.CompareTag("Player")) return;

        int baseValue = (int)coinValue;

        // Aplica bonus de botín permanente
        // Nivel 1 = +0.4, Nivel 2 = +0.8, ... Nivel 5 = +2.0
        // Moneda de 1 con nivel 5 da 3 (1 + 2)
        float lootBonus = PlayerManager.Instance?.Stats?.GetLootBonus() ?? 0f;
        int finalValue = Mathf.RoundToInt(baseValue + lootBonus);
        finalValue = Mathf.Max(baseValue, finalValue); // nunca menos que el valor base

        CurrencyManager.Instance?.AddCoins(finalValue);
        Destroy(gameObject);
    }
}