using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeathFade : MonoBehaviour
{
    public Image panelRed;
    public float fadeInDuration = 1.5f;

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        Color c = panelRed.color;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / fadeInDuration);
            panelRed.color = c;
            yield return null;
        }
        c.a = 0f;
        panelRed.color = c;
        panelRed.raycastTarget = false;
    }
}