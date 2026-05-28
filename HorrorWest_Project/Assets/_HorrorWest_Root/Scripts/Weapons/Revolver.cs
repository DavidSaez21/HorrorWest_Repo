using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// Revolver — semiautomático, 6 balas, recarga automática.
// Toda la lógica de recarga y munición vive en WeaponBase.
// Aquí solo está lo exclusivo del revólver: muzzle flash y spawn de bala.
// ─────────────────────────────────────────────────────────────────────────────
public class Revolver : WeaponBase
{
    [Header("Revolver Settings")]
    [SerializeField] private GameObject bulletPrefab;

    [Header("VFX")]
    [SerializeField] private ParticleSystem muzzleFlashFX;

    protected override void Start()
    {
        fireMode = FireMode.SemiAuto;
        base.Start();   // inicializa currentAmmo = magazineSize
    }

    public override void Fire(Vector2 origin, Vector2 direction)
    {
        Vector2 spawnPos = shootPoint != null ? (Vector2)shootPoint.position : origin;

        if (TryConsumeAmmoAndFire(spawnPos, direction))
            muzzleFlashFX?.Play();
    }

    protected override void SpawnProjectiles(Vector2 origin, Vector2 direction)
    {
        if (bulletPrefab == null) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);

        GameObject bullet = Instantiate(bulletPrefab, origin, rotation);
        if (bullet.TryGetComponent(out Projectile proj))
        {
            proj.Init(direction, bulletSpeed, damage, range);
            proj.SetLastBullet(IsLastBullet);
        }
    }

    #region Debug
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
    }
    #endregion
}