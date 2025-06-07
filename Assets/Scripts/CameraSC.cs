using UnityEngine;

public class CameraSC : MonoBehaviour
{
    [SerializeField] private GameObject tortuga;
    private float initialX;
    private Vector3 initialPosition;
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private float maxXPosition = 20f;

    private float GetCameraRightEdge()
    {
        float cameraHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;
        return transform.position.x + cameraHalfWidth;
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
        if (tortuga != null)
        {
            Vector3 currentPosition = transform.position;

            if (tortuga.transform.position.x > GetCameraRightEdge())
            {
                tortuga.GetComponent<NewEmptyCSharpScript>().ForceReset();
                return;
            }

            if (tortuga.transform.position.x > initialX)
            {
                float targetX = Mathf.Min(tortuga.transform.position.x, maxXPosition);
                float newX = Mathf.Lerp(currentPosition.x, targetX, smoothSpeed);
                transform.position = new Vector3(newX, currentPosition.y, currentPosition.z);
            }
        }
    }
}