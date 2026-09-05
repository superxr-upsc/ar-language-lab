using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;

namespace CodeBase.Infrastructure.SaveLoad.AutoSaver
{
    public class AutoSaveService : IInitializable, IDisposable
    {
        private readonly ISaveService _saveService;

        private CancellationTokenSource _cts;
        private int _autoSaveIntervalSeconds = 60;

        public AutoSaveService(ISaveService saveService)
        {
            _saveService = saveService;
        }

        public void Initialize()
        {
            _cts = new CancellationTokenSource();
        }

        public void StartSaving()
        {
            AutoSaveLoopAsync(_cts.Token)
                .Forget();
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public void SetAutoSaveIntervalSeconds(int intervalSeconds)
        {
            _autoSaveIntervalSeconds = intervalSeconds;
            
            _cts?.Cancel();
            StartSaving();
        }
        
        private async UniTaskVoid AutoSaveLoopAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(_autoSaveIntervalSeconds),
                        cancellationToken: cancellationToken);

                    await _saveService.TrySaveIfDirtyAsync();
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
