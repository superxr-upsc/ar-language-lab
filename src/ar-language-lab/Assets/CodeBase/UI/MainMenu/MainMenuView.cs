using CodeBase.Common.LoggerService;
using CodeBase.Gameplay.Lessons;
using CodeBase.Infrastructure.ProjectResourcesProvider;
using CodeBase.Infrastructure.WindowsManagement;
using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using CodeBase.UI.LessonsListWindow;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CodeBase.UI
{
    public class MainMenuView : ViewBase
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _someOtherButton;

        private IWindowsManagementService _windowsManagementService;
        private IProjectResourcesProvider _projectResourcesProvider;

        [Inject]
        private void Construct(IWindowsManagementService windowsManagementService, IProjectResourcesProvider projectResourcesProvider)
        {
            _windowsManagementService = windowsManagementService;
            _projectResourcesProvider = projectResourcesProvider;
            
            _playButton.onClick.AddListener(OnPlayButtonPressed);
            _settingsButton.onClick.AddListener(OnSettingsButtonPressed);
            _someOtherButton.onClick.AddListener(OnSomeOtherButtonPressed);
        }

        private void OnDestroy()
        {
            _playButton.onClick.RemoveListener(OnPlayButtonPressed);
            _settingsButton.onClick.RemoveListener(OnSettingsButtonPressed);
            _someOtherButton.onClick.RemoveListener(OnSomeOtherButtonPressed);
        }
        
        private void OnPlayButtonPressed()
        {
            var gameLessonsData = _projectResourcesProvider.LoadResource<GameLessons>();
            _windowsManagementService.CreateWindow<LessonsListPresenter, LessonsListView, GameLessons>(
                UILayer.NotificationLayer, gameLessonsData);
        }

        private void OnSettingsButtonPressed()
        {
            GameLogger.Log("Settings button pressed!");
        }

        private void OnSomeOtherButtonPressed()
        {
            GameLogger.Log("Some other button pressed!");
        }
    }
}