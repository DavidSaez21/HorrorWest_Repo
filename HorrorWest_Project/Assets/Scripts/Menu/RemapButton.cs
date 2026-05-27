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

    private PlayerInputActions _inputActions;
    private bool _isWaiting = false;

    void Start()
    {
        _inputActions = new PlayerInputActions();
        _inputActions.Enable();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_isWaiting)
            StartRebind();
    }

    private void StartRebind()
    {
        _isWaiting = true;

        InputAction action = _inputActions.asset.FindAction(actionName);
        action.Disable();

        action.PerformInteractiveRebinding()
            .WithTargetBinding(bindingIndex)
            .WithControlsExcluding("Mouse")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(operation =>
            {
                operation.Dispose();
                action.Enable();
                _isWaiting = false;

                string path = action.bindings[bindingIndex].effectivePath;
                string keyName = path.Split('/')[1].ToLower();

                Debug.Log("Path completo: " + path);
                Debug.Log("Nombre extraido: " + keyName);

                Sprite[] sprites = Resources.LoadAll<Sprite>(spritesPath + "/" + keyName);
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

    void OnDestroy()
    {
        _inputActions.Dispose();
    }
}