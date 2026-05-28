using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class RebindingManager : MonoBehaviour
{
    public InputActionAsset inputActions;

    void Start()
    {
        StartCoroutine(LoadBindings());
    }

    private IEnumerator LoadBindings()
    {
        yield return null; // Espera un frame para que el PlayerInput se inicialice

        string savedBindings = PlayerPrefs.GetString("rebindings", string.Empty);
        if (!string.IsNullOrEmpty(savedBindings))
        {
            inputActions.LoadBindingOverridesFromJson(savedBindings);
            Debug.Log("Rebindings cargados correctamente");
        }
        else
        {
            Debug.Log("No hay rebindings guardados");
        }
    }
}