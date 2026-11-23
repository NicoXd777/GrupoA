using System.Collections;
using UnityEngine;

// Ponlo en los bloques de arriba.
// Cuando Pikachu los golpea desde abajo, suena el sonido y el bloque rebota.
[RequireComponent(typeof(AudioSource))]
public class BloqueSonido : MonoBehaviour
{
    public AudioClip sonido;          // arrastra aquí pika-pikachu

    AudioSource audioSource;
    Vector3 posicionOriginal;
    bool rebotando;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        posicionOriginal = transform.position;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        // Solo reacciona si el que choca es Pikachu
        if (col.gameObject.GetComponent<MovimientoPikachu>() == null) return;

        // Solo suena si Pikachu está debajo del bloque (lo golpea con la cabeza)
        if (col.transform.position.y < transform.position.y)
        {
            audioSource.PlayOneShot(sonido);
            if (!rebotando) StartCoroutine(Rebote());
        }
    }

    IEnumerator Rebote()
    {
        rebotando = true;
        float duracion = 0.2f;
        for (float t = 0f; t < duracion; t += Time.deltaTime)
        {
            float altura = Mathf.Sin(t / duracion * Mathf.PI) * 0.25f;
            transform.position = posicionOriginal + Vector3.up * altura;
            yield return null;
        }
        transform.position = posicionOriginal;
        rebotando = false;
    }
}
