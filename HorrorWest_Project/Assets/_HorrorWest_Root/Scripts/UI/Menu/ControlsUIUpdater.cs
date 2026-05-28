using UnityEngine;
using UnityEngine.InputSystem;

public class ControlsUIUpdater : MonoBehaviour
{
    [Header("Input")]
    public InputActionAsset inputActions;

    [Header("Sprites Path")]
    public string spritesPath = "Keys";

    [Header("Key Sprites")]
    public SpriteRenderer icon_W;
    public SpriteRenderer icon_S;
    public SpriteRenderer icon_A;
    public SpriteRenderer icon_D;
    public SpriteRenderer icon_Fire;
    public SpriteRenderer icon_Aim;
    public SpriteRenderer icon_Reload;

    void OnEnable()
    {
        UpdateIcons();
    }

    private void UpdateIcons()
    {
        UpdateIcon(icon_W, "Move", 1);
        UpdateIcon(icon_S, "Move", 2);
        UpdateIcon(icon_A, "Move", 3);
        UpdateIcon(icon_D, "Move", 4);
        UpdateIcon(icon_Fire, "Fire", 0);
        UpdateIcon(icon_Aim, "Aim", 0);
        UpdateIcon(icon_Reload, "Reload", 0);
    }

    private void UpdateIcon(SpriteRenderer sr, string actionName, int bindingIndex)
    {
        if (sr == null) return;

        InputAction action = inputActions.FindAction(actionName);
        if (action == null) return;

        string path = action.bindings[bindingIndex].effectivePath;
        if (string.IsNullOrEmpty(path)) return;

        string keyName = path.Split('/')[1].ToLower();

        Sprite[] sprites = Resources.LoadAll<Sprite>(spritesPath + "/" + keyName.ToUpper());
        if (sprites.Length > 0)
            sr.sprite = sprites[0];
    }
}