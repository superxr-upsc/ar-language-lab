using CodeBase.Infrastructure.GameStateMachineService.StateMachine;
using CodeBase.Infrastructure.GameStateMachineService.States;
using Zenject;

namespace CodeBase.Infrastructure.Installers
{
    public class BootstrapSceneInstaller : MonoInstaller, IInitializable
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<BootstrapSceneInstaller>().FromInstance(this).AsSingle();
        }

        public void Initialize()
        {
            Container.Resolve<IGameStateMachine>()
                .Enter<BootstrapState>();
        }
    }
}