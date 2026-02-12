using UnityEngine;
using UnityEngine.AI;

public class PatrollingEnemyAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waypointWaitTime = 2f;
    
    [Header("Chase Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float chaseTimeout = 2f;
    
    private NavMeshAgent agent;
    private Transform player;
    
    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;
    private float chaseTimer = 0f;
    
    private enum State { Patrolling, Waiting, Chasing, Returning }
    private State currentState = State.Patrolling;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
        else
        {
            Debug.LogError("No waypoints assigned to " + gameObject.name);
        }
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        switch (currentState)
        {
            case State.Patrolling:
                Patrol();
                
                // Check if player is in range
                if (distanceToPlayer <= detectionRange)
                {
                    StartChasing();
                }
                break;
                
            case State.Waiting:
                Wait();
                
                // Can still detect player while waiting
                if (distanceToPlayer <= detectionRange)
                {
                    StartChasing();
                }
                break;
                
            case State.Chasing:
                Chase(distanceToPlayer);
                break;
                
            case State.Returning:
                ReturnToPatrol();
                break;
        }
    }

    void Patrol()
    {
        // Check if we've reached the current waypoint
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentState = State.Waiting;
            waitTimer = 0f;
        }
    }

    void Wait()
    {
        waitTimer += Time.deltaTime;
        
        if (waitTimer >= waypointWaitTime)
        {
            // Move to next waypoint
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            agent.SetDestination(waypoints[currentWaypointIndex].position);
            currentState = State.Patrolling;
        }
    }

    void StartChasing()
    {
        currentState = State.Chasing;
        chaseTimer = 0f;
    }

    void Chase(float distanceToPlayer)
    {
        // Move towards player
        agent.SetDestination(player.position);
        
        // Check if player is still in range
        if (distanceToPlayer <= detectionRange)
        {
            // Player is still close, reset timer
            chaseTimer = 0f;
        }
        else
        {
            // Player is out of range, increment timer
            chaseTimer += Time.deltaTime;
            
            if (chaseTimer >= chaseTimeout)
            {
                // Give up and return to patrol
                currentState = State.Returning;
            }
        }
    }

    void ReturnToPatrol()
    {
        // Find the nearest waypoint
        int nearestWaypointIndex = FindNearestWaypoint();
        currentWaypointIndex = nearestWaypointIndex;
        
        agent.SetDestination(waypoints[currentWaypointIndex].position);
        
        // Once we reach the waypoint, resume patrolling
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentState = State.Waiting;
            waitTimer = 0f;
        }
    }

    int FindNearestWaypoint()
    {
        int nearestIndex = 0;
        float nearestDistance = Vector3.Distance(transform.position, waypoints[0].position);
        
        for (int i = 1; i < waypoints.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, waypoints[i].position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }
        
        return nearestIndex;
    }

    // Visualize detection range in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
