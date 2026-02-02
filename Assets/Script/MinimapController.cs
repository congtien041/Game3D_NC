using UnityEngine;

public class MinimapController : MonoBehaviour
{
    void OnEnable()
    {
        Time.timeScale = 1;
    }

    void Update()
    {
        // Nếu game đang pause thì ẩn arrow
        if (Time.timeScale == 0)
            gameObject.SetActive(false);
    }

    public void ShowArrow()
    {
        gameObject.SetActive(true);
    }
}
