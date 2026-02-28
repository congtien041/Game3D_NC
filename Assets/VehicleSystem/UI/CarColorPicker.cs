using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace VehicleSystem.UI
{
    public class CarColorPicker : MonoBehaviour
    {
        public Slider hueSlider; 
        public Image sliderHandleImage; 
        private string paintMaterialName = "body_paint"; 
        private float emissionIntensity = 0.5f;
        private Transform carRoot; 
        private List<Material> carPaintMaterials = new List<Material>();

        private void Start()
        {
            if (hueSlider != null)
            {
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

        public void SetupNewCar(Transform spawnedCarRoot)
        {
            carRoot = spawnedCarRoot;
            carPaintMaterials.Clear(); // Xóa sạch bộ nhớ của chiếc xe cũ (nếu có)

            Renderer[] allRenderers = carRoot.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in allRenderers)
            {
                foreach (Material mat in rend.materials)
                {
                    if (mat.name.Contains(paintMaterialName))
                    {
                        carPaintMaterials.Add(mat);
                    }
                }
            }

            Debug.Log($"<color=cyan>Đã setup xe mới! Tìm thấy {carPaintMaterials.Count} mảnh lưới vỏ xe.</color>");

            if (hueSlider != null)
            {
                UpdateCarColor(hueSlider.value);
            }
        }

        public void UpdateCarColor(float hueValue)
        {
            Color newColor = Color.HSVToRGB(hueValue, 1f, 1f);

            if (sliderHandleImage != null)
            {
                sliderHandleImage.color = newColor;
            }

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
        }
    }
}