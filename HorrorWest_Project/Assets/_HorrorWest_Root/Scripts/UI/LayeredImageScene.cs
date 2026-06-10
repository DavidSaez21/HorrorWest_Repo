using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Gestiona una escena de imágenes en capas (lore/cinemática).
/// Coloca este script en un GameObject vacío en la escena.
/// 
/// SETUP:
/// 1. Crea un Canvas con un Image base (siempre visible).
/// 2. Añade N imágenes más encima (todas con alpha = 0 al inicio).
/// 3. Arrastra las imágenes en orden en el array 'layers' (la [0] es la base).
/// 4. Pon el nombre exacto de la siguiente escena en 'nextSceneName'.
/// </summary>
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

    // ── estado interno ──────────────────────────────────────────
    private int _currentLayerIndex = 0; // siguiente capa a mostrar
    private bool _isFading = false;

    void Start()
    {
        if (layers == null || layers.Length == 0)
        {
            Debug.LogError("[LayeredImageScene] No hay capas asignadas.");
            return;
        }

        // Aseguramos que la capa base sea totalmente visible
        SetAlpha(layers[0], 1f);

        // El resto arrancan invisibles
        for (int i = 1; i < layers.Length; i++)
            SetAlpha(layers[i], 0f);

        // La primera capa ya está mostrada, el próximo click mostrará la [1]
        _currentLayerIndex = 1;
    }

    void Update()
    {
        // Acepta click de ratón O tap en pantalla táctil
        if (_isFading) return;

        if (Input.GetMouseButtonDown(0))
            HandleClick();
    }

    void HandleClick()
    {
        if (_currentLayerIndex < layers.Length)
        {
            // Hay más capas: hacer fade-in de la siguiente
            StartCoroutine(FadeInLayer(layers[_currentLayerIndex]));
            _currentLayerIndex++;
        }
        else
        {
            // Se acabaron las capas: ir a la siguiente escena
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
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogWarning("[LayeredImageScene] 'nextSceneName' está vacío. Configúralo en el Inspector.");
    }

    // Utilidad: cambia alpha sin tocar RGB
    static void SetAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}