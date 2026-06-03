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
    [SerializeField] private Animator legsAnimator;

    // ── Estado interno ────────────────────────────────────────────────────────
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float currentLegAngle = 0f;
    private float targetLegAngle = 0f;
    private float speedMultiplier = 1f;
    private bool isFlipping = false;
    private bool isMoving = false;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput.normalized * moveSpeed * speedMultiplier;

        bool moving = moveInput != Vector2.zero;

        // Animación
        if (legsAnimator != null)
            legsAnimator.SetBool("isWalking", moving);

        // Partículas
        if (particulas != null)
        {
            if (moving && !isMoving) { particulas.Play(); isMoving = true; }
            if (!moving && isMoving) { particulas.Stop(); isMoving = false; }
        }
    }

    // ── Input callbacks ───────────────────────────────────────────────────────
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // ── API pública ───────────────────────────────────────────────────────────

    public void SetAimSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    public void UpdateLegAngle(float torsoAngle)
    {
        if (moveInput != Vector2.zero && !isFlipping)
        {
            float moveAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
            float diff = Mathf.DeltaAngle(currentLegAngle, moveAngle);

            // Solo actualiza el target si el movimiento no contradice demasiado las piernas
            if (Mathf.Abs(diff) < maxLegTorsoAngle)
                targetLegAngle = moveAngle;
        }

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

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, moveInput.normalized * 1.5f);

        if (legsTransform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, legsTransform.up * 1.2f);
        }
    }
    #endregion
}