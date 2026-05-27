using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// AimCamera — zoom suave al apuntar y offset hacia el cursor.
//
// ANTES: usaba Input.mousePosition (sistema viejo) mezclado con el resto
//        del proyecto que usa el Input System nuevo.
// AHORA: lee la posición del ratón desde la cámara directamente igual
//        que PlayerAiming — consistente con el resto del proyecto.
// ─────────────────────────────────────────────────────────────────────────────
public class AimCamera : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float normalSize = 5f;
    [SerializeField] private float aimSize = 3.5f;
    [SerializeField] private float zoomSpeed = 5f;

    [Header("Aim Offset Settings")]
    [SerializeField] private float aimOffsetStrength = 0.3f;
    [SerializeField] private float offsetSpeed = 5f;

    [Header("References")]
    [SerializeField] private Transform target;

    // ── Estado interno ────────────────────────────────────────────────────────
    private Camera cam;
    private float targetSize;
    private Vector3 targetPosition;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        cam = GetComponent<Camera>();
        targetSize = normalSize;
        cam.orthographicSize = normalSize;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        bool isAiming = PlayerAiming.Instance != null && PlayerAiming.Instance.IsAiming;

        // ── Zoom ──────────────────────────────────────────────────────────────
        targetSize = isAiming ? aimSize : normalSize;
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, zoomSpeed * Time.deltaTime);

        // ── Posición ──────────────────────────────────────────────────────────
        if (isAiming)
        {
            // Usa la dirección de apuntado de PlayerAiming en lugar de Input.mousePosition
            Vector2 aimDir = PlayerAiming.Instance.GetAimDirection();
            Vector3 aimOffset = new Vector3(aimDir.x, aimDir.y, 0f) * (cam.orthographicSize * aimOffsetStrength);
            targetPosition = new Vector3(
                target.position.x + aimOffset.x,
                target.position.y + aimOffset.y,
                transform.position.z
            );
        }
        else
        {
            targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, offsetSpeed * Time.deltaTime);
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