using System.Collections;
using UnityEngine;

public enum FireMode { SemiAuto, FullAuto }

// ─────────────────────────────────────────────────────────────────────────────
// WeaponBase — clase abstracta compartida por todas las armas.
//
// ANTES: ReloadCoroutine, InstantReload y los eventos de munición estaban
// duplicados en Revolver, Shotgun y Rifle.
// AHORA: toda esa lógica vive aquí. Cada arma solo sobreescribe lo que
// es exclusivamente suyo (SpawnProjectiles).
// ─────────────────────────────────────────────────────────────────────────────
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

    [Header("Ammo")]
    [SerializeField] protected int magazineSize = 6;
    [SerializeField] protected float reloadTime = 1.5f;

    [Header("References")]
    [SerializeField] protected Transform shootPoint;

    [Header("Audio")]
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip reloadSound;
    private AudioSource _audioSource;

    // ── Eventos — WeaponUI se suscribe a estos ────────────────────────────────
    public event System.Action<int> OnAmmoChanged;
    public event System.Action OnStartReload;
    public event System.Action OnEndReload;

    // ── Estado compartido ─────────────────────────────────────────────────────
    protected int currentAmmo;
    protected bool isReloading = false;
    protected bool IsLastBullet = false;
    private float nextFireTime = 0f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    protected virtual void Start()
    {
        currentAmmo = magazineSize;
        _audioSource = gameObject.AddComponent<AudioSource>();
    }

    // ── API pública ───────────────────────────────────────────────────────────

    /// <summary>Dispara desde origin hacia direction. Llamado desde PlayerShoot.</summary>
    public abstract void Fire(Vector2 origin, Vector2 direction);

    /// <summary>Recarga manual (tecla R). No hace nada si ya recarga o está lleno.</summary>
    public virtual void ManualReload()
    {
        if (isReloading || currentAmmo == magazineSize) return;
        if (PlayerManager.Instance.Stats != null && PlayerManager.Instance.Stats.hasInfiniteAmmo) return;
        StartCoroutine(ReloadCoroutine());
    }

    /// <summary>Recarga instantánea. Usada por la mejora ReloadOnKill.</summary>
    public void InstantReload()
    {
        StopAllCoroutines();
        currentAmmo = magazineSize;
        isReloading = false;
        IsLastBullet = false;
        OnEndReload?.Invoke();
        OnAmmoChanged?.Invoke(currentAmmo);
    }

    // ── Helpers para subclases ────────────────────────────────────────────────
    protected bool CanFire() => Time.time >= nextFireTime;
    protected void ResetFireCooldown() => nextFireTime = Time.time + (1f / fireRate);

    protected bool HasInfiniteAmmo =>
        PlayerManager.Instance.Stats != null && PlayerManager.Instance.Stats.hasInfiniteAmmo;

    /// <summary>
    /// Lógica de disparo compartida: comprueba munición, actualiza IsLastBullet,
    /// llama a SpawnProjectiles y gestiona la recarga automática.
    /// Las subclases llaman a esto desde Fire() en lugar de duplicar la lógica.
    /// </summary>
    protected bool TryConsumeAmmoAndFire(Vector2 origin, Vector2 direction)
    {
        if (isReloading || !CanFire()) return false;

        if (currentAmmo <= 0 && !HasInfiniteAmmo)
        {
            StartCoroutine(ReloadCoroutine());
            return false;
        }

        IsLastBullet = currentAmmo == 1 && !HasInfiniteAmmo;

        SpawnProjectiles(origin, direction);

        if (fireSound != null)
            _audioSource.PlayOneShot(fireSound);

        if (!HasInfiniteAmmo)
        {
            currentAmmo--;
            OnAmmoChanged?.Invoke(currentAmmo);
            if (currentAmmo <= 0)
                StartCoroutine(ReloadCoroutine());
        }

        ResetFireCooldown();
        return true;
    }

    /// <summary>
    /// Cada arma implementa aquí cómo instancia sus proyectiles.
    /// El revólver y el rifle spawnan 1 bala. La escopeta spawna N perdigones.
    /// </summary>
    protected abstract void SpawnProjectiles(Vector2 origin, Vector2 direction);

    // ── Recarga ───────────────────────────────────────────────────────────────
    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        OnStartReload?.Invoke();

        if (reloadSound != null)
            _audioSource.PlayOneShot(reloadSound);

        float adjustedReloadTime = reloadTime;
        if (PlayerManager.Instance.Stats != null)
            adjustedReloadTime /= PlayerManager.Instance.Stats.GetReloadSpeed();

        yield return new WaitForSeconds(adjustedReloadTime);

        currentAmmo = magazineSize;
        isReloading = false;
        IsLastBullet = false;
        OnEndReload?.Invoke();
        OnAmmoChanged?.Invoke(currentAmmo);
    }

    // ── Getters para WeaponUI ─────────────────────────────────────────────────
    public int GetCurrentAmmo() => currentAmmo;
    public int GetMagazineSize() => magazineSize;
    public float GetReloadTime() => reloadTime;
    public bool IsReloading() => isReloading;

    #region Debug
    protected virtual void OnDrawGizmosSelected()
    {
        if (shootPoint == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(shootPoint.position, 0.1f);
        Gizmos.DrawRay(shootPoint.position, shootPoint.up * range);
    }
    #endregion
}