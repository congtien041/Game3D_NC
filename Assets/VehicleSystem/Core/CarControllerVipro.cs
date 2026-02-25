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
        private float stuckTimeLimit = 2.0f; 
        private float stuckDistanceThreshold = 0.5f; 
        private float stuckTimer = 0f;
        private Vector3 lastRecordedPosition;
        private Transform[] cachedTrackPaths; 

        [Header("Nitro")]
        [HideInInspector] public bool isSpinning = false; 
        [HideInInspector] public float externalTorqueMultiplier = 1f; 
        [HideInInspector] public float externalSpeedMultiplier = 1f;
        
        [Header("CountDown & Status")]
        [HideInInspector] public bool isEngineOn = false; 
        [HideInInspector] public bool isCountdown = false; 
        [HideInInspector] public float currentspeed;
        private bool isTargetable = true;  
        private Renderer[] allRenderers;
        [Header("Steering Settings")]
        private float maxSteerAngleLowSpeed = 35f; 
        private float maxSteerAngleHighSpeed = 8f; 
        private float highSpeedThreshold = 120f;
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
        }

        private void Update()
        {
            if (isCountdown || !isEngineOn || isSpinning) return;
            
            CheckAndResetFlip();
            CheckStuckAndReset();  
        }

        private void FixedUpdate()
        {
            if (isCountdown || isSpinning) return;

            HandleInput();
            HandleMotor();
            HandleSteering();
            UpdateWheelMeshes();
        }
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
        }

        private void HandleInput()
        {
            if (!isEngineOn) { currentMotorTorque = 0f; currentBrakeForce = brakeForce; return; }
            
            // --- XỬ LÝ ĐỘNG CƠ ---
            currentMotorTorque = motorTorque;
            if (Input.GetKey(KeyCode.Space)) { currentBrakeForce = brakeForce; currentMotorTorque = 0f; }
            else { currentBrakeForce = 0f; }

            // --- XỬ LÝ GÓC LÁI (QUAN TRỌNG) ---
            float horizontalInput = Input.GetAxis("Horizontal");

            // 1. Lấy tốc độ hiện tại (km/h)
            float speedKmh = rb.linearVelocity.magnitude * 3.6f;

            // 2. Tính tỷ lệ tốc độ (từ 0 đến 1)
            // Nếu speed là 0 -> t = 0 (Lấy góc LowSpeed)
            // Nếu speed là 120 -> t = 1 (Lấy góc HighSpeed)
            float t = Mathf.Clamp01(speedKmh / highSpeedThreshold);

            // 3. Nội suy góc lái (Càng nhanh góc càng hẹp dần)
            float dynamicSteerAngle = Mathf.Lerp(maxSteerAngleLowSpeed, maxSteerAngleHighSpeed, t);

            // 4. Áp dụng
            currentSteeringAngle = dynamicSteerAngle * horizontalInput;
        }
        private void HandleMotor()
        {
            float speed = rb.linearVelocity.magnitude * 3.6f; 
            float actualMaxSpeed = maxSpeed * externalSpeedMultiplier;
            float actualTorque = currentMotorTorque * externalTorqueMultiplier;
            float speedFactor = 1.0f - (speed / actualMaxSpeed);
            speedFactor = Mathf.Clamp(speedFactor, 0f, 1.0f);
            currentspeed = speed; 
            float finalTorque = actualTorque * speedFactor;
            Debug.Log("Spedd: " + speed);
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