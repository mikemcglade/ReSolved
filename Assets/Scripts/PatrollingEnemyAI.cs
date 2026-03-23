using UnityEngine;
using UnityEngine.AI;

public class PatrollingEnemyAI : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────
    // WEEK 4 ADDITIONS:
    //   • Area patrol mode (random wandering within a zone)
    //   • Patrol speed / chase speed split
    //   • Per-enemy detection range + chase timeout tuning
    //   • Chase speed boost applied to NavMeshAgent at runtime
    // ─────────────────────────────────────────────────────────

    [Header("Patrol Mode")]
    [Tooltip("Waypoint = fixed route. Area = random wander inside the zone.")]
    [SerializeField] private PatrolMode patrolMode = PatrolMode.Waypoint;

    public enum PatrolMode { Waypoint, Area }

    // ── Waypoint settings ──────────────────────────────────
    [Header("Waypoint Patrol Settings")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waypointWaitTime = 2f;

    // ── Area patrol settings ───────────────────────────────
    [Header("Area Patrol Settings")]
    [Tooltip("Centre of the wander zone.")]
    [SerializeField] private Transform areaCenter;
    [Tooltip("Radius of the wander zone.")]
    [SerializeField] private float areaRadius = 10f;
    [Tooltip("Seconds to wait at each random destination before picking the next.")]
    [SerializeField] private float areaWaitTime = 2f;

    // ── Speed settings ─────────────────────────────────────
    [Header("Speed Settings")]
    [Tooltip("NavMeshAgent speed while patrolling.")]
    [SerializeField] private float patrolSpeed = 3f;
    [Tooltip("NavMeshAgent speed while chasing the player.")]
    [SerializeField] private float chaseSpeed = 5f;

    // ── Chase / detection settings ─────────────────────────
    [Header("Chase Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float chaseTimeout = 2f;

    // ── Visual feedback ────────────────────────────────────
    [Header("Visual Feedback")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color chaseColor = Color.red;
    [SerializeField] private ParticleSystem alertParticle;

    // ── Private references ─────────────────────────────────
    private NavMeshAgent agent;
    private Transform player;
    private Renderer[] enemyRenderers;
    private Material[][] originalMaterials;

    // ── Waypoint state ─────────────────────────────────────
    private int currentWaypointIndex = 0;

    // ── Shared patrol/chase timers ─────────────────────────
    private float waitTimer = 0f;
    private float chaseTimer = 0f;

    // ── State machine ──────────────────────────────────────
    private enum State { Patrolling, Waiting, Chasing, Returning }
    private State currentState = State.Patrolling;

    // ══════════════════════════════════════════════════════
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Cache all renderers (handles multi-material Blender imports)
        enemyRenderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[enemyRenderers.Length][];
        for (int i = 0; i < enemyRenderers.Length; i++)
            originalMaterials[i] = enemyRenderers[i].sharedMaterials;

        // Ensure alert particle is off at start
        if (alertParticle != null)
            alertParticle.Stop();

        // Apply patrol speed at start
        agent.speed = patrolSpeed;

        // Begin first patrol move
        StartFirstPatrol();
    }

    void StartFirstPatrol()
    {
        if (patrolMode == PatrolMode.Waypoint)
        {
            if (waypoints.Length > 0)
                agent.SetDestination(waypoints[currentWaypointIndex].position);
            else
                Debug.LogError("[PatrollingEnemyAI] No waypoints assigned to " + gameObject.name);
        }
        else // Area
        {
            if (areaCenter == null)
            {
                // Fall back to using the enemy's own position as the center
                Debug.LogWarning("[PatrollingEnemyAI] No areaCenter assigned on " + gameObject.name
                    + " — using enemy's own position.");
                areaCenter = transform;
            }
            MoveToRandomAreaPoint();
        }
    }

    // ══════════════════════════════════════════════════════
    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrolling:
                Patrol();
                if (distanceToPlayer <= detectionRange && CanSeePlayer())
                    StartChasing();
                break;

            case State.Waiting:
                Wait();
                if (distanceToPlayer <= detectionRange && CanSeePlayer())
                    StartChasing();
                break;

            case State.Chasing:
                Chase(distanceToPlayer);
                break;

            case State.Returning:
                ReturnToPatrol();
                break;
        }
    }

    // ══════════════════════════════════════════════════════
    //  PATROL
    // ══════════════════════════════════════════════════════
    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentState = State.Waiting;
            waitTimer = 0f;
        }
    }

    void Wait()
    {
        waitTimer += Time.deltaTime;

        float waitDuration = (patrolMode == PatrolMode.Waypoint) ? waypointWaitTime : areaWaitTime;

        if (waitTimer >= waitDuration)
        {
            if (patrolMode == PatrolMode.Waypoint)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                agent.SetDestination(waypoints[currentWaypointIndex].position);
            }
            else
            {
                MoveToRandomAreaPoint();
            }
            currentState = State.Patrolling;
        }
    }

    // ── Area patrol helper ─────────────────────────────────
    void MoveToRandomAreaPoint()
    {
        // Try up to 10 times to find a valid NavMesh point inside the radius
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector3 randomPoint = areaCenter.position + Random.insideUnitSphere * areaRadius;
            randomPoint.y = areaCenter.position.y; // Keep on the same floor level

            NavMeshHit navHit;
            if (NavMesh.SamplePosition(randomPoint, out navHit, areaRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(navHit.position);
                return;
            }
        }

        // Fallback: go back to center if no valid point found
        agent.SetDestination(areaCenter.position);
        Debug.LogWarning("[PatrollingEnemyAI] Could not find valid NavMesh point in area for "
            + gameObject.name + " — moving to center.");
    }

    // ══════════════════════════════════════════════════════
    //  CHASE
    // ══════════════════════════════════════════════════════
    void StartChasing()
    {
        currentState = State.Chasing;
        chaseTimer = 0f;

        // Boost speed for the chase
        agent.speed = chaseSpeed;

        SetMaterialColors(chaseColor);

        if (alertParticle != null)
            alertParticle.Play();
    }

    void Chase(float distanceToPlayer)
    {
        agent.SetDestination(player.position);

        if (distanceToPlayer <= detectionRange && CanSeePlayer())
        {
            chaseTimer = 0f;
        }
        else
        {
            chaseTimer += Time.deltaTime;
            if (chaseTimer >= chaseTimeout)
                currentState = State.Returning;
        }
    }

    // ══════════════════════════════════════════════════════
    //  RETURN TO PATROL
    // ══════════════════════════════════════════════════════
    void ReturnToPatrol()
    {
        if (patrolMode == PatrolMode.Waypoint)
        {
            int nearestIndex = FindNearestWaypoint();
            currentWaypointIndex = nearestIndex;
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
        else
        {
            // For area patrol, just wander back toward the zone center
            agent.SetDestination(areaCenter.position);
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentState = State.Waiting;
            waitTimer = 0f;

            // Restore patrol speed
            agent.speed = patrolSpeed;

            ResetMaterialColors();

            if (alertParticle != null)
                alertParticle.Stop();
        }
    }

    // ══════════════════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════════════════
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
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange))
        {
            if (hit.transform.CompareTag("Player"))
                return true;
        }
        return false;
    }

    void SetMaterialColors(Color color)
    {
        foreach (Renderer renderer in enemyRenderers)
            foreach (Material material in renderer.materials)
                material.color = color;
    }

    void ResetMaterialColors()
    {
        for (int i = 0; i < enemyRenderers.Length; i++)
            enemyRenderers[i].materials = originalMaterials[i];
    }

    // ══════════════════════════════════════════════════════
    //  GIZMOS
    // ══════════════════════════════════════════════════════
    void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Area patrol zone
        if (patrolMode == PatrolMode.Area && areaCenter != null)
        {
            Gizmos.color = new Color(0f, 0.8f, 1f, 0.25f);
            Gizmos.DrawSphere(areaCenter.position, areaRadius);
            Gizmos.color = new Color(0f, 0.8f, 1f, 1f);
            Gizmos.DrawWireSphere(areaCenter.position, areaRadius);
        }

        // Line-of-sight ray (Play mode only)
        if (player != null)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            Gizmos.color = CanSeePlayer() ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, transform.position + directionToPlayer * detectionRange);
        }
    }
}
