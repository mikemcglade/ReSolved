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
    
    [Header("Visual Feedback")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color chaseColor = Color.red;
    [SerializeField] private ParticleSystem alertParticle;
    
    private NavMeshAgent agent;
    private Transform player;
    private Renderer[] enemyRenderers;
    private Material[][] originalMaterials;
    
    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;
    private float chaseTimer = 0f;
    
    private enum State { Patrolling, Waiting, Chasing, Returning }
    private State currentState = State.Patrolling;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // Get all renderers (handles multiple materials like player script)
        enemyRenderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[enemyRenderers.Length][];
        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            originalMaterials[i] = enemyRenderers[i].sharedMaterials;
        }
        
        // Make sure alert particle is off at start
        if (alertParticle != null)
        {
            alertParticle.Stop();
        }
        
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
                
                // Check if player is in range AND visible
                if (distanceToPlayer <= detectionRange && CanSeePlayer())
                {
                    StartChasing();
                }
                break;
                
            case State.Waiting:
                Wait();
                
                // Can still detect player while waiting (if visible)
                if (distanceToPlayer <= detectionRange && CanSeePlayer())
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
        
        // Change all materials to chase color
        SetMaterialColors(chaseColor);
        
        // Start alert particle effect
        if (alertParticle != null)
        {
            alertParticle.Play();
        }
    }

    void Chase(float distanceToPlayer)
    {
        // Move towards player
        agent.SetDestination(player.position);
        
        // Check if player is still in range AND visible
        if (distanceToPlayer <= detectionRange && CanSeePlayer())
        {
            // Player is still close and visible, reset timer
            chaseTimer = 0f;
        }
        else
        {
            // Player is out of range or not visible, increment timer
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
            
            // Reset to original material colors
            ResetMaterialColors();
            
            // Stop alert particle effect
            if (alertParticle != null)
            {
                alertParticle.Stop();
            }
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

    bool CanSeePlayer()
    {
        // Direction to player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        
        // Cast a ray from enemy towards player
        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange))
        {
            // Check if the ray hit the player
            if (hit.transform.CompareTag("Player"))
            {
                return true; // Can see player
            }
        }
        
        return false; // Wall or obstacle blocking view
    }

    void SetMaterialColors(Color color)
    {
        foreach (Renderer renderer in enemyRenderers)
        {
            foreach (Material material in renderer.materials)
            {
                material.color = color;
            }
        }
    }

    void ResetMaterialColors()
    {
        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            enemyRenderers[i].materials = originalMaterials[i];
        }
    }

    // Visualize detection range in Scene view
    void OnDrawGizmosSelected()
    {
        // Detection range sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Line of sight ray (only in Play mode when player exists)
        if (player != null)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            
            // Draw green line if can see player, red if blocked
            if (CanSeePlayer())
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            
            Gizmos.DrawLine(transform.position, transform.position + directionToPlayer * detectionRange);
        }
    }
}
