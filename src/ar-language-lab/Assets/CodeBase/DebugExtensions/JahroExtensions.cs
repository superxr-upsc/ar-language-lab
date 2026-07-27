using CodeBase.Common.LoggerService;
using JahroConsole;
using RSG;
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
            Promise.UnhandledException += LogPromiseException;
            Jahro.OnConsoleShow += OnJahroWindowShow;
            Jahro.OnConsoleHide += OnJahroWindowHide;

            InitializeJahroCommandsArray();

            foreach (var command in _jahroCommands) 
                Jahro.RegisterObject(command);
        }

        private void LogPromiseException(object sender, ExceptionEventArgs e) => 
            GameLogger.LogError($"Exception : {e.Exception} | Message : {e.Exception.Message}");

        private void OnJahroWindowShow()
        {
        }

        private void OnJahroWindowHide()
        {
        }

        private void OnDestroy()
        {
            Promise.UnhandledException -= LogPromiseException;
            Jahro.OnConsoleHide -= OnJahroWindowHide;
            Jahro.OnConsoleShow -= OnJahroWindowShow;
            
            foreach (var command in _jahroCommands) 
                Jahro.UnregisterObject(command);
        }
    }
}