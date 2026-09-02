using CodeBase.Infrastructure.GameStateMachineService.StateInfrastructure;
using CodeBase.Infrastructure.GameStateMachineService.StateMachine;
using CodeBase.Infrastructure.Loading;
using CodeBase.Infrastructure.StaticData;
using CodeBase.Infrastructure.WindowsManagement;

namespace CodeBase.Infrastructure.GameStateMachineService.States
{
    public class EnterGameplaySceneState : SimpleState
    {
        private readonly IGameStateMachine _stateMachine;
        private readonly ISceneLoader _sceneLoader;
        private readonly IWindowsManagementService _windowsManagementService;

        public EnterGameplaySceneState(IGameStateMachine stateMachine, ISceneLoader sceneLoader, IWindowsManagementService windowsManagementService)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _windowsManagementService = windowsManagementService;
        }
        
        public override void Enter()
        {
            base.Enter();
            _windowsManagementService.CloseAllWindows();
            _sceneLoader.ShowLoadingScreen();
            _sceneLoader.LoadScene(Scenes.GameplaySceneInfo.Name, EnterGameplayLoopState);
        }

        private void EnterGameplayLoopState()
        {
            _stateMachine.Enter<EnterGameplayLoopState>();
        }
    }
}