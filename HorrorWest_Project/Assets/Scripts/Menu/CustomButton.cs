using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("Animacion")]
    public float hoverScale = 1.15f;
    public float animSpeed = 8f;

    [Header("Evento al hacer click")]
    public UnityEvent onClicked;

    private Vector3 originalScale;
    private bool isHovered = false;
    private SpriteRenderer sr;
    private Color normalColor;
    public Color hoverColor = Color.yellow;

    void Start()
    {
        originalScale = transform.localScale;
        sr = GetComponent<SpriteRenderer>();
        normalColor = sr.color;
    }

    void Update()
    {
        Vector3 targetScale = isHovered ? originalScale * hoverScale : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animSpeed);
    }

    private void OnDisable()
    {
        isHovered = false;
        transform.localScale = originalScale;
        if (sr != null)
            sr.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        sr.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        sr.color = normalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        onClicked.Invoke();
    }
}