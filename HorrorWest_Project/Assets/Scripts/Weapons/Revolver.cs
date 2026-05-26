using System.Collections;
using UnityEngine;

public class Revolver : WeaponBase
{
    [Header("Revolver Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int cylinderSize = 6;
    [SerializeField] private float reloadTime = 1.5f;

    private int currentAmmo;
    private bool isReloading = false;

    public event System.Action OnStartReload;
    public event System.Action OnEndReload;
    public event System.Action<int> OnAmmoChanged;

    private void Start()
    {
        fireMode = FireMode.SemiAuto;
        currentAmmo = cylinderSize;
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
        SpawnBullet(spawnPosition, direction);

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
        currentAmmo = cylinderSize;
        isReloading = false;
        IsLastBullet = false;
        OnEndReload?.Invoke();
        OnAmmoChanged?.Invoke(currentAmmo);
    }

    public override void ManualReload()
    {
        if (isReloading || currentAmmo == cylinderSize) return;
        if (PlayerStats.Instance != null && PlayerStats.Instance.hasInfiniteAmmo) return;
        StartCoroutine(ReloadCoroutine());
    }

    private void SpawnBullet(Vector2 origin, Vector2 direction)
    {
        if (bulletPrefab == null) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);

        GameObject bullet = Instantiate(bulletPrefab, origin, rotation);

        if (bullet.TryGetComponent(out BulletProjectile proj))
        {
            proj.Init(direction, bulletSpeed, damage, range);
            proj.SetLastBullet(IsLastBullet);
        }
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        OnStartReload?.Invoke();

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = cylinderSize;
        isReloading = false;
        IsLastBullet = false;
        OnEndReload?.Invoke();
        OnAmmoChanged?.Invoke(currentAmmo);
    }

    public int GetCurrentAmmo() => currentAmmo;
    public int GetCylinderSize() => cylinderSize;
    public bool IsReloading() => isReloading;
}