using UnityEngine;

public class Scaler : MonoBehaviour
{
    [SerializeField]
    float maxScale = default;

    [SerializeField]
    float transitionTime = default;

    float startScale;
    float scaleDelta;

    bool growing = true;

    private void Awake()
    {
        startScale = transform.localScale.x;
        scaleDelta = (maxScale - startScale) / transitionTime;
    }

    void Update()
    {
        if (growing)
        {
            transform.localScale += Vector3.one * scaleDelta * Time.deltaTime;
            if (transform.localScale.x >= maxScale)
            {
                growing = !growing;
            }
        }
        else
        {
            transform.localScale -= Vector3.one * scaleDelta * Time.deltaTime;
            if (transform.localScale.x <= startScale)
            {
                growing = !growing;
            }
        }
        
    }
}
