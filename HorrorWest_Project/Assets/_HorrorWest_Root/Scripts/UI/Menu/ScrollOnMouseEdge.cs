using UnityEngine;
using UnityEngine.UI;

public class ScrollOnMouseEdge : MonoBehaviour
{
    public ScrollRect scrollRect;
    [Range(0f, 0.3f)]
    public float edgeThreshold = 0.1f; // zona del borde en porcentaje de pantalla
    public float scrollSpeed = 0.5f;

    void Update()
    {
        float mouseY = Input.mousePosition.y / Screen.height;

        if (mouseY < edgeThreshold)
        {
            // Cursor cerca del borde inferior, baja el panel
            scrollRect.verticalNormalizedPosition -= scrollSpeed * Time.deltaTime;
        }
        else if (mouseY > 1f - edgeThreshold)
        {
            // Cursor cerca del borde superior, sube el panel
            scrollRect.verticalNormalizedPosition += scrollSpeed * Time.deltaTime;
        }

        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
    }
}