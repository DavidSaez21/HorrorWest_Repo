using UnityEngine;
using UnityEngine.InputSystem;

// ─────────────────────────────────────────────────────────────────────────────
// PlayerShoot — gestiona el disparo, cambio de arma y Dual Wield.
// Referencia PlayerAiming (fusionado) en lugar de los dos scripts anteriores.
// ─────────────────────────────────────────────────────────────────────────────
public class PlayerShoot : MonoBehaviour
{
    [Header("Primary Weapons")]
    [SerializeField] private WeaponBase[] weapons;

    [Header("Secondary Weapons (Dual Wield)")]
    [SerializeField] private WeaponBase[] secondaryWeapons;
    [SerializeField] private Transform secondWeaponPivot;

    [Header("References")]
    [SerializeField] private PlayerAiming playerAiming;

    // ── Estado interno ────────────────────────────────────────────────────────
    private WeaponBase currentWeapon;
    private WeaponBase currentSecondaryWeapon;
    private bool isFiring = false;
    private bool isDualWield = false;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Start()
    {
        foreach (WeaponBase w in weapons)
            w.gameObject.SetActive(false);
        foreach (WeaponBase w in secondaryWeapons)
            w.gameObject.SetActive(false);

        if (weapons.Length > 0)
            EquipWeapon(0);
    }

    private void Update()
    {
        // Disparo automático (FullAuto)
        if (isFiring && currentWeapon != null && currentWeapon.fireMode == FireMode.FullAuto)
        {
            TryFire();
            if (isDualWield) TryFireSecondary();
        }

        #region Debug
        if (debugMode)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) EquipWeapon(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) EquipWeapon(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) EquipWeapon(2);
        }
        #endregion
    }

    // ── Input callbacks ───────────────────────────────────────────────────────
    public void OnFire(InputAction.CallbackContext context)
    {
        // No dispara si el juego está pausado
        if (Time.timeScale == 0f) return;

        if (context.performed)
        {
            isFiring = true;
            if (currentWeapon != null && currentWeapon.fireMode == FireMode.SemiAuto)
            {
                TryFire();
                if (isDualWield) TryFireSecondary();
            }
        }
        else if (context.canceled)
        {
            isFiring = false;
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        currentWeapon?.ManualReload();
        if (isDualWield) currentSecondaryWeapon?.ManualReload();
    }

    // ── Disparo ───────────────────────────────────────────────────────────────
    private void TryFire()
    {
        if (currentWeapon == null || playerAiming == null) return;
        currentWeapon.Fire(playerAiming.GetWeaponPosition(), playerAiming.GetAimDirection());
    }

    private void TryFireSecondary()
    {
        if (currentSecondaryWeapon == null || playerAiming == null) return;
        Vector2 origin = secondWeaponPivot != null
            ? (Vector2)secondWeaponPivot.position
            : (Vector2)transform.position;
        currentSecondaryWeapon.Fire(origin, playerAiming.GetAimDirection());
    }

    // ── Gestión de armas ──────────────────────────────────────────────────────
    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length) return;

        currentWeapon?.gameObject.SetActive(false);
        currentSecondaryWeapon?.gameObject.SetActive(false);

        currentWeapon = weapons[index];
        currentWeapon.gameObject.SetActive(true);

        if (isDualWield && index < secondaryWeapons.Length)
        {
            currentSecondaryWeapon = secondaryWeapons[index];
            currentSecondaryWeapon.gameObject.SetActive(true);
        }

        isFiring = false;
    }

    public void ActivateDualWield()
    {
        isDualWield = true;
        int currentIndex = System.Array.IndexOf(weapons, currentWeapon);
        if (currentIndex >= 0 && currentIndex < secondaryWeapons.Length)
        {
            currentSecondaryWeapon = secondaryWeapons[currentIndex];
            currentSecondaryWeapon.gameObject.SetActive(true);
        }
    }

    public WeaponBase GetCurrentWeapon() => currentWeapon;

    #region Debug
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    #endregion
}