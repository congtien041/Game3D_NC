using UnityEngine;
using System;

public class PrometeoNPCController : MonoBehaviour
{
    // --- CẤU HÌNH XE ---
    [Header("THÔNG SỐ XE")]
    public int maxSpeed = 90;
    public int maxReverseSpeed = 45;
    public int accelerationMultiplier = 2;
    public int maxSteeringAngle = 27;
    public float steeringSpeed = 0.5f;
    public int brakeForce = 350;
    public int decelerationMultiplier = 2;
    public Vector3 bodyMassCenter;

    [Header("BÁNH XE")]
    public GameObject frontLeftMesh;
    public WheelCollider frontLeftCollider;
    public GameObject frontRightMesh;
    public WheelCollider frontRightCollider;
    public GameObject rearLeftMesh;
    public WheelCollider rearLeftCollider;
    public GameObject rearRightMesh;
    public WheelCollider rearRightCollider;

    // (Giữ lại biến này để không bị lỗi Missing Reference trong Unity Editor, nhưng sẽ không dùng trong code)
    [Header("HIỆU ỨNG (Không dùng cho Drift nữa)")]
    public bool useEffects = false;
    public ParticleSystem RLWParticleSystem;
    public ParticleSystem RRWParticleSystem;
    public TrailRenderer RLWTireSkid;
    public TrailRenderer RRWTireSkid;

    // --- BIẾN PRIVATE ---
    [HideInInspector] public float carSpeed;
    
    private Rigidbody carRigidbody;
    private float steeringAxis;
    private float throttleAxis;
    private float localVelocityZ;
    private bool deceleratingCar;
    private bool isAccelerating = false; 

    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = bodyMassCenter;
    }

    void Update()
    {
        // 1. Tính toán tốc độ km/h
        carSpeed = (2 * Mathf.PI * frontLeftCollider.radius * frontLeftCollider.rpm * 60) / 1000;
        
        // Lấy vận tốc theo trục dọc của xe (để biết đang tiến hay lùi)
        localVelocityZ = transform.InverseTransformDirection(carRigidbody.linearVelocity).z;

        // 2. Xử lý giảm tốc tự động (Nếu AI không đạp ga/phanh)
        if (!isAccelerating && !deceleratingCar)
        {
            InvokeRepeating("DecelerateCar", 0f, 0.1f);
            deceleratingCar = true;
        }

        // 3. Hiệu ứng quay bánh xe
        AnimateWheelMeshes();
        
        // Tắt hết hiệu ứng Drift nếu lỡ đang bật
        StopDriftEffects();

        // Reset cờ mỗi frame
        isAccelerating = false; 
    }

    // --- CÁC HÀM ĐỂ AI GỌI (PUBLIC) ---

    public void AI_GoForward()
    {
        isAccelerating = true;
        CancelInvoke("DecelerateCar");
        deceleratingCar = false;

        // Tăng ga dần dần
        throttleAxis += (Time.deltaTime * 3f);
        if (throttleAxis > 1f) throttleAxis = 1f;

        // Nếu xe bị trôi lùi (khi đang lên dốc) -> Phanh lại để không tuột
        if (localVelocityZ < -1f) 
        {
            Brakes();
        }
        else if (Mathf.RoundToInt(carSpeed) < maxSpeed)
        {
            // Lực đẩy cơ bản (Đã fix lên 300f cho mạnh)
            float baseTorque = (accelerationMultiplier * 300f) * throttleAxis;
            
            // Logic tự động bù ga khi lên dốc
            if (carSpeed < 20 && throttleAxis > 0.1f)
            {
                ApplyTorque(baseTorque * 5f); // Boost khi đề pa/lên dốc
            }
            else
            {
                ApplyTorque(baseTorque);
            }
        }
        else
        {
            ApplyTorque(0);
        }
    }

    public void AI_Turn(float turnDirection) // -1 (Trái) đến 1 (Phải)
    {
        steeringAxis += (Time.deltaTime * 10f * steeringSpeed * turnDirection);
        steeringAxis = Mathf.Clamp(steeringAxis, -1f, 1f);

        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }

    public void AI_Brake()
    {
        isAccelerating = true;
        CancelInvoke("DecelerateCar");
        deceleratingCar = false;
        Brakes();
    }

    // --- CÁC HÀM HỖ TRỢ (PRIVATE) ---

    void ApplyTorque(float torque)
    {
        frontLeftCollider.motorTorque = torque;
        frontRightCollider.motorTorque = torque;
        rearLeftCollider.motorTorque = torque;
        rearRightCollider.motorTorque = torque;
        
        // Nhả phanh khi đang đạp ga
        frontLeftCollider.brakeTorque = 0;
        frontRightCollider.brakeTorque = 0;
        rearLeftCollider.brakeTorque = 0;
        rearRightCollider.brakeTorque = 0;
    }

    void Brakes()
    {
        frontLeftCollider.brakeTorque = brakeForce;
        frontRightCollider.brakeTorque = brakeForce;
        rearLeftCollider.brakeTorque = brakeForce;
        rearRightCollider.brakeTorque = brakeForce;
    }

    void DecelerateCar()
    {
        if (throttleAxis != 0f)
        {
            if (throttleAxis > 0f) throttleAxis -= (Time.deltaTime * 10f);
            else if (throttleAxis < 0f) throttleAxis += (Time.deltaTime * 10f);
        }
        
        carRigidbody.linearVelocity = carRigidbody.linearVelocity * (1f / (1f + (0.025f * decelerationMultiplier)));
        ApplyTorque(0);
        
        if (carRigidbody.linearVelocity.magnitude < 0.25f)
        {
            carRigidbody.linearVelocity = Vector3.zero;
            CancelInvoke("DecelerateCar");
        }
    }

    void AnimateWheelMeshes()
    {
        try {
            UpdateWheelPose(frontLeftCollider, frontLeftMesh);
            UpdateWheelPose(frontRightCollider, frontRightMesh);
            UpdateWheelPose(rearLeftCollider, rearLeftMesh);
            UpdateWheelPose(rearRightCollider, rearRightMesh);
        } catch {}
    }

    void UpdateWheelPose(WheelCollider collider, GameObject mesh)
    {
        Vector3 pos; Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        mesh.transform.position = pos;
        mesh.transform.rotation = rot;
    }

    void StopDriftEffects()
    {
        if(!useEffects) return;
        if(RLWParticleSystem) RLWParticleSystem.Stop();
        if(RRWParticleSystem) RRWParticleSystem.Stop();
        if(RLWTireSkid) RLWTireSkid.emitting = false;
        if(RRWTireSkid) RRWTireSkid.emitting = false;
    }

    // --- HÀM SET INPUT (Dành cho Replay hoặc Player Control nếu cần) ---
    public void SetRawInput(float throttleInput, float steeringInput)
    {
        throttleAxis = throttleInput;
        steeringAxis = steeringInput;

        if (throttleAxis != 0) CancelInvoke("DecelerateCar");
        
        // Cơ chế chạy/lùi (Đã đồng bộ lực đẩy 300f cho khớp với AI)
        if (throttleAxis > 0) ApplyTorque((accelerationMultiplier * 300f) * throttleAxis);
        else if (throttleAxis < 0) ApplyTorque((accelerationMultiplier * 300f) * throttleAxis);
        else ApplyTorque(0);

        // Cơ chế lái
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);
    }
}