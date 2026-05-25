using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAiming : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform torsoTransform;
    [SerializeField] private Transform weaponOrbitPivot;
    [SerializeField] private Transform weaponTransform;

    [Header("Orbit Settings")]
    [SerializeField] private float orbitRadius = 0.6f;

    private Camera mainCamera;
    private Vector2 mouseWorldPosition;

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
    }

    private void AimAtCursor()
    {
        Vector2 direction = mouseWorldPosition - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (torsoTransform != null)
            torsoTransform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);

        if (weaponOrbitPivot != null)
        {
            weaponOrbitPivot.position = (Vector2)transform.position + direction.normalized * orbitRadius;

            if (weaponTransform != null)
                weaponTransform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        }
    }

    public Vector2 GetAimDirection()
    {
        return (mouseWorldPosition - (Vector2)transform.position).normalized;
    }

    public Vector2 GetWeaponPosition()
    {
        return weaponOrbitPivot != null ? (Vector2)weaponOrbitPivot.position : (Vector2)transform.position;
    }
}