using UnityEngine;
using System.Collections;

public class NitroItem : MonoBehaviour
{
    public float boostDuration = 5f; // Thời gian hiệu lực 5 giây
    public float respawnTime = 10f; // Thời gian vật phẩm xuất hiện lại
    public GameObject itemModel;   // Hình ảnh của vật phẩm để ẩn/hiện

    private bool isAvailable = true;

    private void OnTriggerEnter(Collider healthcare)
    {
        // Kiểm tra nếu vật thể chạm vào có script CarController
        CarController car = healthcare.GetComponentInParent<CarController>();

        if (car != null && isAvailable)
        {
            StartCoroutine(ApplyBoost(car));
        }
    }

    IEnumerator ApplyBoost(CarController car)
    {
        isAvailable = false;
        if(itemModel) itemModel.SetActive(false); // Ẩn vật phẩm đi

        // Kích hoạt Nitro trên xe (giả định dùng 100% năng lượng)
        float originalNitro = car.nitroCharge;
        car.nitroCharge = 100f; 

        // Hiệu lực trong 5 giây bằng cách giữ phím Shift giả lập hoặc 
        // cộng dồn tốc độ trực tiếp (tùy thuộc vào cách bạn muốn xử lý)
        // Ở đây chúng ta cho xe "vô hạn" nitro trong 5 giây
        float timer = 0;
        while (timer < boostDuration)
        {
            car.nitroCharge = 100f; // Giữ thanh nitro luôn đầy
            timer += Time.deltaTime;
            yield return null;
        }

        // Sau 5 giây, trả lại trạng thái bình thường
        if(itemModel) itemModel.SetActive(true);
        isAvailable = true;
    }
}