using System.Collections;
using UnityEngine;

/// <summary>
/// Mueve los ojos del boss siguiendo al jugador mediante traslación.
/// Cada ojo se desplaza dentro de un radio máximo (eyeSocketRadius) 
/// relativo a su posición de reposo, sin salirse del linework.
///
/// Setup:
///   - leftEye / rightEye: los Transform de Ojo_Izq y Ojo_Der
///   - eyeSocketRadius: radio máximo de desplazamiento en unidades de mundo
///     (ajústalo hasta que visualmente no salgan del contorno del linework)
/// </summary>
public class BossEyeTracker : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────

    [Header("Ojos")]
    [SerializeField] private Transform leftEye;
    [SerializeField] private Transform rightEye;

    [Tooltip("Radio máximo de desplazamiento de cada ojo desde su posición de reposo. " +
             "Ajusta hasta que el ojo no salga del linework.")]
    [SerializeField] private float eyeSocketRadius = 0.15f;

    [Tooltip("Qué tan rápido el ojo sigue al jugador. Más alto = más reactivo.")]
    [SerializeField] private float trackingSpeed = 6f;

    [Tooltip("A qué distancia del jugador el ojo llega al límite máximo del socket. " +
             "Más bajo = el ojo se va al extremo aunque el jugador esté cerca.")]
    [SerializeField] private float maxTrackingDistance = 10f;

    // ── Runtime ───────────────────────────────────────────────────────────────

    private Transform _player;

    // Posiciones de reposo de cada ojo (se guardan en Start, nunca cambian)
    private Vector3 _leftEyeRestPos;
    private Vector3 _rightEyeRestPos;

    // Posición actual suavizada de cada ojo
    private Vector3 _leftEyeCurrent;
    private Vector3 _rightEyeCurrent;

    // ── Init (llamado por ChurchBoss) ─────────────────────────────────────────

    public void Initialise(Transform player)
    {
        _player = player;

        // Guardar posiciones de reposo al inicio
        if (leftEye != null)
        {
            _leftEyeRestPos = leftEye.position;
            _leftEyeCurrent = leftEye.position;
        }
        if (rightEye != null)
        {
            _rightEyeRestPos = rightEye.position;
            _rightEyeCurrent = rightEye.position;
        }
    }

    // ── Tracking ──────────────────────────────────────────────────────────────

    private void LateUpdate()
    {
        if (_player == null) return;

        TrackEye(leftEye, _leftEyeRestPos, ref _leftEyeCurrent);
        TrackEye(rightEye, _rightEyeRestPos, ref _rightEyeCurrent);
    }

    private void TrackEye(Transform eye, Vector3 restPos, ref Vector3 current)
    {
        if (eye == null) return;

        // Dirección desde la posición de reposo del ojo hacia el jugador
        Vector3 toPlayer = _player.position - restPos;

        // Normalizar la influencia según distancia (más lejos = más al límite)
        float influence = Mathf.Clamp01(toPlayer.magnitude / maxTrackingDistance);

        // Posición objetivo = reposo + dirección normalizada * radio * influencia
        Vector3 targetPos = restPos + toPlayer.normalized * (eyeSocketRadius * influence);

        // Suavizar el movimiento
        current = Vector3.Lerp(current, targetPos, trackingSpeed * Time.deltaTime);
        eye.position = current;
    }

    // ── Telegraph (para cuando añadamos zonas más adelante) ──────────────────

    public void TelegraphAttack(BossActionQueue.AttackId attackId)
    {
        // De momento los ojos siguen al player siempre.
        // Aquí se añadirá la redirección temporal cuando tengamos las zonas.
    }

    // ── Debug ─────────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        // Dibuja el radio del socket de cada ojo en el Scene View
        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        if (leftEye != null) Gizmos.DrawWireSphere(leftEye.position, eyeSocketRadius);
        if (rightEye != null) Gizmos.DrawWireSphere(rightEye.position, eyeSocketRadius);
    }
}