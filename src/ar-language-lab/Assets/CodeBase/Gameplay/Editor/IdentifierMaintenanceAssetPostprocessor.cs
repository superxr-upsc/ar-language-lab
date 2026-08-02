using System;
using System.Collections.Generic;
using UnityEditor;

namespace CodeBase.Gameplay.Editor
{
    internal sealed class IdentifierMaintenanceAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (!ContainsAssetFile(importedAssets) && !ContainsAssetFile(movedAssets))
                return;

            IdentifierMaintenanceService.QueueReconcile();
        }

        private static bool ContainsAssetFile(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                if (path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}