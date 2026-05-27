using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerShoot playerShoot;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private GameObject reloadPanel;
    [SerializeField] private Image reloadCircle;

    [Header("Ammo Colors")]
    [SerializeField] private Color fullAmmoColor = Color.white;
    [SerializeField] private Color mediumAmmoColor = Color.yellow;
    [SerializeField] private Color lowAmmoColor = Color.red;

    private WeaponBase subscribedWeapon;
    private bool isReloading = false;
    private float reloadTime = 0f;
    private float reloadTimer = 0f;

    private void Start()
    {
        if (reloadPanel != null) reloadPanel.SetActive(false);
        RefreshWeapon();
    }

    private void Update()
    {
        if (playerShoot != null && playerShoot.GetCurrentWeapon() != subscribedWeapon)
            RefreshWeapon();

        if (PlayerManager.Instance.Stats != null && PlayerManager.Instance.Stats.hasInfiniteAmmo)
            SetInfiniteAmmoUI();

        if (isReloading && reloadCircle != null && reloadTime > 0f)
        {
            reloadTimer += Time.deltaTime;
            reloadCircle.fillAmount = Mathf.Clamp01(reloadTimer / reloadTime);
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

        subscribedWeapon.OnAmmoChanged += UpdateAmmo;
        subscribedWeapon.OnStartReload += ShowReloading;
        subscribedWeapon.OnEndReload += HideReloading;

        UpdateAmmo(subscribedWeapon.GetCurrentAmmo());
    }

    private void Unsubscribe()
    {
        if (subscribedWeapon == null) return;
        subscribedWeapon.OnAmmoChanged -= UpdateAmmo;
        subscribedWeapon.OnStartReload -= ShowReloading;
        subscribedWeapon.OnEndReload -= HideReloading;
        subscribedWeapon = null;
    }

    private void UpdateAmmo(int current)
    {
        if (ammoText == null) return;

        if (PlayerManager.Instance.Stats != null && PlayerManager.Instance.Stats.hasInfiniteAmmo)
        {
            SetInfiniteAmmoUI();
            return;
        }

        int maxAmmo = subscribedWeapon != null ? subscribedWeapon.GetMagazineSize() : 0;
        float ammoPercent = maxAmmo > 0 ? (float)current / maxAmmo : 1f;

        ammoText.text = $"{current} / {maxAmmo}";

        // Blanco > 50%, Amarillo entre 25-50%, Rojo < 25%
        if (ammoPercent > 0.5f)
            ammoText.color = fullAmmoColor;
        else if (ammoPercent > 0.25f)
            ammoText.color = mediumAmmoColor;
        else
            ammoText.color = lowAmmoColor;
    }

    private void ShowReloading()
    {
        if (PlayerManager.Instance.Stats != null && PlayerManager.Instance.Stats.hasInfiniteAmmo) return;

        isReloading = true;
        reloadTime = subscribedWeapon != null ? subscribedWeapon.GetReloadTime() : 1f;
        reloadTimer = 0f;

        if (ammoText != null) ammoText.gameObject.SetActive(false);
        if (reloadPanel != null)
        {
            reloadPanel.SetActive(true);
            if (reloadCircle != null) reloadCircle.fillAmount = 0f;
        }
    }

    private void HideReloading()
    {
        isReloading = false;
        if (reloadPanel != null) reloadPanel.SetActive(false);
        if (ammoText != null) ammoText.gameObject.SetActive(true);
    }

    private void SetInfiniteAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = "∞";
            ammoText.color = fullAmmoColor;
            ammoText.gameObject.SetActive(true);
        }
        if (reloadPanel != null) reloadPanel.SetActive(false);
    }

    private void OnDestroy() => Unsubscribe();

    #region Debug
    [Header("Debug")]
    [SerializeField] private bool debugLogWeaponChange = false;

    private void LogWeaponChange(string weaponName)
    {
        if (debugLogWeaponChange)
            Debug.Log($"[WeaponUI] Arma cambiada a: {weaponName}");
    }
    #endregion
}