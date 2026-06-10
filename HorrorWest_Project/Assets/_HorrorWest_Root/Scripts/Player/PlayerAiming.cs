using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAiming : MonoBehaviour
{
    public static PlayerAiming Instance { get; private set; }

    [Header("Weapon Orbit")]
    [SerializeField] private Transform weaponOrbitPivot;
    [SerializeField] private Transform weaponTransform;
    [SerializeField] private float orbitRadius = 0.6f;
    [SerializeField] private float aimSmoothSpeed = 10f;

    [Header("Aim Mode (RMB)")]
    [SerializeField] private float aimSpeedMultiplier = 0.5f;
    [SerializeField] private float aimCritMultiplier = 2f;

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Animator torsoAnimator;

    public bool IsAiming { get; private set; } = false;

    private Camera mainCamera;
    private Vector2 mouseScreenPosition;
    private float targetAngle = 0f;
    private float currentAngle = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        mainCamera = Camera.main;
    }

    private void Update()
    {
        UpdateAim();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        mouseScreenPosition = context.ReadValue<Vector2>();
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsAiming = true;
            playerMovement?.SetAimSpeedMultiplier(aimSpeedMultiplier);
        }
        else if (context.canceled)
        {
            IsAiming = false;
            playerMovement?.SetAimSpeedMultiplier(1f);
        }
    }

    private void UpdateAim()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        Vector2 direction = mouseWorldPosition - (Vector2)transform.position;
        targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, aimSmoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.AngleAxis(currentAngle - 90f, Vector3.forward);

        if (weaponOrbitPivot != null)
        {
            Vector2 smoothDir = new Vector2(
                Mathf.Cos(currentAngle * Mathf.Deg2Rad),
                Mathf.Sin(currentAngle * Mathf.Deg2Rad)
            );
            weaponOrbitPivot.position = (Vector2)transform.position + smoothDir * orbitRadius;
            if (weaponTransform != null)
                weaponTransform.rotation = Quaternion.AngleAxis(currentAngle - 90f, Vector3.forward);
        }

        if (torsoAnimator != null && playerMovement != null)
            torsoAnimator.SetBool("isWalking", playerMovement.IsMoving());
    }

    public Vector2 GetAimDirection() => new Vector2(
        Mathf.Cos(currentAngle * Mathf.Deg2Rad),
        Mathf.Sin(currentAngle * Mathf.Deg2Rad)
    );

    public Vector2 GetWeaponPosition() =>
        weaponOrbitPivot != null ? (Vector2)weaponOrbitPivot.position : (Vector2)transform.position;

    public float GetCritMultiplier() => IsAiming ? aimCritMultiplier : 1f;

    #region Debug
    [Header("Debug")]
    [SerializeField] private bool debugDrawAimRay = false;

    private void OnDrawGizmos()
    {
        if (!debugDrawAimRay || !Application.isPlaying) return;

        Gizmos.color = IsAiming ? Color.red : Color.yellow;
        Gizmos.DrawRay(transform.position, GetAimDirection() * 3f);

        if (weaponOrbitPivot != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(weaponOrbitPivot.position, 0.1f);
        }
    }
    #endregion
}