using UnityEngine;
using UnityEngine.InputSystem;

// ─────────────────────────────────────────────────────────────────────────────
// PlayerAiming — fusión de PlayerAim y PlayerAiming.
//
// ANTES: dos scripts con nombres casi idénticos haciendo cosas distintas
//   · PlayerAiming  → rotación del torso, órbita del arma, dirección de disparo
//   · PlayerAim     → estado de apuntado (RMB), multiplicador de velocidad y crít
//
// AHORA: un solo script, un solo singleton, cero dependencias circulares.
// ─────────────────────────────────────────────────────────────────────────────
public class PlayerAiming : MonoBehaviour
{
    public static PlayerAiming Instance { get; private set; }

    [Header("Torso & Weapon Orbit")]
    [SerializeField] private Transform torsoTransform;
    [SerializeField] private Transform weaponOrbitPivot;
    [SerializeField] private Transform weaponTransform;
    [SerializeField] private float orbitRadius = 0.6f;
    [SerializeField] private float aimSmoothSpeed = 10f;

    [Header("Aim Mode (RMB)")]
    [SerializeField] private float aimSpeedMultiplier = 0.5f;
    [SerializeField] private float aimCritMultiplier = 2f;

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;

    // ── Estado público ────────────────────────────────────────────────────────
    public bool IsAiming { get; private set; } = false;

    // ── Estado interno ────────────────────────────────────────────────────────
    private Camera mainCamera;
    private Vector2 mouseWorldPosition;
    private float targetAngle = 0f;
    private float currentAngle = 0f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        mainCamera = Camera.main;
    }

    private void Update()
    {
        UpdateAim();

        if (playerMovement != null)
            playerMovement.UpdateLegAngle(currentAngle);
    }

    // ── Input callbacks (asignados desde el PlayerInput component) ────────────

    /// <summary>Recibe la posición del ratón desde el Input System.</summary>
    public void OnLook(InputAction.CallbackContext context)
    {
        if (mainCamera == null) return;
        Vector2 screenPos = context.ReadValue<Vector2>();
        mouseWorldPosition = mainCamera.ScreenToWorldPoint(screenPos);
    }

    /// <summary>Click derecho — entra y sale del modo apuntado.</summary>
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

    // ── Lógica de apuntado ────────────────────────────────────────────────────
    private void UpdateAim()
    {
        Vector2 direction = mouseWorldPosition - (Vector2)transform.position;
        targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, aimSmoothSpeed * Time.deltaTime);

        // Rota el torso
        if (torsoTransform != null)
            torsoTransform.rotation = Quaternion.AngleAxis(currentAngle - 90f, Vector3.forward);

        // Órbita del arma alrededor del personaje
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
    }

    // ── API pública ───────────────────────────────────────────────────────────

    /// <summary>Dirección normalizada hacia el cursor. Usada por PlayerShoot.</summary>
    public Vector2 GetAimDirection() => new Vector2(
        Mathf.Cos(currentAngle * Mathf.Deg2Rad),
        Mathf.Sin(currentAngle * Mathf.Deg2Rad)
    );

    /// <summary>Posición mundial del pivot del arma. Usada por PlayerShoot.</summary>
    public Vector2 GetWeaponPosition() =>
        weaponOrbitPivot != null ? (Vector2)weaponOrbitPivot.position : (Vector2)transform.position;

    /// <summary>
    /// Multiplicador de crítico al apuntar. Usado por PlayerStats.CalculateDamage.
    /// Devuelve el multiplicador si está apuntando, 1 si no.
    /// </summary>
    public float GetCritMultiplier() => IsAiming ? aimCritMultiplier : 1f;

    #region Debug
    [Header("Debug")]
    [SerializeField] private bool debugDrawAimRay = false;

    private void OnDrawGizmos()
    {
        if (!debugDrawAimRay || !Application.isPlaying) return;

        // Rayo hacia el cursor
        Gizmos.color = IsAiming ? Color.red : Color.yellow;
        Vector2 dir = GetAimDirection();
        Gizmos.DrawRay(transform.position, dir * 3f);

        // Pivot del arma
        if (weaponOrbitPivot != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(weaponOrbitPivot.position, 0.1f);
        }
    }
    #endregion
}