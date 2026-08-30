using System;
using CodeBase.Infrastructure.SaveLoad.Data;

namespace CodeBase.Infrastructure.SaveLoad.Migration
{
    public interface ISaveMigration
    {
        Type SaveDataType { get; }
        int FromVersion { get; }
        int ToVersion { get; }
        bool CanMigrate(ISaveData saveData);
        void Migrate(ISaveData saveData);
    }
}


