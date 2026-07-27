using CodeBase.Infrastructure.GameStateMachineService.StateInfrastructure;
using CodeBase.Infrastructure.GameStateMachineService.StateMachine;
using CodeBase.Infrastructure.Loading;
using CodeBase.Infrastructure.StaticData;
using RSG;

namespace CodeBase.Infrastructure.GameStateMachineService.States
{
    public class EnterSampleSceneState : SimpleState
    {
        private readonly IGameStateMachine _stateMachine;
        private readonly ISceneLoader _sceneLoader;

        public EnterSampleSceneState(IGameStateMachine stateMachine, ISceneLoader sceneLoader)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
        }
        
        public override void Enter()
        {
            base.Enter();
            
            _sceneLoader.LoadScene(Scenes.GameplaySceneInfo.Name);
        }
    }
}