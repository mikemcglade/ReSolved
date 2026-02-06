using UnityEngine;

public class FollowingEnemyController : MonoBehaviour
{
    public float speed = 3f;
    public float followDistance = 10f;
    public float rotationSpeed = 5f; // New variable for rotation speed

    private Transform player;
    private Rigidbody enemyRb;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        enemyRb = GetComponent<Rigidbody>();
        enemyRb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
    }

    void FixedUpdate()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= followDistance)
            {
                // Movement
                Vector3 direction = (player.position - transform.position).normalized;
                enemyRb.MovePosition(transform.position + direction * speed * Time.fixedDeltaTime);

                // Rotation
                Quaternion lookRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.fixedDeltaTime * rotationSpeed);
            }
        }
    }
}