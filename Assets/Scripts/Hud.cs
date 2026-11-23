using UnityEngine;

// Cartel arriba a la izquierda con el nombre y los controles.
public class Hud : MonoBehaviour
{
    void OnGUI()
    {
        float escala = Screen.height / 1080f;
        var caja = new Rect(20 * escala, 20 * escala, 520 * escala, 120 * escala);

        GUI.color = new Color(0f, 0f, 0f, 0.35f);
        GUI.DrawTexture(caja, Texture2D.whiteTexture);
        GUI.color = Color.white;

        var titulo = new GUIStyle(GUI.skin.label)
        {
            fontSize = (int)(44 * escala),
            fontStyle = FontStyle.Bold
        };
        titulo.normal.textColor = new Color(1f, 0.85f, 0.23f);
        GUI.Label(new Rect(caja.x + 18 * escala, caja.y + 8 * escala, caja.width, 60 * escala), "GRUPO A", titulo);

        var controles = new GUIStyle(GUI.skin.label) { fontSize = (int)(26 * escala) };
        controles.normal.textColor = Color.white;
        GUI.Label(new Rect(caja.x + 18 * escala, caja.y + 66 * escala, caja.width, 40 * escala),
            "A: avanzar    ↑: retroceder    V: saltar", controles);
    }
}
