using UnityEngine;
using System.Collections.Generic;

public class PersistentCanvas : MonoBehaviour
{
    public static PersistentCanvas HUDInstance { get; private set; }

    [SerializeField] private string canvasId = "default";
    private static Dictionary<string, PersistentCanvas> _instances = new();

    private void Awake()
    {
        if (_instances.ContainsKey(canvasId) && _instances[canvasId] != null)
        {
            Destroy(gameObject);
            return;
        }
        _instances[canvasId] = this;
        DontDestroyOnLoad(gameObject);

        if (canvasId == "hud")
            HUDInstance = this;
    }
}