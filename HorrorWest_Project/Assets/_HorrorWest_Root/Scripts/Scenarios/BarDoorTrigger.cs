using UnityEngine;

public class BarDoorTrigger : MonoBehaviour
{
    [SerializeField] private BarEntrance barEntrance;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            barEntrance.EnterBar();
    }
}