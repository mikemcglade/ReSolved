using UnityEngine;

public class PlasmaPulseBehavior : MonoBehaviour
{
    public float speed = 10f;
    public float maxDistance = 10f;
    public float growthRate = 0.5f;
    public float lifetime = 3f; // Destroy after this many seconds as backup
    
    private Vector3 startPosition;
    private float initialScale;
    private Rigidbody rb;

    void Start()
    {
        startPosition = transform.position;
        initialScale = transform.localScale.x;
        
        // Get or add Rigidbody for proper physics
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Configure Rigidbody
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        // Set initial velocity in the direction the projectile is facing
        rb.velocity = transform.forward * speed;
        
        // Destroy after lifetime as backup (in case it doesn't hit anything)
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Check distance traveled
        float distanceTraveled = Vector3.Distance(transform.position, startPosition);
        
        // Scale effect (grows as it travels)
        float scaleMultiplier = 1f + (distanceTraveled / maxDistance) * growthRate;
        transform.localScale = Vector3.one * initialScale * scaleMultiplier;

        // Destroy if traveled too far
        if (distanceTraveled >= maxDistance)
        {
            Destroy(gameObject);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Destroy on contact with anything except the player
        if (!other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
