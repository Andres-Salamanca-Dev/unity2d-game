using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Turtle : MonoBehaviour
{
   
    [Header("Slingshot")]
    public float maxDragDistance = 2f;
    public float pushForce = 8f;
    public float resetTime = 5f;

    [Header("Trayectoria – puntos")]
    public int dotCount = 40; // nº máximo de puntos
    public float dotSpacing = .05f; // Δt entre puntos
    public GameObject dotPrefab; //Prefab
    public Color dotColor = new Color(1f, 1f, 1f, 1f);

    
    Rigidbody2D rb; Camera cam; CameraSC camSC;
    Vector3 pivot; bool isDragging, wasLaunched;
    float launchTime; Vector2 dragDir, impulse, v0;
    Transform[] dotsPool;          
    float tiempoVuelo;

    CampoViento viento;
    VisualizadorMatematico visor;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        camSC = cam.GetComponent<CameraSC>();
        pivot = transform.position;

       
        dotsPool = new Transform[dotCount];
        for (int i = 0; i < dotCount; i++)
        {
            GameObject d = Instantiate(dotPrefab, transform);
            d.GetComponent<SpriteRenderer>().color = dotColor;
            d.SetActive(false);
            dotsPool[i] = d.transform;
        }

        viento = Object.FindFirstObjectByType<CampoViento>();
        visor = Object.FindFirstObjectByType<VisualizadorMatematico>();

        rb.bodyType = RigidbodyType2D.Kinematic;
    }

   
    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame &&
            GetComponent<Collider2D>().OverlapPoint(ScreenToWorld(mouse.position.ReadValue())))
        { isDragging = true; MostrarPuntos(true); }

        if (mouse.leftButton.wasReleasedThisFrame && isDragging)
        { isDragging = false; Lanzar(); }

        if (isDragging) Arrastrar(ScreenToWorld(mouse.position.ReadValue()));

        if (wasLaunched)
        {
            tiempoVuelo += Time.deltaTime;
            if (viento && viento.vientoActivo)
                rb.AddForce(viento.ObtenerFuerzaViento(transform.position));

            if (Time.time - launchTime >= resetTime)
            { Reiniciar(); camSC.ResetCamera(); }
        }
    }

    void Arrastrar(Vector2 mouseWs)
    {
        Vector2 pos = mouseWs;
        if (Vector2.Distance(pos, pivot) > maxDragDistance)
            pos = (Vector2)pivot + (pos - (Vector2)pivot).normalized * maxDragDistance;

        transform.position = new Vector3(pos.x, pos.y, transform.position.z);

        dragDir = (Vector2)pivot - pos;
        impulse = dragDir * pushForce;
        DibujarPuntos();
    }

    void Lanzar()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.AddForce(impulse, ForceMode2D.Impulse);

        v0 = impulse / rb.mass;
        wasLaunched = true; launchTime = Time.time; tiempoVuelo = 0;
        MostrarPuntos(false);
    }

    void DibujarPuntos()
    {
        Vector2 g = Physics2D.gravity;
        Vector2 p0 = transform.position;
        Vector2 v0temp = impulse / rb.mass;

        for (int i = 0; i < dotsPool.Length; i++)
        {
            float t = i * dotSpacing;
            Vector2 pos = p0 + v0temp * t + 0.5f * g * t * t;

            dotsPool[i].position = pos;
            dotsPool[i].gameObject.SetActive(true);
        }
    }

    void MostrarPuntos(bool estado)
    {
        foreach (var d in dotsPool) d.gameObject.SetActive(estado);
    }

    void Reiniciar()
    {
        transform.position = pivot;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;

        wasLaunched = false;
        MostrarPuntos(false);
    }

    Vector2 ScreenToWorld(Vector2 v) => cam.ScreenToWorldPoint(v);

    public bool IsLaunched() => wasLaunched;
    public float GetTiempoVuelo() => tiempoVuelo;
    public bool  IsAiming()        => isDragging;
    public Vector2 PredictedV0()   => v0;
    
    public Vector3 StartPosition => pivot;
    public void ForceReset() { Reiniciar(); camSC.ResetCamera(); }
}