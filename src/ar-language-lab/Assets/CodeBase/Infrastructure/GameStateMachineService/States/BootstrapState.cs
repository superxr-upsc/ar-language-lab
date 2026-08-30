using CodeBase.Common.LoggerService;
using CodeBase.Gameplay.SpeechSyntesis;
using CodeBase.Infrastructure.GameStateMachineService.StateInfrastructure;
using CodeBase.Infrastructure.GameStateMachineService.StateMachine;
using CodeBase.Infrastructure.Localization;
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
        private readonly ITTSService _ttsService;

        public BootstrapState(IGameStateMachine stateMachine, 
            ILocalizationService localizationService,
            IVuforiaService vuforiaService,
            ISaveService saveService,
            AutoSaveService autoSaveService,
            ITTSService ttsService)
        {
            _stateMachine = stateMachine;
            _localizationService = localizationService;
            _vuforiaService = vuforiaService;
            _saveService = saveService;
            _autoSaveService = autoSaveService;
            _ttsService = ttsService;
        }
        
        public override void Enter()
        {
            base.Enter();

            InitializeAndLoadGameplay().Forget();
        }

        private async UniTaskVoid InitializeAndLoadGameplay()
        {
            await _localizationService.InitializeAsync();
            await _saveService.LoadAsync<SaveData>();
            
            _autoSaveService.StartSaving();
            
            await _vuforiaService.InitializeVuforia();
            await _ttsService.InitializeAsync();
            
            _stateMachine.Enter<EnterGameplaySceneState>();
        }
    }
}