using UnityEngine;

public class CameraConfiner : MonoBehaviour
{
    [Header("Límites de la cámara")]
    [SerializeField] private bool isActive = false;
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (!isActive) return;

        

        float camHeight = _cam.orthographicSize;
        float camWidth = _cam.orthographicSize * _cam.aspect;

        float clampedX = Mathf.Clamp(transform.position.x, minBounds.x + camWidth, maxBounds.x - camWidth);
        float clampedY = Mathf.Clamp(transform.position.y, minBounds.y + camHeight, maxBounds.y - camHeight);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }

    public void SetBounds(Vector2 min, Vector2 max)
    {
        minBounds = min;
        maxBounds = max;
        isActive = true;
    }

    public void ClearBounds()
    {
        isActive = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (!isActive) return;
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2f, (minBounds.y + maxBounds.y) / 2f, 0f);
        Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}