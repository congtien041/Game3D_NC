using System;
using UnityEngine;
using UnityEngine.UI;

public class PrometeoCarController : MonoBehaviour
{
    [Header("CAR SETUP")]
    [Range(20, 300)]
    public int maxSpeed = 150; 
    [Range(10, 120)]
    public int maxReverseSpeed = 45;
    [Range(1, 10)]
    public int accelerationMultiplier = 2; 
    [Space(10)]
    [Range(10, 45)]
    public int maxSteeringAngle = 27;
    [Range(0.1f, 1f)]
    public float steeringSpeed = 0.5f;
    [Space(10)]
    [Range(100, 600)]
    public int brakeForce = 350;
    [Range(1, 10)]
    public int decelerationMultiplier = 2;
    [Range(1, 10)]
    public int handbrakeDriftMultiplier = 5;
    [Space(10)]
    public Vector3 bodyMassCenter = new Vector3(0, -0.5f, 0); // Đặt thấp để tránh lật

    [Header("ARCADE FEATURES (VIP)")]
    public bool useNitro = true;
    public float maxNitro = 100f;
    public float currentNitro;
    public float nitroConsumeRate = 20f; // Tốn bao nhiêu nitro mỗi giây
    public float nitroRegenRate = 5f;    // Hồi nitro khi chạy thường
    public float nitroForce = 15000f;    // Lực đẩy Nitro
    [Space(5)]
    public bool useDriftBoost = true;
    public float driftDurationForBoost = 1.5f; // Cần drift bao lâu để có boost
    public float miniBoostForce = 20000f;      // Lực bắn khi xong drift

    [Header("WHEELS")]
    public GameObject frontLeftMesh;
    public WheelCollider frontLeftCollider;
    public GameObject frontRightMesh;
    public WheelCollider frontRightCollider;
    public GameObject rearLeftMesh;
    public WheelCollider rearLeftCollider;
    public GameObject rearRightMesh;
    public WheelCollider rearRightCollider;

    [Header("EFFECTS")]
    public bool useEffects = false;
    public ParticleSystem RLWParticleSystem;
    public ParticleSystem RRWParticleSystem;
    public TrailRenderer RLWTireSkid;
    public TrailRenderer RRWTireSkid;
    // Thêm hiệu ứng Nitro nếu muốn sau này (Option)
    
    [Header("UI")]
    public bool useUI = false;
    public Text carSpeedText;
    public Text nitroText; // UI hiển thị lượng Nitro

    [Header("SOUNDS")]
    public bool useSounds = false;
    public AudioSource carEngineSound;
    public AudioSource tireScreechSound;
    float initialCarEngineSoundPitch;

    [Header("CONTROLS")]
    public bool useTouchControls = false;
    public GameObject throttleButton;
    PrometeoTouchInput throttlePTI;
    public GameObject reverseButton;
    PrometeoTouchInput reversePTI;
    public GameObject turnRightButton;
    PrometeoTouchInput turnRightPTI;
    public GameObject turnLeftButton;
    PrometeoTouchInput turnLeftPTI;
    public GameObject handbrakeButton;
    PrometeoTouchInput handbrakePTI;
    // Nút Nitro cho mobile
    public GameObject nitroButton; 
    PrometeoTouchInput nitroPTI;

    // --- CAR DATA ---
    [HideInInspector] public float carSpeed; 
    [HideInInspector] public bool isDrifting; 
    [HideInInspector] public bool isTractionLocked;

    // --- PRIVATE VARIABLES ---
    Rigidbody carRigidbody;
    float steeringAxis;
    float throttleAxis;
    float driftingAxis;
    float localVelocityX;
    float localVelocityZ;
    bool deceleratingCar;
    bool touchControlsSetup = false;
    
    // Drift Logic
    WheelFrictionCurve FLwheelFriction, FRwheelFriction, RLwheelFriction, RRwheelFriction;
    float FLWextremumSlip, FRWextremumSlip, RLWextremumSlip, RRWextremumSlip;
    
    // Arcade Logic Variables
    float driftTimer = 0f;
    bool isNitroActive = false;

    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = bodyMassCenter;
        carRigidbody.interpolation = RigidbodyInterpolation.Interpolate; // Mượt hơn
        carRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous; // Tránh xuyên tường

        currentNitro = maxNitro;

        SetupWheelFriction();

        if(carEngineSound != null) initialCarEngineSoundPitch = carEngineSound.pitch;

        if (useTouchControls) SetupTouchControls();
    }

    [Obsolete]
    void Update()
    {
        // 1. TÍNH TOÁN DỮ LIỆU CƠ BẢN
        // Dùng velocity thay vì RPM để chuẩn xác hơn
        carSpeed = carRigidbody.velocity.magnitude * 3.6f; // km/h
        carSpeed = Mathf.Round(carSpeed);

        localVelocityX = transform.InverseTransformDirection(carRigidbody.velocity).x;
        localVelocityZ = transform.InverseTransformDirection(carRigidbody.velocity).z;

        // 2. XỬ LÝ INPUT & LOGIC (Không xử lý vật lý nặng ở đây)
        HandleInput();
        
        // 3. XỬ LÝ HIỆU ỨNG & UI
        UpdateUI();
        UpdateSounds();
        AnimateWheelMeshes();
        HandleDriftEffects();
    }

    void FixedUpdate()
    {
        // 4. XỬ LÝ VẬT LÝ (Chạy ở tần số cố định để ổn định Physics)
        ApplyEngineForce();
        ApplySteering();
        ApplyNitroPhysics();
    }

    // --- INPUT HANDLING ---
    void HandleInput()
    {
        // Reset flags
        isNitroActive = false;
        bool isBrakingInput = false;

        if (useTouchControls && touchControlsSetup)
        {
            if (throttlePTI.buttonPressed) { CancelInvoke("DecelerateCar"); deceleratingCar = false; GoForward(); }
            if (reversePTI.buttonPressed) { CancelInvoke("DecelerateCar"); deceleratingCar = false; GoReverse(); }
            
            if (turnLeftPTI.buttonPressed) TurnLeft();
            else if (turnRightPTI.buttonPressed) TurnRight();
            else ResetSteeringAngle();

            if (handbrakePTI.buttonPressed) {
                CancelInvoke("DecelerateCar"); deceleratingCar = false; Handbrake();
                isBrakingInput = true;
            } else {
                RecoverTraction();
            }

            // Nitro Input Mobile
            if (nitroPTI != null && nitroPTI.buttonPressed) ActivateNitro();

            if (!throttlePTI.buttonPressed && !reversePTI.buttonPressed && !isBrakingInput && !deceleratingCar)
            {
                InvokeRepeating("DecelerateCar", 0f, 0.1f);
                deceleratingCar = true;
            }
        }
        else
        {
            // Keyboard Input
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) { 
                CancelInvoke("DecelerateCar"); deceleratingCar = false; GoForward(); 
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) { 
                CancelInvoke("DecelerateCar"); deceleratingCar = false; GoReverse(); 
            }

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) TurnLeft();
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) TurnRight();
            else ResetSteeringAngle();

            if (Input.GetKey(KeyCode.Space)) {
                CancelInvoke("DecelerateCar"); deceleratingCar = false; Handbrake();
                isBrakingInput = true;
            } 
            if (Input.GetKeyUp(KeyCode.Space)) {
                RecoverTraction();
                // Check Drift Boost khi thả phanh tay
                if(useDriftBoost) CheckMiniBoost();
            }

            // Nitro Input PC (Left Shift)
            if (Input.GetKey(KeyCode.LeftShift)) ActivateNitro();

            if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow)) {
                ThrottleOff();
            }

            if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow) && !Input.GetKey(KeyCode.Space) && !deceleratingCar) {
                InvokeRepeating("DecelerateCar", 0f, 0.1f);
                deceleratingCar = true;
            }
        }
        
        // Hồi Nitro nếu không dùng
        if (!isNitroActive && currentNitro < maxNitro)
        {
            currentNitro += nitroRegenRate * Time.deltaTime;
        }
    }

    // --- PHYSICS METHODS ---

    public void GoForward()
    {
        throttleAxis = Mathf.MoveTowards(throttleAxis, 1f, Time.deltaTime * 3f);
        if (localVelocityZ < -1f) Brakes(); // Đang lùi mà nhấn tới -> Phanh
    }

    public void GoReverse()
    {
        throttleAxis = Mathf.MoveTowards(throttleAxis, -1f, Time.deltaTime * 3f);
        if (localVelocityZ > 1f) Brakes(); // Đang tới mà nhấn lùi -> Phanh
    }

    // Áp dụng lực động cơ trong FixedUpdate
    void ApplyEngineForce()
    {
        // Logic Drift Boost Timer
        if (isDrifting && Mathf.Abs(carSpeed) > 10) driftTimer += Time.fixedDeltaTime;
        else driftTimer = 0f;

        if (Mathf.Abs(localVelocityX) > 2.5f) isDrifting = true;
        else isDrifting = false;

        float currentMaxSpeed = maxSpeed; 
        
        // Logic Motor Torque
        if (throttleAxis > 0) // Đi tới
        {
             if (carSpeed < currentMaxSpeed)
             {
                float torque = (accelerationMultiplier * 50f) * throttleAxis;
                SetMotorTorque(torque);
             }
             else SetMotorTorque(0);
        }
        else if (throttleAxis < 0) // Đi lùi
        {
             if (carSpeed < maxReverseSpeed)
             {
                float torque = (accelerationMultiplier * 50f) * throttleAxis;
                SetMotorTorque(torque);
             }
             else SetMotorTorque(0);
        }

        // Tự động phanh nhẹ khi không nhấn ga (giả lập ma sát động cơ)
        if (throttleAxis == 0) SetMotorTorque(0);
    }
    
    void SetMotorTorque(float torque)
    {
        frontLeftCollider.motorTorque = torque;
        frontRightCollider.motorTorque = torque;
        rearLeftCollider.motorTorque = torque;
        rearRightCollider.motorTorque = torque;
        // Xả phanh khi đang nhấn ga
        if(torque != 0) {
            frontLeftCollider.brakeTorque = 0;
            frontRightCollider.brakeTorque = 0;
            rearLeftCollider.brakeTorque = 0;
            rearRightCollider.brakeTorque = 0;
        }
    }

    // --- NITRO & BOOST LOGIC (VIP) ---
    void ActivateNitro()
    {
        if (useNitro && currentNitro > 0)
        {
            isNitroActive = true;
            currentNitro -= nitroConsumeRate * Time.deltaTime;
          Debug.Log("Nitro");

        }
    }

    void ApplyNitroPhysics()
    {
        if (isNitroActive)
        {
            // Thêm lực đẩy thẳng về phía trước
            carRigidbody.AddForce(transform.forward * nitroForce, ForceMode.Force);
            Debug.Log("Nitro Activated!");
        }
    }

    void CheckMiniBoost()
    {
        if (driftTimer >= driftDurationForBoost)
        {
            // Bắn xe tới trước (Impulse - lực tức thời)
            carRigidbody.AddForce(transform.forward * miniBoostForce, ForceMode.Impulse);
            Debug.Log("Mini Boost Activated!");
            // Ở đây có thể play sound effect boost hoặc particle boost
        }
        driftTimer = 0f;
    }

    // --- STEERING LOGIC ---
    void ApplySteering()
    {
        // Dynamic Steering: Chạy càng nhanh lái càng ít để tránh lật xe
        float speedFactor = Mathf.Clamp01(carSpeed / maxSpeed);
        float dynamicSteerAngle = Mathf.Lerp(maxSteeringAngle, maxSteeringAngle * 0.5f, speedFactor);

        float targetAngle = steeringAxis * dynamicSteerAngle;
        
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, targetAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, targetAngle, steeringSpeed);
    }

    public void TurnLeft()
    {
        steeringAxis = Mathf.MoveTowards(steeringAxis, -1f, Time.deltaTime * 10f * steeringSpeed);
    }

    public void TurnRight()
    {
        steeringAxis = Mathf.MoveTowards(steeringAxis, 1f, Time.deltaTime * 10f * steeringSpeed);
    }

    public void ResetSteeringAngle()
    {
        steeringAxis = Mathf.MoveTowards(steeringAxis, 0f, Time.deltaTime * 10f * steeringSpeed);
    }

    // --- BRAKING & DRIFTING ---
    public void Brakes()
    {
        frontLeftCollider.brakeTorque = brakeForce;
        frontRightCollider.brakeTorque = brakeForce;
        rearLeftCollider.brakeTorque = brakeForce;
        rearRightCollider.brakeTorque = brakeForce;
    }

    public void Handbrake()
    {
        driftingAxis = Mathf.MoveTowards(driftingAxis, 1f, Time.deltaTime);
        
        // Logic trượt bánh (Slip)
        float slipMultiplier = handbrakeDriftMultiplier * driftingAxis;
        
        SetSideFriction(frontLeftCollider, FLWextremumSlip, FLwheelFriction, slipMultiplier);
        SetSideFriction(frontRightCollider, FRWextremumSlip, FRwheelFriction, slipMultiplier);
        SetSideFriction(rearLeftCollider, RLWextremumSlip, RLwheelFriction, slipMultiplier);
        SetSideFriction(rearRightCollider, RRWextremumSlip, RRwheelFriction, slipMultiplier);

        isTractionLocked = true;
    }

    void SetSideFriction(WheelCollider wheel, float baseExtremum, WheelFrictionCurve curve, float multiplier)
    {
        curve.extremumSlip = baseExtremum * multiplier;
        wheel.sidewaysFriction = curve;
    }

    public void RecoverTraction()
    {
        isTractionLocked = false;
        driftingAxis = Mathf.MoveTowards(driftingAxis, 0f, Time.deltaTime / 1.5f);

        if (FLwheelFriction.extremumSlip > FLWextremumSlip)
        {
            float slipMultiplier = handbrakeDriftMultiplier * driftingAxis;
            // Nếu đang hồi phục mà multiplier nhỏ hơn 1 (mặc định) thì reset về mặc định luôn
            if(slipMultiplier < 1f) slipMultiplier = 1f;

            SetSideFriction(frontLeftCollider, FLWextremumSlip, FLwheelFriction, slipMultiplier);
            SetSideFriction(frontRightCollider, FRWextremumSlip, FRwheelFriction, slipMultiplier);
            SetSideFriction(rearLeftCollider, RLWextremumSlip, RLwheelFriction, slipMultiplier);
            SetSideFriction(rearRightCollider, RRWextremumSlip, RRwheelFriction, slipMultiplier);

            if(driftingAxis > 0) Invoke("RecoverTraction", Time.deltaTime);
        }
        else
        {
             // Reset về default
             RestoreDefaultFriction();
             driftingAxis = 0f;
        }
    }
    
    void RestoreDefaultFriction()
    {
        FLwheelFriction.extremumSlip = FLWextremumSlip; frontLeftCollider.sidewaysFriction = FLwheelFriction;
        FRwheelFriction.extremumSlip = FRWextremumSlip; frontRightCollider.sidewaysFriction = FRwheelFriction;
        RLwheelFriction.extremumSlip = RLWextremumSlip; rearLeftCollider.sidewaysFriction = RLwheelFriction;
        RRwheelFriction.extremumSlip = RRWextremumSlip; rearRightCollider.sidewaysFriction = RRwheelFriction;
    }

    [Obsolete]
    public void DecelerateCar()
    {
        if (Mathf.Abs(throttleAxis) < 0.1f) throttleAxis = 0f;
        else throttleAxis = Mathf.MoveTowards(throttleAxis, 0f, Time.deltaTime * 10f);

        // Hãm tốc bằng drag giả lập
        carRigidbody.velocity = carRigidbody.velocity * (1f / (1f + (0.025f * decelerationMultiplier)));
        
        // Ngắt lực
        SetMotorTorque(0);

        if (carRigidbody.velocity.magnitude < 0.25f)
        {
            carRigidbody.velocity = Vector3.zero;
            CancelInvoke("DecelerateCar");
        }
    }
    
    public void ThrottleOff()
    {
        SetMotorTorque(0);
    }

    // --- VISUALS & SOUNDS ---
    void UpdateUI()
    {
        if (useUI && carSpeedText != null)
            carSpeedText.text = Mathf.RoundToInt(Mathf.Abs(carSpeed)).ToString();
            
        if (useUI && nitroText != null)
            nitroText.text = "NITRO: " + Mathf.RoundToInt(currentNitro).ToString();
    }

    void UpdateSounds()
    {
        if (!useSounds) return;

        if (carEngineSound != null)
        {
            float pitch = initialCarEngineSoundPitch + (Mathf.Abs(carSpeed) / 100f);
            carEngineSound.pitch = pitch;
        }

        if ((isDrifting || isTractionLocked) && Mathf.Abs(carSpeed) > 12f)
        {
            if (!tireScreechSound.isPlaying) tireScreechSound.Play();
        }
        else
        {
            tireScreechSound.Stop();
        }
    }

    void HandleDriftEffects()
    {
        if (!useEffects) return;

        if (isDrifting || isTractionLocked)
        {
             if(!RLWParticleSystem.isPlaying) RLWParticleSystem.Play();
             if(!RRWParticleSystem.isPlaying) RRWParticleSystem.Play();
        }
        else
        {
             RLWParticleSystem.Stop();
             RRWParticleSystem.Stop();
        }

        if ((isTractionLocked || Mathf.Abs(localVelocityX) > 5f) && Mathf.Abs(carSpeed) > 12f)
        {
            RLWTireSkid.emitting = true;
            RRWTireSkid.emitting = true;
        }
        else
        {
            RLWTireSkid.emitting = false;
            RRWTireSkid.emitting = false;
        }
    }

    void AnimateWheelMeshes()
    {
        ApplyWheelPose(frontLeftCollider, frontLeftMesh);
        ApplyWheelPose(frontRightCollider, frontRightMesh);
        ApplyWheelPose(rearLeftCollider, rearLeftMesh);
        ApplyWheelPose(rearRightCollider, rearRightMesh);
    }

    void ApplyWheelPose(WheelCollider collider, GameObject mesh)
    {
        Vector3 pos; Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        mesh.transform.position = pos;
        mesh.transform.rotation = rot;
    }

    // --- SETUP HELPERS ---
    void SetupWheelFriction()
    {
        // Lưu giá trị ma sát mặc định để tính toán drift
        FLwheelFriction = frontLeftCollider.sidewaysFriction; FLWextremumSlip = FLwheelFriction.extremumSlip;
        FRwheelFriction = frontRightCollider.sidewaysFriction; FRWextremumSlip = FRwheelFriction.extremumSlip;
        RLwheelFriction = rearLeftCollider.sidewaysFriction; RLWextremumSlip = RLwheelFriction.extremumSlip;
        RRwheelFriction = rearRightCollider.sidewaysFriction; RRWextremumSlip = RRwheelFriction.extremumSlip;
    }

    void SetupTouchControls()
    {
        if (throttleButton != null && reverseButton != null && turnRightButton != null && turnLeftButton != null && handbrakeButton != null)
        {
            throttlePTI = throttleButton.GetComponent<PrometeoTouchInput>();
            reversePTI = reverseButton.GetComponent<PrometeoTouchInput>();
            turnLeftPTI = turnLeftButton.GetComponent<PrometeoTouchInput>();
            turnRightPTI = turnRightButton.GetComponent<PrometeoTouchInput>();
            handbrakePTI = handbrakeButton.GetComponent<PrometeoTouchInput>();
            if(nitroButton != null) nitroPTI = nitroButton.GetComponent<PrometeoTouchInput>();
            touchControlsSetup = true;
        }
        else
        {
            Debug.LogWarning("Touch controls not fully assigned!");
        }
    }
}