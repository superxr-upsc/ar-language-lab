using System;
using System.Collections;
using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.LoadingScreen
{
    public class LoadingScreenView : ViewBase
    {
        [SerializeField] private TMP_Text _loadingText;
        [SerializeField] private TMP_Text _progressText;
        [SerializeField] private Slider _progressSlider;
        
        [SerializeField] private CanvasGroup _canvasGroup;
        private Coroutine _loadingAnimationCoroutine;
        
        protected override void OpenWindowAnimation(Action<ViewBase> resolve, Action<Exception> reject)
        {
            if (_loadingAnimationCoroutine == null)
                _loadingAnimationCoroutine = StartCoroutine(LoadingAnimation());

            base.OpenWindowAnimation(resolve, reject);
        }

        protected override void CloseWindowAnimation(Action resolve, Action<Exception> reject)
        {
            if (_loadingAnimationCoroutine != null)
            {
                StopCoroutine(_loadingAnimationCoroutine);
                _loadingAnimationCoroutine = null;
            }

            Tween.Alpha(_canvasGroup, 0f, 0.5f)
                .OnComplete(() =>
                {
                    base.CloseWindowAnimation(resolve, reject);
                });
        }

        public void UpdateProgressText(string progress) => 
            _progressText.text = progress;
        
        public void UpdateProgressBar(float progress)
        {
            _progressSlider.value = progress;
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