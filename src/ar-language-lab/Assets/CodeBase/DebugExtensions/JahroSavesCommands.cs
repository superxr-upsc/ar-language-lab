using CodeBase.Infrastructure.SaveLoad;
using CodeBase.Infrastructure.SaveLoad.AutoSaver;
using CodeBase.Infrastructure.StaticData;
using CodeBase.Infrastructure.WindowsManagement;
using Cysharp.Threading.Tasks;
using JahroConsole;
using UnityEngine.SceneManagement;

namespace CodeBase.DebugExtensions
{
    public class JahroSavesCommands : IJahroCommands
    {
        private readonly ISaveService _saveService;
        private readonly AutoSaveService _autoSaveService;
        private readonly IWindowsManagementService _windowsManagementService;

        public JahroSavesCommands(ISaveService saveService, 
            AutoSaveService autoSaveService,
            IWindowsManagementService windowsManagementService)
        {
            _saveService = saveService;
            _autoSaveService = autoSaveService;
            _windowsManagementService = windowsManagementService;
        }
        
        [JahroCommand("reset-progress", "saves", "Reset the whole player progress and reload the game.")]
        public void ResetProgress()
        {
            ResetProgressAsync()
                .Forget();
        }
        
        [JahroCommand("set-auto-save-interval", "saves", "Set the auto save interval in seconds.")]
        public void SetAutoSaveInterval(int timeInSeconds)
        {
            _autoSaveService.SetAutoSaveIntervalSeconds(timeInSeconds);
        }

        private async UniTaskVoid ResetProgressAsync()
        {
            _windowsManagementService.CloseAllWindows();
            await _saveService.ResetProgressAsync();
            var currentSceneName = Scenes.BootstrapSceneInfo.Name;
            SceneManager.LoadScene(currentSceneName);
        }
    }
}