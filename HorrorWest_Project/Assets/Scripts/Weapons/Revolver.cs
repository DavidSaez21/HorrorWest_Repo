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

        if (currentAmmo <= 0)
        {
            StartCoroutine(ReloadCoroutine());
            return;
        }

        // Si hay shootPoint usa su posición, si no usa el origen que manda PlayerShoot
        Vector2 spawnPosition = shootPoint != null ? (Vector2)shootPoint.position : origin;
        SpawnBullet(spawnPosition, direction);

        currentAmmo--;
        ResetFireCooldown();
        OnAmmoChanged?.Invoke(currentAmmo);

        if (currentAmmo <= 0)
            StartCoroutine(ReloadCoroutine());
    }

    private void SpawnBullet(Vector2 origin, Vector2 direction)
    {
        if (bulletPrefab == null) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);

        GameObject bullet = Instantiate(bulletPrefab, origin, rotation);

        if (bullet.TryGetComponent(out BulletProjectile proj))
            proj.Init(direction, bulletSpeed, damage, range);
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        OnStartReload?.Invoke();

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = cylinderSize;
        isReloading = false;
        OnEndReload?.Invoke();
        OnAmmoChanged?.Invoke(currentAmmo);
    }

    public int GetCurrentAmmo() => currentAmmo;
    public int GetCylinderSize() => cylinderSize;
    public bool IsReloading() => isReloading;
}