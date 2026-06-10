using UnityEngine;

public class AimCamera : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float normalSize = 5f;
    [SerializeField] private float aimSize = 3.5f;
    [SerializeField] private float zoomSpeed = 5f;

    [Header("Aim Offset Settings")]
    [SerializeField] private float aimOffsetStrength = 0.3f;
    [SerializeField] private float offsetSpeed = 5f;

    [Header("Follow Settings")]
    [SerializeField] private float smoothTime = 0.05f;

    [Header("References (opcional — se autoasignan)")]
    [SerializeField] private Transform target;
    [SerializeField] private Rigidbody2D targetRb;

    private Camera cam;
    private float targetSize;
    private Vector3 targetPosition;
    private Vector3 camVelocity = Vector3.zero;
    private Vector3 currentAimOffset = Vector3.zero;
    private bool isAiming = false;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        targetSize = normalSize;
        cam.orthographicSize = normalSize;
    }

    private void Start()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        if (target != null && targetRb != null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (target == null) target = player.transform;
            if (targetRb == null) targetRb = player.GetComponent<Rigidbody2D>();
        }
    }

    private void FixedUpdate()
    {
        // Reintenta encontrar al player si no está asignado
        if (target == null) { FindPlayer(); return; }

        isAiming = PlayerAiming.Instance != null && PlayerAiming.Instance.IsAiming;

        targetSize = isAiming ? aimSize : normalSize;
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, zoomSpeed * Time.fixedDeltaTime);

        Vector2 playerPos = targetRb != null ? targetRb.position : (Vector2)target.position;

        if (isAiming)
        {
            Vector2 aimDir = PlayerAiming.Instance.GetAimDirection();
            Vector3 targetAimOffset = new Vector3(aimDir.x, aimDir.y, 0f) * (cam.orthographicSize * aimOffsetStrength);
            currentAimOffset = Vector3.Lerp(currentAimOffset, targetAimOffset, offsetSpeed * Time.fixedDeltaTime);
        }
        else
        {
            currentAimOffset = Vector3.Lerp(currentAimOffset, Vector3.zero, offsetSpeed * Time.fixedDeltaTime);
        }

        targetPosition = new Vector3(
            playerPos.x + currentAimOffset.x,
            playerPos.y + currentAimOffset.y,
            transform.position.z
        );

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref camVelocity,
            smoothTime,
            Mathf.Infinity,
            Time.fixedDeltaTime
        );
    }

    #region Debug
    [Header("Debug")]
    [SerializeField] private bool debugDrawTarget = false;

    private void OnDrawGizmos()
    {
        if (!debugDrawTarget || target == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, target.position);
        Gizmos.DrawWireSphere(target.position, 0.2f);
    }
    #endregion
}