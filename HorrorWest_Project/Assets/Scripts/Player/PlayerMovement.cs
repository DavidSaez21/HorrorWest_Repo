using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Leg Settings")]
    [SerializeField] private float legFollowSpeed = 8f;
    [SerializeField] private float maxLegTorsoAngle = 90f;
    [SerializeField] private float flipSpeed = 20f;
    [SerializeField] private ParticleSystem particulas;

    [Header("References")]
    [SerializeField] private Transform legsTransform;

    // ── Estado interno ────────────────────────────────────────────────────────
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float currentLegAngle = 0f;
    private float targetLegAngle = 0f;
    private float speedMultiplier = 1f;
    private bool isFlipping = false;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // moveSpeed viene del Inspector; PlayerStats lo modifica vía SetAimSpeedMultiplier
        // En el paso 4 (PlayerManager) conectaremos GetMoveSpeed() de PlayerStats aquí
        rb.linearVelocity = moveInput.normalized * moveSpeed * speedMultiplier;
    }

    // ── Input callbacks ───────────────────────────────────────────────────────
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // ── API pública ───────────────────────────────────────────────────────────

    /// <summary>
    /// Llamado desde PlayerAiming para ralentizar al entrar en modo apuntado.
    /// </summary>
    public void SetAimSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    /// <summary>
    /// Llamado desde PlayerAiming cada frame para sincronizar la rotación de piernas.
    /// </summary>
    public void UpdateLegAngle(float torsoAngle)
    {
        // Si hay input de movimiento, las piernas apuntan hacia donde se mueve
        if (moveInput != Vector2.zero && !isFlipping)
            targetLegAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;

        // Si el torso gira demasiado respecto a las piernas, las piernas flipean
        float angleDiff = Mathf.DeltaAngle(currentLegAngle, torsoAngle);
        if (Mathf.Abs(angleDiff) > maxLegTorsoAngle && !isFlipping)
        {
            targetLegAngle = torsoAngle;
            isFlipping = true;
        }

        float speed = isFlipping ? flipSpeed : legFollowSpeed;
        currentLegAngle = Mathf.LerpAngle(currentLegAngle, targetLegAngle, speed * Time.deltaTime);

        if (isFlipping && Mathf.Abs(Mathf.DeltaAngle(currentLegAngle, targetLegAngle)) < 1f)
            isFlipping = false;

        if (legsTransform != null)
            legsTransform.rotation = Quaternion.AngleAxis(currentLegAngle - 90f, Vector3.forward);
    }

    #region Debug
    [Header("Debug")]
    [SerializeField] private bool debugDrawMovement = false;

    private void OnDrawGizmos()
    {
        if (!debugDrawMovement || !Application.isPlaying) return;

        // Dirección de movimiento actual
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, moveInput.normalized * 1.5f);

        // Dirección de las piernas
        if (legsTransform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, legsTransform.up * 1.2f);
        }
    }
    #endregion
}