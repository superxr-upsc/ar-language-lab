using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBase.Infrastructure.InputService
{
    public interface IInputService
    {
        bool IsInputEnabled { get; }
        InputAction TapAction { get; }
        InputAction DoubleTapAction { get; }
        InputAction PressAction { get; }
        Vector2 PointAction { get; }

        void EnableInput();
        void DisableInput();
    }
}