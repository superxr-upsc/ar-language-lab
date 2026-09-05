using CodeBase.Infrastructure.GameStateMachineService.StateInfrastructure;
using CodeBase.Infrastructure.Loading;
using CodeBase.Infrastructure.SaveLoad;
using CodeBase.Infrastructure.StaticData;
using CodeBase.Infrastructure.WindowsManagement;
using CodeBase.UI;

namespace CodeBase.Infrastructure.GameStateMachineService.States
{
    public class EnterMainMenuState : SimpleState
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly IWindowsManagementService _windowsManagementService;

        private readonly ISaveService _saveService;

        public EnterMainMenuState(ISceneLoader sceneLoader, 
            IWindowsManagementService windowsManagementService,
            ISaveService saveService)
        {
            _sceneLoader = sceneLoader;
            _windowsManagementService = windowsManagementService;
            _saveService = saveService;
        }
        
        public override void Enter()
        {
            base.Enter();

            _saveService.SaveAsync();            
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