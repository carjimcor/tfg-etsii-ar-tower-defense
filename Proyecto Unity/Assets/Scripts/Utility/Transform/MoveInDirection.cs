using UnityEngine;

public class MoveInDirection : MonoBehaviour
{
    public float speed;
    public Vector3 direction;
    public Space space;

    Vector3 movement;
    // Start is called before the first frame update
    void Start()
    {
        movement = direction.normalized * speed;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(movement * Time.deltaTime, space);
    }
}
