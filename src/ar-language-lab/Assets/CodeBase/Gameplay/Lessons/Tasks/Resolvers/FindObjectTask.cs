using CodeBase.Common.Extensions;
using CodeBase.Gameplay.ARObjects;
using CodeBase.Infrastructure.Localization;
using CodeBase.Infrastructure.Vuforia;
using Cysharp.Threading.Tasks;

namespace CodeBase.Gameplay.Lessons.Tasks
{
    public class FindObjectTask : TaskResolverBase
    {
        private readonly ILocalizationService _localization;
        private readonly ILessonManagementService _lessonManagementService;

        private ARObjectConfig _selectedObjectConfig;
        private ARObjectBase _selectedObject;

        public FindObjectTask(TaskData taskData, 
            IARCameraProvider cameraProvider, 
            ILocalizationService localization, 
            ILessonManagementService lessonManagementService) 
            : base(taskData, cameraProvider)
        {
            _lessonManagementService = lessonManagementService;
            _localization = localization;
        }

        public override void Run()
        {
            base.Run();
            _selectedObject.Observer.NearCameraEntered += OnNearCameraEntered;
        }

        public override void Dispose()
        {
            if (_selectedObject != null) 
                _selectedObject.Observer.NearCameraEntered -= OnNearCameraEntered;

            _selectedObject = null;
            _selectedObjectConfig = null;
        }

        protected override bool TryResolveTargets()
        {
            _selectedObjectConfig = _taskData.TargetObjects.PickRandom();
            if (_selectedObjectConfig == null)
                return false;
            
            _selectedObject = _lessonManagementService.GetObject(_selectedObjectConfig);
            return _selectedObject != null;
        }

        protected override async UniTask<string> GetQuestDescription()
        {
            var objectName = await _localization.GetStringAsync(_selectedObjectConfig.LocalisationKey);
            var questDescription = await _localization.GetStringAsync(_taskData.DescriptionLocalizationKey, LocalizationConsts.DefaultStringTableName, new { ObjectName = objectName });
            return questDescription;
        }

        private void OnNearCameraEntered(float arg1, float arg2) => 
            CompleteTask();
    }
}