using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(PrometeoNPCController))]
[RequireComponent(typeof(NavMeshAgent))]
public class AStarCarAI : MonoBehaviour
{
    [Header("MỤC TIÊU")]
    public Transform playerTarget;

    [Header("CẤU HÌNH LÁI A*")]
    public float lookAheadDistance = 50f; // Nhìn xa 5m trên đường NavMesh
    public float steeringSensitivity = 1.5f; // Độ nhạy lái

    [Header("CẢM BIẾN NÉ TƯỜNG (DỰ PHÒNG)")]
    // Vẫn cần cảm biến vì xe có thể trượt ra khỏi NavMesh khi drift
    public float sensorLength = 6f;
    public float sensorStartOffset = 2f;
    public float sensorSideOffset = 0.8f;
    public LayerMask obstacleLayer;

    private PrometeoNPCController carController;
    private NavMeshAgent agent;

    void Start()
    {
        carController = GetComponent<PrometeoNPCController>();
        agent = GetComponent<NavMeshAgent>();

        if (playerTarget == null && GameObject.FindWithTag("Player"))
            playerTarget = GameObject.FindWithTag("Player").transform;

        // --- CẤU HÌNH NAVMESH ---
        agent.updatePosition = false; 
        agent.updateRotation = false; 
        agent.updateUpAxis = false;

        // --- CẤU HÌNH VẬT LÝ (AUTO FIX LỖI ĐỨNG YÊN) ---
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) 
        {
            rb.sleepThreshold = 0f;       // Không bao giờ ngủ
            rb.isKinematic = false;       // BẮT BUỘC TẮT (Quan trọng nhất)
            rb.useGravity = true;         // Phải có trọng lực
            
            // Nếu xe quá nặng, giảm xuống 1500kg
            if (rb.mass > 2000f) rb.mass = 1500f; 
            
            // Lực cản không khí thấp thôi
            rb.linearDamping = 0.05f; 
        }
    }

    // Thêm biến đếm thời gian
    private float pathUpdateTimer = 0f;

    void Update()
    {
        if (playerTarget == null) return;

        // 1. ĐỒNG BỘ VỊ TRÍ AGENT VỚI XE
        agent.nextPosition = transform.position;

        // 2. TỐI ƯU: Chỉ tìm đường lại sau mỗi 0.2 giây
        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer > 0.2f) 
        {
            agent.SetDestination(playerTarget.position);
            pathUpdateTimer = 0f;
        }
        DriveCarOnPath();

    }

    void FixedUpdate()
    {
        if (GetComponent<Rigidbody>().linearVelocity.magnitude < 0.1f)
        {
            // Đẩy nhẹ xe về phía trước để thắng ma sát nghỉ
            GetComponent<Rigidbody>().AddForce(transform.forward * 2000f, ForceMode.Force);
        }
    }

    void DriveCarOnPath()
    {
        // 1. CHỜ TÍNH TOÁN: Nếu Agent đang tính đường thì giữ nguyên ga, đừng phanh
        if (agent.pathPending) 
        {
            carController.AI_GoForward();
            return;
        }

        Vector3 targetPoint;

        // 2. KIỂM TRA ĐƯỜNG ĐI
        // Nếu không có đường (hoặc đường bị lỗi do Player ra khỏi map)
        if (!agent.hasPath || agent.path.status == NavMeshPathStatus.PathInvalid)
        {
            // --- CHẾ ĐỘ DỰ PHÒNG (FALLBACK) ---
            // NavMesh bó tay -> Chuyển sang chế độ "Mad Max": Lao thẳng tới Player bất chấp địa hình
            targetPoint = playerTarget.position;
            
            // Debug màu đỏ để biết đang chạy chế độ dự phòng
            Debug.DrawLine(transform.position, targetPoint, Color.red);
        }
        else
        {
            // --- CHẾ ĐỘ NAVMESH (BÌNH THƯỜNG) ---
            // Có đường đi đàng hoàng -> Lấy điểm corner tiếp theo
            if (agent.path.corners.Length >= 2)
            {
                targetPoint = agent.path.corners[1]; // Điểm tiếp theo

                // Nếu điểm tiếp theo quá gần (< 5m) và còn điểm nữa -> Nhìn xa hơn chút để lái mượt
                if (Vector3.Distance(transform.position, targetPoint) < 5f && agent.path.corners.Length > 2)
                {
                    targetPoint = agent.path.corners[2];
                }
            }
            else
            {
                // Trường hợp hiếm: Có đường nhưng không có corner -> Lao tới đích luôn
                targetPoint = playerTarget.position;
            }
            
            // Debug màu xanh để biết đang chạy NavMesh tốt
            for (int i = 0; i < agent.path.corners.Length - 1; i++)
                Debug.DrawLine(agent.path.corners[i], agent.path.corners[i + 1], Color.blue);
        }

        // 3. LÁI TỚI MỤC TIÊU ĐÃ CHỌN
        DriveToPoint(targetPoint);
    }

    void DriveToPoint(Vector3 target)
    {
        // 1. Tính toán góc lái
        Vector3 localTarget = transform.InverseTransformPoint(target);
        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        
        // Tăng độ nhạy lái lên (chia cho 25 thay vì 30-40)
        float steerInput = Mathf.Clamp(targetAngle / 25f * steeringSensitivity, -1f, 1f);

        // 2. Cảm biến né tường (Né đè lên lái)
        float avoidance = CheckWallSensors();
        if (avoidance != 0) steerInput = avoidance;

        // 3. THỰC HIỆN LỆNH LÁI
        carController.AI_ReleaseHandbrake(); // Luôn đảm bảo đã nhả phanh tay
        carController.AI_Turn(steerInput);

        // 4. LOGIC GA / PHANH (SỬA LẠI ĐỂ KHÔNG BỊ KẸT)
        
        // TRƯỜNG HỢP A: Xe đang chạy chậm hoặc đứng yên (< 15 km/h)
        // -> TUYỆT ĐỐI KHÔNG PHANH, CỨ ĐẠP GA DÙ ĐANG CUA GẮT
        if (carController.carSpeed < 15f)
        {
            carController.AI_GoForward();
            
            // Mẹo: Nếu kẹt quá lâu (velocity ~ 0), có thể xe bị vướng -> Lùi nhẹ (Tùy chọn)
        }
        // TRƯỜNG HỢP B: Xe đang chạy nhanh (> 15 km/h)
        else 
        {
            // Nếu cần cua gấp (> 0.6) thì mới nhấp phanh drift
            if (Mathf.Abs(steerInput) > 0.6f && carController.carSpeed > 50f)
            {
                // carController.AI_Handbrake();
            }
            // Nếu cua vừa vừa thì nhả chân ga (không phanh, không ga)
            else if (Mathf.Abs(steerInput) > 0.4f && carController.carSpeed > 60f)
            {
                // Thả trôi (Coasting) để vào cua mượt hơn
            }
            // Còn lại đường thẳng -> Đạp ga
            else
            {
                carController.AI_GoForward();
            }
        }
    }

    // Giữ nguyên cảm biến raycast để tránh trường hợp xe drift văng ra khỏi NavMesh
    float CheckWallSensors()
    {
        Vector3 pos = transform.position + transform.up * 0.5f + transform.forward * sensorStartOffset;
        Vector3 fwd = transform.forward;
        Vector3 right = transform.right;
        RaycastHit hit;
        float avoidVal = 0;

        if (Physics.Raycast(pos, fwd, out hit, sensorLength, obstacleLayer))
        {
            Vector3 localNormal = transform.InverseTransformDirection(hit.normal);
            avoidVal = (localNormal.x < 0) ? 1f : -1f;
        }
        else if (Physics.Raycast(pos + right * sensorSideOffset, (fwd + right).normalized, out hit, sensorLength * 0.8f, obstacleLayer))
        {
            avoidVal = -1f;
        }
        else if (Physics.Raycast(pos - right * sensorSideOffset, (fwd - right).normalized, out hit, sensorLength * 0.8f, obstacleLayer))
        {
            avoidVal = 1f;
        }
        return avoidVal;
    }
    
}