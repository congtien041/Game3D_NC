using UnityEngine;
using System.Collections.Generic;

public class CarController : MonoBehaviour
{
    [Header("Wheels Setup")]
    public List<Wheel> wheels;
    public TrailRenderer[] tireMarks; 
    public ParticleSystem[] tireSmoke; 

    [Header("Engine & Speed Stats")]
    public float baseMaxSpeed = 120f; 
    public float baseAcceleration = 1500f;
    public float maxSteerAngle = 25f;
    public float downforce = 150f; 
    public float speedKmh;

    [Header("Stability Settings")]
    public float antiRollForce = 10000f;    
    public float tractionStiffness = 3f; 
    public float highSpeedStiffness = 5f; 

    [Header("Gear System")]
    public int currentGear = 1;
    private float[] gearSpeedLimits = { 0, 50, 90, 140, 190, 250, 350 }; 

    [Header("Nitro System")]
    public float nitroCharge = 0f;
    public float maxNitro = 100f;
    public ParticleSystem nitroFire; 
    [HideInInspector] public bool isItemBoostActive = false; // Biến hỗ trợ Item Nitro

    [Header("Car Data (Read Only)")]
    public bool isDrifting; // KHAI BÁO ĐÚNG Ở ĐÂY

    private Rigidbody carRb;
    private float moveInput;
    private float steerInput;
    private int upgradeLevel;

    [System.Serializable]
    public struct Wheel {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public Axel axel;
    }
    public enum Axel { Front, Rear }

    void Start() {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = new Vector3(0, -1.2f, 0.5f); 
        int index = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        upgradeLevel = PlayerPrefs.GetInt("CarUpgrade_" + index, 0);
        ApplyUpgradeStats();
    }

    void ApplyUpgradeStats() {
        baseMaxSpeed += (upgradeLevel * 25f);
        baseAcceleration += (upgradeLevel * 250f);
        downforce += (upgradeLevel * 100f); 
    }

    void Update() {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
        // Nếu Unity bản cũ báo lỗi linearVelocity, hãy đổi thành carRb.velocity
        speedKmh = carRb.linearVelocity.magnitude * 3.6f;
        HandleDriftLogic();
        HandleEffects();
        HandleSounds();
        AnimateWheels();
    }

    void FixedUpdate() {
        HandleMotor();
        HandleSteering();
        HandleGears();
        ApplyDownforce();
        ApplyAntiRoll();
        AdjustTraction();
    }

    void HandleMotor() {
        float torque = moveInput * (baseAcceleration / (currentGear * 0.7f));
        // Nitro kích hoạt khi nhấn Shift HOẶC ăn Item
        bool usingNitro = (Input.GetKey(KeyCode.LeftShift) && nitroCharge > 0) || isItemBoostActive;

        if (usingNitro) {
            torque *= (2.5f + (upgradeLevel * 0.1f));
            if (!isItemBoostActive) nitroCharge -= Time.fixedDeltaTime * 20f;
        } else if (nitroCharge < maxNitro) {
            nitroCharge += Time.fixedDeltaTime * 5f;
        }

        if (speedKmh > baseMaxSpeed && !usingNitro) torque = 0;

        foreach (var wheel in wheels) {
            if (wheel.axel == Axel.Rear) wheel.wheelCollider.motorTorque = torque;
            wheel.wheelCollider.brakeTorque = (Input.GetKey(KeyCode.Space)) ? 6000f : 0f;
        }
    }

    void HandleSteering() {
        float speedFactor = Mathf.Clamp01(1 - (speedKmh / (350f + upgradeLevel * 15f)));
        float responsiveSteer = steerInput * (maxSteerAngle * speedFactor);
        foreach (var wheel in wheels) {
            if (wheel.axel == Axel.Front) wheel.wheelCollider.steerAngle = responsiveSteer;
        }
    }

    void ApplyDownforce() { carRb.AddForce(-transform.up * downforce * carRb.linearVelocity.magnitude); }

    void ApplyAntiRoll() {
        for (int i = 0; i < wheels.Count; i += 2) {
            Wheel wheelL = wheels[i]; Wheel wheelR = wheels[i+1];
            float travelL = 1.0f; float travelR = 1.0f;
            WheelHit hit;
            if (wheelL.wheelCollider.GetGroundHit(out hit))
                travelL = (-wheelL.wheelCollider.transform.InverseTransformPoint(hit.point).y - wheelL.wheelCollider.radius) / wheelL.wheelCollider.suspensionDistance;
            if (wheelR.wheelCollider.GetGroundHit(out hit))
                travelR = (-wheelR.wheelCollider.transform.InverseTransformPoint(hit.point).y - wheelR.wheelCollider.radius) / wheelR.wheelCollider.suspensionDistance;
            float antiRollForceAmount = (travelL - travelR) * antiRollForce;
            if (wheelL.wheelCollider.isGrounded) carRb.AddForceAtPosition(wheelL.wheelCollider.transform.up * -antiRollForceAmount, wheelL.wheelCollider.transform.position);
            if (wheelR.wheelCollider.isGrounded) carRb.AddForceAtPosition(wheelR.wheelCollider.transform.up * antiRollForceAmount, wheelR.wheelCollider.transform.position);
        }
    }

    void AdjustTraction() {
        foreach (var wheel in wheels) {
            WheelFrictionCurve sf = wheel.wheelCollider.sidewaysFriction;
            if (!isDrifting) sf.stiffness = Mathf.Lerp(tractionStiffness, highSpeedStiffness, speedKmh / 300f);
            else sf.stiffness = 0.8f; 
            wheel.wheelCollider.sidewaysFriction = sf;
        }
    }

    void HandleDriftLogic() {
        isDrifting = Input.GetKey(KeyCode.Space) && Mathf.Abs(steerInput) > 0.3f && speedKmh > 30f;
        if (isDrifting) carRb.AddForce(transform.right * steerInput * 0.6f * carRb.linearVelocity.magnitude, ForceMode.Acceleration);
    }

    void HandleGears() {
        if (speedKmh > gearSpeedLimits[currentGear] && currentGear < 6) currentGear++;
        else if (speedKmh < gearSpeedLimits[currentGear - 1] && currentGear > 1) currentGear--;
    }

    void HandleSounds() {
        if (GlobalAudio.Instance == null) return;
        float pitch = Mathf.Lerp(0.8f, 2.5f, speedKmh / 200f);
        GlobalAudio.Instance.engineSource.pitch = pitch;
        GlobalAudio.Instance.StartEngineSound();
        if (isDrifting) GlobalAudio.Instance.StartBrakeLoop(); else GlobalAudio.Instance.StopBrakeLoop();
    }

    void HandleEffects() {
        foreach (var trail in tireMarks) trail.emitting = isDrifting;
        foreach (var smoke in tireSmoke) {
            if (isDrifting) { if (!smoke.isPlaying) smoke.Play(); } else smoke.Stop();
        }
        bool showFire = (Input.GetKey(KeyCode.LeftShift) && nitroCharge > 0) || isItemBoostActive;
        if (showFire) { if (nitroFire && !nitroFire.isPlaying) nitroFire.Play(); }
        else { if (nitroFire) nitroFire.Stop(); }
    }

    void AnimateWheels() {
        foreach (var wheel in wheels) {
            Vector3 pos; Quaternion rot;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
    }
}