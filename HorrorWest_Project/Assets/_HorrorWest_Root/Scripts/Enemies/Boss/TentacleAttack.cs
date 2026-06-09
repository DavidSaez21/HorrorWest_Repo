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

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null) _sr.enabled = false;
        if (_animator != null) _animator.enabled = false;
    }

    public void Execute(Action onComplete)
    {
        if (_sr != null) _sr.enabled = true;
        if (_animator != null)
        {
            _animator.enabled = true;
            _animator.Play("AC_Idle_TentaculoIZQ", 0, 0f);
        }
        StartCoroutine(RunSequence(onComplete));
    }

    private IEnumerator RunSequence(Action onComplete)
    {
        // Espera un frame para que el Animator procese el Play()
        yield return null;

        // Espera a que el clip Idle termine
        yield return WaitForClipEnd("AC_Idle_TentaculoIZQ");

        // Lanza el clip de ataque
        _animator.ResetTrigger(AnimAttack);
        _animator.SetTrigger(AnimAttack);

        yield return null;

        // Espera a que el clip Attack termine
        yield return WaitForClipEnd("AC_Attacking_TentaculoIZQ");

        // Fin
        if (_sr != null) _sr.enabled = false;
        if (_animator != null) _animator.enabled = false;

        onComplete?.Invoke();
    }

    private IEnumerator WaitForClipEnd(string clipName)
    {
        // Espera a que el Animator entre en ese estado
        while (!_animator.GetCurrentAnimatorStateInfo(0).IsName(clipName))
            yield return null;

        // Espera a que ese estado llegue al final
        while (_animator.GetCurrentAnimatorStateInfo(0).IsName(clipName) &&
               _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;
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