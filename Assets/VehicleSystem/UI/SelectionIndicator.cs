using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace VehicleSystem.UI
{
    public class SelectionIndicator : MonoBehaviour
    {
        [SerializeField] private RectTransform indicatorBar;
        [SerializeField] private RectTransform[] tabButtons;
        [SerializeField] private Button[] buttons;
        private float moveSpeed = 1800f;
        public TabVisualController tabVisualController;
        private int currentTabIndex = 3;
        private Coroutine moveCoroutine;

        private void Awake()
        {
            if (tabButtons == null || buttons == null || tabButtons.Length != buttons.Length)
            {
                return;
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                int capturedIndex = i;
                buttons[i].onClick.AddListener(() => OnTabButtonClicked(capturedIndex));
                // Đã xóa dòng gọi SetState ở đây
            }
        }

        private void Start()
        {
            if (tabButtons != null && tabButtons.Length > 0)
            {
                int lastIndex = tabButtons.Length - 1; 
                MoveToTab(lastIndex, true);
                Canvas.ForceUpdateCanvases(); 
            }
        }

        public void OnTabButtonClicked(int index)
        {
            MoveToTab(index, false);
        }

        public void MoveToTab(int index, bool instant)
        {
            if (tabButtons == null || index < 0 || index >= tabButtons.Length) return;
            if (index == currentTabIndex) return;

            currentTabIndex = index;

            // --- GỌI ĐỔI MÀU Ở ĐÂY ---
            // Gọi ở đây thì dù bấm chuột hay game tự chạy code nó đều đổi màu chuẩn
            if (tabVisualController != null)
            {
                tabVisualController.SetState(index);
            }

            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
            }

            Vector3 targetWorldPosition = tabButtons[index].position;
            Vector3 targetLocalPosition = indicatorBar.parent.InverseTransformPoint(targetWorldPosition);
            Vector2 finalTargetPosition = new Vector2(targetLocalPosition.x, indicatorBar.anchoredPosition.y);

            if (instant)
            {
                indicatorBar.anchoredPosition = finalTargetPosition;
            }
            else
            {
                moveCoroutine = StartCoroutine(MoveIndicatorCoroutine(finalTargetPosition));
            }
        }

        private IEnumerator MoveIndicatorCoroutine(Vector2 targetPositionFixedY)
        {
            Vector2 startPosition = indicatorBar.anchoredPosition;
            float elapsedTime = 0f;
            
            float distance = Vector2.Distance(startPosition, targetPositionFixedY);
            if (distance <= 0f) yield break;

            float moveTime = distance / moveSpeed;

            while (elapsedTime < moveTime)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsedTime / moveTime);

                indicatorBar.anchoredPosition = Vector2.Lerp(startPosition, targetPositionFixedY, normalizedTime);

                yield return null;
            }

            indicatorBar.anchoredPosition = targetPositionFixedY;
        }
    }
}