using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PrometeoNPCController))]
[RequireComponent(typeof(NavMeshAgent))]
public class PrometeoCarAI : MonoBehaviour
{
    [Header("MỤC TIÊU")]
    public Transform targetTransform;

    [Header("CẢM BIẾN TRÁNH TƯỜNG (RAYCAST)")]
    public float sensorLength = 10f;       // Độ dài tia cảm biến (nhìn xa bao nhiêu)
    public float frontSensorStartPoint = 2f; // Vị trí bắt đầu tia (tính từ tâm xe về phía trước)
    public float sensorAngle = 30f;        // Góc mở của 2 tia chéo
    public LayerMask obstacleLayer;        // Layer của Tường (để tránh nhận diện nhầm Player)

    [Header("CẤU HÌNH LÁI")]
    public float corneringSpeed = 25f;     // Tốc độ an toàn khi vào cua
    public float stopDistance = 5f;
    
    private NavMeshAgent agent;
    private PrometeoNPCController carController;
    private float updateTimer;

    // Biến để xử lý tránh né
    private float avoidMultiplier = 0f;
    private bool isAvoiding = false;

    void Start()
    {
        carController = GetComponent<PrometeoNPCController>();
        agent = GetComponent<NavMeshAgent>();
        
        agent.updatePosition = false;
        agent.updateRotation = false;
    }

    void Update()
    {
        // Cập nhật đường đi (5 lần/giây)
        updateTimer += Time.deltaTime;
        if (targetTransform != null && updateTimer >= 0.2f)
        {
            agent.SetDestination(targetTransform.position);
            updateTimer = 0;
        }
    }

    void FixedUpdate()
    {
        if (targetTransform == null) return;
        agent.nextPosition = transform.position;

        // Dừng nếu đến gần
        if (Vector3.Distance(transform.position, targetTransform.position) < stopDistance)
        {
            carController.AI_Brake();
            return;
        }

        // 1. Kiểm tra cảm biến va chạm trước
        CheckSensors();

        // 2. Điều khiển xe dựa trên cảm biến hoặc NavMesh
        if (isAvoiding)
        {
            // Nếu phát hiện tường: Ưu tiên tránh tường
            AvoidObstacles();
        }
        else
        {
            // Nếu đường thoáng: Chạy theo NavMesh
            FollowPath();
        }
    }

    void CheckSensors()
    {
        Vector3 sensorStartPos = transform.position + (transform.forward * frontSensorStartPoint);
        Vector3 sensorStartPosRight = sensorStartPos + (transform.right * 0.8f); // Lệch phải một chút
        Vector3 sensorStartPosLeft = sensorStartPos - (transform.right * 0.8f);  // Lệch trái một chút
        
        RaycastHit hit;
        avoidMultiplier = 0;
        isAvoiding = false;

        // --- TIA GIỮA (QUAN TRỌNG NHẤT) ---
        // Phát hiện tường ngay trước mặt
        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength, obstacleLayer))
        {
            Debug.DrawLine(sensorStartPos, hit.point, Color.red);
            isAvoiding = true;
            // Nếu tường ở bên phải -> Lái trái (-1), và ngược lại
            // Dùng hit.normal để biết hướng bật ra của tường
            if (hit.normal.x < 0) avoidMultiplier = -1; else avoidMultiplier = 1;
        }

        // --- TIA PHẢI (Góc 30 độ) ---
        else if (Physics.Raycast(sensorStartPosRight, Quaternion.AngleAxis(sensorAngle, transform.up) * transform.forward, out hit, sensorLength / 1.2f, obstacleLayer))
        {
            Debug.DrawLine(sensorStartPosRight, hit.point, Color.red);
            isAvoiding = true;
            avoidMultiplier = -1; // Có tường bên phải -> Lái sang trái
        }

        // --- TIA TRÁI (Góc -30 độ) ---
        else if (Physics.Raycast(sensorStartPosLeft, Quaternion.AngleAxis(-sensorAngle, transform.up) * transform.forward, out hit, sensorLength / 1.2f, obstacleLayer))
        {
            Debug.DrawLine(sensorStartPosLeft, hit.point, Color.red);
            isAvoiding = true;
            avoidMultiplier = 1; // Có tường bên trái -> Lái sang phải
        }
    }

    void AvoidObstacles()
    {
        // Khi tránh tường: Phanh gấp và đánh lái ngược lại
        carController.AI_Turn(avoidMultiplier);
        
        // Nếu xe đang lao quá nhanh vào tường -> Phanh chết (Handbrake)
        if (carController.carSpeed > 15) 
        {
             carController.AI_Handbrake(); 
             // Nhấp nhả ga để quay đầu
             if(Mathf.Abs(carController.carSpeed) < 5) carController.AI_GoForward(); 
        }
        else 
        {
             carController.AI_ReleaseHandbrake();
             carController.AI_GoForward(); // Tốc độ chậm thì đi tới để lách
        }
    }

    void FollowPath()
    {
        if (agent.pathPending) return;

        Vector3 localTarget = transform.InverseTransformPoint(agent.steeringTarget);
        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        float steerAmount = Mathf.Clamp(targetAngle / 45f, -1f, 1f);

        // --- LOGIC VÀO CUA THÔNG MINH ---
        // Tính độ gắt của cua (Góc lái càng lớn = Cua càng gắt)
        float turnSharpness = Mathf.Abs(steerAmount);

        carController.AI_Turn(steerAmount);

        // Nếu cua gắt (> 0.5) hoặc điểm tiếp theo quá gần (sắp tông vào góc tường)
        float distToTurn = Vector3.Distance(transform.position, agent.steeringTarget);
        
        if (turnSharpness > 0.4f || distToTurn < 8f) 
        {
            // Xe đang đi nhanh (> cornerSpeed) thì Phanh trước, không Drift lung tung
            if (carController.carSpeed > corneringSpeed)
            {
                carController.AI_ReleaseHandbrake(); // Nhả drift để bám đường (Drift dễ văng vào tường)
                carController.AI_Brake();           // Phanh chân (ABS)
            }
            else
            {
                // Tốc độ đã chậm -> Rà ga nhẹ để qua cua
                carController.AI_GoForward(); 
            }
        }
        else
        {
            // Đường thẳng -> Tốc độ cao
            carController.AI_ReleaseHandbrake();
            carController.AI_GoForward();
        }
    }

    // Vẽ tia Raycast để Debug
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 start = transform.position + (transform.forward * frontSensorStartPoint);
        Gizmos.DrawRay(start, transform.forward * sensorLength);
        Gizmos.DrawRay(start, Quaternion.AngleAxis(sensorAngle, transform.up) * transform.forward * (sensorLength/1.2f));
        Gizmos.DrawRay(start, Quaternion.AngleAxis(-sensorAngle, transform.up) * transform.forward * (sensorLength/1.2f));
    }
}