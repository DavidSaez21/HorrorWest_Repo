using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAiming playerAiming;

    [Header("Weapons")]
    [SerializeField] private WeaponBase[] weapons;
    private WeaponBase currentWeapon;

    private bool isFiring = false;

    #region Debug
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    #endregion

    private void Start()
    {
        foreach (WeaponBase w in weapons)
            w.gameObject.SetActive(false);

        if (weapons.Length > 0)
            EquipWeapon(0);
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isFiring = true;

            if (currentWeapon != null && currentWeapon.fireMode == FireMode.SemiAuto)
                TryFire();
        }

        if (context.canceled)
            isFiring = false;
    }

    private void Update()
    {
        if (isFiring && currentWeapon != null && currentWeapon.fireMode == FireMode.FullAuto)
            TryFire();

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

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length) return;

        if (currentWeapon != null)
            currentWeapon.gameObject.SetActive(false);

        currentWeapon = weapons[index];
        currentWeapon.gameObject.SetActive(true);
        isFiring = false;
    }

    public WeaponBase GetCurrentWeapon() => currentWeapon;
}