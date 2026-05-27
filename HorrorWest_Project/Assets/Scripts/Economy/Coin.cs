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
        // Si el imán está activo vuela hacia el player
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

        // Si el player ya tiene imán activo al aterrizar, activa el modo imán
        CheckMagnet();
    }

    public void CheckMagnet()
    {
        if (PlayerManager.Instance.Stats != null && PlayerManager.Instance.Stats.hasCoinMagnet)
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

        CurrencyManager.Instance.AddCoins((int)coinValue);
        Destroy(gameObject);
    }
}