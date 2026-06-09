using System;
using System.Collections;
using UnityEngine;

public class TentacleAttack : MonoBehaviour
{
    [Header("Game Feel")]
    [SerializeField] private float hitStopFrames = 3f;
    [SerializeField] private GameObject impactParticlePrefab;

    [HideInInspector] public float damage = 18f;

    private static readonly int AnimAttack = Animator.StringToHash("Attack");

    private Animator _animator;
    private SpriteRenderer _sr;
    private Action _onComplete;
    private bool _idleDone;
    private bool _attackDone;

    private Collider2D _col;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _sr = GetComponent<SpriteRenderer>();
        _col = GetComponent<Collider2D>();
        if (_sr != null) _sr.enabled = false;
        if (_col != null) _col.enabled = false;
    }

    public void Execute(Action onComplete)
    {
        _onComplete = onComplete;

        // Reset flags en cada ejecución
        _idleDone = false;
        _attackDone = false;

        if (_col != null) _col.enabled = false;

        if (_animator != null)
        {
            _animator.speed = 1f;
            _animator.Play("AC_Idle_TentaculoIZQ", 0, 0f);
        }

        if (_sr != null) _sr.enabled = true;
    }

    // Animation Event — llamar en el frame del golpe del clip AC_Attacking
    public void EnableCollider() { if (_col != null) _col.enabled = true; }
    public void DisableCollider() { if (_col != null) _col.enabled = false; }

    // Animation Event — último frame del clip AC_Idle_TentaculoIZQ
    public void OnIdleAnimationEnd()
    {
        if (_idleDone) return;
        _idleDone = true;
        _animator?.SetTrigger(AnimAttack);
    }

    // Animation Event — último frame del clip AC_Attacking_TentaculoIZQ
    public void OnAttackAnimationEnd()
    {
        if (_attackDone) return;
        _attackDone = true;
        if (_sr != null) _sr.enabled = false;
        if (_col != null) _col.enabled = false;
        _onComplete?.Invoke();
        _onComplete = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!other.TryGetComponent(out PlayerHealth ph)) return;
        ph.TakeDamage(damage);
        StartCoroutine(HitStop());
        SpawnImpactParticles(other.transform.position);
    }

    private IEnumerator HitStop()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopFrames / 60f);
        Time.timeScale = 1f;
    }

    private void SpawnImpactParticles(Vector3 position)
    {
        if (impactParticlePrefab == null) return;
        Instantiate(impactParticlePrefab, position, Quaternion.identity);
    }
}