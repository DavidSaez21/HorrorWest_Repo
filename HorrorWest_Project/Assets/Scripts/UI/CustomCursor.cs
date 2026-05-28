using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    public RectTransform cursorVisual;
    public Vector2 hotspot = Vector2.zero;

    void Start()
    {
        Cursor.visible = false;
    }

    void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        cursorVisual.position = new Vector3(mousePos.x - hotspot.x, mousePos.y - hotspot.y, 0);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        Cursor.visible = !hasFocus;
    }
}