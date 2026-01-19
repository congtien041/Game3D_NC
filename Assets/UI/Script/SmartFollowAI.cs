using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PrometeoNPCController))]
public class SmartFollowAIs : MonoBehaviour
{
    [Header("MỤC TIÊU")]
    public Transform playerTarget;

    [Header("CẤU HÌNH BÁM ĐUỔI")]
    // Giảm xuống 1m để đường đi mượt hơn, AI bo cua chuẩn hơn
    public float recordDistance = 1f; 
    
    // AI chỉ được nhìn xa tối đa 2 điểm. Đừng để cao quá nó sẽ cắt cua.
    public int maxLookAhead = 2; 

    [Header("CẢM BIẾN (GIỮ NGUYÊN)")]
    public float sensorLength = 10f;
    public float sensorStartOffset = 2f; 
    public float sensorSideOffset = 0.8f; 
    public LayerMask obstacleLayer;

    private List<Vector3> pathPoints = new List<Vector3>();
    private PrometeoNPCController carController;
    private Vector3 lastRecordedPos;

    void Start()
    {
        carController = GetComponent<PrometeoNPCController>();
        if (playerTarget == null && GameObject.FindWithTag("Player"))
            playerTarget = GameObject.FindWithTag("Player").transform;

        if (playerTarget != null) lastRecordedPos = playerTarget.position;
    }

    void Update()
    {
        if (playerTarget == null) return;

        // Ghi lại điểm mỗi 1 mét
        if ((playerTarget.position - lastRecordedPos).sqrMagnitude > recordDistance * recordDistance)
        {
            pathPoints.Add(playerTarget.position);
            lastRecordedPos = playerTarget.position;
        }
    }

    void FixedUpdate()
    {
        if (playerTarget == null) return;
        HandlePathProgress();
        DriveCar();
    }

    void HandlePathProgress()
    {
        if (pathPoints.Count == 0) return;
        Vector3 currentPos = transform.position;

        // Logic xóa điểm cũ: Chỉ xóa khi xe đã thực sự đi đến gần điểm đó (3m)
        // Điều này ép xe phải đi qua điểm đó chứ không được bỏ qua
        if ((currentPos - pathPoints[0]).sqrMagnitude < 9f) // 3*3 = 9
        {
            pathPoints.RemoveAt(0);
        }
    }

    void DriveCar()
    {
        Vector3 targetPoint;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // --- LOGIC CHỌN MỤC TIÊU MỚI (QUAN TRỌNG) ---
        
        // Chỉ khi nào GẦN Player (< 10m) HOẶC hết đường chạy -> Mới được lao thẳng vào Player
        if (pathPoints.Count == 0 || distanceToPlayer < 10f)
        {
            targetPoint = playerTarget.position;
        }
        else
        {
            // Còn ở xa -> BẮT BUỘC chạy theo đường vẽ (Breadcrumbs)
            // Lấy điểm gần nhất hoặc điểm kế tiếp (để lái mượt hơn xíu)
            // Không cho phép nhìn quá xa (Mathf.Min)
            int targetIndex = Mathf.Min(maxLookAhead, pathPoints.Count - 1);
            targetPoint = pathPoints[targetIndex];
        }

        // --- ĐIỀU KHIỂN XE ---
        Vector3 localTarget = transform.InverseTransformPoint(targetPoint);
        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        
        // Tăng độ nhạy lái: Chia cho 35 thay vì 45 để xe bẻ lái gắt hơn
        float steerInput = Mathf.Clamp(targetAngle / 35f, -1f, 1f);

        // Check tường (Ưu tiên số 1)
        float avoidanceInput = CheckWallSensors();

        carController.AI_ReleaseHandbrake();

        if (avoidanceInput != 0)
        {
            // Có tường -> Né
            carController.AI_Turn(avoidanceInput);
            
            // Nếu góc né gắt quá thì nhấp phanh tay drift, còn lại cứ ga
            if(Mathf.Abs(avoidanceInput) > 0.8f && carController.carSpeed > 20) 
                carController.AI_Handbrake();
            else 
                carController.AI_GoForward();
        }
        else
        {
            // Đường thoáng -> Lái theo điểm
            carController.AI_Turn(steerInput);
            
            // LOGIC GA/PHANH THÔNG MINH:
            // Nếu góc cua gắt (> 20 độ) thì đừng đạp ga (thả trôi xe để cua)
            // Nếu góc cua cực gắt (> 45 độ) thì phanh drift
            if (Mathf.Abs(targetAngle) > 45f && carController.carSpeed > 40)
            {
                carController.AI_Handbrake(); // Drift
            }
            else if (Mathf.Abs(targetAngle) > 20f && carController.carSpeed > 60)
            {
                // Không làm gì cả (Thả chân ga ra để xe tự chậm lại vào cua)
            }
            else
            {
                carController.AI_GoForward(); // Đường thẳng -> Đạp lút cán
            }
        }
    }

    // Giữ nguyên hàm CheckWallSensors và OnDrawGizmos cũ
    float CheckWallSensors()
    {
        Vector3 pos = transform.position + transform.up * 0.5f + transform.forward * sensorStartOffset;
        Vector3 fwd = transform.forward;
        Vector3 right = transform.right;
        
        RaycastHit hit;
        float avoidVal = 0;
        bool hitSomething = false;

        if (Physics.Raycast(pos, fwd, out hit, sensorLength, obstacleLayer))
        {
            Vector3 localNormal = transform.InverseTransformDirection(hit.normal);
            avoidVal = (localNormal.x < 0) ? 1f : -1f;
            hitSomething = true;
        }
        else if (Physics.Raycast(pos + right * sensorSideOffset, (fwd + right * 0.5f).normalized, out hit, sensorLength * 0.8f, obstacleLayer))
        {
            avoidVal = -1f; hitSomething = true;
        }
        else if (Physics.Raycast(pos - right * sensorSideOffset, (fwd - right * 0.5f).normalized, out hit, sensorLength * 0.8f, obstacleLayer))
        {
            avoidVal = 1f; hitSomething = true;
        }
        return hitSomething ? avoidVal : 0;
    }

    void OnDrawGizmos()
    {
        if (pathPoints.Count > 0)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < pathPoints.Count - 1; i++)
                Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
            
            Gizmos.color = Color.red;
            // Vẽ điểm AI đang nhắm tới
            if(pathPoints.Count > 0) 
                Gizmos.DrawWireSphere(pathPoints[Mathf.Min(maxLookAhead, pathPoints.Count - 1)], 1f);
        }
    }
}