using System;
using System.Threading;
using CodeBase.Common.LoggerService;
using CodeBase.Infrastructure.SaveLoad.Data;
using CodeBase.Infrastructure.SaveLoad.Migration;
using CodeBase.Infrastructure.SaveLoad.Serialization;
using CodeBase.Infrastructure.SaveLoad.Storage;
using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.SaveLoad
{
    public class SaveService : ISaveService
    {
        public ISaveData SaveData => _saveData;

        private readonly ISaveStorage _saveStorage;
        private readonly ISaveSerializer _saveSerializer;
        private readonly ISaveMigrationRunner _saveMigrationRunner;
        private readonly SemaphoreSlim _operationLock = new(1, 1);

        private ISaveData _saveData;
        private bool _isDirty = false;
        
        public SaveService()
        {
            _saveStorage = new FileSaveStorage();
            _saveSerializer = new NewtonsoftSaveSerializer();
            _saveMigrationRunner = new SaveMigrationRunner();
        }

        public async UniTask LoadAsync<TSaveData>()
            where TSaveData : class, ISaveData, new()
        {
            if (!await _saveStorage.ExistsAsync())
                _saveData = new TSaveData();

            var payload = await _saveStorage.ReadAsync();
            if (string.IsNullOrWhiteSpace(payload))
                _saveData = new TSaveData();

            var saveData = _saveSerializer.Deserialize<TSaveData>(payload) ?? new TSaveData();
            var migrated = _saveMigrationRunner.RunMigrations(saveData);

            if (migrated)
                MarkDirty();

            _saveData = saveData;
            
            GameLogger.Log("[SAVES] Game state loaded!");
        }

        public async UniTask SaveAsync()
        {
            await _operationLock.WaitAsync();

            try
            {
                if (_saveData == null)
                    throw new ArgumentNullException(nameof(_saveData));

                var payload = _saveSerializer.Serialize(_saveData);
                await _saveStorage.WriteAsync(payload);
                MarkClean();
                
                GameLogger.Log("[SAVES] Game state saved!");
            }
            finally
            {
                _operationLock.Release();
            }
        }

        public async UniTask<bool> TrySaveIfDirtyAsync()
        {
            if (!IsDirty())
                return false;

            await SaveAsync();
            return true;
        }

        public UniTask ResetProgressAsync()
        {
            _saveData = new SaveData();
            MarkDirty();
            
            return SaveAsync();
        }

        public void MarkDirty() =>
            _isDirty = true;

        public void MarkClean() =>
            _isDirty = false;

        public bool IsDirty() =>
            _isDirty;
    }
}