using JahroConsole;
using UnityEngine;

namespace CodeBase.DebugExtensions
{
    public class JahroExtensions : MonoBehaviour
    {
        private IJahroCommands[] _jahroCommands;

        private void InitializeJahroCommandsArray()
        {
            _jahroCommands = new IJahroCommands[]
            {
                new JahroSceneCommands(),
            };
        }

        private void Start()
        {
            Jahro.OnConsoleShow += OnJahroWindowShow;
            Jahro.OnConsoleHide += OnJahroWindowHide;

            InitializeJahroCommandsArray();

            foreach (var command in _jahroCommands) 
                Jahro.RegisterObject(command);
        }

        private void OnJahroWindowShow()
        {
            OnJahroConsoleWindowChangedVisibility(true);
        }

        private void OnJahroWindowHide()
        {
            OnJahroConsoleWindowChangedVisibility(false);
        }

        private void OnDestroy()
        {
            Jahro.OnConsoleHide -= OnJahroWindowHide;
            Jahro.OnConsoleShow -= OnJahroWindowShow;
            
            foreach (var command in _jahroCommands) 
                Jahro.UnregisterObject(command);
        }

        private void OnJahroConsoleWindowChangedVisibility(bool isOpen) => 
            Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }
}