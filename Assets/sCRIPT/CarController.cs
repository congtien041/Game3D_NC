using UnityEngine;
using System.Collections.Generic;

public class CarController : MonoBehaviour
{
    public List<CarController.Wheel> wheels;
    public float maxAcceleration = 600f;
    public float brakeForce = 3000f;
    public float nitroPower = 1200f; // Chỉ số Nitro
    
    private Rigidbody carRb;
    private float moveInput;

    void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = new Vector3(0, -0.5f, 0);
        
        // Nhận chỉ số nâng cấp từ Selection
        int selectedIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        int upgradeLevel = PlayerPrefs.GetInt("CarUpgrade_" + selectedIndex, 0);
        maxAcceleration += (upgradeLevel * 50f); // Mỗi cấp tăng 50 đơn vị tốc độ
    }

    void FixedUpdate()
    {
        moveInput = Input.GetAxis("Vertical");
        float currentTorque = moveInput * maxAcceleration;

        // Nhấn Left Shift để dùng Nitro
        if (Input.GetKey(KeyCode.LeftShift)) 
        {
            currentTorque = moveInput * nitroPower;
            // Bạn có thể thêm hiệu ứng lửa tại đây
        }

        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Rear) wheel.wheelCollider.motorTorque = currentTorque;
            wheel.wheelCollider.brakeTorque = Input.GetKey(KeyCode.Space) ? brakeForce : 0f;
        }
    }
    
    // Giữ nguyên AnimateWheels và Struct Wheel từ code cũ của bạn
    [System.Serializable] public struct Wheel { public GameObject wheelModel; public WheelCollider wheelCollider; public Axel axel; }
    public enum Axel { Front, Rear }
}