using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Leg Settings")]
    [SerializeField] private float legFollowSpeed = 8f;
    [SerializeField] private Transform legsTransform;
    [SerializeField] private Transform hatTransform;
    [SerializeField] private Animator legsAnimator;

    [Header("Particles")]
    [SerializeField] private ParticleSystem particulas;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float speedMultiplier = 1f;
    private bool isMoving = false;
    private float currentLegAngle = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 newPosition = rb.position + moveInput.normalized * moveSpeed * speedMultiplier * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        bool moving = moveInput != Vector2.zero;

        if (legsAnimator != null)
            legsAnimator.SetBool("isWalking", moving);
    }

    private void LateUpdate()
    {
        float targetAngle = transform.eulerAngles.z;
        currentLegAngle = Mathf.LerpAngle(currentLegAngle, targetAngle, legFollowSpeed * Time.deltaTime);

        if (legsTransform != null)
            legsTransform.rotation = Quaternion.AngleAxis(currentLegAngle, Vector3.forward);

        if (hatTransform != null)
            hatTransform.rotation = Quaternion.AngleAxis(currentLegAngle, Vector3.forward);

        // ✅ Partículas dentro del player pero con rotación mundial independiente
        if (particulas != null)
        {
            bool moving = moveInput != Vector2.zero;

            if (moving && !isMoving) { particulas.Play(); isMoving = true; }
            if (!moving && isMoving) { particulas.Stop(); isMoving = false; }

            if (moving)
            {
                // Apunta en dirección contraria al movimiento, ignorando rotación del parent
                float moveAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
                particulas.transform.rotation = Quaternion.AngleAxis(moveAngle - 90f, Vector3.forward);
            }
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void SetAimSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    public bool IsMoving() => moveInput != Vector2.zero;

    #region Debug
    [Header("Debug")]
    [SerializeField] private bool debugDrawMovement = false;

    private void OnDrawGizmos()
    {
        if (!debugDrawMovement || !Application.isPlaying) return;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, moveInput.normalized * 1.5f);
    }
    #endregion
}