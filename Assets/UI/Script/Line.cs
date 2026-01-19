using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PrometeoNPCController))]
[RequireComponent(typeof(LineRenderer))] // Bắt buộc có LineRenderer để vẽ lằn
public class LineFollowerAI : MonoBehaviour
{
    [Header("MỤC TIÊU")]
    public Transform playerTarget;

    [Header("CẤU HÌNH ĐƯỜNG DẪN (CÁI LẰN)")]
    public float recordDistance = 2.0f; // Cứ 2 mét Player đi thì vẽ 1 điểm
    public float eatDistance = 4.0f;    // AI đến gần 4 mét thì coi như "đã đi qua" điểm đó
    
    [Header("CẤU HÌNH LÁI XE")]
    public float speedLimit = 60f;      // Tốc độ tối đa
    public float turnSpeed = 2f;        // Độ nhạy lái

    // Các thành phần
    private PrometeoNPCController carController;
    private LineRenderer lineRenderer;
    private List<Vector3> pathPoints = new List<Vector3>(); // Danh sách điểm tạo nên cái lằn
    private Vector3 lastPlayerPos;

    void Start()
    {
        carController = GetComponent<PrometeoNPCController>();
        lineRenderer = GetComponent<LineRenderer>();

        // Cấu hình LineRenderer (Vẽ lằn)
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 0.5f;
        lineRenderer.endWidth = 0.5f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Tạo vật liệu cơ bản
        lineRenderer.startColor = Color.red; // Màu đỏ cho dễ nhìn
        lineRenderer.endColor = Color.yellow;
        
        lastPlayerPos = playerTarget.position;
    }

    void Update()
    {
        // 1. PLAYER VẼ ĐƯỜNG (RECORD)
        float distToLastPoint = Vector3.Distance(playerTarget.position, lastPlayerPos);
        
        // Nếu Player đi được 1 đoạn thì thêm điểm mới
        if (distToLastPoint > recordDistance)
        {
            pathPoints.Add(playerTarget.position);
            lastPlayerPos = playerTarget.position;
            UpdateVisualLine(); // Cập nhật hình ảnh cái lằn
        }

        // 2. ENEMY ĂN ĐƯỜNG (FOLLOW)
        if (pathPoints.Count > 0)
        {
            // Kiểm tra khoảng cách từ xe AI đến điểm đầu tiên của lằn
            float distToTarget = Vector3.Distance(transform.position, pathPoints[0]);

            // Nếu đã đến gần điểm đó -> Xóa nó đi (Coi như đã đi qua)
            if (distToTarget < eatDistance)
            {
                pathPoints.RemoveAt(0); // Xóa điểm đầu tiên
                UpdateVisualLine();     // Cập nhật lại hình ảnh (lằn ngắn đi)
            }
        }
    }

    void FixedUpdate()
    {
        // Điều khiển vật lý xe
        DriveAlongLine();
    }

    void DriveAlongLine()
    {
        // Nếu hết đường (đã bắt kịp Player)
        if (pathPoints.Count == 0)
        {
            carController.AI_Brake();
            return;
        }

        // Lấy điểm tiếp theo trên lằn
        Vector3 targetPoint = pathPoints[0];

        // --- TÍNH TOÁN LÁI ---
        // Chuyển điểm đó sang hệ toạ độ của xe
        Vector3 localTarget = transform.InverseTransformPoint(targetPoint);
        
        // Tính góc cần quay (Atan2 trả về radian, nhân Rad2Deg ra độ)
        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        
        // Chuyển góc thành Input lái (-1 đến 1)
        float steerInput = Mathf.Clamp(targetAngle / 45f, -1f, 1f);

        // --- ĐIỀU KHIỂN ---
        carController.AI_Turn(steerInput);

        // Xử lý thông minh: Góc lái càng lớn -> Xe càng phải đi chậm lại để ôm cua
        float speedFactor = 1f - Mathf.Abs(steerInput); // Nếu lái gắt (1), speedFactor gần 0
        
        // Logic Ga/Phanh
        if (Mathf.Abs(steerInput) > 0.6f && carController.carSpeed > 30)
        {
            // Cua quá gắt và đang nhanh -> Drift hoặc Phanh
            carController.AI_Handbrake(); 
        }
        else
        {
            carController.AI_ReleaseHandbrake();
            
            // Giới hạn tốc độ để bám sát lằn
            if (carController.carSpeed < speedLimit)
            {
                carController.AI_GoForward();
            }
            else
            {
                // Thả ga nếu quá nhanh
                carController.AI_Brake(); 
            }
        }
    }

    // Hàm cập nhật hình ảnh cái lằn cho người chơi thấy
    void UpdateVisualLine()
    {
        lineRenderer.positionCount = pathPoints.Count;
        lineRenderer.SetPositions(pathPoints.ToArray());
    }
}