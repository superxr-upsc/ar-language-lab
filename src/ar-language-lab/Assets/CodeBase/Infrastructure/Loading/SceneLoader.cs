using System;
using System.Collections;
using CodeBase.Infrastructure.CoroutineRunner;
using CodeBase.Infrastructure.WindowsManagement;
using CodeBase.UI.LoadingScreen;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeBase.Infrastructure.Loading
{
  public class SceneLoader : ISceneLoader
  {
    private readonly ICoroutineRunner _coroutineRunner;
    private readonly IWindowsManagementService _windowsManagementService;

    private LoadingScreenPresenter  _loadingScreenPresenter;
    private LoadingScreenData _loadingScreenData;
    
    public SceneLoader(ICoroutineRunner coroutineRunner, IWindowsManagementService windowsManagementService)
    {
      _coroutineRunner = coroutineRunner;
      _windowsManagementService = windowsManagementService;
    }

    public void LoadScene(string name, Action onLoaded = null)
    {
      _coroutineRunner.RunCoroutine(Load(name, onLoaded));
    }

    public void ShowLoadingScreen()
    {
      _loadingScreenData = new LoadingScreenData();
      _loadingScreenPresenter = _windowsManagementService.CreateWindow<LoadingScreenPresenter, LoadingScreenView, LoadingScreenData>(UILayer.MainLayer, _loadingScreenData);
    }
    
    public void CloseLoadingScreen() => 
      _loadingScreenPresenter?.DisposeAsync()
        .Catch(Debug.LogError);
    
    public void UpdateProgress(float progress, string message)
    {
      _loadingScreenData?.SetCurrentProgress(progress);
      _loadingScreenData?.SetCurrentProgress(message);
    }

    private IEnumerator Load(string nextScene, Action onLoaded)
    {
      if (SceneManager.GetActiveScene().name == nextScene)
      {
        onLoaded?.Invoke();
        yield break;
      }

      AsyncOperation waitNextScene = SceneManager.LoadSceneAsync(nextScene);

      while (!waitNextScene.isDone)
        yield return null;
      
      onLoaded?.Invoke();
    }
  }
}