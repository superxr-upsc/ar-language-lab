using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase.Infrastructure.SaveLoad.Data;

namespace CodeBase.Infrastructure.SaveLoad.Migration
{
    public class SaveMigrationRunner : ISaveMigrationRunner
    {
        private readonly Dictionary<Type, List<ISaveMigration>> _migrationsByType;

        public SaveMigrationRunner(ISaveMigration[] migrations = null)
        {
            if (migrations == null)
            {
                _migrationsByType = new Dictionary<Type, List<ISaveMigration>>();
            }
            else
            {
                _migrationsByType = migrations
                    .GroupBy(x => x.SaveDataType)
                    .ToDictionary(x => x.Key, x => x.OrderBy(m => m.FromVersion).ToList());
            }
        }

        public bool RunMigrations<TSaveData>(TSaveData saveData) where TSaveData : class, ISaveData
        {
            if (saveData == null)
                throw new ArgumentNullException(nameof(saveData));

            if (!_migrationsByType.TryGetValue(typeof(TSaveData), out var migrations) || migrations.Count == 0)
                return false;

            var migrated = false;
            var keepMigrating = true;

            while (keepMigrating)
            {
                keepMigrating = false;

                foreach (var migration in migrations)
                {
                    if (!migration.CanMigrate(saveData))
                        continue;

                    migration.Migrate(saveData);
                    migrated = true;
                    keepMigrating = true;
                    break;
                }
            }

            return migrated;
        }
    }
    }


