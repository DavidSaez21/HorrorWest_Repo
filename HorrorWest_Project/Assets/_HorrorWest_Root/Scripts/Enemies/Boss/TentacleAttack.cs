using System;
using System.Collections;
using UnityEngine;

public class TentacleAttack : MonoBehaviour
{
    [Header("Animation Clips")]
    [SerializeField] private string idleClipName = "AC_Idle_TentaculoIZQ";
    [SerializeField] private string attackClipName = "AC_Attacking_TentaculoIZQ";

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
            _animator.Play(idleClipName, 0, 0f);
        }
        StartCoroutine(RunSequence(onComplete));
    }

    private IEnumerator RunSequence(Action onComplete)
    {
        yield return null;
        yield return WaitForClipEnd(idleClipName);
        _animator.ResetTrigger(AnimAttack);
        _animator.SetTrigger(AnimAttack);
        yield return null;
        yield return WaitForClipEnd(attackClipName);
        yield return null;
        yield return null;
        if (_sr != null) _sr.enabled = false;
        if (_animator != null) _animator.enabled = false;
        onComplete?.Invoke();
    }

    private IEnumerator WaitForClipEnd(string clipName)
    {
        while (!_animator.GetCurrentAnimatorStateInfo(0).IsName(clipName))
            yield return null;

        while (_animator.GetCurrentAnimatorStateInfo(0).IsName(clipName) &&
               _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.95f)
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