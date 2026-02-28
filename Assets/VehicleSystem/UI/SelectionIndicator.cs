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
        [SerializeField] private float moveSpeed = 800f;

        private int currentTabIndex = -1;
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
            }
        }

        private void Start()
        {
            if (tabButtons != null && tabButtons.Length > 0)
            {
                Canvas.ForceUpdateCanvases(); 
                
                MoveToTab(0, true);
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