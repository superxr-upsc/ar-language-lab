using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Tasks
{
    public class ActiveTaskView : ViewBase
    {
        public Button PlayAudioButton => _playAudioButton;
        
        [SerializeField] private TMP_Text _taskDescriptionText;
        [SerializeField] private Button _playAudioButton;
        [SerializeField] private RectTransform _progressBarBackground;
        [SerializeField] private RectTransform _progressBar;
        
        public void SetTaskDescription(string taskDescription) => 
            _taskDescriptionText.text = taskDescription;
        
        public void UpdateProgressBar(float progress)
        {
            var clampedProgress = Mathf.Clamp01(progress);
            var backgroundWidth = _progressBarBackground.rect.width;
            var rightInset = (1f - clampedProgress) * backgroundWidth;

            var offsetMax = _progressBar.offsetMax;
            offsetMax.x = -rightInset;
            _progressBar.offsetMax = offsetMax;
        }
    }
}