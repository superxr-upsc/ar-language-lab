using CodeBase.Infrastructure.StaticData;
using JahroConsole;
using UnityEngine.SceneManagement;

namespace CodeBase.DebugExtensions {
    public class JahroSceneCommands : IJahroCommands
    {
        [JahroCommand("restart", "scene", "Restarts the game from bootstrap scene")]
        public void Restart()
        {
            var currentSceneName = Scenes.BootstrapSceneInfo.Name;
            SceneManager.LoadScene(currentSceneName);
        }
        
        [JahroCommand("reload-scene", "scene", "Reloads the current scene")]
        public void ReloadScene()
        {
            var currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        }
    }
}