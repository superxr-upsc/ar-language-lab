using UnityEngine;

namespace CodeBase.Common.Extensions {
    public static class LayerExtensions 
    {
        public static void SetLayerRecursively(this GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform) 
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }
    }
}