using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RemapButton : MonoBehaviour, IPointerDownHandler
{
    [Header("Configuracion")]
    public string actionName;
    public int bindingIndex;
    public Image keyImage;

    [Header("Sprites")]
    public string spritesPath = "Keys";

    [Header("Input")]
    public InputActionAsset inputActions;

    private bool _isWaiting = false;

    void Start()
    {
        string savedBindings = PlayerPrefs.GetString("rebindings", string.Empty);
        if (!string.IsNullOrEmpty(savedBindings))
            inputActions.LoadBindingOverridesFromJson(savedBindings);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_isWaiting)
            StartRebind();
    }

    private void StartRebind()
    {
        _isWaiting = true;

        InputAction action = inputActions.FindAction(actionName);
        action.Disable();

        action.PerformInteractiveRebinding()
            .WithTargetBinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(operation =>
            {
                operation.Dispose();
                action.Enable();
                _isWaiting = false;

                string bindings = inputActions.SaveBindingOverridesAsJson();
                PlayerPrefs.SetString("rebindings", bindings);
                PlayerPrefs.Save();

                string path = action.bindings[bindingIndex].effectivePath;
                string keyName = path.Split('/')[1].ToLower();

                Sprite[] sprites = Resources.LoadAll<Sprite>(spritesPath + "/" + keyName.ToUpper());
                Sprite newSprite = sprites.Length > 0 ? sprites[0] : null;

                if (newSprite != null && keyImage != null)
                    keyImage.sprite = newSprite;
            })
            .OnCancel(operation =>
            {
                operation.Dispose();
                action.Enable();
                _isWaiting = false;
            })
            .Start();
    }
}