using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Tasks
{
    public class ActiveTaskView : ViewBase
    {
        public Button PlayAudioButton => _playAudioButton;
        public Button TranslateButton => _translateButton;
        
        [SerializeField] private TMP_Text _taskDescriptionText;
        [SerializeField] private Button _playAudioButton;
        [SerializeField] private Button _translateButton;
        [SerializeField] private Slider _progressBar;
        
        public void SetTaskDescription(string taskDescription) => 
            _taskDescriptionText.text = taskDescription;
        
        public void UpdateProgressBar(float progress)
        {
            _progressBar.value = progress;
        }
    }
}