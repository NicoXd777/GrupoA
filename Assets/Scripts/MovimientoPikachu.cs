using UnityEngine;
using UnityEngine.InputSystem;

// Controles:
//   A          -> avanza (derecha)
//   Flecha ↑   -> retrocede (izquierda)
//   V          -> salta
public class MovimientoPikachu : MonoBehaviour
{
    public float velocidad = 5f;
    public float fuerzaSalto = 8f;

    Rigidbody2D rb;
    bool enElSuelo;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        var teclado = Keyboard.current;
        if (teclado == null) return;

        float mover = 0f;
        if (teclado.aKey.isPressed) mover = 1f;         // avanzar
        if (teclado.upArrowKey.isPressed) mover = -1f;  // retroceder

        rb.linearVelocity = new Vector2(mover * velocidad, rb.linearVelocity.y);

        // Mirar hacia donde camina (la cola queda atrás)
        if (mover != 0f)
        {
            var escala = transform.localScale;
            escala.x = Mathf.Abs(escala.x) * (mover > 0f ? -1f : 1f);
            transform.localScale = escala;
        }

        if (teclado.vKey.wasPressedThisFrame && enElSuelo)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
            enElSuelo = false;
        }
    }

    // Está en el suelo si lo que toca está debajo de él
    void OnCollisionStay2D(Collision2D col)
    {
        foreach (var contacto in col.contacts)
        {
            if (contacto.normal.y > 0.5f) enElSuelo = true;
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        enElSuelo = false;
    }
}
