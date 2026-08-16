using System.Collections;
using CodeBase.Common.LoggerService;
using CodeBase.Gameplay.SpeechSyntesis;
using CodeBase.Infrastructure.EventBroker;
using CodeBase.Infrastructure.EventBroker.Handlers;
using CodeBase.Services.InputService;
using UnityEngine;
using UnityEngine.InputSystem;
using Vuforia;
using Zenject;

namespace CodeBase.Infrastructure.Vuforia
{
    public class ARCamera : MonoBehaviour, IGameLoopInitializable
    {
        public Camera Camera => _camera;
        public Speaker Speaker => _speaker;
        public bool IsFlashTorchEnabled => _isFlashTorchEnabled;
        public bool IsAutofocusEnabled => _isAutofocusEnabled;
        
        [SerializeField] private Camera _camera;
        [SerializeField] private Speaker _speaker;

        private IInputService _inputService;
        private IEventBrokerService _eventBrokerService;
        private IVuforiaService _vuforiaService;

        private bool _isAutofocusEnabled = true;
        private bool _isFlashTorchEnabled = false;
        private bool _isFocusing = false;

        [Inject]
        private void Construct(IInputService inputService, 
            IEventBrokerService eventBrokerService,
            IVuforiaService vuforiaService)
        {
            _inputService = inputService;
            _eventBrokerService = eventBrokerService;
            _vuforiaService = vuforiaService;

            _inputService.DoubleTapAction.performed += OnDoubleTap;
            
            _eventBrokerService.Subscribe(this);
        }

        private void OnDestroy()
        {
            _inputService.DoubleTapAction.performed -= OnDoubleTap;
            _eventBrokerService.Unsubscribe(this);
        }

        public void OnGameLoopInitialized()
        {
            //TODO : Apply loading from settings data
            SwitchAutofocus(true);
            SwitchFlashTorch(false);
        }
        
        public void OnDoubleTap(InputAction.CallbackContext context)
        {
            if (!context.performed || _isFocusing)
                return;

            _isFocusing = true;
            TriggerAutofocusEvent();
        }

        public void SwitchFlashTorch(bool ON)
        {
            if (_vuforiaService.SetDeviceFlashTorch(ON))
            {
                GameLogger.Log("Successfully turned flash " + ON);
                _isFlashTorchEnabled = ON;
            }
            else
            {
                GameLogger.Log("Failed to set the flash torch " + ON);
                _isFlashTorchEnabled = false;
            }
        }

        public void SwitchAutofocus(bool ON)
        {
            if (ON)
            {
                if (_vuforiaService.SetDeviceFocusMode(FocusMode.FOCUS_MODE_CONTINUOUSAUTO))
                {
                    GameLogger.Log("Successfully enabled continuous autofocus.");
                    _isAutofocusEnabled = true;
                }
                else
                {
                    // Fallback to fixed focus mode
                    GameLogger.Log("Failed to enable continuous autofocus, switching to fixed focus mode");
                    _isAutofocusEnabled = false;
                    _vuforiaService.SetDeviceFocusMode(FocusMode.FOCUS_MODE_FIXED);
                }
            }
            else
            {
                GameLogger.Log("Disabling continuous autofocus (enabling fixed focus mode).");
                _isAutofocusEnabled = false;
                _vuforiaService.SetDeviceFocusMode(FocusMode.FOCUS_MODE_FIXED);
            }
        }

        public void TriggerAutofocusEvent()
        {
            _vuforiaService.SetDeviceFocusMode(FocusMode.FOCUS_MODE_TRIGGERAUTO);
            StartCoroutine(RestoreOriginalFocusMode());
        }

        private IEnumerator RestoreOriginalFocusMode()
        {
            // Wait 1.5 seconds
            yield return new WaitForSeconds(1.5f);
            
            _vuforiaService.SetDeviceFocusMode(_isAutofocusEnabled
                ? FocusMode.FOCUS_MODE_CONTINUOUSAUTO
                : FocusMode.FOCUS_MODE_FIXED);
            
            _isFocusing = false;
        }
    }
}