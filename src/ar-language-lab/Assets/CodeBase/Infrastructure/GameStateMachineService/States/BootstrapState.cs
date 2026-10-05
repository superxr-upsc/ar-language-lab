using System.Linq;
using CodeBase.Common.LoggerService;
using CodeBase.Gameplay.Lessons;
using CodeBase.Gameplay.SpeechSyntesis;
using CodeBase.Infrastructure.GameStateMachineService.StateInfrastructure;
using CodeBase.Infrastructure.GameStateMachineService.StateMachine;
using CodeBase.Infrastructure.Loading;
using CodeBase.Infrastructure.Localization;
using CodeBase.Infrastructure.ProjectResourcesProvider;
using CodeBase.Infrastructure.SaveLoad;
using CodeBase.Infrastructure.SaveLoad.AutoSaver;
using CodeBase.Infrastructure.SaveLoad.Data;
using CodeBase.Infrastructure.Vuforia;
using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.GameStateMachineService.States
{
    public class BootstrapState : SimpleState
    {
        private readonly IGameStateMachine _stateMachine;
        private readonly ILocalizationService _localizationService;
        private readonly IVuforiaService _vuforiaService;
        private readonly ISaveService _saveService;
        private readonly AutoSaveService _autoSaveService;
        private readonly ISceneLoader _sceneLoader;
        private readonly ITTSService _ttsService;
        private readonly IProjectResourcesProvider _resourcesProvider;

        public BootstrapState(IGameStateMachine stateMachine, 
            ILocalizationService localizationService,
            IVuforiaService vuforiaService,
            ISaveService saveService,
            AutoSaveService autoSaveService,
            ISceneLoader sceneLoader,
            ITTSService ttsService,
            IProjectResourcesProvider resourcesProvider)
        {
            _stateMachine = stateMachine;
            _localizationService = localizationService;
            _vuforiaService = vuforiaService;
            _saveService = saveService;
            _autoSaveService = autoSaveService;
            _sceneLoader = sceneLoader;
            _ttsService = ttsService;
            _resourcesProvider = resourcesProvider;
        }
        
        public override void Enter()
        {
            base.Enter();

            InitializeAndLoadGameplay().Forget();
        }

        private async UniTaskVoid InitializeAndLoadGameplay()
        {
            _sceneLoader.ShowLoadingScreen();

            _sceneLoader.UpdateProgress(0.1f, "Loading Save Data...");
            await _saveService.LoadAsync<SaveData>();
            _autoSaveService.StartSaving();

            InitFistLessonIfEmpty();
            
            _sceneLoader.UpdateProgress(0.35f, "Initializing Localization...");
            await _localizationService.InitializeAsync(_saveService.SaveData.Settings.Language);
            
            _sceneLoader.UpdateProgress(0.65f, "Initializing Vuforia...");
            await _vuforiaService.InitializeVuforia();
            
            _sceneLoader.UpdateProgress(0.85f, "Initializing TTS models...");
            await _ttsService.InitializeAsync();
            
            _stateMachine.Enter<EnterMainMenuState>();
        }

        private void InitFistLessonIfEmpty()
        {
            var config = _resourcesProvider.LoadResource<GameLessons>();
            if (_saveService.SaveData.Lessons.Progress != null &&
                _saveService.SaveData.Lessons.Progress.Count != 0) return;
            
            var firstLesson = config.Lessons.FirstOrDefault();
            var progress = new LessonProgress
            {
                LessonId = firstLesson.Id,
                LastCompletedTaskId = string.Empty,
                IsOpen = true,
                IsComplete = false
            };
                
            _saveService.SaveData.Lessons.Progress.Add(progress);
            _saveService.MarkDirty();
        }
    }
}