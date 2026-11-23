using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Arma la decoración, los bordes y la zona de caída del nivel, y guarda la escena.
// Corre una vez solo; también desde el menú: GrupoA → Diseñar nivel.
[InitializeOnLoad]
public static class DisenarNivel
{
    const string Clave = "GrupoA_Diseno_v4";

    static DisenarNivel()
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
        Disenar();
    }

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }

    [MenuItem("GrupoA/Diseñar nivel")]
    public static void Disenar()
    {
        var mov = Object.FindFirstObjectByType<MovimientoPikachu>(FindObjectsInactive.Include);
        if (mov == null)
        {
            Debug.LogError("[GrupoA] No encontré a Pikachu con el script MovimientoPikachu.");
            return;
        }
        var pikachu = mov.gameObject;

        // Pisos y bloques
        var pisos = new List<SpriteRenderer>();
        var bloques = new List<SpriteRenderer>();
        foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (sr.transform.IsChildOf(pikachu.transform) || sr.transform.parent != null) continue;
            if (sr.GetComponent<BloqueSonido>() != null) bloques.Add(sr);
            else if (sr.transform.position.y < -1f) pisos.Add(sr);
        }
        if (pisos.Count < 2)
        {
            Debug.LogError("[GrupoA] No encontré los 2 pisos.");
            return;
        }
        pisos.Sort((a, b) => a.bounds.center.x.CompareTo(b.bounds.center.x));
        var pisoIzq = pisos[0];
        var pisoDer = pisos[pisos.Count - 1];

        float izq = pisoIzq.bounds.min.x;
        float der = pisoDer.bounds.max.x;
        float huecoIzq = pisoIzq.bounds.max.x;
        float huecoDer = pisoDer.bounds.min.x;
        float sueloArriba = Mathf.Max(pisoIzq.bounds.max.y, pisoDer.bounds.max.y);
        float sueloAbajo = Mathf.Min(pisoIzq.bounds.min.y, pisoDer.bounds.min.y);

        // Sprites y material de base (los mismos que ya usa la escena)
        Sprite cuadrado = pisoIzq.sprite;
        Material material = pisoIzq.sharedMaterial;

        // Cámara: azul noche
        var cam = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
        Vector2 camCentro = Vector2.zero;
        float camAlto = 10f, camAncho = 17.8f;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Hex("#0B1440");
            camAlto = cam.orthographicSize * 2f;
            camAncho = camAlto * 16f / 9f;
            camCentro = cam.transform.position;
        }

        // Decoración (se borra y se vuelve a crear: ya sin sol, nubes ni colinas)
        var viejo = GameObject.Find("Decoracion");
        if (viejo != null) Object.DestroyImmediate(viejo);
        var deco = new GameObject("Decoracion").transform;

        // Fondo de noche (imagen) que cubre toda la cámara
        const string rutaFondo = "Assets/Sprites/FondoNoche.png";
        var importador = AssetImporter.GetAtPath(rutaFondo) as TextureImporter;
        if (importador != null &&
            (importador.textureType != TextureImporterType.Sprite || importador.spriteImportMode != SpriteImportMode.Single))
        {
            importador.textureType = TextureImporterType.Sprite;
            importador.spriteImportMode = SpriteImportMode.Single;
            importador.maxTextureSize = 2048;
            importador.SaveAndReimport();
        }
        var fondo = AssetDatabase.LoadAssetAtPath<Sprite>(rutaFondo);
        if (fondo != null)
        {
            var tam = fondo.bounds.size;
            float escala = Mathf.Max(camAncho / tam.x, camAlto / tam.y);
            Crear("Fondo", deco, fondo, material, camCentro, Vector2.one * escala, Color.white, -100);
        }
        else
        {
            Debug.LogWarning("[GrupoA] No encontré Assets/Sprites/FondoNoche.png");
        }

        // Pikachu: cada pieza en su capa (así se ven las puntas negras, ojos, etc.)
        foreach (var sr in pikachu.GetComponentsInChildren<SpriteRenderer>(true))
        {
            string n = sr.name.ToLower();
            if (n.Contains("cola")) sr.sortingOrder = 4;
            else if (n.Contains("cuerpo")) sr.sortingOrder = 5;
            else if (n.Contains("oreja")) sr.sortingOrder = 5;
            else if (n.Contains("cabeza")) sr.sortingOrder = 6;
            else if (n.Contains("punta"))
            {
                sr.sortingOrder = 7;
                sr.transform.localScale = new Vector3(0.12f, 0.22f, 1f);
            }
            else if (n.Contains("ojo") || n.Contains("mejilla") || n.Contains("nariz")) sr.sortingOrder = 8;
        }

        // Pisos: tierra café con pasto verde brillante
        foreach (var piso in pisos)
        {
            piso.color = Hex("#7A5232");
            piso.sortingOrder = 0;
            var pasto = piso.transform.Find("Pasto");
            if (pasto != null) Object.DestroyImmediate(pasto.gameObject);
            var p = Crear("Pasto", piso.transform, cuadrado, material, Vector2.zero, Vector2.one, Hex("#4CC75A"), 1);
            p.transform.localPosition = new Vector3(0f, 0.38f, 0f);
            p.transform.localScale = new Vector3(1f, 0.25f, 1f);
        }

        // Bloques: amarillo eléctrico (como los rayos y la luna)
        foreach (var bloque in bloques)
        {
            bloque.color = Hex("#E0A800");
            bloque.sortingOrder = 0;
            var brillo = bloque.transform.Find("Brillo");
            if (brillo != null) Object.DestroyImmediate(brillo.gameObject);
            var b = Crear("Brillo", bloque.transform, cuadrado, material, Vector2.zero, Vector2.one, Hex("#FFE066"), 1);
            b.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            b.transform.localScale = new Vector3(0.85f, 0.7f, 1f);
        }

        // Física sin fricción para las paredes (así Pikachu no se queda pegado)
        const string rutaMaterial = "Assets/Fisica/SinFriccion.physicsMaterial2D";
        var sinFriccion = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(rutaMaterial);
        if (sinFriccion == null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Fisica")) AssetDatabase.CreateFolder("Assets", "Fisica");
            sinFriccion = new PhysicsMaterial2D("SinFriccion") { friction = 0f, bounciness = 0f };
            AssetDatabase.CreateAsset(sinFriccion, rutaMaterial);
        }

        // Bordes invisibles en las orillas de los pisos
        Pared("ParedIzquierda", new Vector2(izq - 0.5f, sueloArriba + 8f), sinFriccion);
        Pared("ParedDerecha", new Vector2(der + 0.5f, sueloArriba + 8f), sinFriccion);

        // Zona de caída debajo del hueco del centro
        var zona = GameObject.Find("ZonaCaida");
        if (zona == null) zona = new GameObject("ZonaCaida");
        zona.transform.position = new Vector3((huecoIzq + huecoDer) / 2f, sueloAbajo - 2f, 0f);
        var gatillo = zona.GetComponent<BoxCollider2D>();
        if (gatillo == null) gatillo = zona.AddComponent<BoxCollider2D>();
        gatillo.isTrigger = true;
        gatillo.size = new Vector2((der - izq) + 4f, 1f);
        var zonaCaida = zona.GetComponent<ZonaCaida>();
        if (zonaCaida == null) zonaCaida = zona.AddComponent<ZonaCaida>();
        if (zona.GetComponent<AudioSource>() == null) zona.AddComponent<AudioSource>().playOnAwake = false;
        zonaCaida.sonidoCaida = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sonidos/pikachu.mp3");

        // Cartel con nombre y controles
        var hud = GameObject.Find("HUD");
        if (hud == null) hud = new GameObject("HUD");
        if (hud.GetComponent<Hud>() == null) hud.AddComponent<Hud>();

        var escena = pikachu.scene;
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        AssetDatabase.SaveAssets();

        Debug.Log("[GrupoA] Nivel diseñado y guardado: fondo de noche, bordes, pasto, bloques y Game Over.");
    }

    static GameObject Crear(string nombre, Transform padre, Sprite sprite, Material material,
                            Vector2 posicion, Vector2 escala, Color color, int orden)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        go.transform.position = posicion;
        go.transform.localScale = new Vector3(escala.x, escala.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        if (material != null) sr.sharedMaterial = material;
        sr.color = color;
        sr.sortingOrder = orden;
        return go;
    }

    static void Pared(string nombre, Vector2 posicion, PhysicsMaterial2D material)
    {
        var pared = GameObject.Find(nombre);
        if (pared == null) pared = new GameObject(nombre);
        pared.transform.position = posicion;
        var col = pared.GetComponent<BoxCollider2D>();
        if (col == null) col = pared.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 20f);
        col.sharedMaterial = material;
    }
}
