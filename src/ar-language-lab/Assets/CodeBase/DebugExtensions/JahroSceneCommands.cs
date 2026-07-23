using JahroConsole;
using UnityEngine.SceneManagement;

namespace CodeBase.DebugExtensions {
    public class JahroSceneCommands : IJahroCommands
    {
        [JahroCommand("reload-scene", "scene", "Reloads the current scene")]
        public void ReloadScene()
        {
            var currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        }
    }
}