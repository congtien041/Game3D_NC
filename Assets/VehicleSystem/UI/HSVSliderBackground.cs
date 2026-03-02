using UnityEngine;
using UnityEngine.UI;

namespace VehicleSystem.UI
{
    [RequireComponent(typeof(Image))]
    public class HSVSliderBackground : MonoBehaviour
    {
        private void Start()
        {
            GenerateGradientBackground();
        }

        private void GenerateGradientBackground()
        {
            Image bgImage = GetComponent<Image>();
            
            // Độ phân giải của dải màu (256 pixel là quá đủ để mượt)
            int width = 256; 
            int height = 1;
            
            Texture2D tex = new Texture2D(width, height);
            tex.wrapMode = TextureWrapMode.Clamp; // Giúp viền không bị lem màu

            // Vòng lặp để tô màu cho từng pixel từ trái sang phải
            for (int x = 0; x < width; x++)
            {
                // Quy đổi vị trí pixel (0-256) thành dải màu Hue (0-1)
                float hue = (float)x / (width - 1);
                
                // Chuyển mã Hue thành màu thực tế
                Color color = Color.HSVToRGB(hue, 1f, 1f);
                
                tex.SetPixel(x, 0, color);
            }
            
            tex.Apply(); // Áp dụng các pixel đã vẽ

            // Tạo thành một tấm ảnh (Sprite) và dán thẳng vào UI
            bgImage.sprite = Sprite.Create(tex, new Rect(0, 0, width, height), Vector2.zero);
            bgImage.color = Color.white; // Phải là màu trắng để hiển thị đúng màu gốc
        }
    }
}