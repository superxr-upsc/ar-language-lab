using CodeBase.Infrastructure.GameStateMachineService.StateMachine;
using CodeBase.Infrastructure.GameStateMachineService.States;
using CodeBase.Infrastructure.Vuforia;
using CodeBase.Infrastructure.WindowsManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CodeBase.UI
{
    public class GameplaySerttingsView : MonoBehaviour
    {
        [SerializeField] private Toggle _flashToggle;
        [SerializeField] private Button _closeButton;
        
        private IVuforiaService _vuforiaService;
        private IGameStateMachine _gameStateMachine;
        private IWindowsManagementService _windowsService;

        [Inject]
        private void Construct(IVuforiaService vuforiaService, 
            IGameStateMachine gameStateMachine,
            IWindowsManagementService windowsManagementService)
        {
            _vuforiaService = vuforiaService;
            _gameStateMachine = gameStateMachine;
            _windowsService = windowsManagementService;
        }
        
        private void Awake()
        {
            _flashToggle.isOn = false;
            _flashToggle.onValueChanged.AddListener(ToggleFlash);
        }

        private void OnDestroy() => 
            _flashToggle.onValueChanged.RemoveListener(ToggleFlash);

        private void ToggleFlash(bool isOn) => 
            _vuforiaService.SetDeviceFlashTorch(isOn);

        public void OnCloseGameplay()
        {
            _windowsService.CloseAllWindows();
            _vuforiaService.SetVuforiaState(false);
            _vuforiaService.SetDeviceFlashTorch(false);
            _gameStateMachine.Enter<EnterMainMenuState>();
        }
    }
}