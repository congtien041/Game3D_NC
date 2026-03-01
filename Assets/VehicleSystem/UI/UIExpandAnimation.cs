using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VehicleSystem.UI
{
    public class UIExpandAnimation : MonoBehaviour
    {
        private float duration = 0.25f; 
        private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public bool isLooping = false; 

        private Dictionary<RectTransform, Coroutine> activeCoroutines = new Dictionary<RectTransform, Coroutine>();

        public void Show(RectTransform targetUI)
        {
            if (targetUI == null) return;

            targetUI.gameObject.SetActive(true); 

            if (activeCoroutines.ContainsKey(targetUI) && activeCoroutines[targetUI] != null)
            {
                StopCoroutine(activeCoroutines[targetUI]);
            }

            activeCoroutines[targetUI] = StartCoroutine(AnimateScale(0f, 1f, false, targetUI));
        }

        public void Hide(RectTransform targetUI)
        {
            if (targetUI == null) return;

            if (targetUI.gameObject.activeInHierarchy) 
            {
                if (activeCoroutines.ContainsKey(targetUI) && activeCoroutines[targetUI] != null)
                {
                    StopCoroutine(activeCoroutines[targetUI]);
                }
                
                activeCoroutines[targetUI] = StartCoroutine(AnimateScale(targetUI.localScale.x, 0f, true, targetUI));
            }
        }

        private IEnumerator AnimateScale(float startScale, float targetScale, bool isHiding, RectTransform targetUI)
        {
            float time = 0f;

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float t = time / duration;
                float currentScale = Mathf.Lerp(startScale, targetScale, easeCurve.Evaluate(t));
                
                targetUI.localScale = new Vector3(currentScale, 1, 1);
                yield return null;
            }

            targetUI.localScale = new Vector3(targetScale, 1, 1);

            if (isLooping)
            {
                if (isHiding) Show(targetUI); 
                else Hide(targetUI);         
            }
            else if (isHiding)
            {
                targetUI.gameObject.SetActive(false); 
            }
        }
    }
}