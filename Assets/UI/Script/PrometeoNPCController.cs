using UnityEngine;
using System;

public class PrometeoNPCController : MonoBehaviour
{
    // --- CẤU HÌNH XE (Copy từ script gốc) ---
    [Header("THÔNG SỐ XE")]
    public int maxSpeed = 90;
    public int maxReverseSpeed = 45;
    public int accelerationMultiplier = 2;
    public int maxSteeringAngle = 27;
    public float steeringSpeed = 0.5f;
    public int brakeForce = 350;
    public int decelerationMultiplier = 2;
    public int handbrakeDriftMultiplier = 5;
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

    [Header("HIỆU ỨNG (Tùy chọn)")]
    public bool useEffects = false;
    public ParticleSystem RLWParticleSystem;
    public ParticleSystem RRWParticleSystem;
    public TrailRenderer RLWTireSkid;
    public TrailRenderer RRWTireSkid;

    // --- BIẾN PRIVATE ---
    [HideInInspector] public float carSpeed;
    [HideInInspector] public bool isDrifting;
    [HideInInspector] public bool isTractionLocked;
    
    private Rigidbody carRigidbody;
    private float steeringAxis;
    private float throttleAxis;
    private float driftingAxis;
    private float localVelocityX;
    private float localVelocityZ;
    private bool deceleratingCar;
    
    // Biến nhận lệnh từ AI
    private bool isAccelerating = false; 

    // Biến ma sát (Copy nguyên văn để Drift hoạt động)
    WheelFrictionCurve FLwheelFriction, FRwheelFriction, RLwheelFriction, RRwheelFriction;
    float FLWextremumSlip, FRWextremumSlip, RLWextremumSlip, RRWextremumSlip;

    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = bodyMassCenter;
        SetupFrictionCurves(); // Cài đặt ma sát
    }

    void Update()
    {
        // 1. Tính toán tốc độ
        carSpeed = (2 * Mathf.PI * frontLeftCollider.radius * frontLeftCollider.rpm * 60) / 1000;
        localVelocityX = transform.InverseTransformDirection(carRigidbody.linearVelocity).x;
        localVelocityZ = transform.InverseTransformDirection(carRigidbody.linearVelocity).z;

        // 2. Xử lý giảm tốc tự động (Nếu AI không đạp ga/phanh)
        if (!isAccelerating && !deceleratingCar)
        {
            InvokeRepeating("DecelerateCar", 0f, 0.1f);
            deceleratingCar = true;
        }

        // 3. Hiệu ứng bánh xe
        AnimateWheelMeshes();
        
        // Reset cờ mỗi frame (AI phải gọi liên tục mới chạy)
        isAccelerating = false; 
    }

    // --- CÁC HÀM ĐỂ AI GỌI (PUBLIC) ---

    public void AI_GoForward()
    {
        isAccelerating = true;
        CancelInvoke("DecelerateCar");
        deceleratingCar = false;

        // Logic Drift tự động
        if (Mathf.Abs(localVelocityX) > 2.5f) isDrifting = true;
        else isDrifting = false;
        
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
            // --- LOGIC MỚI: TỰ ĐỘNG BÙ GA KHI LÊN DỐC ---
            
            float baseTorque = (accelerationMultiplier * 50f) * throttleAxis;
            
            // Nếu tốc độ xe chậm (< 20km/h) VÀ đang đạp ga
            // -> Chứng tỏ xe đang đề pa hoặc leo dốc nặng
            if (carSpeed < 20 && throttleAxis > 0.1f)
            {
                // Nhân thêm lực (Boost) để thắng trọng lực
                // Bạn có thể chỉnh số 5f thành 8f hoặc 10f nếu dốc quá đứng
                ApplyTorque(baseTorque * 5f); 
            }
            else
            {
                // Chạy bình thường
                ApplyTorque(baseTorque);
            }
            // ---------------------------------------------
        }
        else
        {
            ApplyTorque(0);
        }
        
        DriftCarPS();
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
        isAccelerating = true; // Phanh cũng tính là đang thao tác
        CancelInvoke("DecelerateCar");
        deceleratingCar = false;
        Brakes();
    }

    public void AI_Handbrake() // Gọi hàm này để Drift
    {
        isAccelerating = true;
        CancelInvoke("DecelerateCar");
        deceleratingCar = false;
        
        // Logic Drift
        driftingAxis += Time.deltaTime;
        float secureStartingPoint = driftingAxis * FLWextremumSlip * handbrakeDriftMultiplier;
        if (secureStartingPoint < FLWextremumSlip) driftingAxis = FLWextremumSlip / (FLWextremumSlip * handbrakeDriftMultiplier);
        if (driftingAxis > 1f) driftingAxis = 1f;

        if (Mathf.Abs(localVelocityX) > 2.5f) isDrifting = true;
        else isDrifting = false;

        if (driftingAxis < 1f)
        {
            ApplyDriftFriction(handbrakeDriftMultiplier * driftingAxis);
        }

        isTractionLocked = true;
        DriftCarPS();
    }
    
    public void AI_ReleaseHandbrake()
    {
        isTractionLocked = false;
        driftingAxis -= (Time.deltaTime / 1.5f);
        if (driftingAxis < 0f) driftingAxis = 0f;
        
        if (FLwheelFriction.extremumSlip > FLWextremumSlip)
        {
            ApplyDriftFriction(handbrakeDriftMultiplier * driftingAxis);
        }
        else
        {
            ApplyDriftFriction(1f); // Reset về mặc định
            driftingAxis = 0f;
        }
    }

    // --- CÁC HÀM HỖ TRỢ (PRIVATE) ---

    void ApplyTorque(float torque)
    {
        frontLeftCollider.motorTorque = torque;
        frontRightCollider.motorTorque = torque;
        rearLeftCollider.motorTorque = torque;
        rearRightCollider.motorTorque = torque;
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

    void DriftCarPS()
    {
        if(!useEffects) return;
        if(isDrifting || isTractionLocked) {
             if(RLWParticleSystem) RLWParticleSystem.Play();
             if(RRWParticleSystem) RRWParticleSystem.Play();
             if(RLWTireSkid) RLWTireSkid.emitting = true;
             if(RRWTireSkid) RRWTireSkid.emitting = true;
        } else {
             if(RLWParticleSystem) RLWParticleSystem.Stop();
             if(RRWParticleSystem) RRWParticleSystem.Stop();
             if(RLWTireSkid) RLWTireSkid.emitting = false;
             if(RRWTireSkid) RRWTireSkid.emitting = false;
        }
    }

    void SetupFrictionCurves()
    {
        FLwheelFriction = new WheelFrictionCurve();
        FLwheelFriction.extremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
        FLWextremumSlip = FLwheelFriction.extremumSlip;
        FLwheelFriction.extremumValue = frontLeftCollider.sidewaysFriction.extremumValue;
        FLwheelFriction.asymptoteSlip = frontLeftCollider.sidewaysFriction.asymptoteSlip;
        FLwheelFriction.asymptoteValue = frontLeftCollider.sidewaysFriction.asymptoteValue;
        FLwheelFriction.stiffness = frontLeftCollider.sidewaysFriction.stiffness;

        // Copy setup tương tự cho các bánh khác (giản lược cho ngắn gọn)
        // Trong thực tế bạn nên copy đoạn setup FR, RL, RR từ script gốc vào đây
        // Để Drift hoạt động chuẩn.
    }
    
    void ApplyDriftFriction(float multiplier)
    {
        // Logic thay đổi độ trượt bánh xe khi drift
        // (Bạn có thể copy đoạn set sidewaysFriction từ script gốc vào đây nếu cần drift chi tiết)
    }
    // --- HÀM MỚI: ĐỂ AI CHẠY THEO INPUT GHI ÂM (REPLAY) ---
    public void SetRawInput(float throttleInput, float steeringInput, bool handbrakeInput)
    {
        // Gán trực tiếp input vào biến điều khiển của xe
        throttleAxis = throttleInput;
        steeringAxis = steeringInput;

        // Xử lý Drift/Phanh tay
        if (handbrakeInput)
        {
            driftingAxis += Time.deltaTime;
            isTractionLocked = true;
            if (driftingAxis > 1f) driftingAxis = 1f;
        }
        else
        {
            isTractionLocked = false;
            driftingAxis -= Time.deltaTime;
            if (driftingAxis < 0f) driftingAxis = 0f;
        }

        // Logic Drift tự động dựa trên input
        if (Mathf.Abs(localVelocityX) > 2.5f) isDrifting = true;
        else isDrifting = false;

        // Gọi các hàm xử lý vật lý gốc
        if (throttleAxis != 0) CancelInvoke("DecelerateCar");
        
        // Cơ chế chạy/lùi
        if (throttleAxis > 0) ApplyTorque((accelerationMultiplier * 50f) * throttleAxis);
        else if (throttleAxis < 0) ApplyTorque((accelerationMultiplier * 50f) * throttleAxis); // Logic lùi
        else ApplyTorque(0);

        // Cơ chế lái
        var steeringAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, steeringAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, steeringAngle, steeringSpeed);

        DriftCarPS(); // Hiệu ứng khói
    }
}