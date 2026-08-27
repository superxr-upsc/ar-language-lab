using CodeBase.Common.Extensions;
using CodeBase.Common.LoggerService;
using CodeBase.Gameplay.ARObjects;
using CodeBase.Gameplay.SpeechSyntesis;
using CodeBase.Infrastructure.Localization;
using CodeBase.Infrastructure.Vuforia;
using Cysharp.Threading.Tasks;

namespace CodeBase.Gameplay.Lessons.Tasks
{
    public class FindObjectByVoiceTask : TaskResolverBase
    {
        private readonly ILocalizationService _localization;
        private readonly ILessonManagementService _lessonManagementService;
        private readonly Speaker _speaker;

        private ARObjectConfig _selectedObjectConfig;
        private ARObjectBase _selectedObject;

        private bool _completed = false;
        
        public FindObjectByVoiceTask(TaskData taskData, ILocalizationService localization, IARCameraProvider cameraProvider, ILessonManagementService lessonManagementService) : base(taskData)
        {
            _lessonManagementService = lessonManagementService;
            _localization = localization;
            _speaker = cameraProvider.GetSpeaker();
        }

        public override void Run()
        {
            _selectedObjectConfig = _taskData.TargetObjects.PickRandom();
            if (_selectedObjectConfig == null)
            {
                CompleteTask();
                return;
            }
            
            _selectedObject = _lessonManagementService.GetObject(_selectedObjectConfig);
            if (_selectedObject == null)
            {
                CompleteTask();
                return;
            }

            PlayQuestDescriptionForQuest().Forget();
            
            _selectedObject.Observer.NearCameraEntered += OnNearCameraEntered;
        }

        private async UniTaskVoid PlayQuestDescriptionForQuest()
        {
            var objectName = await _localization.GetStringAsync(_selectedObjectConfig.LocalisationKey);
            var questDescription = await _localization.GetStringAsync(_taskData.DescriptionLocalizationKey, LocalizationConsts.DefaultStringTableName, new { ObjectName = objectName });

            await _speaker.SpeakAsync(questDescription);
        }

        public override void Dispose()
        {
            if (_selectedObject != null) 
                _selectedObject.Observer.NearCameraEntered -= OnNearCameraEntered;

            _selectedObject = null;
            _selectedObjectConfig = null;
        }

        protected override bool IsTaskComplete() => 
            _completed;

        protected override void CompleteTask()
        {
            _completed = true;
            base.CompleteTask();
        }

        private void OnNearCameraEntered(float arg1, float arg2)
        {
            _completed = true;
            CompleteTask();
        }
    }
}