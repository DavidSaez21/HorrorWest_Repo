using UnityEngine;
using System.Collections;

public class GenerarSangre2D : MonoBehaviour
{
    [Header("Efecto de Sangre")]
    public GameObject prefabCharcoSangre; // Arrastra tu nuevo prefab aquí
    public float yOffset = -0.5f; // Ajusta esto para que salga en los pies, no en la cabeza

    [ContextMenu("Simular Muerte")]
    public void Morir()
    {
        // 1. Calculamos la posición donde aparecerá (la posición del personaje + el offset de los pies)
        Vector3 posicionAparicion = transform.position;
        posicionAparicion.y += yOffset;

        // 2. Creamos la sangre. Usamos Quaternion.identity porque en 2D no necesitamos rotarla.
        GameObject sangre = Instantiate(prefabCharcoSangre, posicionAparicion, Quaternion.identity);

        // 3. (Opcional) Hacer que el charco empiece pequeño y crezca para darle un efecto jugoso
        sangre.transform.localScale = Vector3.zero; // Empieza en tamaño 0
        StartCoroutine(CrecerCharco(sangre.transform));
    }

    IEnumerator CrecerCharco(Transform transformSangre)
    {
        float tiempoTranscurrido = 0;
        float tiempoCrecimiento = 0.5f; // Medio segundo en crecer

        // Asumiendo que el tamaño original del prefab es 1. Puedes cambiar Vector3.one por otro valor si tu prefab es más grande.
        Vector3 escalaFinal = Vector3.one;

        while (tiempoTranscurrido < tiempoCrecimiento)
        {
            transformSangre.localScale = Vector3.Lerp(Vector3.zero, escalaFinal, (tiempoTranscurrido / tiempoCrecimiento));
            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        transformSangre.localScale = escalaFinal;
    }
}