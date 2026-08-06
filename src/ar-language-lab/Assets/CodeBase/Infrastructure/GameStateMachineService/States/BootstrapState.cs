using CodeBase.Common.LoggerService;
using CodeBase.Infrastructure.GameStateMachineService.StateInfrastructure;
using CodeBase.Infrastructure.GameStateMachineService.StateMachine;
using CodeBase.Infrastructure.Localization;
using CodeBase.Infrastructure.Vuforia;
using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.GameStateMachineService.States
{
    public class BootstrapState : SimpleState
    {
        private readonly IGameStateMachine _stateMachine;
        private readonly ILocalizationService _localizationService;
        private readonly IVuforiaService _vuforiaService;

        public BootstrapState(IGameStateMachine stateMachine, 
            ILocalizationService localizationService,
            IVuforiaService vuforiaService)
        {
            _stateMachine = stateMachine;
            _localizationService = localizationService;
            _vuforiaService = vuforiaService;
        }
        
        public override void Enter()
        {
            base.Enter();

            InitializeAndLoadGameplay().Forget();
        }

        private async UniTaskVoid InitializeAndLoadGameplay()
        {
            await InitializeLocalisationAsync();
            await InitializeVuforiaAsync();
            
            _stateMachine.Enter<EnterGameplaySceneState>();
        }

        private async UniTask InitializeLocalisationAsync()
        {
            await _localizationService.InitializeAsync();
            GameLogger.Log($"[BootstrapState] LocalizationService initialized with locale '{_localizationService.CurrentLocaleCode}'");
        }

        private async UniTask InitializeVuforiaAsync()
        {
            await _vuforiaService.InitializeVuforia();
            GameLogger.Log("[BootstrapState] VuforiaService initialized");
        }
    }
}