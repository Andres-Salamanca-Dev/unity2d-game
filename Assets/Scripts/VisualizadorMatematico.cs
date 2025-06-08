using UnityEngine;
using UnityEngine.InputSystem;

public class VisualizadorMatematico : MonoBehaviour
{
    [Header("Ajustes de HUD")]
    [SerializeField, Range(0.5f, 5f)] float guiScale = 1f;
    [SerializeField] Color colorEncabezado = new Color(1f, .9f, .25f);
    [SerializeField] Color colorSeccion = new Color(.45f, .75f, 1f);
    [SerializeField] Color colorEcuacion = new Color(.4f, 1f, .4f);

    Turtle turtle;
    CampoViento viento;
    bool visible;

    void Start()
    {
        turtle = Object.FindFirstObjectByType<Turtle>();
        viento = Object.FindFirstObjectByType<CampoViento>();
    }

    void Update()
    {
        if (Keyboard.current?.iKey.wasPressedThisFrame == true)
            visible = !visible;
    }

    void OnGUI()
    {
        if (!visible || turtle == null) return;

        // Tengo que verificar este escalado global
        float factorPantalla = Screen.height / 1080f;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
                    new Vector3(factorPantalla * guiScale,
                                factorPantalla * guiScale, 1));

        const int W = 700, H = 520;
        GUI.color = new Color(0, 0, 0, .85f);
        GUI.Box(new Rect(5, 5, W, H), GUIContent.none);
        GUI.color = Color.white;

        // Estilos
        GUIStyle encabezado = new GUIStyle(GUI.skin.label)
        { fontSize = 26, fontStyle = FontStyle.Bold, normal = { textColor = colorEncabezado } };

        GUIStyle seccion = new GUIStyle(GUI.skin.label)
        { fontSize = 20, fontStyle = FontStyle.Bold, normal = { textColor = colorSeccion } };

        GUIStyle texto = new GUIStyle(GUI.skin.label) { fontSize = 18 };
        GUIStyle ecu = new GUIStyle(texto)
        { fontStyle = FontStyle.Italic, normal = { textColor = colorEcuacion }, wordWrap = true };

        int x = 20, y = 20, dy = 26;

      
        GUI.Label(new Rect(x, y, W, 30), "PARÁMETROS MATEMÁTICOS", encabezado);
        y += 40;

        string estado = turtle.IsAiming() ? "APUNTANDO"
                     : turtle.IsLaunched() ? "EN VUELO"
                     : "DETENIDA";
        GUI.Label(new Rect(x, y, W, 25), $"ESTADO: {estado}", seccion); y += dy + 4;

        Rigidbody2D rb = turtle.GetComponent<Rigidbody2D>();
        GUI.Label(new Rect(x, y, W, 25), "Cinemática", seccion); y += dy;
        GUI.Label(new Rect(x, y, W, 25), $"Posición r = ({turtle.transform.position.x:F2}, {turtle.transform.position.y:F2})", texto); y += dy;
        GUI.Label(new Rect(x, y, W, 25), $"Velocidad v = ({rb.linearVelocity.x:F2}, {rb.linearVelocity.y:F2}) m/s", texto); y += dy;
        GUI.Label(new Rect(x, y, W, 25), $"Rapidez ‖v‖ = {rb.linearVelocity.magnitude:F2} m/s", texto); y += dy + 4;

        // Fricción y Viento
        GUI.Label(new Rect(x, y, W, 25), "Fricción • Viento", seccion); y += dy;
        float h = turtle.transform.position.y;
        float factor = viento ? viento.FactorFriccionPorAltura(h) : 1f;
        GUI.Label(new Rect(x, y, W, 25), $"Factor fricción altura = {factor:F3}", texto); y += dy;

        if (viento && viento.vientoActivo)
        {
            Vector2 Fw = viento.ObtenerFuerzaViento(turtle.transform.position);
            GUI.Label(new Rect(x, y, W, 25), $"Fuerza viento = ({Fw.x:F1}, {Fw.y:F1}) N", texto); y += dy;
        }
        y += 8;

        // Ecu parametrica
        if (turtle.IsAiming() || turtle.IsLaunched())
        {
            Vector2 r0 = turtle.transform.position;
            Vector2 v0 = turtle.IsAiming() ? turtle.PredictedV0() : rb.linearVelocity;

            GUI.Label(new Rect(x, y, W, 25), "Ecuación paramétrica  r(t)", seccion); y += dy;
            GUI.Label(new Rect(x, y, W - 40, 40), $"x(t) = {r0.x:F1} + {v0.x:F1}·t", ecu); y += dy;
            GUI.Label(new Rect(x, y, W - 40, 40), $"y(t) = {r0.y:F1} + {v0.y:F1}·t - 4.9·t²   ← parábola", ecu); y += dy + 4;
        }

        // Datos de vuelo - Recordar cambiar esta parte
        if (turtle.IsLaunched())
        {
            GUI.Label(new Rect(x, y, W, 25), "Datos de vuelo", seccion); y += dy;

            float tVuelo = turtle.GetTiempoVuelo();
            GUI.Label(new Rect(x, y, W, 25), $"Tiempo de vuelo t = {tVuelo:F2} s", texto); y += dy;

            float xDist = Mathf.Abs(turtle.transform.position.x - turtle.StartPosition.x);
            GUI.Label(new Rect(x, y, W, 25), $"Distancia horizontal Δx = {xDist:F2} m", texto); y += dy;

            // altura máxima estimada  h_max = (v0y²) / (2g)
            float v0y = turtle.PredictedV0().y;
            float hMax = v0y > 0 ? (v0y * v0y) / (2f * 9.81f) : 0;
            GUI.Label(new Rect(x, y, W, 25), $"Altura máxima estimada = {hMax:F2} m", texto); y += dy;
        }

        
        GUI.Label(new Rect(x, H - 35, W, 25), "I - ocultar HUD", texto);
    }
}