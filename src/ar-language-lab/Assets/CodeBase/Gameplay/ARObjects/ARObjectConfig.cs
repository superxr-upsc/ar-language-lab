using CodeBase.Gameplay.Identifiers;
using UnityEngine;

namespace CodeBase.Gameplay.ARObjects
{
    [CreateAssetMenu(fileName = "ARObjectConfig", menuName = "AR/ObjectConfig")]
    public class ARObjectConfig : UniqueScriptableObject
    {
        protected override string Prefix => IdentifierUtility.ObjectConfigPrefix;
        
        public string LocalisationKey;
        public string VuforiaKey;
        public ARObjectBase Prefab;
    }
}