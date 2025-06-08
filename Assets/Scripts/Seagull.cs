using UnityEngine;

public class Seagull : MonoBehaviour
{
    [Header("Efectos")]
    [SerializeField] GameObject destroyEffect;
    [SerializeField] AudioClip destroySound;

    [Header("Parámetros de muerte")]
    [Tooltip("Tiempo de gracia tras aparecer")]
    [SerializeField] float graceTime = 0.5f;

    [Tooltip("Fuerza mínima para morir si no viene desde arriba")]
    [SerializeField] float minImpactForce = 0.8f;

    [Tooltip("Cuánto más alto debe venir el atacante")]
    [SerializeField] float alturaUmbral = 0.15f;

    float startTime;

    void Start() => startTime = Time.time;

    void OnCollisionEnter2D(Collision2D col)
    {
        if (Time.time - startTime < graceTime) return;

        // magnitud del impactoo
        float Fimpacto = col.relativeVelocity.magnitude;

        // posicin vertical del atacante vs gaviot
        float alturaAtacante = col.transform.position.y;
        bool vieneDeArriba = alturaAtacante > transform.position.y + alturaUmbral;

        if (col.collider.CompareTag("Player"))
        {
            if (vieneDeArriba || Fimpacto > minImpactForce)
            {
                Debug.Log("Seagull destruida por tortuga");
                Morir();
            }
            return;
        }

        // -------- GOLPE DE UNA CAJA --------
        if (col.collider.CompareTag("Box"))
        {
            if (vieneDeArriba)
            {
                Debug.Log("Seagull destruida por caja");
                Morir();
            }
            return;
        }

        // -------- CUALQUIER OTRO OBJETO --------
        if (Fimpacto > minImpactForce)
        {
            Debug.Log($"Seagull destruida por {col.gameObject.name} con F={Fimpacto:F2}");
            Morir();
        }
    }

 
    void Morir()
    {
        if (destroyEffect) Instantiate(destroyEffect, transform.position, Quaternion.identity);
        if (destroySound) AudioSource.PlayClipAtPoint(destroySound, transform.position);
        Destroy(gameObject);
    }
}