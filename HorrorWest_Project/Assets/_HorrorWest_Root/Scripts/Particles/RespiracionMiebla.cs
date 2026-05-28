using UnityEngine;

public class RespiracionNiebla : MonoBehaviour
{
    [Header("Material y Referencias del Shader")]
    public Material materialNiebla;
    public string refColor = "_ColorNiebla";
    public string refVelocidad = "_Velocidad";

    [Header("Color Progresivo")]
    public Gradient gradienteColor;
    [Tooltip("Segundos que tarda en recorrer el gradiente completo")]
    public float duracionCicloColor = 10f;

    [Header("Movimiento Aleatorio (Viento)")]
    public Vector2 velocidadMinima = new Vector2(0.01f, 0f);
    public Vector2 velocidadMaxima = new Vector2(0.4f, 0f);

    [Tooltip("Cada cuántos segundos el viento cambia de fuerza")]
    public float tiempoEntreCambios = 4f;

    [Tooltip("Qué tan suave es la transición entre la velocidad vieja y la nueva")]
    public float suavidadTransicion = 1.5f;

    private Vector2 velocidadActual;
    private Vector2 velocidadDestino;
    private float timerViento = 0f;

    void Start()
    {
        if (materialNiebla != null)
        {
            // Arrancamos con una velocidad aleatoria inicial
            velocidadActual = new Vector2(Random.Range(velocidadMinima.x, velocidadMaxima.x), Random.Range(velocidadMinima.y, velocidadMaxima.y));
            velocidadDestino = velocidadActual;
            materialNiebla.SetVector(refVelocidad, velocidadActual);
        }
    }

    void Update()
    {
        if (materialNiebla == null) return;

        // 1. COLOR PROGRESIVO (Se mantiene igual, fluido e infinito)
        float tiempoColor = Mathf.PingPong(Time.time / duracionCicloColor, 1f);
        materialNiebla.SetColor(refColor, gradienteColor.Evaluate(tiempoColor));

        // 2. LÓGICA DE VIENTO ALEATORIO
        timerViento += Time.deltaTime;

        // Si es hora de cambiar el viento...
        if (timerViento >= tiempoEntreCambios)
        {
            // Escogemos una nueva velocidad al azar entre los límites que pongas
            velocidadDestino = new Vector2(
                Random.Range(velocidadMinima.x, velocidadMaxima.x),
                Random.Range(velocidadMinima.y, velocidadMaxima.y)
            );

            // Reiniciamos el reloj del viento
            timerViento = 0f;
        }

        // 3. TRANSICIÓN SUAVE DE VELOCIDAD
        // Lerp empuja la velocidad actual hacia la de destino suavemente fotograma a fotograma
        velocidadActual = Vector2.Lerp(velocidadActual, velocidadDestino, Time.deltaTime * suavidadTransicion);
        materialNiebla.SetVector(refVelocidad, velocidadActual);
    }
}