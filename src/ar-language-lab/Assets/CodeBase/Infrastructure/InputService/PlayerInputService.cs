using CodeBase.Common.LoggerService;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBase.Services.InputService
{
    public class PlayerInputService : IInputService
    {
        public bool IsInputEnabled => _playerActionMapCache is { enabled: true };
        public InputAction TapAction { get; private set; }
        public InputAction DoubleTapAction { get; private set; }
        public InputAction PressAction { get; private set; }
        
        public Vector2 PointAction => _pointAction.ReadValue<Vector2>();

        private readonly InputAction _pointAction;
        private InputActionMap _playerActionMapCache;

        public PlayerInputService()
        {
            var playerActionMap = GetPlayerActionMap();
            
            TapAction = playerActionMap.FindAction(PlayerInputActionNames.TapActionName);
            DoubleTapAction = playerActionMap.FindAction(PlayerInputActionNames.DoubleTapActionName);
            PressAction = playerActionMap.FindAction(PlayerInputActionNames.PressActionName);
            _pointAction = playerActionMap.FindAction(PlayerInputActionNames.PointActionName);
        }
        
        public void EnableInput() => 
            GetPlayerActionMap().Enable();

        public void DisableInput() => 
            GetPlayerActionMap().Disable();
        
        private InputActionMap GetPlayerActionMap()
        {
            if (_playerActionMapCache != null)
                return _playerActionMapCache;
            
            var defaultActionAsset = InputSystem.actions;
            var playerActionMap = defaultActionAsset.FindActionMap(PlayerInputActionNames.PlayerActionMapName);

            if (playerActionMap != null)
            {
                _playerActionMapCache = playerActionMap;
                return _playerActionMapCache;
            }
            
            GameLogger.LogError($"There is no ActionMap with name [{PlayerInputActionNames.PlayerActionMapName}]");
            _playerActionMapCache = null;
            return null;
        } 
    }
}