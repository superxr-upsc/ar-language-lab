using CodeBase.Gameplay.Identifiers;
using Unity.Collections;
using UnityEngine;

namespace CodeBase.Gameplay.ARObjects
{
    [CreateAssetMenu(fileName = "ARObjectConfig", menuName = "AR/ObjectConfig")]
    public class ARObjectConfig : UniqueScriptableObject
    {
        public string LocalisationKey;
    }
}