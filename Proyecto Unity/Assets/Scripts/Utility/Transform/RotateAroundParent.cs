using UnityEngine;

public class RotateAroundParent : MonoBehaviour
{
    Transform parent = default;

    Vector3 axis = default;

    float anglePerSecond = 180;

    private void Awake()
    {
        parent = transform.parent;
        axis = new Vector3(Random.value, Random.value);
    }

    void Update()
    {
        transform.RotateAround(parent.position, axis, anglePerSecond * Time.deltaTime);
    }
}
