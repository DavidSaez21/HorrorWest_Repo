using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class LayeredImageScene : MonoBehaviour
{
    [Header("Capas de imagen (en orden de aparición)")]
    [Tooltip("Índice 0 = imagen base (ya visible). El resto aparecen en orden al hacer click.")]
    public Image[] layers;

    [Header("Configuración")]
    [Tooltip("Duración del fade-in de cada capa en segundos")]
    public float fadeDuration = 1.0f;
    [Tooltip("Nombre de la escena a cargar cuando se acaben las imágenes")]
    public string nextSceneName;

    private int _currentLayerIndex = 0;
    private bool _isFading = false;
    private PlayerInput _playerInput;

    void Start()
    {
        // Desactiva el input del player para que no dispare
        _playerInput = FindFirstObjectByType<PlayerInput>();
        if (_playerInput != null) _playerInput.DeactivateInput();

        if (layers == null || layers.Length == 0)
        {
            Debug.LogError("[LayeredImageScene] No hay capas asignadas.");
            return;
        }

        SetAlpha(layers[0], 1f);
        for (int i = 1; i < layers.Length; i++)
            SetAlpha(layers[i], 0f);

        _currentLayerIndex = 1;
    }

    void Update()
    {
        if (_isFading) return;
        if (Input.GetMouseButtonDown(0))
            HandleClick();
    }

    void HandleClick()
    {
        if (_currentLayerIndex < layers.Length)
        {
            StartCoroutine(FadeInLayer(layers[_currentLayerIndex]));
            _currentLayerIndex++;
        }
        else
        {
            LoadNextScene();
        }
    }

    IEnumerator FadeInLayer(Image img)
    {
        _isFading = true;
        float elapsed = 0f;
        Color c = img.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / fadeDuration);
            img.color = c;
            yield return null;
        }
        c.a = 1f;
        img.color = c;
        _isFading = false;
    }

    void LoadNextScene()
    {
        // Reactiva el input del player al salir
        if (_playerInput != null) _playerInput.ActivateInput();

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogWarning("[LayeredImageScene] 'nextSceneName' está vacío.");
    }

    static void SetAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}