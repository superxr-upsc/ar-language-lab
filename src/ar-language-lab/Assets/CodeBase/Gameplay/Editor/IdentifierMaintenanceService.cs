using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase.Gameplay.ARObjects;
using CodeBase.Gameplay.Identifiers;
using CodeBase.Gameplay.Lessons;
using UnityEditor;

namespace CodeBase.Gameplay.Editor
{
    public static class IdentifierMaintenanceService
    {
        private static bool _isProcessing = false;
        private static bool _reconcileQueued = false;

        [InitializeOnLoadMethod]
        private static void Initialize() => 
            EditorApplication.delayCall += () => ReconcileAllIds(saveAssets: false);

        [MenuItem("Tools/AR Language Lab/Reconcile Content IDs")]
        public static void ReconcileFromMenu() => 
            ReconcileAllIds(saveAssets: true);

        public static void ReconcileAllIds(bool saveAssets)
        {
            if (_isProcessing)
                return;

            _isProcessing = true;
            try
            {
                var hasChanges = false;
                hasChanges |= ReconcileLessonConfigs();
                hasChanges |= ReconcileObjectConfigs();

                if (!hasChanges)
                    return;

                if (saveAssets)
                    AssetDatabase.SaveAssets();
            }
            finally
            {
                _isProcessing = false;
            }
        }

        public static void QueueReconcile()
        {
            if (_reconcileQueued)
                return;

            _reconcileQueued = true;
            EditorApplication.delayCall += () =>
            {
                _reconcileQueued = false;
                ReconcileAllIds(saveAssets: false);
            };
        }

        private static bool ReconcileLessonConfigs()
        {
            var hasChanges = false;
            var usedLessonIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (var lessonConfig in LoadAssetsInStableOrder<LessonConfig>())
            {
                if (lessonConfig == null)
                    continue;

                var isDirty = false;

                if (!IdentifierUtility.HasPrefix(lessonConfig.Id, IdentifierUtility.LessonConfigPrefix) ||
                    !usedLessonIds.Add(lessonConfig.Id))
                {
                    lessonConfig.Id = NextUniqueId(IdentifierUtility.LessonConfigPrefix, usedLessonIds);
                    isDirty = true;
                }

                var tasks = lessonConfig.Tasks;
                if (tasks != null && tasks.Length > 0)
                {
                    var usedTaskIds = new HashSet<string>(StringComparer.Ordinal);
                    for (int i = 0; i < tasks.Length; i++)
                    {
                        var task = tasks[i];
                        var validAndUnique = IdentifierUtility.HasPrefix(task.Id, IdentifierUtility.TaskDataPrefix) &&
                                             usedTaskIds.Add(task.Id);

                        if (validAndUnique)
                            continue;

                        task.Id = NextUniqueId(IdentifierUtility.TaskDataPrefix, usedTaskIds);
                        tasks[i] = task;
                        isDirty = true;
                    }
                }

                if (!isDirty)
                    continue;

                EditorUtility.SetDirty(lessonConfig);
                hasChanges = true;
            }

            return hasChanges;
        }

        private static bool ReconcileObjectConfigs()
        {
            var hasChanges = false;
            var usedObjectIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (var objectConfig in LoadAssetsInStableOrder<ARObjectConfig>())
            {
                if (objectConfig == null)
                    continue;

                if (IdentifierUtility.HasPrefix(objectConfig.Id, IdentifierUtility.ObjectConfigPrefix) &&
                    usedObjectIds.Add(objectConfig.Id))
                    continue;

                objectConfig.Id = NextUniqueId(IdentifierUtility.ObjectConfigPrefix, usedObjectIds);
                EditorUtility.SetDirty(objectConfig);
                hasChanges = true;
            }

            return hasChanges;
        }

        private static IEnumerable<T> LoadAssetsInStableOrder<T>() where T : UnityEngine.Object
        {
            return AssetDatabase
                .FindAssets($"t:{typeof(T).Name}")
                .OrderBy(AssetDatabase.GUIDToAssetPath, StringComparer.Ordinal)
                .Select(guid => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)));
        }

        private static string NextUniqueId(string prefix, HashSet<string> usedIds)
        {
            string id;
            do
            {
                id = IdentifierUtility.CreateId(prefix);
            } while (!usedIds.Add(id));

            return id;
        }
    }
}

