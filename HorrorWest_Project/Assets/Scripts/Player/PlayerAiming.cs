using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAiming : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform torsoTransform;
    [SerializeField] private Transform weaponOrbitPivot;
    [SerializeField] private Transform weaponTransform;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Orbit Settings")]
    [SerializeField] private float orbitRadius = 0.6f;

    [Header("Smoothing")]
    [SerializeField] private float aimSmoothSpeed = 10f;    // Más bajo = más lag, más alto = más instantáneo

    private Camera mainCamera;
    private Vector2 mouseWorldPosition;
    private float torsoAngle = 0f;
    private float currentAngle = 0f;    // Ángulo actual suavizado

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Vector2 mouseScreenPos = context.ReadValue<Vector2>();
        mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPos);
    }

    private void Update()
    {
        AimAtCursor();

        if (playerMovement != null)
            playerMovement.UpdateLegAngle(torsoAngle);
    }

    private void AimAtCursor()
    {
        Vector2 direction = mouseWorldPosition - (Vector2)transform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Suavizado del ángulo — LerpAngle para que no haya saltos en 0/360
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, aimSmoothSpeed * Time.deltaTime);
        torsoAngle = currentAngle;

        if (torsoTransform != null)
            torsoTransform.rotation = Quaternion.AngleAxis(currentAngle - 90f, Vector3.forward);

        if (weaponOrbitPivot != null)
        {
            Vector2 smoothDirection = new Vector2(
                Mathf.Cos(currentAngle * Mathf.Deg2Rad),
                Mathf.Sin(currentAngle * Mathf.Deg2Rad)
            );

            weaponOrbitPivot.position = (Vector2)transform.position + smoothDirection * orbitRadius;

            if (weaponTransform != null)
                weaponTransform.rotation = Quaternion.AngleAxis(currentAngle - 90f, Vector3.forward);
        }
    }

    public Vector2 GetAimDirection()
    {
        return new Vector2(
            Mathf.Cos(currentAngle * Mathf.Deg2Rad),
            Mathf.Sin(currentAngle * Mathf.Deg2Rad)
        );
    }

    public Vector2 GetWeaponPosition()
    {
        return weaponOrbitPivot != null ? (Vector2)weaponOrbitPivot.position : (Vector2)transform.position;
    }
}