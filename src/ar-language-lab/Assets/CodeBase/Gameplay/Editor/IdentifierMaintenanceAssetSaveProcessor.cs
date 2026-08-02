using UnityEditor;

namespace CodeBase.Gameplay.Editor
{
    internal sealed class IdentifierMaintenanceAssetSaveProcessor : AssetModificationProcessor
    {
        public static string[] OnWillSaveAssets(string[] paths)
        {
            IdentifierMaintenanceService.ReconcileAllIds(saveAssets: false);
            return paths;
        }
    }
}