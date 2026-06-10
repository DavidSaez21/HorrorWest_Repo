using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class VictoryManager : MonoBehaviour
{
    [Header("Paneles")]
    public Image panelBlack;
    public Image imageVictory;
    public Image imageText;
    public Image btnMainMenuImage;
    public TMP_Text btnMainMenuText;

    [Header("Tiempos")]
    public float fadeInDuration = 2f;
    public float textFadeInDuration = 1.5f;
    public float delayBeforeText = 1f;
    public float delayBeforeButton = 4f;
    public float buttonFadeInDuration = 1f;

    void Start()
    {
        StartCoroutine(VictorySequence());
    }

    private IEnumerator VictorySequence()
    {
        // Fade in de la imagen de victoria (panel negro desaparece)
        float elapsed = 0f;
        Color c = panelBlack.color;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / fadeInDuration);
            panelBlack.color = c;
            yield return null;
        }
        c.a = 0f;
        panelBlack.color = c;

        // Espera antes de mostrar la imagen de texto
        yield return new WaitForSeconds(delayBeforeText);

        // Fade in de la imagen con texto
        elapsed = 0f;
        Color ct = imageText.color;
        while (elapsed < textFadeInDuration)
        {
            elapsed += Time.deltaTime;
            ct.a = Mathf.Lerp(0f, 1f, elapsed / textFadeInDuration);
            imageText.color = ct;
            yield return null;
        }
        ct.a = 1f;
        imageText.color = ct;

        // Espera antes de mostrar el botón
        yield return new WaitForSeconds(delayBeforeButton);

        // Fade in del botón
        elapsed = 0f;
        Color cb = btnMainMenuImage.color;
        Color cbt = btnMainMenuText.color;
        while (elapsed < buttonFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / buttonFadeInDuration);
            cb.a = alpha;
            cbt.a = alpha;
            btnMainMenuImage.color = cb;
            btnMainMenuText.color = cbt;
            yield return null;
        }
        cb.a = 1f;
        cbt.a = 1f;
        btnMainMenuImage.color = cb;
        btnMainMenuText.color = cbt;
    }
}