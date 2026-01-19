using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PrometeoNPCController))]
[RequireComponent(typeof(NavMeshAgent))]
public class AStarCarAI : MonoBehaviour
{
    [Header("MỤC TIÊU")]
    public Transform playerTarget;

    [Header("CẤU HÌNH LÁI A*")]
    public float lookAheadDistance = 50f;
    public float steeringSensitivity = 1.5f;

    [Header("AUTO UNSTUCK (TỰ GIẢI CỨU)")]
    public float minSpeedThreshold = 2f;    
    public float timeToWaitBeforeReverse = 2f; 
    public float reverseDuration = 1.5f;    
    
    // Thêm biến lực đẩy lùi
    public float reverseForce = 15000f; // Lực đẩy vật lý (cần số lớn vì xe nặng)

    [Header("CẢM BIẾN NÉ TƯỜNG")]
    public float sensorLength = 8f;
    public float sensorSideOffset = 1f;
    public LayerMask obstacleLayer;

    private PrometeoNPCController carController;
    private NavMeshAgent agent;
    private Rigidbody rb; // Cần Rigidbody để đẩy xe
    
    // Các biến đếm giờ
    private float pathUpdateTimer = 0f;
    private float stuckTimer = 0f;       
    private float reversingTimer = 0f;   
    private bool isReversing = false;    

    void Start()
    {
        carController = GetComponent<PrometeoNPCController>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>(); // Lấy Rigidbody

        if (playerTarget == null && GameObject.FindWithTag("Player"))
            playerTarget = GameObject.FindWithTag("Player").transform;

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

        agent.nextPosition = transform.position;

        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer > 0.2f) 
        {
            agent.SetDestination(playerTarget.position);
            pathUpdateTimer = 0f;
        }

        // --- MÁY TRẠNG THÁI ---
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

    void FixedUpdate()
    {
        // Fix lỗi trôi khi đứng yên (chỉ chạy khi KHÔNG lùi)
        if (!isReversing && rb.linearVelocity.magnitude < 0.1f)
        {
            rb.AddForce(transform.forward * 2000f, ForceMode.Force);
        }
    }

    // --- 1. KIỂM TRA KẸT ---
    void CheckIfStuck()
    {
        // Nếu tốc độ < 2 và đang không lùi
        if (Mathf.Abs(carController.carSpeed) < minSpeedThreshold)
        {
            stuckTimer += Time.deltaTime; 
            
            if (stuckTimer > timeToWaitBeforeReverse)
            {
                StartReverse(); 
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    // --- 2. BẮT ĐẦU LÙI ---
    void StartReverse()
    {
        isReversing = true;
        reversingTimer = 0f;
        stuckTimer = 0f; 
    }

    // --- 3. XỬ LÝ LÙI BẰNG VẬT LÝ (QUAN TRỌNG) ---
    void HandleReverseState()
    {
        reversingTimer += Time.deltaTime;

        // A. NGẮT GA: Không gọi AI_GoForward() ở đây nữa để động cơ thả lỏng

        // B. BẺ LÁI: Vẫn bẻ lái để đuôi xe văng ra
        carController.AI_Turn(-1f); 

        // C. ĐẨY XE: Dùng lực vật lý tác động trực tiếp vào Rigidbody
        // Dùng ForceMode.Force và nhân với Time.deltaTime (hoặc đẩy trong FixedUpdate)
        // Nhưng ở Update, ta dùng lực tức thời nhẹ hoặc đẩy mạnh trong FixedUpdate.
        // Ở đây đẩy trực tiếp ngược hướng forward:
        
        // Mẹo: Đẩy ngược hướng xe + một chút hướng lên trên để tránh ma sát bánh xe
        Vector3 pushDirection = -transform.forward; 
        
        // Đẩy xe lùi lại (ForceMode.Force cần gọi liên tục)
        rb.AddForce(pushDirection * reverseForce * Time.deltaTime, ForceMode.Force);

        // Nếu lùi xong
        if (reversingTimer > reverseDuration)
        {
            isReversing = false; 
            reversingTimer = 0f;
            pathUpdateTimer = 1f; 
            
            // Dừng hẳn xe lại một chút để không bị trôi lực cũ
            rb.linearVelocity = Vector3.zero;
        }
    }

    // --- 4. LÁI BÌNH THƯỜNG ---
    void DriveCarOnPath()
    {
        if (agent.pathPending) return;

        Vector3 targetPoint = playerTarget.position;

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
        
        // Chỉ đạp ga khi không kẹt
        if (Mathf.Abs(steerInput) > 0.8f && carController.carSpeed > 30f)
        {
             carController.AI_GoForward();
        }
        else
        {
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
        {
             return (Vector3.Dot(hit.normal, right) > 0) ? 1f : -1f;
        }
        if (Physics.Raycast(pos + right * sensorSideOffset, (fwd + right).normalized, out hit, sensorLength * 0.8f, obstacleLayer))
             return -1f;
        if (Physics.Raycast(pos - right * sensorSideOffset, (fwd - right).normalized, out hit, sensorLength * 0.8f, obstacleLayer))
             return 1f;

        return 0;
    }
}