using UnityEngine;
using UnityEngine.InputSystem;

public class CampoViento : MonoBehaviour
{
    [Header("Activa / desactiva con V")]
    public bool vientoActivo;
    public float fuerzaBaseViento = 5f;
    public float frecuenciaVariacion = 0.5f;

    public enum TipoCampo { Variado, UniformeDerecha, UniformeIzquierda, UniformeArriba, UniformeAbajo }
    public TipoCampo tipoCampoActual = TipoCampo.UniformeDerecha;

    [Header("Visual")]
    [SerializeField] int gridWidth = 11;
    [SerializeField] int gridHeight = 7;
    [SerializeField] float gridSpacing = 2f;

    [Header("Fricción variable")]
    public float coeficienteFriccionBase = 0.1f;
    public float alturaReferencia = 10f;

    GameObject[,] flechas;
    GameObject flechaPrefab;
    Camera cam;
    float pulso;

    void Start()
    {
        cam = Camera.main;
        CrearFlechaPrefab();
        CrearGrid();
        ActualizarVisualizacion();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.vKey.wasPressedThisFrame)
            {
                vientoActivo = !vientoActivo;
                ActualizarVisualizacion();
            }
            if (kb.cKey.wasPressedThisFrame && vientoActivo)
            {
                tipoCampoActual = (TipoCampo)(((int)tipoCampoActual + 1) % 5);
            }
        }

        if (vientoActivo)
        {
            pulso += Time.deltaTime * 2f; // recordar probar otra animación
            ActualizarDirecciones();
            ReposicionarGridSiCamara();
        }
    }

    // fisica
    public Vector2 ObtenerFuerzaViento(Vector2 pos)
    {
        if (!vientoActivo) return Vector2.zero;

        Vector2 F = tipoCampoActual switch
        {
            TipoCampo.Variado => new Vector2(Mathf.Sin(pos.y * frecuenciaVariacion),
                                                        Mathf.Cos(pos.x * frecuenciaVariacion) * 0.3f) * fuerzaBaseViento,
            TipoCampo.UniformeDerecha => Vector2.right * fuerzaBaseViento,
            TipoCampo.UniformeIzquierda => Vector2.left * fuerzaBaseViento,
            TipoCampo.UniformeArriba => Vector2.up * fuerzaBaseViento * .5f,
            TipoCampo.UniformeAbajo => Vector2.down * fuerzaBaseViento * .5f,
            _ => Vector2.zero
        };

        return F * FactorFriccionPorAltura(pos.y);
    }

    public float FactorFriccionPorAltura(float h) =>
        1f - coeficienteFriccionBase * Mathf.Exp(-h / alturaReferencia);

    // visual
    void CrearFlechaPrefab()
    {
        flechaPrefab = new GameObject("Arrow");
        var sr = flechaPrefab.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(TextureFlecha(), new Rect(0, 0, 32, 8), new Vector2(.1f, .5f), 32);
        sr.sortingOrder = 3;
        flechaPrefab.SetActive(false);
    }

    Texture2D TextureFlecha()
    {
        Texture2D tex = new Texture2D(32, 8);
        Color baseC = new Color(.2f, .4f, .9f, .9f);
        Color[] p = new Color[32 * 8];

        for (int i = 0; i < p.Length; i++) p[i] = Color.clear;
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 24; x++) p[y * 32 + x] = baseC;            
        for (int y = 0; y < 8; y++)
            for (int x = 24; x < 32; x++)
                if (x > 28 - Mathf.Abs(y - 4)) p[y * 32 + x] = baseC;      
        tex.SetPixels(p); tex.Apply();
        return tex;
    }

    void CrearGrid()
    {
        flechas = new GameObject[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
            {
                var go = Instantiate(flechaPrefab, transform);
                flechas[x, y] = go;
            }
        ReposicionarGridSiCamara();
    }

    void ReposicionarGridSiCamara()
    {
        Vector2 camMin = cam.ViewportToWorldPoint(new Vector3(0, 0));
        Vector2 start = camMin + Vector2.one * gridSpacing * .5f;

        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 pos = new Vector3(start.x + x * gridSpacing,
                                          start.y + y * gridSpacing, 0);
                if (flechas[x, y])
                    flechas[x, y].transform.position = pos;
            }
    }

    void ActualizarVisualizacion()
    {
        foreach (GameObject f in flechas)
            if (f) f.SetActive(vientoActivo);
    }

    void ActualizarDirecciones()
    {
        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
            {
                Transform flecha = flechas[x, y]?.transform;
                if (!flecha || !flecha.gameObject.activeSelf) continue;

                Vector2 pos = flecha.position;
                Vector2 F = ObtenerFuerzaViento(pos);
                float mag = F.magnitude;

                // direccion
                if (mag > .01f)
                {
                    float z = Mathf.Atan2(F.y, F.x) * Mathf.Rad2Deg;
                    flecha.rotation = Quaternion.Euler(0, 0, z);
                }

                float t = Mathf.Clamp01(mag / fuerzaBaseViento);
                Color c = Color.Lerp(new Color(.2f, .4f, 1f),
                                     Color.cyan, t * 0.6f);
                flecha.GetComponent<SpriteRenderer>().color = c;

                // escala + pequeño pulso
                float escalaBase = Mathf.Lerp(.5f, 1.5f, t);
                float pulsoLocal = 1f + .1f * Mathf.Sin(pulso + (x + y) * .4f);
                flecha.localScale = Vector3.one * escalaBase * pulsoLocal;
            }
    }
}