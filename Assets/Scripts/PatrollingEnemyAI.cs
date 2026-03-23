using UnityEngine;
using UnityEngine.AI;

public class PatrollingEnemyAI : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────
    // WEEK 5 ADDITIONS:
    //   • Field of view (angle-based detection cone)       [5A]
    //   • Sound / hearing radius (omnidirectional)         [5B]
    //   • Catch detection (trigger → lives deduction)      [5C]
    //   • Enemy type colours (Wanderer vs Guardian)        [5C]
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
    [SerializeField] private Transform areaCenter;
    [SerializeField] private float areaRadius = 10f;
    [SerializeField] private float areaWaitTime = 2f;

    // ── Speed settings ─────────────────────────────────────
    [Header("Speed Settings")]
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float chaseSpeed = 5f;

    // ── Chase / detection settings ─────────────────────────
    [Header("Chase Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float chaseTimeout = 2f;

    // ── Week 5A: FOV ───────────────────────────────────────
    [Header("Week 5 - Detection")]
    [Tooltip("Half-angle of the vision cone in degrees. 60 = 120 degree cone.")]
    [SerializeField] private float viewAngle = 60f;

    // ── Week 5B: Hearing ───────────────────────────────────
    [Tooltip("Omnidirectional hearing radius. Smaller than detectionRange.")]
    [SerializeField] private float hearingRange = 4f;

    // ── Week 5C: Enemy type ────────────────────────────────
   public enum EnemyType { Wanderer, Guardian }

    [Header("Week 5 - Enemy Type")]
    [Tooltip("Wanderer = area patrol (crimson). Guardian = waypoint patrol (blue).")]
    [SerializeField] private EnemyType enemyType = EnemyType.Guardian;

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

        enemyRenderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[enemyRenderers.Length][];
        for (int i = 0; i < enemyRenderers.Length; i++)
            originalMaterials[i] = enemyRenderers[i].sharedMaterials;

        ApplyTypeColour();

        if (alertParticle != null)
            alertParticle.Stop();

        agent.speed = patrolSpeed;
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
        else
        {
            if (areaCenter == null)
            {
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
                if (CanSeePlayer() || CanHearPlayer())
                    StartChasing();
                break;

            case State.Waiting:
                Wait();
                if (CanSeePlayer() || CanHearPlayer())
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

    void MoveToRandomAreaPoint()
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector3 randomPoint = areaCenter.position + Random.insideUnitSphere * areaRadius;
            randomPoint.y = areaCenter.position.y;

            NavMeshHit navHit;
            if (NavMesh.SamplePosition(randomPoint, out navHit, areaRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(navHit.position);
                return;
            }
        }

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
            agent.SetDestination(areaCenter.position);
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentState = State.Waiting;
            waitTimer = 0f;
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

    // ── Week 5A: FOV detection ─────────────────────────────
    bool CanSeePlayer()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > detectionRange) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > viewAngle) return false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange))
            return hit.transform.CompareTag("Player");

        return false;
    }

    // ── Week 5B: Hearing detection ─────────────────────────
    bool CanHearPlayer()
    {
        return Vector3.Distance(transform.position, player.position) <= hearingRange;
    }

    // ── Week 5C: Enemy type colours ────────────────────────
    void ApplyTypeColour()
    {
        Color skinCol, tentacleCol;

        if (enemyType == EnemyType.Wanderer)
        {
            skinCol     = new Color(0.55f, 0.1f,  0.1f);
            tentacleCol = new Color(0.8f,  0.2f,  0.0f);
        }
        else
        {
            skinCol     = new Color(0.15f, 0.15f, 0.5f);
            tentacleCol = new Color(0.2f,  0.0f,  0.7f);
        }

        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            Material[] mats = enemyRenderers[i].materials;
            for (int j = 0; j < mats.Length; j++)
            {
                if (mats[j].name.ToLower().Contains("tentacle"))
                    mats[j].color = tentacleCol;
                else
                    mats[j].color = skinCol;
            }
            enemyRenderers[i].materials = mats;
        }

        for (int i = 0; i < enemyRenderers.Length; i++)
            originalMaterials[i] = enemyRenderers[i].sharedMaterials;
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.cyan;
        Vector3 leftEdge  = Quaternion.AngleAxis(-viewAngle, Vector3.up) * transform.forward * detectionRange;
        Vector3 rightEdge = Quaternion.AngleAxis( viewAngle, Vector3.up) * transform.forward * detectionRange;
        Gizmos.DrawLine(transform.position, transform.position + leftEdge);
        Gizmos.DrawLine(transform.position, transform.position + rightEdge);

        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, hearingRange);

        if (patrolMode == PatrolMode.Area && areaCenter != null)
        {
            Gizmos.color = new Color(0f, 0.8f, 1f, 0.25f);
            Gizmos.DrawSphere(areaCenter.position, areaRadius);
            Gizmos.color = new Color(0f, 0.8f, 1f, 1f);
            Gizmos.DrawWireSphere(areaCenter.position, areaRadius);
        }

        if (player != null)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            Gizmos.color = CanSeePlayer() ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, transform.position + directionToPlayer * detectionRange);
        }
    }
}
