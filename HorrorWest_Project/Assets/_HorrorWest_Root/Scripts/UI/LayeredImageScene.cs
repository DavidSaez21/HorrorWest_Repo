using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class LayeredImageScene : MonoBehaviour
{
    [Header("Capas de imagen (en orden de aparición)")]
    public Image[] layers;

    [Header("Configuración")]
    public float fadeDuration = 1.0f;
    public float displayDuration = 2.0f;
    public float delayAfterLast = 2.0f;
    public string nextSceneName;

    private PlayerInput _playerInput;

    void Start()
    {
        _playerInput = FindFirstObjectByType<PlayerInput>();
        if (_playerInput != null) _playerInput.DeactivateInput();

        GameObject hud = GameObject.Find("HUDCanva");
        if (hud != null) hud.SetActive(false);

        if (layers == null || layers.Length == 0)
        {
            Debug.LogError("[LayeredImageScene] No hay capas asignadas.");
            return;
        }

        SetAlpha(layers[0], 1f);
        for (int i = 1; i < layers.Length; i++)
            SetAlpha(layers[i], 0f);

        StartCoroutine(AutoPlaySequence());
    }

    private IEnumerator AutoPlaySequence()
    {
        yield return new WaitForSeconds(displayDuration);

        for (int i = 1; i < layers.Length; i++)
        {
            yield return StartCoroutine(FadeInLayer(layers[i]));
            yield return new WaitForSeconds(displayDuration);
        }

        yield return new WaitForSeconds(delayAfterLast);

        LoadNextScene();
    }

    IEnumerator FadeInLayer(Image img)
    {
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
    }

    void LoadNextScene()
    {
        if (_playerInput != null) _playerInput.ActivateInput();

        GameObject hud = GameObject.Find("HUDCanva");
        if (hud != null) hud.SetActive(true);

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneFader.Instance?.FadeToScene(nextSceneName);
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