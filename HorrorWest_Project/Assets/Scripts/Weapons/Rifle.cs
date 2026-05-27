using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// Rifle — automático, cargador de 30 balas, mayor rango y daño.
// Lo exclusivo del rifle: fireMode FullAuto y spawn de bala.
// El cargador grande se configura desde el Inspector en magazineSize.
// ─────────────────────────────────────────────────────────────────────────────
public class Rifle : WeaponBase
{
    [Header("Rifle Settings")]
    [SerializeField] private GameObject bulletPrefab;

    protected override void Start()
    {
        fireMode = FireMode.FullAuto;
        base.Start();
    }

    public override void Fire(Vector2 origin, Vector2 direction)
    {
        Vector2 spawnPos = shootPoint != null ? (Vector2)shootPoint.position : origin;
        TryConsumeAmmoAndFire(spawnPos, direction);
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