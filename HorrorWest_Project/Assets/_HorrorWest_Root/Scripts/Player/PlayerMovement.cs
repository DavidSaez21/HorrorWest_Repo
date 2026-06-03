using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private ParticleSystem particulas;

    [Header("Leg Settings")]
    [SerializeField] private float legFollowSpeed = 8f;
    [SerializeField] private Transform legsTransform;
    [SerializeField] private Transform hatTransform;
    [SerializeField] private Animator legsAnimator;

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
        rb.linearVelocity = moveInput.normalized * moveSpeed * speedMultiplier;

        bool moving = moveInput != Vector2.zero;

        if (legsAnimator != null)
            legsAnimator.SetBool("isWalking", moving);

        if (particulas != null)
        {
            if (moving && !isMoving) { particulas.Play(); isMoving = true; }
            if (!moving && isMoving) { particulas.Stop(); isMoving = false; }
        }
    }

    private void Update()
    {
        float targetAngle = transform.eulerAngles.z;
        currentLegAngle = Mathf.LerpAngle(currentLegAngle, targetAngle, legFollowSpeed * Time.deltaTime);

        if (legsTransform != null)
            legsTransform.rotation = Quaternion.AngleAxis(currentLegAngle, Vector3.forward);

        if (hatTransform != null)
            hatTransform.rotation = Quaternion.AngleAxis(currentLegAngle, Vector3.forward);
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