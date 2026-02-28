using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace VehicleSystem.UI
{
    public class SplashLoader : MonoBehaviour
    {
        [Header("UI Elements")]
        [Tooltip("Kéo GameObject chứa toàn bộ giao diện Splash (Canvas hoặc Panel) vào đây để tắt nó đi khi xong")]
        [SerializeField] private GameObject splashUI;
        [Tooltip("Kéo Slider trong Canvas vào đây")]
        [SerializeField] private Slider loadingSlider;

        [Header("Loading Settings")]
        [Tooltip("Thời gian giả lập loading chạy từ 0 đến 100% (tính bằng giây)")]
        [SerializeField] private float fakeLoadDuration = 4f; // Chỉnh số này để load nhanh hay chậm

        [Header("Audio Settings")]
        [Tooltip("Kéo component Audio Source vào đây")]
        [SerializeField] private AudioSource audioSource;
        [Tooltip("Kéo file âm thanh tiếng pô xe vào đây")]
        [SerializeField] private AudioClip exhaustSound;

        private void Start()
        {
            // Đảm bảo giá trị slider bắt đầu từ 0
            if (loadingSlider != null)
            {
                loadingSlider.value = 0f;
            }

            // Nếu bạn quên gán splashUI, nó sẽ tự động lấy chính GameObject đang gắn script này
            if (splashUI == null)
            {
                splashUI = gameObject;
            }

            StartCoroutine(SimulateLoadingAndPlaySound());
        }

        private IEnumerator SimulateLoadingAndPlaySound()
        {
            float elapsedTime = 0f;

            // 1. Giả lập quá trình loading bằng cách cho slider tăng dần theo thời gian
            while (elapsedTime < fakeLoadDuration)
            {
                elapsedTime += Time.deltaTime;
                
                if (loadingSlider != null)
                {
                    // Hàm Mathf.Clamp01 giúp giới hạn giá trị từ 0 đến 1
                    loadingSlider.value = Mathf.Clamp01(elapsedTime / fakeLoadDuration);
                }

                yield return null; // Đợi frame tiếp theo
            }

            // Đảm bảo thanh slider đạt mốc 1 (100%) khi thoát vòng lặp
            if (loadingSlider != null)
            {
                loadingSlider.value = 1f;
            }

            // 2. Slider đã đầy! Phát tiếng bô xe
            if (audioSource != null && exhaustSound != null)
            {
                audioSource.PlayOneShot(exhaustSound);
                
                // Chờ tiếng pô kêu đủ phê (khoảng 1.5 giây) rồi mới tắt UI
                yield return new WaitForSeconds(1.5f); 
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }

            // 3. Âm thanh đã phát xong, TẮT giao diện loading đi để lộ ra UI chính
            if (splashUI != null)
            {
                splashUI.SetActive(false);
            }
        }
    }
}