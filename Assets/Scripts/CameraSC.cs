using UnityEngine;

public class CameraSC : MonoBehaviour
{
    [SerializeField] private GameObject tortuga;
    private float initialX;
    private Vector3 initialPosition;
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private float maxXPosition = 20f;

    float GetCameraRightEdge()
    {
        float half = Camera.main.orthographicSize * Camera.main.aspect;
        return transform.position.x + half;
    }

    public void ResetCamera()
    {
        transform.position = initialPosition;
    }

    void Start()
    {
        initialX = transform.position.x;
        initialPosition = transform.position;
    }

    void LateUpdate()
    {
        if (tortuga == null) return;

        Vector3 cur = transform.position;

        if (tortuga.transform.position.x > GetCameraRightEdge())
        {
            tortuga.GetComponent<Turtle>().ForceReset();
            return;
        }

        if (tortuga.transform.position.x > initialX)
        {
            float targetX = Mathf.Min(tortuga.transform.position.x, maxXPosition);
            float newX = Mathf.Lerp(cur.x, targetX, smoothSpeed);
            transform.position = new Vector3(newX, cur.y, cur.z);
        }
    }
}