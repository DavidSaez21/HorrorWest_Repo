using UnityEngine;

public class PersistentCanvas : MonoBehaviour
{
    [SerializeField] private string canvasId = "default";
    private static System.Collections.Generic.Dictionary<string, PersistentCanvas> _instances = new();

    private void Awake()
    {
        if (_instances.ContainsKey(canvasId) && _instances[canvasId] != null)
        {
            Destroy(gameObject);
            return;
        }
        _instances[canvasId] = this;
        DontDestroyOnLoad(gameObject);
    }
}