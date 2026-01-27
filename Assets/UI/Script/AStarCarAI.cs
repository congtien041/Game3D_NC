using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PrometeoNPCController))]
[RequireComponent(typeof(NavMeshAgent))]
public class AStarCarAI : MonoBehaviour
{
    [Header("MỤC TIÊU")]
    public Transform playerTarget;
    public Transform finishLine; // KÉO OBJECT ĐÍCH VÀO ĐÂY

    [Header("CẤU HÌNH ĐUỔI BẮT (RUBBER BAND)")]
    public float catchUpDistance = 30f;   // Nếu xa hơn khoảng này sẽ tăng tốc
    public float boostMultiplier = 1.5f;  // Tốc độ nhân lên bao nhiêu (VD: 1.5 lần)
    
    [Header("CẤU HÌNH LÁI A*")]
    public float lookAheadDistance = 50f;
    public float steeringSensitivity = 1.5f;

    [Header("AUTO UNSTUCK (TỰ GIẢI CỨU)")]
    public float minSpeedThreshold = 2f;    
    public float timeToWaitBeforeReverse = 2f; 
    public float reverseDuration = 1.5f;    
    public float reverseForce = 15000f; 

    [Header("CẢM BIẾN NÉ TƯỜNG")]
    public float sensorLength = 8f;
    public float sensorSideOffset = 1f;
    public LayerMask obstacleLayer;

    private PrometeoNPCController carController;
    private NavMeshAgent agent;
    private Rigidbody rb; 
    
    // Biến lưu thông số gốc để reset khi không boost
    private int defaultMaxSpeed;
    private int defaultAccel;

    // Biến trạng thái
    private float pathUpdateTimer = 0f;
    private float stuckTimer = 0f;       
    private float reversingTimer = 0f;   
    private bool isReversing = false;    
    private Transform currentTarget; 
    [Header("CHEAT TỐC ĐỘ")]
    public float extraPushForce = 10000f; // Lực đẩy thêm vào mông xe
    public float maxExtraSpeed = 150f;    // Giới hạn tốc độ tối đa khi đẩy

    void Start()
    {
        carController = GetComponent<PrometeoNPCController>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>(); 

        // Lưu lại thông số gốc của xe
        defaultMaxSpeed = carController.maxSpeed;
        defaultAccel = carController.accelerationMultiplier;

        if (playerTarget == null && GameObject.FindWithTag("Player"))
            playerTarget = GameObject.FindWithTag("Player").transform;

        // Mặc định mục tiêu ban đầu là Player
        currentTarget = playerTarget;

        agent.updatePosition = false; 
        agent.updateRotation = false; 
        agent.updateUpAxis = false;
        
        if (rb != null) {
            rb.isKinematic = false;
            rb.linearDamping = 0.05f; 
        }
    }

    void Update()
    {
        if (playerTarget == null) return;

        // --- 1. LOGIC CHỌN MỤC TIÊU (PLAYER HAY ĐÍCH?) ---
        UpdateRaceStatus();

        // --- 2. LOGIC TĂNG TỐC (RUBBER BANDING) ---
        UpdateRubberBanding();

        // --- 3. CẬP NHẬT ĐƯỜNG ĐI ---
        agent.nextPosition = transform.position;
        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer > 0.2f) 
        {
            // Đi tới mục tiêu hiện tại (đã được tính toán ở bước 1)
            if (currentTarget != null)
                agent.SetDestination(currentTarget.position);
            
            pathUpdateTimer = 0f;
        }

        // --- 4. MÁY TRẠNG THÁI LÁI XE ---
        if (isReversing)
        {
            HandleReverseState();
        }
        else
        {
            CheckIfStuck();
            DriveCarOnPath();
        }
    }

    void UpdateRaceStatus()
    {
        // Nếu không có đích thì mặc định luôn đuổi theo Player
        if (finishLine == null) {
            currentTarget = playerTarget;
            return;
        }

        // Tính khoảng cách tới đích của cả 2
        float distPlayerToFinish = Vector3.Distance(playerTarget.position, finishLine.position);
        float distNpcToFinish = Vector3.Distance(transform.position, finishLine.position);

        // Nếu NPC gần đích hơn Player -> NPC đang dẫn trước -> Chạy về đích
        if (distNpcToFinish < distPlayerToFinish)
        {
            currentTarget = finishLine;
        }
        else
        {
            // Nếu Player gần đích hơn -> NPC đang thua -> Đuổi theo Player (để tạo áp lực)
            currentTarget = playerTarget;
        }
    }

    void UpdateRubberBanding()
    {
        // Chỉ kích hoạt tăng tốc khi đang đuổi theo Player (chưa vượt)
        if (currentTarget == playerTarget)
        {
            float distToPlayer = Vector3.Distance(transform.position, playerTarget.position);

            if (distToPlayer > catchUpDistance)
            {
                // Kích hoạt Boost
                carController.maxSpeed = Mathf.RoundToInt(defaultMaxSpeed * boostMultiplier);
                carController.accelerationMultiplier = Mathf.RoundToInt(defaultAccel * boostMultiplier);
            }
            else
            {
                // Về bình thường
                carController.maxSpeed = defaultMaxSpeed;
                carController.accelerationMultiplier = defaultAccel;
            }
        }
        else
        {
            // Khi đang dẫn trước (chạy về đích), chạy tốc độ bình thường (hoặc bạn có thể giảm tốc nếu muốn Player dễ thắng hơn)
            carController.maxSpeed = defaultMaxSpeed;
            carController.accelerationMultiplier = defaultAccel;
        }
    }

    void FixedUpdate()
    {
        if (!isReversing && rb.linearVelocity.magnitude < 0.1f)
        {
            rb.AddForce(transform.forward * 2000f, ForceMode.Force);
        }
    }

    // --- CÁC HÀM CŨ GIỮ NGUYÊN ---
    void CheckIfStuck()
    {
        if (Mathf.Abs(carController.carSpeed) < minSpeedThreshold)
        {
            stuckTimer += Time.deltaTime; 
            if (stuckTimer > timeToWaitBeforeReverse) StartReverse(); 
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
        Vector3 pushDirection = -transform.forward; 
        rb.AddForce(pushDirection * reverseForce * Time.deltaTime, ForceMode.Force);

        if (reversingTimer > reverseDuration)
        {
            isReversing = false; 
            reversingTimer = 0f;
            pathUpdateTimer = 1f; 
            rb.linearVelocity = Vector3.zero;
        }
    }

    void DriveCarOnPath()
    {
        if (agent.pathPending || currentTarget == null) return;

        // Dùng currentTarget thay vì playerTarget
        Vector3 targetPoint = currentTarget.position;

        if (agent.hasPath && agent.path.corners.Length >= 2)
        {
            targetPoint = agent.path.corners[1];
            if (Vector3.Distance(transform.position, targetPoint) < 5f && agent.path.corners.Length > 2)
            {
                targetPoint = agent.path.corners[2];
            }
        }

        Vector3 localTarget = transform.InverseTransformPoint(targetPoint);
        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        float steerInput = Mathf.Clamp(targetAngle / 25f * steeringSensitivity, -1f, 1f);

        float avoidance = CheckWallSensors();
        if (avoidance != 0) steerInput = avoidance;

        carController.AI_Turn(steerInput);
        
        if (Mathf.Abs(steerInput) > 0.8f && carController.carSpeed > 30f)
             carController.AI_GoForward();
        else
            carController.AI_GoForward();

        if (Mathf.Abs(steerInput) < 0.5f || carController.carSpeed < 30f)
        {
            carController.AI_GoForward();

            // --- CODE MỚI: ĐẨY THÊM LỰC ---
            // Chỉ đẩy nếu chưa đạt tốc độ tối đa
            if (carController.carSpeed < maxExtraSpeed)
            {
                // Đẩy theo hướng mũi xe (transform.forward)
                rb.AddForce(transform.forward * extraPushForce * Time.deltaTime, ForceMode.Force);
            }
            // -----------------------------
        }
        else
        {
            // Khi cua gắt thì vẫn đạp ga nhưng không đẩy thêm lực (để tránh văng xe)
            carController.AI_GoForward();
        }
    }

    float CheckWallSensors()
    {
        Vector3 pos = transform.position + transform.up * 0.5f + transform.forward * 2.5f;
        Vector3 fwd = transform.forward;
        Vector3 right = transform.right;
        RaycastHit hit;
        
        if (Physics.Raycast(pos, fwd, out hit, sensorLength, obstacleLayer))
             return (Vector3.Dot(hit.normal, right) > 0) ? 1f : -1f;
        if (Physics.Raycast(pos + right * sensorSideOffset, (fwd + right).normalized, out hit, sensorLength * 0.8f, obstacleLayer))
             return -1f;
        if (Physics.Raycast(pos - right * sensorSideOffset, (fwd - right).normalized, out hit, sensorLength * 0.8f, obstacleLayer))
             return 1f;

        return 0;
    }
}