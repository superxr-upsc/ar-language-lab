using System;
using System.Collections;
using CodeBase.Common.LoggerService;
using CodeBase.Infrastructure.CoroutineRunner;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeBase.Infrastructure.Loading
{
  public class SceneLoader : ISceneLoader
  {
    private readonly ICoroutineRunner _coroutineRunner;

    public SceneLoader(ICoroutineRunner coroutineRunner)
    {
      _coroutineRunner = coroutineRunner;
    }

    public void LoadScene(string name, Action onLoaded = null) =>
      _coroutineRunner.RunCoroutine(Load(name, onLoaded));

    private IEnumerator Load(string nextScene, Action onLoaded)
    {
      GameLogger.Log($"Loading scene: {nextScene}");
      if (SceneManager.GetActiveScene().name == nextScene)
      {
        GameLogger.Log($"Scene {nextScene} is already loaded.");
        onLoaded?.Invoke();
        yield break;
      }

      AsyncOperation waitNextScene = SceneManager.LoadSceneAsync(nextScene);

      while (!waitNextScene.isDone)
        yield return null;

      GameLogger.Log($"Scene loaded !");
      onLoaded?.Invoke();
    }
  }
}