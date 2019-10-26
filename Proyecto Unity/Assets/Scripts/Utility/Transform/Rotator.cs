using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField]
    Vector3 rotationAxis = default;

    [SerializeField]
    float anglePerSecond = default;

    void Update()
    {
        transform.Rotate(rotationAxis, anglePerSecond * Time.deltaTime);
    }
}
