using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("References")]
    [SerializeField] private Transform legsTransform;   // Sprite de las piernas (hijo del player)

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Conectado manualmente en el inspector del PlayerInput (Invoke Unity Events)
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
        RotateLegs();
    }

    private void RotateLegs()
    {
        if (moveInput == Vector2.zero) return;

        // Las piernas rotan hacia la dirección de movimiento
        float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg - 90f;
        if (legsTransform != null)
            legsTransform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}