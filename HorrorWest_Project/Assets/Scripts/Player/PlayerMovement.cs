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

    [Header("References")]
    [SerializeField] private Transform legsTransform;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float currentLegAngle = 0f;
    private float targetLegAngle = 0f;
    private float speedMultiplier = 1f;     // Modificado por PlayerAim al apuntar

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
        if (moveInput != Vector2.zero)
            targetLegAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;

        float angleDiff = Mathf.DeltaAngle(currentLegAngle, torsoAngle);

        if (Mathf.Abs(angleDiff) > maxLegTorsoAngle)
        {
            float excess = angleDiff - Mathf.Sign(angleDiff) * maxLegTorsoAngle;
            targetLegAngle = currentLegAngle + excess;
        }

        currentLegAngle = Mathf.LerpAngle(currentLegAngle, targetLegAngle, legFollowSpeed * Time.deltaTime);

        if (legsTransform != null)
            legsTransform.rotation = Quaternion.AngleAxis(currentLegAngle - 90f, Vector3.forward);
    }
}