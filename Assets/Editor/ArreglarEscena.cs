using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Revisa y arregla la escena una vez (al salir de Play / al cargar).
// También se puede usar desde el menú: GrupoA → Arreglar escena.
[InitializeOnLoad]
public static class ArreglarEscena
{
    const string Clave = "GrupoA_EscenaArreglada_v1";

    static ArreglarEscena()
    {
        if (SessionState.GetBool(Clave, false)) return;
        EditorApplication.delayCall += Intentar;
        EditorApplication.playModeStateChanged += estado =>
        {
            if (estado == PlayModeStateChange.EnteredEditMode)
                EditorApplication.delayCall += Intentar;
        };
    }

    static void Intentar()
    {
        if (SessionState.GetBool(Clave, false)) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        SessionState.SetBool(Clave, true);
        Arreglar();
    }

    [MenuItem("GrupoA/Arreglar escena")]
    public static void Arreglar()
    {
        // 1. Buscar a Pikachu
        GameObject pikachu = null;
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            string n = t.name.ToLower();
            if (n.Contains("pikachu") || n.Contains("picachu") || n.Contains("pikachi") || n.Contains("picachi"))
            {
                if (pikachu == null || t.parent == null) pikachu = t.gameObject;
            }
        }
        if (pikachu == null)
        {
            var mov = Object.FindFirstObjectByType<MovimientoPikachu>(FindObjectsInactive.Include);
            if (mov != null) pikachu = mov.transform.root.gameObject;
        }
        if (pikachu == null)
        {
            Debug.LogError("[GrupoA] No encontré a Pikachu. Ponle de nombre 'Pikachu' al objeto padre y usa GrupoA → Arreglar escena.");
            return;
        }
        pikachu = pikachu.transform.root.gameObject;
        pikachu.SetActive(true);

        // 2. Las piezas (hijos) no deben tener física propia
        foreach (var rb in pikachu.GetComponentsInChildren<Rigidbody2D>(true))
            if (rb.gameObject != pikachu) Object.DestroyImmediate(rb);
        foreach (var mov in pikachu.GetComponentsInChildren<MovimientoPikachu>(true))
            if (mov.gameObject != pikachu) Object.DestroyImmediate(mov);
        foreach (var c in pikachu.GetComponentsInChildren<Collider2D>(true))
            if (c.gameObject != pikachu) Object.DestroyImmediate(c);

        // 3. El padre: Rigidbody 2D + collider + script
        var cuerpo = pikachu.GetComponent<Rigidbody2D>();
        if (cuerpo == null) cuerpo = pikachu.AddComponent<Rigidbody2D>();
        cuerpo.bodyType = RigidbodyType2D.Dynamic;
        cuerpo.simulated = true;
        if (cuerpo.gravityScale <= 0f) cuerpo.gravityScale = 1f;
        cuerpo.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (pikachu.GetComponent<Collider2D>() == null)
        {
            var cap = pikachu.AddComponent<CapsuleCollider2D>();
            cap.size = new Vector2(0.9f, 1.3f);
        }

        var movimiento = pikachu.GetComponent<MovimientoPikachu>();
        if (movimiento == null) movimiento = pikachu.AddComponent<MovimientoPikachu>();
        movimiento.enabled = true;

        // 4. Pisos y bloques: collider; los de arriba además con sonido
        var sonido = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sonidos/pika-pikachu.mp3");
        int pisos = 0, bloques = 0;
        foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (sr.transform.IsChildOf(pikachu.transform)) continue;
            if (sr.transform.parent != null) continue;

            var go = sr.gameObject;
            if (go.GetComponent<Collider2D>() == null) go.AddComponent<BoxCollider2D>();

            if (go.transform.position.y > -1f)
            {
                var bloque = go.GetComponent<BloqueSonido>();
                if (bloque == null) bloque = go.AddComponent<BloqueSonido>();
                if (bloque.sonido == null) bloque.sonido = sonido;
                bloques++;
            }
            else
            {
                pisos++;
            }
        }

        // 5. Guardar la escena
        var escena = pikachu.scene;
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);

        Debug.Log($"[GrupoA] Escena arreglada y guardada: Pikachu listo, {pisos} pisos, {bloques} bloques con sonido.");
    }
}
