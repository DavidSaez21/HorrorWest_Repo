using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Leg Rotation Settings")]
    [SerializeField] private float legFollowSpeed = 8f;
    [SerializeField] private float maxLegTorsoAngle = 90f;
    [SerializeField] private float flipSpeed = 20f;

    [Header("References")]
    [SerializeField] private Transform legsTransform;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float currentLegAngle = 0f;
    private float targetLegAngle = 0f;
    private bool isFlipping = false;
    private float speedMultiplier = 1f;
    private float currentTorsoAngle = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void SetAimSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput.normalized * moveSpeed * speedMultiplier;
    }

    public void UpdateLegAngle(float torsoAngle)
    {
        currentTorsoAngle = torsoAngle;

        if (moveInput != Vector2.zero && !isFlipping)
            targetLegAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;

        float angleDiff = Mathf.DeltaAngle(currentLegAngle, torsoAngle);

        if (Mathf.Abs(angleDiff) > maxLegTorsoAngle && !isFlipping)
        {
            // Las piernas flipean hacia donde está mirando el torso
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
}