using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [Header("Referencias")]
    [SerializeField] private Image fadeImage;

    [Header("Configuración")]
    [SerializeField] private float fadeDuration = 0.5f;

    // Escenas que gestionan sus propias transiciones
    private readonly string[] _skipFadeScenes = { "SCN_Victory", "SCN_Death" };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Empieza transparente
        SetAlpha(0f);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (ShouldSkip(scene.name)) return;
        StartCoroutine(FadeIn());
    }

    public void FadeToScene(string sceneName)
    {
        if (ShouldSkip(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            return;
        }
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeIn()
    {
        SetAlpha(1f);
        yield return StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator FadeOut()
    {
        yield return StartCoroutine(Fade(0f, 1f));
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(from, to, elapsed / fadeDuration));
            yield return null;
        }
        SetAlpha(to);
    }

    private void SetAlpha(float alpha)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }

    private bool ShouldSkip(string sceneName)
    {
        foreach (string s in _skipFadeScenes)
            if (s == sceneName) return true;
        return false;
    }
}