using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace VehicleSystem.UI
{
    public class CarColorPicker : MonoBehaviour
    {
        [Header("UI References")]
        public Slider hueSlider; 
        public Image sliderHandleImage; 

        [Header("Paint Settings")]
        [Tooltip("Tên material chứa lớp sơn của xe")]
        public string paintMaterialName = "body_paint"; 
        
        [Tooltip("Cường độ phát sáng (Neon effect)")]
        public float emissionIntensity = 1.5f; // Tăng lên một chút để màu sơn nổi bật và có phong cách neon

        private Transform carRoot; 
        private List<Material> carPaintMaterials = new List<Material>();

        private void Start()
        {
            if (hueSlider != null)
            {
                // Dải màu Hue trong HSV luôn chạy từ 0 đến 1
                hueSlider.minValue = 0f;
                hueSlider.maxValue = 1f;
                
                float savedHue = PlayerPrefs.GetFloat("SavedCarColor", 0f);
                hueSlider.value = savedHue;
                
                hueSlider.onValueChanged.AddListener(UpdateCarColor);
                
                if (sliderHandleImage != null)
                {
                    sliderHandleImage.color = Color.HSVToRGB(savedHue, 1f, 1f);
                }
            }
        }

        // Gọi hàm này và truyền vào Object Cha (Root) của chiếc xe
        public void SetupNewCar(Transform spawnedCarRoot)
        {
            carRoot = spawnedCarRoot;
            carPaintMaterials.Clear(); 

            // Quét toàn bộ các object con bên trong Root để tìm lớp sơn
            Renderer[] allRenderers = carRoot.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in allRenderers)
            {
                foreach (Material mat in rend.materials)
                {
                    // Lấy đúng material có chứa tên khai báo
                    if (mat.name.Contains(paintMaterialName))
                    {
                        carPaintMaterials.Add(mat);
                    }
                }
            }

            Debug.Log($"<color=cyan>Đã setup xe để đổi màu! Tìm thấy {carPaintMaterials.Count} mảnh lưới vỏ xe.</color>");

            // Áp dụng màu đang lưu trên Slider cho xe mới
            if (hueSlider != null)
            {
                UpdateCarColor(hueSlider.value);
            }
        }

        public void UpdateCarColor(float hueValue)
        {
            // Chuyển đổi giá trị 0-1 thành màu sắc thực tế (Đỏ -> Vàng -> Xanh lá -> Xanh dương -> Tím -> Đỏ)
            Color newColor = Color.HSVToRGB(hueValue, 1f, 1f);

            // Đổi màu cục Handle
            if (sliderHandleImage != null)
            {
                sliderHandleImage.color = newColor;
            }

            // Đổi màu toàn bộ vỏ xe
            if (carPaintMaterials.Count > 0)
            {
                foreach (Material mat in carPaintMaterials)
                {
                    mat.color = newColor;
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", newColor * emissionIntensity);
                }
            }

            PlayerPrefs.SetFloat("SavedCarColor", hueValue);
            PlayerPrefs.Save();
        }
    }
}