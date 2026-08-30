using CodeBase.Infrastructure.SaveLoad.Data;

namespace CodeBase.Infrastructure.SaveLoad.Migration
{
    public interface ISaveMigrationRunner
    {
        bool RunMigrations<TSaveData>(TSaveData saveData) where TSaveData : class, ISaveData;
    }
}


