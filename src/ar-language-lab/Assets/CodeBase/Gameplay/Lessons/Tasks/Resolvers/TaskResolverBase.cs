using System;
using System.Threading;
using CodeBase.Gameplay.SpeechSyntesis;
using CodeBase.Infrastructure.Localization;
using CodeBase.Infrastructure.Vuforia;
using CodeBase.UI.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Gameplay.Lessons.Tasks.Resolvers
{
    public class TaskResolverBase : IDisposable
    {
        protected readonly TaskData _taskData;
        private readonly Speaker _speaker;
        private Perspective _perspective;
        
        private string _cachedDescription;
        private AudioClip _cachedAudioClip;

        private bool _targetsResolved;
        private bool _isTargetValid;
        private bool _isLocalizationCached;
        private UniTaskCompletionSource _cachingCompletionSource;
        private CancellationTokenSource _runCts;

        protected TaskResolverBase(TaskData taskData, IARCameraProvider cameraProvider, Perspective perspective = Perspective.None)
        {
            _taskData = taskData;
            _speaker = cameraProvider.GetSpeaker();
            _perspective = perspective;
        }

        public event Action<TaskData> TaskCompleted;

        public virtual void Run(ActiveTaskData viewData)
        {
            EnsureTargetsResolved();

            viewData.TaskDescription = _cachedDescription;
            viewData.TaskAudioClip = _cachedAudioClip;
            
            if (_cachedAudioClip != null)
            {
                if (_perspective is Perspective.Audio or Perspective.Mix)
                    _speaker.Speak(_cachedAudioClip);
            }
            else
            {
                CancelRunCts();
                _runCts = new CancellationTokenSource();
                ApplyAudioWhenReadyAsync(viewData, _runCts.Token).Forget();
            }
        }

        public virtual async UniTask<string> GetQuestDescription(string locale = LocalizationConsts.DefaultLocaleCode) => 
            string.Empty;

        public async UniTask PrepareTaskDescriptionAsync(CancellationToken cancellationToken = default)
        {
            if (!EnsureTargetsResolved())
                return;

            if (string.IsNullOrEmpty(_cachedDescription))
            {
                cancellationToken.ThrowIfCancellationRequested();
                _cachedDescription = await GetQuestDescription();
            }
        }

        public async UniTask CacheLocalizationData(CancellationToken cancellationToken = default)
        {
            if (_isLocalizationCached)
                return;

            if (_cachingCompletionSource != null)
            {
                await _cachingCompletionSource.Task.AttachExternalCancellation(cancellationToken);
                return;
            }

            if (!EnsureTargetsResolved())
                return;

            var completionSource = new UniTaskCompletionSource();
            _cachingCompletionSource = completionSource;

            try
            {
                await PrepareTaskDescriptionAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                if (_cachedAudioClip == null)
                    _cachedAudioClip = await _speaker.GenerateAudioClipAsync(_cachedDescription, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                _isLocalizationCached = _cachedAudioClip != null;
                completionSource.TrySetResult();
            }
            catch (OperationCanceledException)
            {
                completionSource.TrySetCanceled(cancellationToken);
                throw;
            }
            catch (Exception exception)
            {
                completionSource.TrySetException(exception);
                throw;
            }
            finally
            {
                if (_cachingCompletionSource == completionSource)
                    _cachingCompletionSource = null;
            }
        }

        public virtual void Dispose()
        {
            CancelRunCts();
        }

        protected virtual void CompleteTask()
        {
            CancelRunCts();
            TaskCompleted?.Invoke(_taskData);
        }

        protected virtual bool TryResolveTargets() => 
            false;

        private bool EnsureTargetsResolved()
        {
            if (_targetsResolved)
                return _isTargetValid;

            _targetsResolved = true;
            _isTargetValid = TryResolveTargets();
            return _isTargetValid;
        }

        private async UniTaskVoid ApplyAudioWhenReadyAsync(ActiveTaskData viewData, CancellationToken cancellationToken)
        {
            try
            {
                await CacheLocalizationData(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                viewData.TaskDescription = _cachedDescription;
                viewData.TaskAudioClip = _cachedAudioClip;

                if (_cachedAudioClip != null && _perspective is Perspective.Audio or Perspective.Mix)
                    _speaker.Speak(_cachedAudioClip);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void CancelRunCts()
        {
            if (_runCts == null)
                return;

            _runCts.Cancel();
            _runCts.Dispose();
            _runCts = null;
        }
    }
}
