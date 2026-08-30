using CodeBase.Common.LoggerService;
using CodeBase.Gameplay.SpeechSyntesis;
using CodeBase.Infrastructure.GameStateMachineService.StateInfrastructure;
using CodeBase.Infrastructure.GameStateMachineService.StateMachine;
using CodeBase.Infrastructure.Localization;
using CodeBase.Infrastructure.SaveLoad;
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
        private readonly ITTSService _ttsService;

        public BootstrapState(IGameStateMachine stateMachine, 
            ILocalizationService localizationService,
            IVuforiaService vuforiaService,
            ISaveService saveService,
            ITTSService ttsService)
        {
            _stateMachine = stateMachine;
            _localizationService = localizationService;
            _vuforiaService = vuforiaService;
            _saveService = saveService;
            _ttsService = ttsService;
        }
        
        public override void Enter()
        {
            base.Enter();

            InitializeAndLoadGameplay().Forget();
        }

        private async UniTaskVoid InitializeAndLoadGameplay()
        {
            GameLogger.Log("LOADING GAMEPLAY......");
            
            await _localizationService.InitializeAsync();
            await _saveService.LoadAsync<SaveData>();
            await _vuforiaService.InitializeVuforia();
            await _ttsService.InitializeAsync();
            GameLogger.Log("ENTERING GAMEPLAY SCENE......");
            _stateMachine.Enter<EnterGameplaySceneState>();
        }
    }
}