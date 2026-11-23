using UnityEngine;
using UnityEngine.InputSystem;

// Zona invisible debajo del hueco del centro.
// Si Pikachu cae aquí: "¡OH NOOO!" + "GAME OVER". Con R vuelve a empezar.
public class ZonaCaida : MonoBehaviour
{
    public AudioClip sonidoCaida;     // arrastra aquí pikachu (pika pika pikaaa)

    AudioSource audioSource;
    MovimientoPikachu pikachu;
    Rigidbody2D rb;
    Vector3 inicio;
    bool gameOver;
    float tiempo;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        pikachu = FindFirstObjectByType<MovimientoPikachu>();
        if (pikachu != null)
        {
            inicio = pikachu.transform.position;
            rb = pikachu.GetComponent<Rigidbody2D>();
        }
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (gameOver || pikachu == null) return;
        if (otro.GetComponentInParent<MovimientoPikachu>() == null) return;

        gameOver = true;
        tiempo = 0f;
        pikachu.enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        if (sonidoCaida != null) audioSource.PlayOneShot(sonidoCaida);
    }

    void Update()
    {
        if (!gameOver) return;
        tiempo += Time.deltaTime;

        var teclado = Keyboard.current;
        if (teclado != null && tiempo > 0.5f &&
            (teclado.rKey.wasPressedThisFrame || teclado.enterKey.wasPressedThisFrame))
        {
            Reiniciar();
        }
    }

    void Reiniciar()
    {
        audioSource.Stop();
        pikachu.transform.position = inicio;
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;
        pikachu.enabled = true;
        gameOver = false;
    }

    void OnGUI()
    {
        if (!gameOver) return;

        float escala = Screen.height / 1080f;

        // Fondo oscuro que aparece poco a poco
        GUI.color = new Color(0f, 0f, 0f, Mathf.Clamp01(tiempo * 2f) * 0.65f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Las letras "saltan" al aparecer
        float t = Mathf.Clamp01(tiempo * 3f);
        float rebote = 1f + Mathf.Sin(t * Mathf.PI) * 0.25f;

        float centroY = Screen.height / 2f;
        // Más abajo para que no choque con los bloques de arriba
        Texto("¡OH NOOO!", centroY - 10 * escala, (int)(100 * escala * rebote), new Color(1f, 0.85f, 0.23f));
        Texto("GAME OVER", centroY + 140 * escala, (int)(140 * escala * rebote), new Color(1f, 0.25f, 0.2f));

        if (tiempo > 0.5f && Mathf.Repeat(tiempo, 1f) < 0.7f)
            Texto("Presiona R para volver a intentar", centroY + 280 * escala, (int)(42 * escala), Color.white);
    }

    static void Texto(string texto, float y, int tamano, Color color)
    {
        var estilo = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.Max(tamano, 10),
            fontStyle = FontStyle.Bold
        };
        float alto = tamano * 1.6f;
        var rect = new Rect(0, y - alto / 2f, Screen.width, alto);

        // Sombra
        estilo.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
        float sombra = Mathf.Max(2f, tamano / 25f);
        GUI.Label(new Rect(rect.x + sombra, rect.y + sombra, rect.width, rect.height), texto, estilo);

        estilo.normal.textColor = color;
        GUI.Label(rect, texto, estilo);
    }
}
