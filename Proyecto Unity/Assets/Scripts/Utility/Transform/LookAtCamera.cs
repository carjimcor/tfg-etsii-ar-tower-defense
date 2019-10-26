using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    Vector3 lookPoint=default;

    public Transform cameraTransform=default;

    void Update()
    {
        lookPoint.z = cameraTransform.forward.z;
        lookPoint.x = cameraTransform.forward.x;

        transform.forward = -lookPoint;
    }
}
