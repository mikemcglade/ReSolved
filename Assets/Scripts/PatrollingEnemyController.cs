using UnityEngine;

public class PatrollingEnemyController : MonoBehaviour
{
    public float speed = 3f;
    public float followDistance = 10f;
    public float rotationSpeed = 5f;
    public Transform[] patrolPoints;
    public float patrolWaitTime = 2f;

    private Transform player;
    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    private enum EnemyState { Patrolling, Chasing }
    private EnemyState currentState = EnemyState.Patrolling;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= followDistance)
        {
            currentState = EnemyState.Chasing;
            ChasePlayer();
        }
        else
        {
            currentState = EnemyState.Patrolling;
            Patrol();
        }
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= patrolWaitTime)
            {
                isWaiting = false;
                waitTimer = 0f;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            }
        }
        else
        {
            Vector3 targetPosition = patrolPoints[currentPatrolIndex].position;
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isWaiting = true;
            }
        }
    }

    void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
}