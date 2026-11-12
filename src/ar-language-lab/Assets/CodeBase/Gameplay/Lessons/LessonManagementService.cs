using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CodeBase.Gameplay.ARObjects;
using CodeBase.Gameplay.Lessons.Saves;
using CodeBase.Gameplay.Lessons.Tasks;
using CodeBase.Gameplay.Lessons.Tutorial;
using CodeBase.Infrastructure.CoroutineRunner;
using CodeBase.Infrastructure.GameFactory;
using CodeBase.Infrastructure.GameStateMachineService.StateMachine;
using CodeBase.Infrastructure.GameStateMachineService.States;
using CodeBase.Infrastructure.Localization;
using CodeBase.Infrastructure.ProjectResourcesProvider;
using CodeBase.Infrastructure.SaveLoad;
using CodeBase.Infrastructure.StaticData;
using CodeBase.Infrastructure.Vuforia;
using CodeBase.Infrastructure.WindowsManagement;
using CodeBase.UI.LessonCompleteWindow;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Vuforia;

namespace CodeBase.Gameplay.Lessons
{
    public class LessonManagementService : ILessonManagementService, IDisposable
    {
        private readonly IGameFactory _gameFactory;
        private readonly IProjectResourcesProvider _resourcesProvider;
        private readonly IARCameraProvider _arCameraProvider;
        private readonly ILocalizationService _localizationService;
        private readonly IVuforiaService _vuforiaService;
        private readonly ISaveService _saveService;
        private readonly IGameStateMachine _gameStateMachine;
        private readonly IWindowsManagementService _windowsManagementService;
        private readonly ICoroutineRunner _coroutineRunner;

        private Perspective _perspective = Perspective.None;
        private LessonConfig _lessonConfig;
        private LessonsGameDataProvider _lessonGameDataProvider;

        private Dictionary<MultiTargetBehaviour, ARObjectBase> _lessonObjects = new();

        private LessonTasksService _lessonTasksService;
        private ARTutorialService _tutorialService;
        private CancellationTokenSource _lessonCts;
        private Coroutine _previewObjectsCoroutine;

        public LessonManagementService(IGameFactory gameFactory,
            IProjectResourcesProvider resourcesProvider,
            IARCameraProvider arCameraProvider,
            ILocalizationService localizationService,
            IVuforiaService vuforiaService,
            ISaveService saveService,
            IGameStateMachine gameStateMachine,
            IWindowsManagementService windowsManagementService,
            ICoroutineRunner coroutineRunner)
        {
            _gameFactory = gameFactory;
            _resourcesProvider = resourcesProvider;
            _arCameraProvider = arCameraProvider;
            _localizationService = localizationService;
            _coroutineRunner = coroutineRunner;
            _vuforiaService = vuforiaService;
            _saveService = saveService;
            _gameStateMachine = gameStateMachine;
            _windowsManagementService = windowsManagementService;
        }

        public void SetPerspective(Perspective perspective) => 
            _perspective = perspective;
        
        public Perspective GetPerspective() => 
            _perspective;

        public async UniTask SetupLesson()
        {
            ResetLessonCancellationToken();
            var cancellationToken = _lessonCts.Token;

            _lessonGameDataProvider = new LessonsGameDataProvider(_saveService, _resourcesProvider);
            _lessonConfig = GetSelectedLesson();

            await SetupGameplayObjectsAsync(cancellationToken);
            await SetupQuestsAsync(cancellationToken);
        }

        public void StartAudioLesson()
        {
            if (_lessonTasksService == null || (_lessonCts != null && _lessonCts.IsCancellationRequested))
                return;

            StopPreviewObjectsCoroutine();

            if (IsTutorialNeeded())
            {
                RunTutorialThen(() =>
                {
                    _previewObjectsCoroutine = _coroutineRunner.RunCoroutine(PreviewCurrentObjects());
                }).Forget();
            }
            else
            {
                _previewObjectsCoroutine = _coroutineRunner.RunCoroutine(PreviewCurrentObjects());
            }
        }
        
        public void StartLesson()
        {
            if (_lessonTasksService == null || (_lessonCts != null && _lessonCts.IsCancellationRequested))
                return;

            if (IsTutorialNeeded())
            {
                RunTutorialThen(StartRegularQuests).Forget();
            }
            else
            {
                StartRegularQuests();
            }
        }

        private bool IsTutorialNeeded() =>
            !_saveService.SaveData.Lessons.TutorialCompleted;

        private async UniTaskVoid RunTutorialThen(Action onComplete)
        {
            _tutorialService = _gameFactory.Create<ARTutorialService>(_lessonObjects.Values.ToList(), _perspective);

            try
            {
                await _tutorialService.RunTutorialAsync(_lessonCts?.Token ?? CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            finally
            {
                _tutorialService?.Dispose();
                _tutorialService = null;
            }

            if (_lessonCts == null || _lessonCts.IsCancellationRequested)
                return;

            onComplete?.Invoke();
        }

        private void StartRegularQuests()
        {
            if (_lessonTasksService == null || (_lessonCts != null && _lessonCts.IsCancellationRequested))
                return;

            foreach (var lessonObject in _lessonObjects)
            {
                lessonObject.Value.SubscribeToNotifyEvents();
            }
            
            _lessonTasksService.OnLessonComplete += OnLessonComplete;
            _lessonTasksService.SelectAndRunNewTask();
            _lessonTasksService.StartBackgroundTasksCaching(_lessonCts?.Token ?? CancellationToken.None);
        }

        public ARObjectBase GetObject(ARObjectConfig config)
        {
            foreach (var arObject in _lessonObjects.Values)
            {
                if (arObject.IsEqualTo(config))
                    return arObject;
            }

            return null;
        }

        public void CleanupLesson()
        {
            CancelLessonCancellationToken();
            StopPreviewObjectsCoroutine();
            _arCameraProvider.GetSpeaker()?.Stop();

            _tutorialService?.Dispose();
            _tutorialService = null;

            if (_lessonTasksService != null)
            {
                _lessonTasksService.OnLessonComplete -= OnLessonComplete;
                _lessonTasksService.Dispose();
                _lessonTasksService = null;
            }
            
            foreach (var arObject in _lessonObjects) 
                arObject.Value.Cleanup();
            
            _lessonObjects.Clear();
        }

        public void LoadMainMenu()
        {
            _windowsManagementService.CloseAllWindows();
            _gameStateMachine.Enter<EnterMainMenuState>();
        }

        public void Dispose()
        {
            CleanupLesson();
        }

        private IEnumerator PreviewCurrentObjects()
        {
            while (!AreAllObjectsInView())
            {
                yield return null;
            }

            var orderedObjects = GetObjectsOrderedLeftToRight();

            foreach (var arObject in orderedObjects)
            {
                if (arObject == null)
                    continue;

                arObject.SetOutlineState(true);
                var clipDuration = arObject.PlayNameAudio();

                if (clipDuration > 0f)
                    yield return new WaitForSeconds(clipDuration + 0.5f);

                arObject.SetOutlineState(false);
            }

            _previewObjectsCoroutine = null;
            StartRegularQuests();
        }

        private bool AreAllObjectsInView()
        {
            if (_lessonObjects.Count == 0)
                return false;

            foreach (var arObject in _lessonObjects.Values)
            {
                if (arObject == null || !arObject.Observer.IsInView())
                    return false;
            }

            return true;
        }

        private List<ARObjectBase> GetObjectsOrderedLeftToRight()
        {
            var activeCamera = _arCameraProvider.GetActiveCamera();
            var orderedObjects = new List<ARObjectBase>(_lessonObjects.Values);

            orderedObjects.Sort((left, right) =>
            {
                var leftX = GetViewportHorizontalPosition(left.transform.position, activeCamera);
                var rightX = GetViewportHorizontalPosition(right.transform.position, activeCamera);
                return leftX.CompareTo(rightX);
            });

            return orderedObjects;
        }

        private static float GetViewportHorizontalPosition(Vector3 worldPosition, Camera camera)
        {
            if (camera == null)
                return worldPosition.x;

            return camera.WorldToViewportPoint(worldPosition).x;
        }

        private void StopPreviewObjectsCoroutine()
        {
            if (_previewObjectsCoroutine == null)
                return;

            _coroutineRunner.EndCoroutine(_previewObjectsCoroutine);
            _previewObjectsCoroutine = null;
        }
        
        private LessonConfig GetSelectedLesson()
        {
            var gameLessons = _resourcesProvider.LoadResource<GameLessons>();
            return gameLessons.GetLesson(_saveService.SaveData.Lessons.SelectedLessonID);
        }

        private void OnLessonComplete()
        {
            CancelLessonCancellationToken();

            _vuforiaService.SetVuforiaState(false);
            _vuforiaService.SetDeviceFlashTorch(false);
            
            _windowsManagementService.CloseAllWindows();
            _windowsManagementService
                .CreateWindow<LessonCompletePresenter, LessonCompleteView, LessonCompleteData>
                    (UILayer.NotificationLayer, new LessonCompleteData());
        }

        private async UniTask SetupGameplayObjectsAsync(CancellationToken cancellationToken)
        {
            var createdObjects = new List<ARObjectBase>(_lessonConfig.ObjectsToUse.Length);

            for (var index = 0; index < _lessonConfig.ObjectsToUse.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var arObjectConfig = _lessonConfig.ObjectsToUse[index];
                var arObject = CreateArObject(arObjectConfig, index);
                if (arObject != null)
                    createdObjects.Add(arObject);
            }

            foreach (var arObject in createdObjects)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await arObject.CacheLocalizationAsync(cancellationToken);
            }
        }

        private async UniTask SetupQuestsAsync(CancellationToken cancellationToken)
        {
            _lessonTasksService = _gameFactory.Create<LessonTasksService>(_lessonConfig, _lessonGameDataProvider, _perspective);
            await _lessonTasksService.PreloadInitialTasksAsync(cancellationToken);
        }

        private ARObjectBase CreateArObject(ARObjectConfig arObjectConfig, int index)
        {
            var markerDatabaseName = MarkerNames.GetNameAtIndex(index);
            if (string.IsNullOrEmpty(markerDatabaseName))
                return null;
            
            var target = _vuforiaService.CreateTarget(markerDatabaseName);
            var arObject = _gameFactory.CreateFromPrefab<ARObjectBase>(arObjectConfig.Prefab, target.transform);
            arObject.Initialize(arObjectConfig, 
                _windowsManagementService,
                _localizationService, 
                target.GetComponent<ARObjectObserver>(),
                _arCameraProvider.GetSpeaker(),
                _perspective);
            
            _lessonObjects[target] = arObject;

            if (_perspective == Perspective.Audio)
                arObject.HideObjectViewForAudioPerspective();
            
            return arObject;
        }

        private void ResetLessonCancellationToken()
        {
            CancelLessonCancellationToken();
            _lessonCts = new CancellationTokenSource();
        }

        private void CancelLessonCancellationToken()
        {
            if (_lessonCts == null)
                return;

            _lessonCts.Cancel();
            _lessonCts.Dispose();
            _lessonCts = null;
        }
    }
}
