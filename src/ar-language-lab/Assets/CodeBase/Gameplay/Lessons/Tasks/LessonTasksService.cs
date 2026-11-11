using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CodeBase.Gameplay.Lessons.Saves;
using CodeBase.Gameplay.Lessons.Tasks.Extensions;
using CodeBase.Gameplay.Lessons.Tasks.Resolvers;
using CodeBase.Infrastructure.CoroutineRunner;
using CodeBase.Infrastructure.GameFactory;
using CodeBase.Infrastructure.Localization;
using CodeBase.Infrastructure.SaveLoad;
using CodeBase.Infrastructure.WindowsManagement;
using CodeBase.UI.Tasks;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace CodeBase.Gameplay.Lessons.Tasks
{
    public class LessonTasksService : IDisposable
    {
        private const int InitialPreloadTasksCount = 2;

        public event Action OnLessonComplete;
        
        private readonly LessonConfig _lessonConfig;
        private readonly IGameFactory _gameFactory;
        private readonly LessonsGameDataProvider _lessonGameDataProvider;
        private readonly IWindowsManagementService _windowsManagementService;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly ISaveService _saveService;
        private readonly Perspective _perspective;
        private readonly ILocalizationService _localizationService;

        private Queue<TaskResolverBase> _taskList = new();
        private TaskResolverBase _currentTask;

        private ActiveTaskData _activeTaskViewData;
        private ActiveTaskPresenter _activeTaskPresenter;

        private CancellationTokenSource _backgroundCachingCts;
        private Coroutine _nextTaskDelayCoroutine;

        public LessonTasksService(LessonConfig lessonConfig,
            IGameFactory gameFactory, 
            LessonsGameDataProvider lessonGameDataProvider,
            IWindowsManagementService windowsManagementService,
            ICoroutineRunner coroutineRunner,
            ISaveService saveService,
            Perspective perspective,
            ILocalizationService localizationService)
        {
            _lessonConfig = lessonConfig;
            _gameFactory = gameFactory;
            _lessonGameDataProvider = lessonGameDataProvider;
            _windowsManagementService = windowsManagementService;
            _localizationService = localizationService;
            _coroutineRunner = coroutineRunner;
            _saveService = saveService;
            _perspective = perspective;
            _activeTaskViewData = new ActiveTaskData();

            BuildTasksQuery();
        }

        public void SelectAndRunNewTask()
        {
            if (_taskList.Count == 0)
            {
                _lessonGameDataProvider.SaveCompletedLesson(_lessonConfig.Id);
                OnLessonComplete?.Invoke();
                return;
            }

            _currentTask = _taskList.Dequeue();

            _currentTask.TaskCompleted += OnTaskCompleted;
            _currentTask.Run(_activeTaskViewData); 
            
            CreateTaskView();
            _activeTaskViewData.SetCurrentProgress(_lessonConfig.GetCompletedTasksValue(_lessonGameDataProvider.GetLastCompletedTaskId(_lessonConfig.Id)));
        }

        public async UniTask PreloadInitialTasksAsync(CancellationToken cancellationToken = default)
        {
            var allTasks = _taskList.ToArray();

            foreach (var task in allTasks)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await task.PrepareTaskDescriptionAsync(cancellationToken);
            }

            var initialTasks = allTasks.Take(InitialPreloadTasksCount);

            foreach (var task in initialTasks)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await task.CacheLocalizationData(cancellationToken);
            }
        }

        public void StartBackgroundTasksCaching(CancellationToken cancellationToken = default)
        {
            CancelBackgroundCaching();
            _backgroundCachingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var remainingTasks = _taskList.ToArray();
            CacheRemainingTasksInBackgroundAsync(remainingTasks, _backgroundCachingCts.Token)
                .Forget();
        }

        public void Dispose()
        {
            CancelBackgroundCaching();

            if (_nextTaskDelayCoroutine != null)
            {
                _coroutineRunner.EndCoroutine(_nextTaskDelayCoroutine);
                _nextTaskDelayCoroutine = null;
            }

            if (_currentTask != null)
            {
                _currentTask.TaskCompleted -= OnTaskCompleted;
                _currentTask.Dispose();
                _currentTask = null;
            }
            
            foreach (var taskResolver in _taskList) 
                taskResolver.Dispose();
            
            _taskList.Clear();
        }

        private async UniTaskVoid CacheRemainingTasksInBackgroundAsync(TaskResolverBase[] tasks, CancellationToken cancellationToken)
        {
            try
            {
                foreach (var task in tasks)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await task.CacheLocalizationData(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void CancelBackgroundCaching()
        {
            if (_backgroundCachingCts == null)
                return;

            _backgroundCachingCts.Cancel();
            _backgroundCachingCts.Dispose();
            _backgroundCachingCts = null;
        }

        private void BuildTasksQuery()
        {
            var tasks = _lessonConfig.Tasks;
            var lastCompletedTask = _lessonGameDataProvider.GetLastCompletedTaskId(_lessonConfig.Id);

            if (!string.IsNullOrEmpty(lastCompletedTask))
            {
                var tasksToRun = tasks
                        .SkipWhile(task => task.Id != lastCompletedTask)
                        .Skip(1)
                        .ToArray();
            
                _taskList = new Queue<TaskResolverBase>(tasksToRun.ToResolvers(_gameFactory, _perspective));    
            }
            else
            {
                _taskList = new Queue<TaskResolverBase>(tasks.ToResolvers(_gameFactory, _perspective));    
            }
        }

        private void OnTaskCompleted(TaskData taskData)
        {
            _currentTask.TaskCompleted -= OnTaskCompleted;
            _lessonGameDataProvider.SaveCompletedTask(_lessonConfig.Id, taskData.Id);

            _nextTaskDelayCoroutine = _coroutineRunner.RunCoroutine(DelayBeforeSelectNextTask());
        }

        private IEnumerator DelayBeforeSelectNextTask()
        {
            yield return new WaitForSeconds(1f);
            _nextTaskDelayCoroutine = null;
            CloseTaskView();
            SelectAndRunNewTask();
        }

        private void CreateTaskView()
        {
            _activeTaskPresenter = _windowsManagementService
                .CreateWindow<ActiveTaskPresenter, ActiveTaskView, ActiveTaskData>(UILayer.NotificationLayer, _activeTaskViewData);
            
            _activeTaskPresenter.Translated
                .Subscribe(OnTaskTranslated)
                .AddTo(_activeTaskPresenter.Disposable);
        }

        private void OnTaskTranslated(bool isTranslated)
        {
            TranslateAsync(isTranslated)
                .Forget();
        }

        private async UniTaskVoid TranslateAsync(bool isTranslated)
        {
            var translation = string.Empty;

            if (isTranslated)
            {
                if (_perspective == Perspective.Audio)
                {
                    var taskCount =
                        _lessonConfig.GetCompletedTasksCount(
                            _lessonGameDataProvider.GetLastCompletedTaskId(_lessonConfig.Id)) + 1;
                    translation = await _localizationService.GetStringAsync("ind_audiotask_description",
                        _saveService.SaveData.Settings.Language, LocalizationConsts.DefaultStringTableName,
                        new { taskNumber = taskCount });
                }
                else
                {
                    translation = await _currentTask.GetQuestDescription(_saveService.SaveData.Settings.Language);
                }
            }
            else
            {
                if (_perspective == Perspective.Audio)
                {
                    var taskCount =
                        _lessonConfig.GetCompletedTasksCount(
                            _lessonGameDataProvider.GetLastCompletedTaskId(_lessonConfig.Id)) + 1;
                    translation = await _localizationService.GetStringAsync("ind_audiotask_description",
                        LocalizationConsts.DefaultLocaleCode, LocalizationConsts.DefaultStringTableName,
                        new { taskNumber = taskCount });
                }
                else
                {
                    translation = await _currentTask.GetQuestDescription();
                }
            }
            
            
            _activeTaskPresenter.UpdateTaskDescription(translation);
        }
        
        private void CloseTaskView()
        {
            _activeTaskViewData.Cleanup();
            _activeTaskPresenter?.Close()
                .Catch(UnityEngine.Debug.LogException);
        }
    }
}
