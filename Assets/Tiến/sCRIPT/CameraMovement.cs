using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Target settings")]
    private Transform activeCar;
    
    [Header("Offset Settings")]
    public float distance = 6.0f;     // Khoảng cách ra sau xe
    public float height = 2.5f;       // Độ cao so với xe
    public float lookAtHeight = 1.5f; // Điểm nhìn cao hơn tâm xe một chút

    [Header("Smoothness")]
    public float rotationDamping = 3.0f; // Độ mượt xoay (quan trọng để ở sau xe)
    public float heightDamping = 2.0f;   // Độ mượt thay đổi cao độ

    public void SetActiveCar(Transform carTransform) {
        activeCar = carTransform;
    }

    void LateUpdate()
    {
        // Tự động tìm xe có tag "Player" nếu chưa gán
        if (activeCar == null) {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player) activeCar = player.transform;
            return;
        }

        float wantedRotationAngle = activeCar.eulerAngles.y;
        float wantedHeight = activeCar.position.y + height;

        float currentRotationAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;

        // 2. Làm mượt góc quay bằng Lerp
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

        // 3. Làm mượt cao độ
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

        // 4. Chuyển đổi góc quay thành Quaternion
        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        // 5. Đặt vị trí camera ra sau xe dựa trên góc quay đã làm mượt
        Vector3 newPosition = activeCar.position;
        newPosition -= currentRotation * Vector3.forward * distance;
        newPosition.y = currentHeight;

        transform.position = newPosition;

        // 6. Luôn nhìn vào xe
        transform.LookAt(activeCar.position + Vector3.up * lookAtHeight);
    }
}