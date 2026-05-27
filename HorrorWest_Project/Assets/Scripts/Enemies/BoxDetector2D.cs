using System.Collections;
using UnityEngine;

public class BoxDetector2D : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private LayerMask detectorLayerMask;
    [SerializeField] private Vector2 detectorSize = new Vector2(8f, 8f);
    [SerializeField] private Vector2 detectorOriginOffset = Vector2.zero;
    [SerializeField] private float detectionDelay = 0.3f;

    public bool PlayerDetected { get; private set; }
    public Transform Target { get; private set; }

    private void OnEnable()
    {
        StartCoroutine(DetectionRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        PlayerDetected = false;
        Target = null;
    }

    private IEnumerator DetectionRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(detectionDelay);

        while (true)
        {
            Vector2 origin = (Vector2)transform.position + detectorOriginOffset;
            Collider2D hit = Physics2D.OverlapBox(origin, detectorSize, 0f, detectorLayerMask);

            if (hit != null)
            {
                PlayerDetected = true;
                Target = hit.transform;
            }
            else
            {
                PlayerDetected = false;
                Target = null;
            }

            yield return wait;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = PlayerDetected ? Color.red : Color.green;
        Vector2 origin = (Vector2)transform.position + detectorOriginOffset;
        Gizmos.DrawWireCube(origin, detectorSize);
    }
}