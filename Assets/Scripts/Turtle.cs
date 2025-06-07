using UnityEngine;
using UnityEngine.InputSystem;

public class NewEmptyCSharpScript : MonoBehaviour
{
    private SpringJoint2D springJoint;
    private Rigidbody2D rb;
    private bool isDragging = false;
    private Camera cam;
    private bool wasLaunched = false;
    private float launchTime;
    private CameraSC cameraScript;

    private Vector3 startPosition;
    public float maxDragDistance = 2f;
    public float launchPower = 500f;
    public float resetTime = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        springJoint = GetComponent<SpringJoint2D>();
        startPosition = transform.position;
        cam = Camera.main;
        cameraScript = cam.GetComponent<CameraSC>();

        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = cam.ScreenToWorldPoint(mouse.position.ReadValue());
            if (IsMouseOverObject(mousePos))
            {
                isDragging = true;
            }
        }

        if (mouse.leftButton.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;
            rb.bodyType = RigidbodyType2D.Dynamic;

            Vector2 dragDirection = (Vector2)startPosition - (Vector2)transform.position;
            rb.AddForce(dragDirection * launchPower);

            if (springJoint != null)
                Destroy(springJoint);

            wasLaunched = true;
            launchTime = Time.time;
        }

        if (isDragging)
        {
            Vector2 mousePos = cam.ScreenToWorldPoint(mouse.position.ReadValue());

            float distance = Vector2.Distance(mousePos, startPosition);
            if (distance > maxDragDistance)
            {
                Vector2 direction = (mousePos - (Vector2)startPosition).normalized;
                mousePos = (Vector2)startPosition + direction * maxDragDistance;
            }

            transform.position = new Vector3(mousePos.x, mousePos.y, transform.position.z);
        }

        if (wasLaunched && Time.time - launchTime >= resetTime)
        {
            ResetTortuga();
            cameraScript.ResetCamera();
        }
    }

    public void ForceReset()
    {
        ResetTortuga();
        cameraScript.ResetCamera();
    }

    private void ResetTortuga()
    {
        transform.position = startPosition;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        wasLaunched = false;

        if (springJoint == null)
        {
            springJoint = gameObject.AddComponent<SpringJoint2D>();
        }
    }

    private bool IsMouseOverObject(Vector2 mousePos)
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            return collider.OverlapPoint(mousePos);
        }
        return false;
    }
}