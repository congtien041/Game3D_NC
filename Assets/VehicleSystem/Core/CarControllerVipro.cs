using UnityEngine;
using VehicleSystem.Core;

namespace VehicleSystem.Core
{    
    public class CarControllerVipro : MonoBehaviour
    {
        [Header("Wheel Colliders")]
        public WheelCollider frontLeftCollider;
        public WheelCollider frontRightCollider;
        public WheelCollider rearLeftCollider;
        public WheelCollider rearRightCollider;

        [Header("Wheel Transforms")]
        public Transform frontLeftMesh;
        public Transform frontRightMesh;
        public Transform rearLeftMesh;
        public Transform rearRightMesh;

        [Header("Car Settings")]
        private float motorTorque = 1500f;   
        private float maxSteeringAngle = 30f;
        private float brakeForce = 3000f;    
        private float decelerationForce = 300f;
        [HideInInspector] public float maxSpeed = 120f;

        [Header("Stability & Recovery")]
        public Transform centerOfMass;   
        private float waitTimeToFlip = 3f;
        
        [Header("--- HỆ THỐNG RESET XE ---")]
        public float maxDistanceFromTrack = 20.0f; 
        public float stuckTimeLimit = 3.0f; 
        private float stuckDistanceThreshold = 0.5f; 
        private float stuckTimer = 0f;
        private Vector3 lastRecordedPosition;
        
        // Lưu mảng các điểm mà không cần dùng Tag
        private Transform[] cachedTrackPaths; 

        [Header("Nitro")]
        [HideInInspector] public bool isSpinning = false; 
        [HideInInspector] public float externalTorqueMultiplier = 1f; 
        [HideInInspector] public float externalSpeedMultiplier = 1f;
        
        [Header("CountDown & Status")]
        [HideInInspector] public bool isEngineOn = false; 
        [HideInInspector] public bool isCountdown = false; 
        private bool isTargetable = true;  
        private Renderer[] allRenderers;

        private float currentMotorTorque;
        private float currentSteeringAngle;
        private float currentBrakeForce;
        private float flipTimer = 0f;     
        private Rigidbody rb;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            if (centerOfMass != null) rb.centerOfMass = centerOfMass.localPosition;
            allRenderers = GetComponentsInChildren<Renderer>();
            
            lastRecordedPosition = transform.position;

            // --- KHÔNG DÙNG TAG NỮA ---
            // Tự động tìm kịch bản TrackPath, sau đó lấy tất cả các điểm con (children) của nó
            TrackPath trackPathManager = Object.FindAnyObjectByType<TrackPath>();
            if (trackPathManager != null)
            {
                int pointCount = trackPathManager.transform.childCount;
                cachedTrackPaths = new Transform[pointCount];
                for (int i = 0; i < pointCount; i++)
                {
                    cachedTrackPaths[i] = trackPathManager.transform.GetChild(i);
                }
            }
            else
            {
                Debug.LogWarning("Không tìm thấy script TrackPath trên map!");
            }
        }

        private void Update()
        {
            if (isCountdown || !isEngineOn || isSpinning) return;
            
            CheckAndResetFlip();
            CheckStuckAndReset();  
            CheckOffTrack();       
        }

        private void FixedUpdate()
        {
            if (isCountdown || isSpinning) return;

            HandleInput();
            HandleMotor();
            HandleSteering();
            UpdateWheelMeshes();
        }

        // ===============================================
        // LÔ-GÍC: BAY RA KHỎI ĐƯỜNG ĐUA BẰNG ĐOẠN THẲNG
        // ===============================================
        private void CheckOffTrack()
        {
            if (cachedTrackPaths == null || cachedTrackPaths.Length < 2) return;

            float minDistanceToRoad = Mathf.Infinity;
            Transform bestResetPoint = null;

            // Duyệt qua TỪNG CẶP điểm nối tiếp nhau theo đúng thứ tự (1-2, 2-3, 3-4...)
            for (int i = 0; i < cachedTrackPaths.Length; i++)
            {
                Transform currentPoint = cachedTrackPaths[i];
                
                // Điểm tiếp theo (Dùng % để điểm cuối cùng nối vòng lại điểm đầu tiên tạo thành Track kín)
                Transform nextPoint = cachedTrackPaths[(i + 1) % cachedTrackPaths.Length];

                // Tính khoảng cách từ xe đến cái BỀ MẶT ĐƯỜNG nối giữa 2 điểm này
                float distanceToSegment = DistanceToLineSegment(transform.position, currentPoint.position, nextPoint.position);

                // Tìm ra đoạn đường nào đang gần xe nhất
                if (distanceToSegment < minDistanceToRoad)
                {
                    minDistanceToRoad = distanceToSegment;
                    
                    // Xác định xem trong đoạn đường này, xe đang đứng nghiêng về điểm nào hơn để lát nữa Reset về đó
                    float distToCurrent = Vector3.Distance(transform.position, currentPoint.position);
                    float distToNext = Vector3.Distance(transform.position, nextPoint.position);
                    
                    bestResetPoint = (distToCurrent < distToNext) ? currentPoint : nextPoint;
                }
            }

            // Bất chấp điểm dài điểm ngắn, chỉ cần xe văng ra khỏi CÁI ĐƯỜNG ẢO đó xa hơn maxDistance là bị bế về!
            if (minDistanceToRoad > maxDistanceFromTrack && bestResetPoint != null)
            {
                ResetCarTo(bestResetPoint, "Bay ra khỏi đường đua!");
            }
        }
        // HÀM TOÁN HỌC ĐỂ TÍNH KHOẢNG CÁCH TỚI ĐOẠN THẲNG
        private float DistanceToLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 lineDirection = lineEnd - lineStart;
            float lineLength = lineDirection.magnitude;
            lineDirection.Normalize();

            Vector3 vectorToPoint = point - lineStart;
            float projectLength = Vector3.Dot(vectorToPoint, lineDirection);

            // Giới hạn projection nằm gọn trong đoạn thẳng
            projectLength = Mathf.Clamp(projectLength, 0f, lineLength);

            Vector3 closestPointOnLine = lineStart + lineDirection * projectLength;
            return Vector3.Distance(point, closestPointOnLine);
        }

        // ===============================================
        // LÔ-GÍC: KẸT XE BẰNG TỌA ĐỘ 
        // ===============================================
        private void CheckStuckAndReset()
        {
            bool isBraking = Input.GetKey(KeyCode.Space);
            float movedDistance = Vector3.Distance(transform.position, lastRecordedPosition);

            if (movedDistance < stuckDistanceThreshold && !isBraking)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer >= stuckTimeLimit)
                {
                    ResetToNearestTrackPath("Kẹt quá lâu!");
                }
            }
            else if (movedDistance >= stuckDistanceThreshold)
            {
                stuckTimer = 0f;
                lastRecordedPosition = transform.position;
            }
        }

        private void ResetToNearestTrackPath(string reason)
        {
            if (cachedTrackPaths == null || cachedTrackPaths.Length == 0) return;

            Transform nearestPath = null;
            float minDistance = Mathf.Infinity;

            foreach (Transform path in cachedTrackPaths)
            {
                float distance = Vector3.Distance(transform.position, path.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPath = path;
                }
            }

            if (nearestPath != null) ResetCarTo(nearestPath, reason);
        }

        private void ResetCarTo(Transform targetPath, string reason)
        {
            transform.position = targetPath.position + Vector3.up * 1.5f;
            transform.rotation = targetPath.rotation;
            
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            stuckTimer = 0f;
            lastRecordedPosition = transform.position;
            
            Debug.Log($"<color=orange>RESET XE: {reason}</color>");
        }

        private void HandleInput()
        {
            if (!isEngineOn) { currentMotorTorque = 0f; currentBrakeForce = brakeForce; return; }
            currentMotorTorque = motorTorque;
            float horizontalInput = Input.GetAxis("Horizontal");
            currentSteeringAngle = maxSteeringAngle * horizontalInput;
            if (Input.GetKey(KeyCode.Space)) { currentBrakeForce = brakeForce; currentMotorTorque = 0f; }
            else { currentBrakeForce = 0f; }
        }

        private void HandleMotor()
        {
            float speed = rb.linearVelocity.magnitude * 3.6f; 
            float actualMaxSpeed = maxSpeed * externalSpeedMultiplier;
            float actualTorque = currentMotorTorque * externalTorqueMultiplier;
            float speedFactor = 1.0f - (speed / actualMaxSpeed);
            speedFactor = Mathf.Clamp(speedFactor, 0f, 1.0f);
            float finalTorque = actualTorque * speedFactor;
            
            frontLeftCollider.motorTorque = finalTorque;
            frontRightCollider.motorTorque = finalTorque;
            rearLeftCollider.motorTorque = finalTorque;
            rearRightCollider.motorTorque = finalTorque;
            
            if (currentBrakeForce > 0) ApplyBrake(currentBrakeForce);
            else if (currentMotorTorque == 0) ApplyBrake(decelerationForce);
            else ApplyBrake(0f);
        }

        private void ApplyBrake(float force)
        {
            frontLeftCollider.brakeTorque = frontRightCollider.brakeTorque = rearLeftCollider.brakeTorque = rearRightCollider.brakeTorque = force;
        }

        private void HandleSteering()
        {
            frontLeftCollider.steerAngle = frontRightCollider.steerAngle = currentSteeringAngle;
        }

        private void UpdateWheelMeshes()
        {
            UpdateSingleWheel(frontLeftCollider, frontLeftMesh);
            UpdateSingleWheel(frontRightCollider, frontRightMesh);
            UpdateSingleWheel(rearLeftCollider, rearLeftMesh);
            UpdateSingleWheel(rearRightCollider, rearRightMesh);
        }

        private void UpdateSingleWheel(WheelCollider col, Transform mesh)
        {
            col.GetWorldPose(out Vector3 pos, out Quaternion rot);
            mesh.position = pos;
            mesh.rotation = rot;
        }
        
        private void CheckAndResetFlip()
        {
            if (transform.up.y < 0.2f) {
                flipTimer += Time.deltaTime; 
                if (flipTimer >= waitTimeToFlip) {
                    transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
                    transform.position = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
                    rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; flipTimer = 0f;
                }
            } else flipTimer = 0f;
        }

        public void SetInvincible(bool state)
        {
            isTargetable = !state;
            if (!state) ToggleMeshes(true); 
            else StartCoroutine(BlinkEffect(true));
        }

        private System.Collections.IEnumerator BlinkEffect(bool active)
        {
            float timer = 0;
            while (timer < 3f)
            {
                ToggleMeshes(false); yield return new WaitForSeconds(0.15f); 
                ToggleMeshes(true);  yield return new WaitForSeconds(0.15f); 
                timer += 0.3f;
            }
            ToggleMeshes(true);
        }

        private void ToggleMeshes(bool show)
        {
            if (allRenderers != null)
                foreach (var renderer in allRenderers)
                    if (renderer != null) renderer.enabled = show; 
        }
    }
}