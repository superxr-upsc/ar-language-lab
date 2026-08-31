using System;
using System.Collections;
using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using PrimeTween;
using TMPro;
using UnityEngine;

namespace CodeBase.Infrastructure.Loading.UI
{
    public class LoadingScreenView : ViewBase
    {
        [SerializeField] private TMP_Text _loadingText;
        [SerializeField] private TMP_Text _progressText;

        [SerializeField] private RectTransform _progressBar;
        [SerializeField] private RectTransform _progressBarBackground;
        
        [SerializeField] private CanvasGroup _canvasGroup;
        
        protected override void OpenWindowAnimation(Action<ViewBase> resolve, Action<Exception> reject)
        {
            StartCoroutine(LoadingAnimation());
            base.OpenWindowAnimation(resolve, reject);
        }

        protected override void CloseWindowAnimation(Action resolve, Action<Exception> reject)
        {
            StopCoroutine(LoadingAnimation());
            Tween.Alpha(_canvasGroup, 0f, 0.5f)
                .OnComplete(() => resolve?.Invoke());
            
            base.CloseWindowAnimation(resolve, reject);
        }

        public void UpdateProgressText(string progress) => 
            _progressText.text = progress;
        
        public void UpdateProgressBar(float progress)
        {
            var clampedProgress = Mathf.Clamp01(progress);
            var backgroundWidth = _progressBarBackground.rect.width;
            var rightInset = (1f - clampedProgress) * backgroundWidth;

            var offsetMax = _progressBar.offsetMax;
            offsetMax.x = -rightInset;
            _progressBar.offsetMax = offsetMax;
        }

        private IEnumerator LoadingAnimation()
        {
            var dotsCount = 0;
            while (true)
            {
                _loadingText.text = $"Loading{new string('.', dotsCount)}";
                dotsCount = (dotsCount + 1) % 4;
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}