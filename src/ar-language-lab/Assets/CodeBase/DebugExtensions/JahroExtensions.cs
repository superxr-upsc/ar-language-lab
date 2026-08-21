using CodeBase.Common.LoggerService;
using CodeBase.Infrastructure.GameFactory;
using JahroConsole;
using RSG;
using UnityEngine;
using Zenject;

namespace CodeBase.DebugExtensions
{
    public class JahroExtensions : MonoBehaviour
    {
        private IJahroCommands[] _jahroCommands;
        private IGameFactory _gameFactory;

        [Inject]
        private void Construct(IGameFactory gameFactory)
        {
            _gameFactory = gameFactory; 
        }
        
        private void InitializeJahroCommandsArray()
        {
            _jahroCommands = new IJahroCommands[]
            {
                _gameFactory.Create<JahroSceneCommands>(),
                _gameFactory.Create<JahroARObjectCommands>(),
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