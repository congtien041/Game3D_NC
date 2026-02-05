using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(PrometeoNPCController))]
[RequireComponent(typeof(NavMeshAgent))]
public class AStarCarAI : MonoBehaviour
{
    [Header("MỤC TIÊU")]
    public Transform playerTarget;
    public Transform finishLine;

    [Header("CẤU HÌNH ĐƯỜNG CONG (BEZIER)")]
    public int curveResolution = 20;     // Số lượng đoạn chia nhỏ (Tia đỏ sẽ chia làm 20 đoạn)
    public float lookAheadRatio = 0.5f;  // 0 -> 1. Càng cao càng nhìn xa về phía đích của đường cong

    [Header("CẤU HÌNH LÁI")]
    public float steeringSpeed = 5f;
    public float maxCornerSpeed = 50f;

    [Header("CHECK ĐƯỜNG (SphereCast)")]
    public float pathCheckRadius = 1.0f;

    [Header("HỖ TRỢ XOAY")]
    public bool enableRotationAssist = true;
    public float rotationAssistSpeed = 2f;
    public float angleThreshold = 10f;

    [Header("CẤU HÌNH KHÁC")]
    public float catchUpDistance = 30f;
    public float boostMultiplier = 1.5f;
    public float minSpeedThreshold = 2f;
    public float timeToWaitBeforeReverse = 2f;
    public float reverseDuration = 1.5f;
    public float reverseForce = 15000f;
    public float sensorLength = 8f;
    public float sensorSideOffset = 1f;
    public LayerMask obstacleLayer;
    public float extraPushForce = 10000f;
    public float maxExtraSpeed = 150f;

    private PrometeoNPCController carController;
    private NavMeshAgent agent;
    private Rigidbody rb;
    private int defaultMaxSpeed;
    private int defaultAccel;
    private float pathUpdateTimer = 0f;
    private float stuckTimer = 0f;
    private float reversingTimer = 0f;
    private bool isReversing = false;
    private Transform currentTarget;
    private float currentSteerInput = 0f;

    // Biến lưu đường cong để vẽ Gizmos
    private List<Vector3> smoothPathPoints = new List<Vector3>();
    private Vector3 currentTargetPointOnCurve;

    void Start()
    {
        carController = GetComponent<PrometeoNPCController>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        defaultMaxSpeed = carController.maxSpeed;
        defaultAccel = carController.accelerationMultiplier;

        if (playerTarget == null && GameObject.FindWithTag("Player"))
            playerTarget = GameObject.FindWithTag("Player").transform;
        currentTarget = playerTarget;

        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        if (rb != null) { rb.isKinematic = false; rb.linearDamping = 0.05f; }
    }

    void Update()
    {
        if (playerTarget == null) return;
        UpdateRaceStatus();
        UpdateRubberBanding();

        // Chỉ cập nhật Path NavMesh mỗi 0.2s để đỡ giật, nhưng đường cong Bezier sẽ tính mỗi khung hình
        agent.nextPosition = transform.position;
        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer > 0.2f)
        {
            if (currentTarget != null) agent.SetDestination(currentTarget.position);
            pathUpdateTimer = 0f;
        }

        if (isReversing) HandleReverseState();
        else { CheckIfStuck(); DriveCarOnPath(); }
    }

    void DriveCarOnPath()
    {
        if (agent.pathPending || currentTarget == null) return;

        // --- 1. TÍNH TOÁN ĐƯỜNG CONG BEZIER ---
        // Thay vì nối thẳng, ta tạo đường cong qua 3 điểm: Xe -> Góc Cua 1 -> Góc Cua 2
        
        Vector3 p0 = transform.position;
        Vector3 p1 = transform.position + transform.forward * 10f; // Mặc định nếu ko có path
        Vector3 p2 = transform.position + transform.forward * 20f;

        if (agent.hasPath && agent.path.corners.Length > 1)
        {
            p1 = agent.path.corners[1]; // Điểm rẽ tiếp theo

            if (agent.path.corners.Length > 2)
            {
                p2 = agent.path.corners[2]; // Điểm sau điểm rẽ
            }
            else
            {
                // Nếu chỉ còn 1 điểm đích cuối cùng, p2 là đích luôn
                p2 = p1; 
            }
        }

        // Tạo đường cong mềm
        GenerateSmoothPath(p0, p1, p2);

        // --- 2. CHỌN MỤC TIÊU TRÊN ĐƯỜNG CONG ---
        // Thay vì lái tới p1 (dễ đâm tường), ta lái tới một điểm nằm trên đường cong
        // lookAheadRatio (0.5) nghĩa là lấy điểm nằm giữa đường cong
        currentTargetPointOnCurve = GetPointOnBezierCurve(p0, p1, p2, lookAheadRatio);

        // Check tường cho điểm mục tiêu này (An toàn tuyệt đối)
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        Vector3 direction = (currentTargetPointOnCurve - rayStart).normalized;
        float distance = Vector3.Distance(rayStart, currentTargetPointOnCurve);

        if (Physics.SphereCast(rayStart, pathCheckRadius, direction, out RaycastHit hit, distance, obstacleLayer))
        {
            // Nếu đường cong bị chặn, bắt buộc nhìn gần lại (về p1) để tránh va chạm
             currentTargetPointOnCurve = Vector3.Lerp(p0, p1, 0.8f); 
        }

        // --- 3. LÁI XE THEO ĐIỂM TRÊN ĐƯỜNG CONG ---
        Vector3 localTarget = transform.InverseTransformPoint(currentTargetPointOnCurve);
        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        
        float targetSteer = Mathf.Clamp(targetAngle / 30f, -1f, 1f); 
        currentSteerInput = Mathf.Lerp(currentSteerInput, targetSteer, Time.deltaTime * steeringSpeed);

        // Né tường khẩn cấp (Override)
        float avoidance = CheckWallSensors();
        if (avoidance != 0) currentSteerInput = Mathf.Lerp(currentSteerInput, avoidance, Time.deltaTime * 10f);

        carController.AI_Turn(currentSteerInput);

        // --- 4. HỖ TRỢ XOAY & GA ---
        Vector3 dirToCurve = (currentTargetPointOnCurve - transform.position).normalized;
        float angleToCurve = Vector3.SignedAngle(transform.forward, dirToCurve, Vector3.up);

        if (enableRotationAssist && rb.linearVelocity.magnitude > 5f)
        {
            if (Mathf.Abs(angleToCurve) > angleThreshold)
            {
                Quaternion targetRotation = Quaternion.LookRotation(dirToCurve);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * rotationAssistSpeed));
            }
        }

        float absSteer = Mathf.Abs(currentSteerInput);
        if (absSteer > 0.5f) 
        {
            if (carController.carSpeed > maxCornerSpeed) carController.AI_Brake();
            else carController.AI_GoForward();
        }
        else 
        {
            carController.AI_GoForward();
            if (absSteer < 0.2f && carController.carSpeed < maxExtraSpeed)
                 rb.AddForce(transform.forward * extraPushForce * Time.deltaTime, ForceMode.Force);
        }
    }

    // --- THUẬT TOÁN BEZIER (TẠO ĐỘ CONG) ---
    void GenerateSmoothPath(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        smoothPathPoints.Clear();
        // Chia đường đi thành 'curveResolution' đoạn nhỏ
        for (int i = 0; i <= curveResolution; i++)
        {
            float t = i / (float)curveResolution;
            Vector3 point = GetPointOnBezierCurve(p0, p1, p2, t);
            smoothPathPoints.Add(point);
        }
    }

    // Công thức Quadratic Bezier: B(t) = (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
    Vector3 GetPointOnBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        
        Vector3 p = uu * p0; // (1-t)^2 * P0
        p += 2 * u * t * p1; // 2(1-t)t * P1
        p += tt * p2;        // t^2 * P2
        
        return p;
    }

    // --- VẼ GIZMOS (Tia đỏ cong chia đoạn) ---
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Vẽ đường cong (Tia đỏ chia nhiều đoạn)
        if (smoothPathPoints.Count > 1)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < smoothPathPoints.Count - 1; i++)
            {
                Gizmos.DrawLine(smoothPathPoints[i], smoothPathPoints[i+1]);
                // Vẽ các điểm chia nhỏ để thấy rõ từng đoạn
                Gizmos.DrawSphere(smoothPathPoints[i+1], 0.2f);
            }
        }

        // Vẽ điểm mục tiêu hiện tại (Màu xanh)
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(currentTargetPointOnCurve, 1f);
        Gizmos.DrawLine(transform.position, currentTargetPointOnCurve);
    }

    // (Giữ nguyên các hàm phụ trợ CheckWallSensors, UpdateRaceStatus, v.v...)
    float CheckWallSensors()
    {
        Vector3 pos = transform.position + transform.up * 0.5f + transform.forward * 2.5f;
        Vector3 fwd = transform.forward;
        Vector3 right = transform.right;
        RaycastHit hit;
        if (Physics.Raycast(pos, fwd, out hit, sensorLength, obstacleLayer)) return (Vector3.Dot(hit.normal, right) > 0) ? 1f : -1f;
        if (Physics.Raycast(pos + right * sensorSideOffset, (fwd + right).normalized, out hit, sensorLength * 0.8f, obstacleLayer)) return -1f;
        if (Physics.Raycast(pos - right * sensorSideOffset, (fwd - right).normalized, out hit, sensorLength * 0.8f, obstacleLayer)) return 1f;
        return 0;
    }
    
    void UpdateRaceStatus() {
        if (finishLine == null) { currentTarget = playerTarget; return; }
        float distPlayer = Vector3.Distance(playerTarget.position, finishLine.position);
        float distNpc = Vector3.Distance(transform.position, finishLine.position);
        currentTarget = (distNpc < distPlayer) ? finishLine : playerTarget;
    }

    void UpdateRubberBanding() {
        if (currentTarget == playerTarget) {
            float dist = Vector3.Distance(transform.position, playerTarget.position);
            if (dist > catchUpDistance) {
                carController.maxSpeed = Mathf.RoundToInt(defaultMaxSpeed * boostMultiplier);
                carController.accelerationMultiplier = Mathf.RoundToInt(defaultAccel * boostMultiplier);
            } else {
                carController.maxSpeed = defaultMaxSpeed;
                carController.accelerationMultiplier = defaultAccel;
            }
        } else {
            carController.maxSpeed = defaultMaxSpeed;
            carController.accelerationMultiplier = defaultAccel;
        }
    }
    
    void CheckIfStuck() {
        if (Mathf.Abs(carController.carSpeed) < minSpeedThreshold) {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > timeToWaitBeforeReverse) StartReverse();
        } else stuckTimer = 0f;
    }

    void StartReverse() {
        isReversing = true;
        reversingTimer = 0f;
        stuckTimer = 0f;
    }

    void HandleReverseState() {
        reversingTimer += Time.deltaTime;
        carController.AI_Turn(-1f);
        rb.AddForce(-transform.forward * reverseForce * Time.deltaTime, ForceMode.Force);
        if (reversingTimer > reverseDuration) {
            isReversing = false;
            reversingTimer = 0f;
            pathUpdateTimer = 1f;
            rb.linearVelocity = Vector3.zero;
        }
    }
}