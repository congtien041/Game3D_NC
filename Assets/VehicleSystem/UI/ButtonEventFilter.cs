using UnityEngine;
using UnityEngine.Events;
using VehicleSystem.UI;

public class ButtonEventFilter : MonoBehaviour
{
    public UnityEvent onMenuClick; 
    public void ExecuteFilter()
    {
        if (CarUpgradeManager.Instance != null && CarUpgradeManager.Instance.IsUpgradeMode)
        {
            Debug.Log("Đang trong chế độ nâng cấp, đã chặn các lệnh Menu.");
        }
        else
        {
            onMenuClick?.Invoke();
        }
    }
}