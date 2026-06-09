using UnityEngine;

public class Shotgun : WeaponBase
{
    [Header("Shotgun Settings")]
    [SerializeField] private GameObject pelletPrefab;
    [SerializeField] private int pelletsPerShot = 5;
    [SerializeField] private float spreadAngle = 30f;

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
        if (pelletPrefab == null) return;
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        for (int i = 0; i < pelletsPerShot; i++)
        {
            float t = pelletsPerShot == 1 ? 0.5f : (float)i / (pelletsPerShot - 1);
            float angleOffset = Mathf.Lerp(-spreadAngle / 2f, spreadAngle / 2f, t);
            float finalAngle = baseAngle + angleOffset;
            Vector2 pelletDir = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad));
            Quaternion rotation = Quaternion.AngleAxis(finalAngle - 90f, Vector3.forward);
            GameObject pellet = Instantiate(pelletPrefab, origin, rotation);
            if (pellet.TryGetComponent(out Projectile proj))
            {
                proj.Init(pelletDir, bulletSpeed, damage, range);
                proj.SetLastBullet(IsLastBullet);
            }
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        if (shootPoint == null) return;
        float baseAngle = shootPoint.eulerAngles.z + 90f;
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        for (int i = 0; i < pelletsPerShot; i++)
        {
            float t = pelletsPerShot == 1 ? 0.5f : (float)i / (pelletsPerShot - 1);
            float angle = baseAngle + Mathf.Lerp(-spreadAngle / 2f, spreadAngle / 2f, t);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Gizmos.DrawRay(shootPoint.position, dir * range);
        }
    }
}