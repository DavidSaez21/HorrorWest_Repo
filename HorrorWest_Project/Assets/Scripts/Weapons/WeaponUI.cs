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
    private int maxAmmo = 0;

    private void Start()
    {
        if (reloadText != null)
            reloadText.gameObject.SetActive(false);

        RefreshWeapon();
    }

    private void Update()
    {
        if (playerShoot != null && playerShoot.GetCurrentWeapon() != subscribedWeapon)
            RefreshWeapon();

        // Actualiza el símbolo de infinito en tiempo real por si se coge la mejora
        if (PlayerStats.Instance != null && PlayerStats.Instance.hasInfiniteAmmo)
            SetInfiniteAmmoUI();
    }

    private void RefreshWeapon()
    {
        Unsubscribe();

        if (playerShoot == null) return;

        subscribedWeapon = playerShoot.GetCurrentWeapon();
        if (subscribedWeapon == null) return;

        if (weaponIcon != null && subscribedWeapon.icon != null)
            weaponIcon.sprite = subscribedWeapon.icon;

        if (subscribedWeapon is Revolver revolver)
        {
            revolver.OnAmmoChanged += UpdateAmmo;
            revolver.OnStartReload += ShowReloading;
            revolver.OnEndReload += HideReloading;
            maxAmmo = revolver.GetCylinderSize();
            UpdateAmmo(revolver.GetCurrentAmmo());
        }
        else if (subscribedWeapon is Shotgun shotgun)
        {
            shotgun.OnAmmoChanged += UpdateAmmo;
            shotgun.OnStartReload += ShowReloading;
            shotgun.OnEndReload += HideReloading;
            maxAmmo = shotgun.GetMagazineSize();
            UpdateAmmo(shotgun.GetCurrentAmmo());
        }
        else if (subscribedWeapon is Rifle rifle)
        {
            rifle.OnAmmoChanged += UpdateAmmo;
            rifle.OnStartReload += ShowReloading;
            rifle.OnEndReload += HideReloading;
            maxAmmo = rifle.GetMagazineSize();
            UpdateAmmo(rifle.GetCurrentAmmo());
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

    private void UpdateAmmo(int current)
    {
        if (ammoText == null) return;

        // Si tiene cargador ilimitado muestra el símbolo ∞
        if (PlayerStats.Instance != null && PlayerStats.Instance.hasInfiniteAmmo)
        {
            SetInfiniteAmmoUI();
            return;
        }

        ammoText.text = $"{current} / {maxAmmo}";
    }

    private void SetInfiniteAmmoUI()
    {
        if (ammoText != null)
            ammoText.text = "∞";

        if (reloadText != null)
            reloadText.gameObject.SetActive(false);

        if (ammoText != null)
            ammoText.gameObject.SetActive(true);
    }

    private void ShowReloading()
    {
        // No mostrar "Recargando" si tiene cargador ilimitado
        if (PlayerStats.Instance != null && PlayerStats.Instance.hasInfiniteAmmo) return;

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