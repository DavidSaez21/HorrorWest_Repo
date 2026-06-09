using UnityEngine;

public class Revolver : WeaponBase
{
    [Header("Revolver Settings")]
    [SerializeField] private GameObject bulletPrefab;

    [Header("VFX")]
    [SerializeField] private GameObject muzzleFlashPrefab;

    protected override void Start()
    {
        fireMode = FireMode.SemiAuto;
        base.Start();
    }

    public override void Fire(Vector2 origin, Vector2 direction)
    {
        Vector2 spawnPos = shootPoint != null ? (Vector2)shootPoint.position : origin;
        if (TryConsumeAmmoAndFire(spawnPos, direction))
        {
            if (muzzleFlashPrefab != null)
            {
                float muzzleAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion muzzleRot = Quaternion.AngleAxis(muzzleAngle, Vector3.forward);
                Instantiate(muzzleFlashPrefab, shootPoint != null ? shootPoint.position : (Vector3)(origin), muzzleRot);
            }
        }
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

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
    }
}