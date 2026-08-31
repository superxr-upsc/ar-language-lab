using System;

namespace CodeBase.Infrastructure.Loading
{
  public interface ISceneLoader
  {
    void LoadScene(string name, Action onLoaded = null);
    void ShowLoadingScreen();
    void CloseLoadingScreen();
    void UpdateProgress(float progress, string message);
  }
}