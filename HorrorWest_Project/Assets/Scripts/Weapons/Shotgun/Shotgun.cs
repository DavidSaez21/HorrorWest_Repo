using System.Collections;
using UnityEngine;

public class Shotgun : WeaponBase
{
    [Header("Shotgun Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int pelletsPerShot = 5;
    [SerializeField] private float spreadAngle = 30f;
    [SerializeField] private int magazineSize = 8;
    [SerializeField] private float reloadTime = 2f;

    private int currentAmmo;
    private bool isReloading = false;

    public event System.Action OnStartReload;
    public event System.Action OnEndReload;
    public event System.Action<int> OnAmmoChanged;

    public float GetReloadTime() => reloadTime;

    private void Start()
    {
        fireMode = FireMode.SemiAuto;
        currentAmmo = magazineSize;
    }

    public override void Fire(Vector2 origin, Vector2 direction)
    {
        if (isReloading || !CanFire()) return;

        bool infiniteAmmo = PlayerStats.Instance != null && PlayerStats.Instance.hasInfiniteAmmo;

        if (currentAmmo <= 0 && !infiniteAmmo)
        {
            StartCoroutine(ReloadCoroutine());
            return;
        }

        IsLastBullet = currentAmmo == 1 && !infiniteAmmo;

        Vector2 spawnPosition = shootPoint != null ? (Vector2)shootPoint.position : origin;
        SpawnPellets(spawnPosition, direction);

        if (!infiniteAmmo)
        {
            currentAmmo--;
            OnAmmoChanged?.Invoke(currentAmmo);

            if (currentAmmo <= 0)
                StartCoroutine(ReloadCoroutine());
        }

        ResetFireCooldown();
    }

    public void InstantReload()
    {
        StopAllCoroutines();
        currentAmmo = magazineSize;
        isReloading = false;
        IsLastBullet = false;
        OnEndReload?.Invoke();
        OnAmmoChanged?.Invoke(currentAmmo);
    }

    public override void ManualReload()
    {
        if (isReloading || currentAmmo == magazineSize) return;
        if (PlayerStats.Instance != null && PlayerStats.Instance.hasInfiniteAmmo) return;
        StartCoroutine(ReloadCoroutine());
    }

    private void SpawnPellets(Vector2 origin, Vector2 direction)
    {
        if (bulletPrefab == null) return;

        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        for (int i = 0; i < pelletsPerShot; i++)
        {
            float t = pelletsPerShot == 1 ? 0.5f : (float)i / (pelletsPerShot - 1);
            float angleOffset = Mathf.Lerp(-spreadAngle / 2f, spreadAngle / 2f, t);
            float finalAngle = baseAngle + angleOffset;

            Vector2 pelletDirection = new Vector2(
                Mathf.Cos(finalAngle * Mathf.Deg2Rad),
                Mathf.Sin(finalAngle * Mathf.Deg2Rad)
            );

            Quaternion rotation = Quaternion.AngleAxis(finalAngle - 90f, Vector3.forward);
            GameObject pellet = Instantiate(bulletPrefab, origin, rotation);

            if (pellet.TryGetComponent(out ShotgunPellet proj))
            {
                proj.Init(pelletDirection, bulletSpeed, damage, range);
                proj.SetLastBullet(IsLastBullet);
            }
        }
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        OnStartReload?.Invoke();

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        isReloading = false;
        IsLastBullet = false;
        OnEndReload?.Invoke();
        OnAmmoChanged?.Invoke(currentAmmo);
    }

    public int GetCurrentAmmo() => currentAmmo;
    public int GetMagazineSize() => magazineSize;
    public bool IsReloading() => isReloading;
}