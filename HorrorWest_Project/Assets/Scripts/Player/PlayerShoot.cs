using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAiming playerAiming;

    [Header("Primary Weapons")]
    [SerializeField] private WeaponBase[] weapons;
    private WeaponBase currentWeapon;

    [Header("Secondary Weapons (Dual Wield)")]
    [SerializeField] private WeaponBase[] secondaryWeapons;
    [SerializeField] private Transform secondWeaponPivot;
    private WeaponBase currentSecondaryWeapon;

    private bool isFiring = false;
    private bool isDualWield = false;

    #region Debug
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    #endregion

    private void Start()
    {
        foreach (WeaponBase w in weapons)
            w.gameObject.SetActive(false);

        foreach (WeaponBase w in secondaryWeapons)
            w.gameObject.SetActive(false);

        if (weapons.Length > 0)
            EquipWeapon(0);
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (currentWeapon != null) currentWeapon.ManualReload();
        if (isDualWield && currentSecondaryWeapon != null) currentSecondaryWeapon.ManualReload();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isFiring = true;

            if (currentWeapon != null && currentWeapon.fireMode == FireMode.SemiAuto)
            {
                TryFire();
                if (isDualWield) TryFireSecondary();
            }
        }

        if (context.canceled)
            isFiring = false;
    }

    private void Update()
    {
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

    private void TryFire()
    {
        if (currentWeapon == null || playerAiming == null) return;
        Vector2 origin = playerAiming.GetWeaponPosition();
        Vector2 direction = playerAiming.GetAimDirection();
        currentWeapon.Fire(origin, direction);
    }

    private void TryFireSecondary()
    {
        if (currentSecondaryWeapon == null || playerAiming == null) return;
        Vector2 origin = secondWeaponPivot != null ? (Vector2)secondWeaponPivot.position : (Vector2)transform.position;
        Vector2 direction = playerAiming.GetAimDirection();
        currentSecondaryWeapon.Fire(origin, direction);
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length) return;

        if (currentWeapon != null)
            currentWeapon.gameObject.SetActive(false);

        if (currentSecondaryWeapon != null)
            currentSecondaryWeapon.gameObject.SetActive(false);

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
}