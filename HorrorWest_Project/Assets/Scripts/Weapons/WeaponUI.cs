using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerShoot playerShoot;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private GameObject reloadPanel;        // Panel que contiene el círculo de recarga
    [SerializeField] private Image reloadCircle;            // Image con Fill Method Radial 360
    [SerializeField] private float reloadTime = 0f;         // Se actualiza según el arma

    [Header("Ammo Colors")]
    [SerializeField] private Color fullAmmoColor = Color.white;
    [SerializeField] private Color lowAmmoColor = Color.red;
    [SerializeField] private float lowAmmoThreshold = 0.5f; // 50% de balas

    private WeaponBase subscribedWeapon;
    private int maxAmmo = 0;
    private bool isReloading = false;
    private float reloadTimer = 0f;

    private void Start()
    {
        if (reloadPanel != null)
            reloadPanel.SetActive(false);

        RefreshWeapon();
    }

    private void Update()
    {
        if (playerShoot != null && playerShoot.GetCurrentWeapon() != subscribedWeapon)
            RefreshWeapon();

        if (PlayerStats.Instance != null && PlayerStats.Instance.hasInfiniteAmmo)
            SetInfiniteAmmoUI();

        // Actualiza el círculo de recarga
        if (isReloading && reloadCircle != null && reloadTime > 0f)
        {
            reloadTimer += Time.deltaTime;
            reloadCircle.fillAmount = reloadTimer / reloadTime;
        }
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
            revolver.OnStartReload += () => ShowReloading(revolver.GetReloadTime());
            revolver.OnEndReload += HideReloading;
            maxAmmo = revolver.GetCylinderSize();
            UpdateAmmo(revolver.GetCurrentAmmo());
        }
        else if (subscribedWeapon is Shotgun shotgun)
        {
            shotgun.OnAmmoChanged += UpdateAmmo;
            shotgun.OnStartReload += () => ShowReloading(shotgun.GetReloadTime());
            shotgun.OnEndReload += HideReloading;
            maxAmmo = shotgun.GetMagazineSize();
            UpdateAmmo(shotgun.GetCurrentAmmo());
        }
        else if (subscribedWeapon is Rifle rifle)
        {
            rifle.OnAmmoChanged += UpdateAmmo;
            rifle.OnStartReload += () => ShowReloading(rifle.GetReloadTime());
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
            revolver.OnStartReload -= () => ShowReloading(0f);
            revolver.OnEndReload -= HideReloading;
        }
        else if (subscribedWeapon is Shotgun shotgun)
        {
            shotgun.OnAmmoChanged -= UpdateAmmo;
            shotgun.OnStartReload -= () => ShowReloading(0f);
            shotgun.OnEndReload -= HideReloading;
        }
        else if (subscribedWeapon is Rifle rifle)
        {
            rifle.OnAmmoChanged -= UpdateAmmo;
            rifle.OnStartReload -= () => ShowReloading(0f);
            rifle.OnEndReload -= HideReloading;
        }

        subscribedWeapon = null;
    }

    private void UpdateAmmo(int current)
    {
        if (ammoText == null) return;

        if (PlayerStats.Instance != null && PlayerStats.Instance.hasInfiniteAmmo)
        {
            SetInfiniteAmmoUI();
            return;
        }

        ammoText.text = $"{current} / {maxAmmo}";

        // Cambia el color según el porcentaje de munición
        float ammoPercent = maxAmmo > 0 ? (float)current / maxAmmo : 1f;
        ammoText.color = ammoPercent <= lowAmmoThreshold ? lowAmmoColor : fullAmmoColor;
    }

    private void ShowReloading(float duration)
    {
        if (PlayerStats.Instance != null && PlayerStats.Instance.hasInfiniteAmmo) return;

        isReloading = true;
        reloadTime = duration;
        reloadTimer = 0f;

        if (ammoText != null)
            ammoText.gameObject.SetActive(false);

        if (reloadPanel != null)
        {
            reloadPanel.SetActive(true);
            if (reloadCircle != null)
                reloadCircle.fillAmount = 0f;
        }
    }

    private void HideReloading()
    {
        isReloading = false;

        if (reloadPanel != null)
            reloadPanel.SetActive(false);

        if (ammoText != null)
            ammoText.gameObject.SetActive(true);
    }

    private void SetInfiniteAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = "∞";
            ammoText.color = fullAmmoColor;
            ammoText.gameObject.SetActive(true);
        }

        if (reloadPanel != null)
            reloadPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }
}