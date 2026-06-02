using UnityEngine;
using UnityEngine.UI;

public class ScrollbarSync : MonoBehaviour
{
    public ScrollRect scrollRect;
    public Scrollbar scrollbarVertical;

    void Update()
    {
        scrollbarVertical.value = scrollRect.verticalNormalizedPosition;
    }

    public void OnScrollbarChanged()
    {
        scrollRect.verticalNormalizedPosition = scrollbarVertical.value;
    }
}