using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerShoot playerShoot;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI reloadText;

    private WeaponBase subscribedWeapon;

    private void Start()
    {
        if (reloadText != null)
            reloadText.gameObject.SetActive(false);

        RefreshWeapon();
    }

    private void Update()
    {
        // Comprueba cada frame si el arma cambió
        if (playerShoot != null && playerShoot.GetCurrentWeapon() != subscribedWeapon)
            RefreshWeapon();
    }

    private void RefreshWeapon()
    {
        // Desuscribirse del arma anterior
        Unsubscribe();

        if (playerShoot == null) return;

        subscribedWeapon = playerShoot.GetCurrentWeapon();
        if (subscribedWeapon == null) return;

        // Actualiza el icono
        if (weaponIcon != null && subscribedWeapon.icon != null)
            weaponIcon.sprite = subscribedWeapon.icon;

        // Suscribirse al nuevo arma según su tipo
        if (subscribedWeapon is Revolver revolver)
        {
            revolver.OnAmmoChanged += UpdateAmmo;
            revolver.OnStartReload += ShowReloading;
            revolver.OnEndReload += HideReloading;
            UpdateAmmo(revolver.GetCurrentAmmo());
            SetMaxAmmo(revolver.GetCylinderSize());
        }
        else if (subscribedWeapon is Shotgun shotgun)
        {
            shotgun.OnAmmoChanged += UpdateAmmo;
            shotgun.OnStartReload += ShowReloading;
            shotgun.OnEndReload += HideReloading;
            UpdateAmmo(shotgun.GetCurrentAmmo());
            SetMaxAmmo(shotgun.GetMagazineSize());
        }
        else if (subscribedWeapon is Rifle rifle)
        {
            rifle.OnAmmoChanged += UpdateAmmo;
            rifle.OnStartReload += ShowReloading;
            rifle.OnEndReload += HideReloading;
            UpdateAmmo(rifle.GetCurrentAmmo());
            SetMaxAmmo(rifle.GetMagazineSize());
        }
    }

    private void Unsubscribe()
    {
        if (subscribedWeapon is Revolver revolver)
        {
            revolver.OnAmmoChanged -= UpdateAmmo;
            revolver.OnStartReload -= ShowReloading;
            revolver.OnEndReload -= HideReloading;
        }
        else if (subscribedWeapon is Shotgun shotgun)
        {
            shotgun.OnAmmoChanged -= UpdateAmmo;
            shotgun.OnStartReload -= ShowReloading;
            shotgun.OnEndReload -= HideReloading;
        }
        else if (subscribedWeapon is Rifle rifle)
        {
            rifle.OnAmmoChanged -= UpdateAmmo;
            rifle.OnStartReload -= ShowReloading;
            rifle.OnEndReload -= HideReloading;
        }

        subscribedWeapon = null;
    }

    private int maxAmmo = 0;

    private void SetMaxAmmo(int max)
    {
        maxAmmo = max;
    }

    private void UpdateAmmo(int current)
    {
        if (ammoText != null)
            ammoText.text = $"{current} / {maxAmmo}";
    }

    private void ShowReloading()
    {
        if (reloadText != null)
            reloadText.gameObject.SetActive(true);

        if (ammoText != null)
            ammoText.gameObject.SetActive(false);
    }

    private void HideReloading()
    {
        if (reloadText != null)
            reloadText.gameObject.SetActive(false);

        if (ammoText != null)
            ammoText.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }
}