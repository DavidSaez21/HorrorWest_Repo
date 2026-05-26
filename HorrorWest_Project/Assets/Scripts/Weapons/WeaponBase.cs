using UnityEngine;

public enum FireMode { SemiAuto, FullAuto }

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Weapon Config")]
    public string weaponName = "Unnamed Weapon";
    public FireMode fireMode = FireMode.SemiAuto;
    public Sprite icon;

    [Header("Stats")]
    [SerializeField] protected float fireRate = 1f;
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float bulletSpeed = 10f;
    [SerializeField] protected float range = 10f;

    [Header("References")]
    [SerializeField] protected Transform shootPoint;

    protected float nextFireTime = 0f;

    public abstract void Fire(Vector2 origin, Vector2 direction);
    public abstract void ManualReload();

    protected bool CanFire() => Time.time >= nextFireTime;
    protected void ResetFireCooldown() => nextFireTime = Time.time + (1f / fireRate);
}