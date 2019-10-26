using UnityEngine;
public class CameraController : MonoBehaviour
{
    [Header("Movement / Zoom Attributes")]
    [SerializeField]
    float panMultiplier = 0.01f;
    [SerializeField]
    float zoomMultiplier = 0.01f;
    [Header("Position Restrictions")]
    [SerializeField]
    Vector3 bottomLeft;
    [SerializeField]
    Vector3 topRight;
    Touch[] touches = new Touch[2];

    Vector2 deltaPosition = Vector2.zero;
    float lastSeparation = 0f;
    float currentSeparation = 0f;
    float separationDelta = 0f;

    Vector3 movement = Vector3.zero;


    private void Update()
    {
        
        if (Input.touchCount == 1)
        {
            // Check if panning
            touches[0] = Input.GetTouch(0);

            if (touches[0].phase != TouchPhase.Moved)
            {
                // If it didn't move, return
                return;
            }

            deltaPosition = -panMultiplier * touches[0].deltaPosition;
            movement = new Vector3(deltaPosition.x, 0f, deltaPosition.y);
            transform.Translate(movement, Space.World);
            if (!InsideBoundaries())
            {
                transform.Translate(-movement, Space.World);
            }
        }
        else if (Input.touchCount == 2)
        {
            // check if zooming
            touches[0] = Input.GetTouch(0);
            touches[1] = Input.GetTouch(1);

            if (touches[0].phase == TouchPhase.Began || touches[1].phase == TouchPhase.Began)
            {
                // Zoom initialization
                lastSeparation = Vector2.Distance(touches[0].position, touches[1].position);
                return;
            }

            if (touches[0].phase != TouchPhase.Moved && touches[1].phase != TouchPhase.Moved)
            {
                // If none moved, get out
                return;
            }

            currentSeparation = Vector2.Distance(touches[0].position, touches[1].position);
            separationDelta = currentSeparation - lastSeparation;
            movement = transform.forward * separationDelta * zoomMultiplier;
            transform.Translate(movement, Space.World);

            if (!InsideBoundaries())
            {
                transform.Translate(-movement, Space.World);
            }

            lastSeparation = currentSeparation;
        }

    }

    bool InsideBoundaries()
    {

        if (transform.position.y < bottomLeft.y || transform.position.y > topRight.y)
            return false;
        if (transform.position.x < bottomLeft.x || transform.position.x > topRight.x)
            return false;
        if (transform.position.z < bottomLeft.z || transform.position.z > topRight.z)
            return false;

        return true;
    }

}
