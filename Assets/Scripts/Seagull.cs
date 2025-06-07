using UnityEngine;

public class Seagull : MonoBehaviour
{
    [SerializeField] private GameObject destroyEffect;
    [SerializeField] private AudioClip destroySound;
    [SerializeField] private float graceTime = 0.5f;
    [SerializeField] private float minImpactForce = 0.5f;

    private float startTime;
    private bool isGrounded = false;

    void Start()
    {
        startTime = Time.time;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
    
        if (Time.time - startTime < graceTime)
        {
            return;
        }

        ContactPoint2D contact = collision.GetContact(0);
        Vector2 impactDirection = contact.normal;

        if (impactDirection.y < -0.8f)
        {
            isGrounded = true;
            return;
        }

        float impactForce = collision.relativeVelocity.magnitude;

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log($"Seagull destruido por Player");
            DestroySeagull();
            return;
        }

        if (collision.gameObject.CompareTag("Box"))
        {
            Rigidbody2D boxRb = collision.gameObject.GetComponent<Rigidbody2D>();

            // Si la caja está cayendo desde arriba
            if (boxRb != null && collision.transform.position.y > transform.position.y + 0.2f)
            {
                Debug.Log($"Seagull destruido por caja desde arriba");
                DestroySeagull();
                return;
            }
        }

        if (impactForce > minImpactForce)
        {
            Debug.Log($"Seagull destruido por: {collision.gameObject.name} con fuerza: {impactForce}");
            DestroySeagull();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    private void DestroySeagull()
    {
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }

        if (destroySound != null)
        {
            AudioSource.PlayClipAtPoint(destroySound, transform.position);
        }

        Destroy(gameObject);
    }
}