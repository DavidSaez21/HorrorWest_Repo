using System.Collections;
using UnityEngine;

/// <summary>
/// Optional proximity detector. No longer required by EnemyMovement — enemies
/// always know where the player is. Keep this if you need a trigger for sound
/// cues, animations, or other gameplay events when the player enters a zone.
/// </summary>
public class BoxDetector2D : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private LayerMask detectorLayerMask;
    [SerializeField] private Vector2 detectorSize = new Vector2(8f, 8f);
    [SerializeField] private Vector2 detectorOriginOffset = Vector2.zero;
    [SerializeField] private float detectionDelay = 0.25f;

    public bool PlayerDetected { get; private set; }
    public Transform Target { get; private set; }

    private void OnEnable() => StartCoroutine(DetectionRoutine());
    private void OnDisable() { StopAllCoroutines(); PlayerDetected = false; Target = null; }

    private IEnumerator DetectionRoutine()
    {
        var wait = new WaitForSeconds(detectionDelay);
        while (true)
        {
            Vector2 origin = (Vector2)transform.position + detectorOriginOffset;
            Collider2D hit = Physics2D.OverlapBox(origin, detectorSize, 0f, detectorLayerMask);

            PlayerDetected = hit != null;
            Target = hit != null ? hit.transform : null;

            yield return wait;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = PlayerDetected ? Color.red : Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position + detectorOriginOffset, detectorSize);
    }
}