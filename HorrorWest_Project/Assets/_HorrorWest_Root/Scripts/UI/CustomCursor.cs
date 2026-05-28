using UnityEngine;
using UnityEngine.UI;

public class CustomCursor : MonoBehaviour
{
    public RectTransform cursorVisual;
    public Vector2 hotspot = Vector2.zero;
    public Image cursorImage;
    public Color normalColor = Color.white;
    public Color enemyColor = Color.red;

    void Start()
    {
        Cursor.visible = false;
    }

    void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        cursorVisual.position = new Vector3(mousePos.x - hotspot.x, mousePos.y - hotspot.y, 0);

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        Collider2D hit = Physics2D.OverlapPoint(worldPos);

        if (hit != null && hit.CompareTag("Enemy"))
        {
            Debug.Log("Enemigo detectado, cambiando color");
            cursorImage.color = enemyColor;
        }
        else
        {
            cursorImage.color = normalColor;
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        Cursor.visible = !hasFocus;
    }
}