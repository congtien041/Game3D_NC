using UnityEngine;
using System.Collections.Generic;

public class CarController : MonoBehaviour
{
    [Header("Wheels Setup")]
    public List<Wheel> wheels;
    public TrailRenderer[] tireMarks;

    [Header("Engine & Speed")]
    public float baseAcceleration = 1500f; // Tốc độ gốc
    public float maxSteerAngle = 25f;
    public float baseDownforce = 150f; 
    public float speedKmh;

    [Header("Gear System")]
    public int currentGear = 1;
    private float[] gearSpeedLimits = { 0, 55, 100, 145, 190, 240, 350 };

    [Header("Nitro & Drift")]
    public float nitroCharge = 0f;
    public float maxNitro = 100f;
    public bool isDrifting;
    public float driftFactor = 0.9f;

    private Rigidbody carRb;
    private float moveInput;
    private float steerInput;
    private int upgradeLevel;

    void Start() {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = new Vector3(0, -1.2f, 0.5f); 
        
        // Lấy cấp độ nâng cấp (0 đến 5)
        int index = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        upgradeLevel = PlayerPrefs.GetInt("CarUpgrade_" + index, 0);

        ApplyUpgradeStats();
    }

    void ApplyUpgradeStats() {
        // Cấp càng cao, gia tốc càng lớn (mỗi cấp +250)
        // Cấp 0: 1500 | Cấp 5: 2750
        baseAcceleration += (upgradeLevel * 250f);

        // Cấp càng cao, lực ép xuống đường càng mạnh (giúp xe cực kỳ ổn định)
        baseDownforce += (upgradeLevel * 40f);
    }

    void Update() {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
        speedKmh = carRb.linearVelocity.magnitude * 3.6f;
        
        HandleDriftInput();
        AnimateWheels();
    }

    void FixedUpdate() {
        HandleMotor();
        HandleSteering();
        ApplyDownforce();
        AdjustTraction();
        HandleGears();
    }

    void HandleMotor() {
        float torque = moveInput * (baseAcceleration / (currentGear * 0.7f));
        
        if (Input.GetKey(KeyCode.LeftShift) && nitroCharge > 0) {
            torque *= (2.5f + (upgradeLevel * 0.1f)); // Nitro cũng mạnh hơn theo cấp
            nitroCharge -= Time.fixedDeltaTime * 20f;
        }

        foreach (var wheel in wheels) {
            if (wheel.axel == Axel.Rear) wheel.wheelCollider.motorTorque = torque;
            wheel.wheelCollider.brakeTorque = (Input.GetKey(KeyCode.Space)) ? 6000f : 0f;
        }
    }

    void HandleSteering() {
        // Cấp cao lái sẽ mượt hơn, ít bị văng hơn
        float speedFactor = Mathf.Clamp01(1 - (speedKmh / (300f + upgradeLevel * 20f)));
        float responsiveSteer = steerInput * (maxSteerAngle * speedFactor);
        
        foreach (var wheel in wheels) {
            if (wheel.axel == Axel.Front) wheel.wheelCollider.steerAngle = responsiveSteer;
        }
    }

    void HandleDriftInput() {
        isDrifting = Input.GetKey(KeyCode.Space) && Mathf.Abs(steerInput) > 0.3f;
        if (isDrifting) {
            nitroCharge = Mathf.MoveTowards(nitroCharge, maxNitro, Time.deltaTime * (10f + upgradeLevel));
            carRb.AddForce(transform.right * steerInput * driftFactor * carRb.linearVelocity.magnitude, ForceMode.Acceleration);
        }
        foreach (var trail in tireMarks) { trail.emitting = isDrifting && speedKmh > 20f; }
    }

    void AdjustTraction() {
        foreach (var wheel in wheels) {
            WheelFrictionCurve sideFriction = wheel.wheelCollider.sidewaysFriction;
            // Cấp cao độ bám (stiffness) sẽ cao hơn
            float baseStiffness = 1.6f + (upgradeLevel * 0.2f); 
            sideFriction.stiffness = (speedKmh > 100 && !isDrifting) ? baseStiffness + 1.2f : baseStiffness; 
            wheel.wheelCollider.sidewaysFriction = sideFriction;
        }
    }

    void HandleGears() {
        if (speedKmh > gearSpeedLimits[currentGear] && currentGear < 6) currentGear++;
        else if (speedKmh < gearSpeedLimits[currentGear - 1] && currentGear > 1) currentGear--;
    }

    void ApplyDownforce() {
        carRb.AddForce(-transform.up * baseDownforce * carRb.linearVelocity.magnitude);
    }

    void AnimateWheels() {
        foreach (var wheel in wheels) {
            Vector3 pos; Quaternion rot;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
    }

    [System.Serializable] public struct Wheel { public GameObject wheelModel; public WheelCollider wheelCollider; public Axel axel; }
    public enum Axel { Front, Rear }
}