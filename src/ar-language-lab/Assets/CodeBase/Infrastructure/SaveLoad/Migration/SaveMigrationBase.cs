using System;
using CodeBase.Infrastructure.SaveLoad.Data;

namespace CodeBase.Infrastructure.SaveLoad.Migration
{
    public abstract class SaveMigrationBase<TSaveData> : ISaveMigration where TSaveData : class, ISaveData
    {
        public Type SaveDataType => typeof(TSaveData);

        public abstract int FromVersion { get; }
        public abstract int ToVersion { get; }

        public bool CanMigrate(ISaveData saveData)
        {
            return saveData is TSaveData typedSaveData
                && typedSaveData.Version == FromVersion
                && CanMigrate(typedSaveData);
        }

        public void Migrate(ISaveData saveData)
        {
            if (saveData is not TSaveData typedSaveData)
                throw new InvalidOperationException($"Invalid save data type for migration. Expected {typeof(TSaveData)}.");

            if (ToVersion <= FromVersion)
                throw new InvalidOperationException($"Migration {GetType().Name} has invalid version step {FromVersion}->{ToVersion}.");

            ApplyMigration(typedSaveData);
            typedSaveData.Version = ToVersion;
        }

        protected virtual bool CanMigrate(TSaveData saveData) => true;

        protected abstract void ApplyMigration(TSaveData saveData);
    }
}


