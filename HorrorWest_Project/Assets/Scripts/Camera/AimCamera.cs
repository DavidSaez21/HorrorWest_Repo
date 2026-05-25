using UnityEngine;

public class AimCamera : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float normalSize = 5f;
    [SerializeField] private float aimSize = 3.5f;
    [SerializeField] private float zoomSpeed = 5f;

    [Header("Aim Offset Settings")]
    [SerializeField] private float aimOffsetStrength = 0.3f;
    [SerializeField] private float offsetSpeed = 5f;

    [Header("References")]
    [SerializeField] private Transform target;

    private Camera cam;
    private float targetSize;
    private Vector3 targetPosition;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        targetSize = normalSize;
        cam.orthographicSize = normalSize;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        bool isAiming = PlayerAim.Instance != null && PlayerAim.Instance.IsAiming;
        targetSize = isAiming ? aimSize : normalSize;

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, zoomSpeed * Time.deltaTime);

        if (isAiming)
        {
            Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;

            Vector3 aimTarget = Vector3.Lerp(target.position, mouseWorld, aimOffsetStrength);
            targetPosition = new Vector3(aimTarget.x, aimTarget.y, transform.position.z);
        }
        else
        {
            targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, offsetSpeed * Time.deltaTime);
    }
}