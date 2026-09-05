using System.Threading.Tasks;
using CodeBase.Infrastructure.SaveLoad.Data;
using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.SaveLoad
{
    public interface ISaveService
    {
        ISaveData SaveData { get; }
        UniTask LoadAsync<TSaveData>()
            where TSaveData : class, ISaveData, new();

        UniTask SaveAsync();

        UniTask<bool> TrySaveIfDirtyAsync();

        UniTask ResetProgressAsync();
        
        void MarkDirty();
        void MarkClean();
        bool IsDirty();
    }
}