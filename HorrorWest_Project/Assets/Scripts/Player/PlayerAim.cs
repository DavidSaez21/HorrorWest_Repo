using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    public static PlayerAim Instance { get; private set; }

    [Header("Aim Settings")]
    [SerializeField] private float aimSpeedMultiplier = 0.5f;
    [SerializeField] private float aimCritMultiplier = 2f;

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;

    public bool IsAiming { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsAiming = true;
            playerMovement.SetAimSpeedMultiplier(aimSpeedMultiplier);
        }

        if (context.canceled)
        {
            IsAiming = false;
            playerMovement.SetAimSpeedMultiplier(1f);
        }
    }

    public float GetCritMultiplier() => IsAiming ? aimCritMultiplier : 1f;
}