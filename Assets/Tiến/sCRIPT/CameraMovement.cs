using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform[] cars;      // all player cars
    public float distance = 6f;
    public float height = 2.5f;
    public float orbitSpeed = 120f;

    private Transform activeCar;
    private float currentAngle;

    void LateUpdate()
    {
        // Tự động tìm xe có tag "Player" nếu chưa gán
        if (activeCar == null) {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player) activeCar = player.transform;
            return;
        }

        if (Input.GetMouseButton(1)) {
            currentAngle += Input.GetAxis("Mouse X") * orbitSpeed * Time.deltaTime;
        }
        UpdateCamera();
    }

    Transform FindActiveCar()
    {
        foreach (Transform car in cars)
        {
            if (car != null && car.gameObject.activeSelf)
                return car;
        }
        return null;
    }

    void UpdateCamera()
    {
        currentAngle = Mathf.Repeat(currentAngle, 360);

        Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);
        Vector3 offset = rotation * new Vector3(0, height, -distance);

        transform.position = activeCar.position + offset;
        transform.LookAt(activeCar.position + Vector3.up * 1.5f);
    }

}
