using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace CodeBase.Services.TimerService {
    public class GameTimer : IGameTimer, IDisposable {
        public float Duration { get; }
        public float Elapsed => _elapsed;
        public float Progress => Mathf.Clamp01(_elapsed / Duration);

        public ReadOnlyReactiveProperty<float> ProgressObservable => _progress;
        public ReadOnlyReactiveProperty<bool> IsRunning => _isRunning;
        public ReadOnlyReactiveProperty<bool> IsPaused => _isPaused;
        public ReadOnlyReactiveProperty<bool> IsCompleted => _isCompleted;

        public UniTask Completed => _completionSource.Task;

        private float _elapsed;

        private readonly ReactiveProperty<float> _progress = new(0f);
        private readonly ReactiveProperty<bool> _isRunning = new(false);
        private readonly ReactiveProperty<bool> _isPaused = new(false);
        private readonly ReactiveProperty<bool> _isCompleted = new(false);

        private readonly CompositeDisposable _disposables = new();
        private readonly UniTaskCompletionSource _completionSource = new();

        public GameTimer(float duration)
        {
            Duration = duration;
        }

        public void Start()
        {
            if (_isRunning.Value || _isCompleted.Value)
                return;

            _isRunning.Value = true;
            _isPaused.Value = false;
            
            
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    if (_isPaused.Value)
                        return;

                    _elapsed += Time.deltaTime;
                    _progress.Value = Progress;

                    if (_elapsed >= Duration)
                        Finish();
                })
                .AddTo(_disposables);
        }

        public void Pause()
        {
            if (!_isRunning.Value || _isCompleted.Value)
                return;

            _isPaused.Value = true;
        }

        public void Resume()
        {
            if (!_isRunning.Value || _isCompleted.Value)
                return;

            _isPaused.Value = false;
        }

        private void Finish()
        {
            _elapsed = Duration;
            _progress.Value = 1f;

            _isRunning.Value = false;
            _isPaused.Value = false;
            _isCompleted.Value = true;

            _completionSource.TrySetResult();
            Dispose();
        }

        public void Stop()
        {
            if (_isCompleted.Value)
                return;

            _isRunning.Value = false;
            _isPaused.Value = false;

            Dispose();
        }

        public void Dispose()
        {
            _disposables.Clear();
        }
    }
}