using CodeBase.Infrastructure.GameStateMachineService.StateInfrastructure;
using CodeBase.Infrastructure.Loading;
using CodeBase.Infrastructure.StaticData;
using CodeBase.Infrastructure.WindowsManagement;
using CodeBase.UI;

namespace CodeBase.Infrastructure.GameStateMachineService.States
{
    public class EnterMainMenuState : SimpleState
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly IWindowsManagementService _windowsManagementService;

        public EnterMainMenuState(ISceneLoader sceneLoader, IWindowsManagementService windowsManagementService)
        {
            _sceneLoader = sceneLoader;
            _windowsManagementService = windowsManagementService;
        }
        
        public override void Enter()
        {
            base.Enter();
            
            _sceneLoader.UpdateProgress(0.95f, "A little bit more...");
            _sceneLoader.LoadScene(Scenes.MainMenuSceneInfo.Name, OnEnteredInMainMenuScene);
        }

        private void OnEnteredInMainMenuScene()
        {
            _windowsManagementService.CreateWindow<MainMenuPresenter, MainMenuView, MainMenuData>(UILayer.MainLayer, new MainMenuData());
            _sceneLoader.UpdateProgress(1, "Done!");
            _sceneLoader.CloseLoadingScreen();
        }
    }
}