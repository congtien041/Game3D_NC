using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PrometeoNPCController))]
public class SmartFollowAI : MonoBehaviour
{
    [Header("MỤC TIÊU (PLAYER)")]
    public Transform playerTarget;
    
    [Header("CẤU HÌNH BÁM ĐUỔI")]
    public float recordDistance = 2f;    // Cứ mỗi 2m Player đi được thì thả 1 điểm
    public float stopDistance = 5f;      // Khoảng cách dừng khi bắt kịp
    public float lookAheadPoints = 2;    // Nhìn xa bao nhiêu điểm (để lái mượt hơn)

    [Header("HỆ THỐNG TRÁNH TƯỜNG (QUAN TRỌNG)")]
    public float sensorLength = 8f;      // Cảm biến nhìn xa 8m
    public float sensorAngle = 30f;      // Góc cảm biến chéo
    public LayerMask obstacleLayer;      // Layer của Tường

    // Danh sách các điểm cần đi theo (Breadcrumbs)
    private List<Vector3> pathPoints = new List<Vector3>();
    private PrometeoNPCController carController;
    private Vector3 lastRecordedPos;

    void Start()
    {
        carController = GetComponent<PrometeoNPCController>();
        lastRecordedPos = playerTarget.position;
    }

    void Update()
    {
        // 1. GHI LẠI ĐƯỜNG ĐI CỦA PLAYER
        // Nếu Player đi xa hơn khoảng cách quy định -> Thêm điểm mới vào danh sách
        if (Vector3.Distance(playerTarget.position, lastRecordedPos) > recordDistance)
        {
            pathPoints.Add(playerTarget.position);
            lastRecordedPos = playerTarget.position;
        }
    }

    void FixedUpdate()
    {
        // 2. XÓA ĐIỂM ĐÃ ĐI QUA
        // Nếu AI đã đến gần điểm đầu tiên trong danh sách -> Xóa nó để đi điểm tiếp theo
        if (pathPoints.Count > 0)
        {
            if (Vector3.Distance(transform.position, pathPoints[0]) < 4f)
            {
                pathPoints.RemoveAt(0);
            }
        }

        // 3. ĐIỀU KHIỂN XE
        DriveCar();
    }

    void DriveCar()
    {
        // Nếu hết điểm để đi (đã bắt kịp Player) -> Phanh
        if (pathPoints.Count == 0 || Vector3.Distance(transform.position, playerTarget.position) < stopDistance)
        {
            carController.AI_Brake();
            return;
        }

        // Lấy điểm mục tiêu (nhìn xa hơn 1 chút để lái mượt)
        int targetIndex = Mathf.Min(Mathf.FloorToInt(lookAheadPoints), pathPoints.Count - 1);
        Vector3 targetPoint = pathPoints[targetIndex];

        // --- PHÂN TÍCH INPUT: TÍNH GÓC LÁI ---
        Vector3 localTarget = transform.InverseTransformPoint(targetPoint);
        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        float steerInput = Mathf.Clamp(targetAngle / 40f, -1f, 1f);

        // --- HỆ THỐNG AN TOÀN: RAYCAST CHECK ---
        // Đây là phần giúp AI không đụng tường dù Player đi sát tường
        float avoidanceInput = CheckWallSensors();

        if (avoidanceInput != 0)
        {
            // Có tường! Ưu tiên tránh tường 100%
            steerInput = avoidanceInput;
            
            // Nếu tường ngay trước mặt -> Phanh gấp + Drift
            if(Mathf.Abs(avoidanceInput) > 0.8f) carController.AI_Handbrake();
            else carController.AI_GoForward();
        }
        else
        {
            // Đường thoáng -> Chạy theo đường Player đã vẽ
            carController.AI_ReleaseHandbrake();
            carController.AI_Turn(steerInput);
            
            // Logic Ga/Phanh dựa trên độ cong của đường
            if (Mathf.Abs(steerInput) > 0.5f) // Cua gắt
            {
                // Nếu đang nhanh quá thì drift
                if (carController.carSpeed > 40) carController.AI_Handbrake();
                else carController.AI_GoForward(); // Chậm thì cứ ga
            }
            else
            {
                carController.AI_GoForward(); // Đường thẳng -> Full Speed
            }
        }
    }

    // Hàm cảm biến trả về hướng cần né (-1: Né trái, 1: Né phải, 0: Không cần né)
    float CheckWallSensors()
    {
        Vector3 pos = transform.position + transform.up * 0.5f; // Nâng cao raycast lên chút
        Vector3 fwd = transform.forward;
        Vector3 right = transform.right;
        
        RaycastHit hit;
        float avoidVal = 0;
        bool hitSomething = false;

        // 1. Tia Giữa (Dài nhất)
        if (Physics.Raycast(pos, fwd, out hit, sensorLength, obstacleLayer))
        {
            Debug.DrawLine(pos, hit.point, Color.red);
            // Nếu tường lệch phải -> Né trái (-1)
            avoidVal = (hit.normal.x < 0) ? -1f : 1f; 
            hitSomething = true;
        }
        // 2. Tia Phải (Góc 30 độ)
        else if (Physics.Raycast(pos, Quaternion.AngleAxis(sensorAngle, transform.up) * fwd, out hit, sensorLength * 0.7f, obstacleLayer))
        {
            Debug.DrawLine(pos, hit.point, Color.red);
            avoidVal = -1f; // Có chướng ngại bên phải -> Lái sang Trái gấp
            hitSomething = true;
        }
        // 3. Tia Trái (Góc -30 độ)
        else if (Physics.Raycast(pos, Quaternion.AngleAxis(-sensorAngle, transform.up) * fwd, out hit, sensorLength * 0.7f, obstacleLayer))
        {
            Debug.DrawLine(pos, hit.point, Color.red);
            avoidVal = 1f; // Có chướng ngại bên trái -> Lái sang Phải gấp
            hitSomething = true;
        }

        return hitSomething ? avoidVal : 0;
    }

    // Vẽ đường đi để Debug
    void OnDrawGizmos()
    {
        if (pathPoints.Count > 0)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
            }
            Gizmos.DrawWireSphere(pathPoints[0], 1f); // Điểm AI đang hướng tới
        }
    }
}