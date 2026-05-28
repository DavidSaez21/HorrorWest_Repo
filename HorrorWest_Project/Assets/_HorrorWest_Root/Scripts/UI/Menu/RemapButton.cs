using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class RemapButton : MonoBehaviour, IPointerDownHandler
{
    [Header("Configuracion")]
    public string actionName;
    public int bindingIndex;
    public SpriteRenderer keySprite;

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
                Debug.Log("Guardado: " + bindings);

                string path = action.bindings[bindingIndex].effectivePath;
                string keyName = path.Split('/')[1].ToLower();

                Sprite[] sprites = Resources.LoadAll<Sprite>(spritesPath + "/" + keyName.ToUpper());
                Sprite newSprite = sprites.Length > 0 ? sprites[0] : null;

                if (newSprite != null)
                    keySprite.sprite = newSprite;
                else
                    Debug.Log("Sprite no encontrado: " + keyName);
            })
            .OnCancel(operation =>
            {
                operation.Dispose();
                action.Enable();
                _isWaiting = false;
                Debug.Log("Rebinding cancelado");
            })
            .Start();
    }
}