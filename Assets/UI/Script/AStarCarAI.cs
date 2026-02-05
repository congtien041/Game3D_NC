using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PrometeoNPCController))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class AStarCarAI : MonoBehaviour
{
    // ================= PLAYER =================
    private Transform playerTarget;
    private bool playerFound = false;

    // ================= WAYPOINT =================
    [Header("WAYPOINT CIRCUIT")]
    public Transform[] waypoints;
    public float waypointReachDistance = 8f;
    private int currentWaypointIndex = 0;

    // ================= RUBBER BAND =================
    [Header("RUBBER BANDING")]
    public float catchUpDistance = 30f;
    public float boostMultiplier = 1.5f;

    // ================= STEERING =================
    [Header("STEERING")]
    public float steeringSensitivity = 1.5f;

    // ================= UNSTUCK =================
    [Header("AUTO UNSTUCK")]
    public float minSpeedThreshold = 2f;
    public float timeToWaitBeforeReverse = 2f;
    public float reverseDuration = 1.5f;
    public float reverseForce = 15000f;

    // ================= WALL SENSOR =================
    [Header("WALL SENSOR")]
    public float sensorLength = 8f;
    public float sensorSideOffset = 1f;
    public LayerMask obstacleLayer;

    // ================= CHEAT =================
    [Header("EXTRA PUSH")]
    public float extraPushForce = 10000f;
    public float maxExtraSpeed = 150f;

    // ================= COMPONENT =================
    private PrometeoNPCController carController;
    private NavMeshAgent agent;
    private Rigidbody rb;

    // ================= DEFAULT =================
    private int defaultMaxSpeed;
    private int defaultAccel;

    // ================= STATE =================
    private float pathUpdateTimer;
    private float stuckTimer;
    private float reversingTimer;
    private bool isReversing;

    // ================= START =================
    void Start()
    {
        carController = GetComponent<PrometeoNPCController>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        defaultMaxSpeed = carController.maxSpeed;
        defaultAccel = carController.accelerationMultiplier;

        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        rb.isKinematic = false;
        rb.linearDamping = 0.05f;
    }

    // ================= UPDATE =================
    void Update()
    {
        // 🔍 TÌM PLAYER (CHỜ SPAWN BUNDLE)
        if (!playerFound)
            TryFindPlayer();

        if (waypoints == null || waypoints.Length == 0)
            return;

        UpdateWaypoint();

        if (playerFound)
            UpdateRubberBanding();

        agent.nextPosition = transform.position;
        pathUpdateTimer += Time.deltaTime;

        if (pathUpdateTimer > 0.2f)
        {
            agent.SetDestination(GetCurrentWaypoint().position);
            pathUpdateTimer = 0f;
        }

        if (isReversing)
            HandleReverseState();
        else
        {
            CheckIfStuck();
            DriveCarOnPath();
        }
    }

    // ================= PLAYER FIND =================
    void TryFindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
            playerFound = true;
            Debug.Log("✅ AI FOUND PLAYER (ASSETBUNDLE)");
        }
    }

    // ================= WAYPOINT =================
    Transform GetCurrentWaypoint()
    {
        return waypoints[currentWaypointIndex];
    }

    void UpdateWaypoint()
    {
        float dist = Vector3.Distance(transform.position, GetCurrentWaypoint().position);
        if (dist < waypointReachDistance)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
                currentWaypointIndex = 0; // vòng tròn
        }
    }

    // ================= RUBBER BAND =================
    void UpdateRubberBanding()
    {
        float distToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distToPlayer > catchUpDistance)
        {
            carController.maxSpeed = Mathf.RoundToInt(defaultMaxSpeed * boostMultiplier);
            carController.accelerationMultiplier = Mathf.RoundToInt(defaultAccel * boostMultiplier);
        }
        else
        {
            carController.maxSpeed = defaultMaxSpeed;
            carController.accelerationMultiplier = defaultAccel;
        }
    }

    // ================= FIXED =================
    void FixedUpdate()
    {
        if (!isReversing && rb.linearVelocity.magnitude < 0.1f)
            rb.AddForce(transform.forward * 2000f, ForceMode.Force);
    }

    // ================= UNSTUCK =================
    void CheckIfStuck()
    {
        if (Mathf.Abs(carController.carSpeed) < minSpeedThreshold)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > timeToWaitBeforeReverse)
                StartReverse();
        }
        else stuckTimer = 0f;
    }

    void StartReverse()
    {
        isReversing = true;
        reversingTimer = 0f;
        stuckTimer = 0f;
    }

    void HandleReverseState()
    {
        reversingTimer += Time.deltaTime;
        carController.AI_Turn(-1f);
        rb.AddForce(-transform.forward * reverseForce * Time.deltaTime, ForceMode.Force);

        if (reversingTimer > reverseDuration)
        {
            isReversing = false;
            reversingTimer = 0f;
            pathUpdateTimer = 1f;
            rb.linearVelocity = Vector3.zero;
        }
    }

    // ================= DRIVE =================
    void DriveCarOnPath()
    {
        if (agent.pathPending || !agent.hasPath) return;

        Vector3 targetPoint = agent.path.corners.Length > 1
            ? agent.path.corners[1]
            : agent.destination;

        Vector3 localTarget = transform.InverseTransformPoint(targetPoint);
        float angle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        float steer = Mathf.Clamp(angle / 25f * steeringSensitivity, -1f, 1f);

        float avoidance = CheckWallSensors();
        if (avoidance != 0) steer = avoidance;

        carController.AI_Turn(steer);
        carController.AI_GoForward();

        if (Mathf.Abs(steer) < 0.5f && carController.carSpeed < maxExtraSpeed)
            rb.AddForce(transform.forward * extraPushForce * Time.deltaTime, ForceMode.Force);
    }

    // ================= WALL SENSOR =================
    float CheckWallSensors()
    {
        Vector3 pos = transform.position + transform.up * 0.5f + transform.forward * 2.5f;
        Vector3 fwd = transform.forward;
        Vector3 right = transform.right;
        RaycastHit hit;

        if (Physics.Raycast(pos, fwd, out hit, sensorLength, obstacleLayer))
            return Vector3.Dot(hit.normal, right) > 0 ? 1f : -1f;

        if (Physics.Raycast(pos + right * sensorSideOffset, (fwd + right).normalized,
            out hit, sensorLength * 0.8f, obstacleLayer))
            return -1f;

        if (Physics.Raycast(pos - right * sensorSideOffset, (fwd - right).normalized,
            out hit, sensorLength * 0.8f, obstacleLayer))
            return 1f;

        return 0f;
    }
}
