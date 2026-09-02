using System;
using CodeBase.Gameplay.Lessons;
using CodeBase.Infrastructure.Localization;
using CodeBase.Infrastructure.SaveLoad.Data;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CodeBase.UI.LessonsListWindow
{
    public class LessonDataView : MonoBehaviour
    {
        [SerializeField] private Button _selectLessonButton;
        
        [SerializeField] private Image _lessonIcon;
        [SerializeField] private Image _doneIcon;

        [SerializeField] private TMP_Text _lessonName;
        [SerializeField] private TMP_Text _lessonDescription;
        [SerializeField] private TMP_Text _lessonProgressText;
        
        private ILocalizationService _localizationService;

        [Inject]
        private void Construct(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }
        
        public void Initialize(LessonConfig lessonConfig, LessonProgress lessonProgress, Action<string> onLessonSelected)
        {
            _lessonIcon.sprite = lessonConfig.LessonIcon;
            _doneIcon.gameObject.SetActive(false);
            
            UpdateLocalizedText(lessonConfig).Forget();

            if (lessonProgress == null)
            {
                _lessonProgressText.text = "0%";
            }
            else
            {
                if (lessonProgress.IsComplete)
                {
                    _lessonProgressText.gameObject.SetActive(false);
                    _doneIcon.gameObject.SetActive(true);
                }
                else
                {
                    _lessonProgressText.text = $"{lessonConfig.GetCompletedTasksPercent(lessonProgress.LastCompletedTaskId)}%";
                }
            }
            
            _selectLessonButton.onClick.AddListener(() => onLessonSelected?.Invoke(lessonConfig.Id));
        }

        public void Cleanup()
        {
            _selectLessonButton.onClick.RemoveAllListeners();
        }

        private async UniTaskVoid UpdateLocalizedText(LessonConfig lessonConfig)
        {
            _lessonName.text = await _localizationService.GetStringAsync(lessonConfig.LessonNameKey);
            _lessonDescription.text = await _localizationService.GetStringAsync(lessonConfig.LessonDescriptionKey);
        }
    }
}