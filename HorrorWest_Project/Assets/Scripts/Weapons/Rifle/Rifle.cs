using System.Collections;
using UnityEngine;

public class Rifle : WeaponBase
{
    [Header("Rifle Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int magazineSize = 30;
    [SerializeField] private float reloadTime = 2.5f;

    private int currentAmmo;
    private bool isReloading = false;

    public event System.Action OnStartReload;
    public event System.Action OnEndReload;
    public event System.Action<int> OnAmmoChanged;

    private void Start()
    {
        fireMode = FireMode.FullAuto;
        currentAmmo = magazineSize;
    }

    public override void Fire(Vector2 origin, Vector2 direction)
    {
        if (isReloading || !CanFire()) return;

        if (currentAmmo <= 0)
        {
            StartCoroutine(ReloadCoroutine());
            return;
        }

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

        currentAmmo = magazineSize;
        isReloading = false;
        OnEndReload?.Invoke();
        OnAmmoChanged?.Invoke(currentAmmo);
    }

    public int GetCurrentAmmo() => currentAmmo;
    public int GetMagazineSize() => magazineSize;
    public bool IsReloading() => isReloading;
}