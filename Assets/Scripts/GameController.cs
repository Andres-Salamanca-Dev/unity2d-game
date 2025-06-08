using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [Header("Panning")]
    [SerializeField] float panSpeed = 1f;

    bool pausado;
    float timeScaleOriginal;
    Vector3 camPosAlPausar;

    bool arrastrando;
    Vector3 mousePrevWorld;

  
    void Start() => timeScaleOriginal = Time.timeScale;

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.pKey.wasPressedThisFrame) TogglePausa();
        if (kb.rKey.wasPressedThisFrame) ReiniciarEscena();

        if (pausado) CamaraDragHorizontal();
    }

    void TogglePausa()
    {
        pausado = !pausado;

        if (pausado)
        {
            camPosAlPausar = Camera.main.transform.position;
            Time.timeScale = 0f;
        }
        else
        {
            Camera.main.transform.position = camPosAlPausar;
            Time.timeScale = timeScaleOriginal;
            arrastrando = false;
        }
    }

    // Panning
    void CamaraDragHorizontal()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.rightButton.wasPressedThisFrame)
        {
            arrastrando = true;
            mousePrevWorld = ScreenToWorld(mouse.position.ReadValue());
        }
        else if (mouse.rightButton.wasReleasedThisFrame)
        {
            arrastrando = false;
        }

        if (arrastrando)
        {
            Vector3 mouseCurrWorld = ScreenToWorld(mouse.position.ReadValue());
            Vector3 delta = mousePrevWorld - mouseCurrWorld;

            delta.y = 0;
            delta.z = 0;

            Camera.main.transform.position += delta * panSpeed;
            mousePrevWorld = mouseCurrWorld;
        }
    }
    
    void ReiniciarEscena()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    Vector3 ScreenToWorld(Vector2 scrPos)
    {
        Vector3 p = scrPos;
        p.z = -Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(p);
    }
}