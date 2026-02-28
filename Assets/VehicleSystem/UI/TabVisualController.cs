using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VehicleSystem.UI
{
    public class TabVisualController : MonoBehaviour
    {
        [Header("UI Components")]
        [Tooltip("Ảnh nền của nút này")]
        [SerializeField] private Image[] backgroundImage;
        
        [Tooltip("Chữ bên trong nút")]
        [SerializeField] private TextMeshProUGUI[] tabText; 
        
        [Tooltip("Ảnh hình tam giác ở góc dưới")]
        [SerializeField] private Image[] cornerTriangle;

        [Header("Trạng thái ĐƯỢC CHỌN (Selected)")]
        [SerializeField] private Color selectedBgColor = Color.white; 
        [SerializeField] private Color selectedTextColor = Color.black; 
        [SerializeField] private Color selectedTriangleColor = Color.black; 

        [Header("Trạng thái BÌNH THƯỜNG (Unselected)")]
        [SerializeField] private Color unselectedBgColor = new Color(0.12f, 0.12f, 0.12f, 1f); 
        [SerializeField] private Color unselectedTextColor = Color.white; 
        [SerializeField] private Color unselectedTriangleColor = Color.white; 

        // Nhận vào vị trí (index) của nút ĐANG ĐƯỢC CHỌN
        public void SetState(int selectedIndex)
        {
            for(int i = 0; i < backgroundImage.Length; i++)
            {
                // Kiểm tra xem vị trí i hiện tại có đúng bằng nút đang được chọn không
                bool isSelected = (i == selectedIndex);

                if (backgroundImage != null && i < backgroundImage.Length && backgroundImage[i] != null)
                    backgroundImage[i].color = isSelected ? selectedBgColor : unselectedBgColor;

                if (tabText != null && i < tabText.Length && tabText[i] != null)
                    tabText[i].color = isSelected ? selectedTextColor : unselectedTextColor;

                if (cornerTriangle != null && i < cornerTriangle.Length && cornerTriangle[i] != null)
                    cornerTriangle[i].color = isSelected ? selectedTriangleColor : unselectedTriangleColor;
            }
        }
    }
}