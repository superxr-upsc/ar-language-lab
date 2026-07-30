using CodeBase.Common.LoggerService;
using CodeBase.Infrastructure.GameStateMachineService.StateInfrastructure;
using CodeBase.Infrastructure.GameStateMachineService.StateMachine;
using CodeBase.Infrastructure.Localization;
using Cysharp.Threading.Tasks;
using RSG;
using IPromise = RSG.IPromise;

namespace CodeBase.Infrastructure.GameStateMachineService.States
{
    public class BootstrapState : SimpleState
    {
        private readonly IGameStateMachine _stateMachine;
        private readonly ILocalizationService _localizationService;

        public BootstrapState(IGameStateMachine stateMachine, ILocalizationService localizationService)
        {
            _stateMachine = stateMachine;
            _localizationService = localizationService;
        }
        
        public override void Enter()
        {
            base.Enter();

            //Right now for testing flow 
            InitializeAndLoadGameplay();
        }

        private async UniTaskVoid InitializeAndLoadGameplay()
        {
            await InitializeLocalisationAsync();
            _stateMachine.Enter<EnterGameplaySceneState>();
        }

        private async UniTask InitializeLocalisationAsync()
        {
            await _localizationService.InitializeAsync();
            GameLogger.Log($"[BootstrapState] LocalizationService initialized with locale '{_localizationService.CurrentLocaleCode}'");
        }
    }
}