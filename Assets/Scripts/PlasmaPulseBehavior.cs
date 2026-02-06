using UnityEngine;

public class PlasmaPulseBehavior : MonoBehaviour
{
    public float speed = 10f;
    public float maxDistance = 10f;
    public float growthRate = 0.5f;
    private Vector3 startPosition;
    private float initialScale;

    void Start()
    {
        startPosition = transform.position;
        initialScale = transform.localScale.x;
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        
        float distanceTraveled = Vector3.Distance(transform.position, startPosition);
        float scaleMultiplier = 1f + (distanceTraveled / maxDistance) * growthRate;
        transform.localScale = Vector3.one * initialScale * scaleMultiplier;

        if (distanceTraveled >= maxDistance)
        {
            Destroy(gameObject);
        }
    }
}