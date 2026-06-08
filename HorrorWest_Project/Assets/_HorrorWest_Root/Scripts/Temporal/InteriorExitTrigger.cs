using UnityEngine;

public class InteriorExitTrigger : MonoBehaviour
{
    [SerializeField] private BarEntrance barEntrance;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            barEntrance.ExitInterior();
    }
}