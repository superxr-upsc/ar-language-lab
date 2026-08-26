using CodeBase.CustomAttributes;
using UnityEngine;

namespace CodeBase.Gameplay.Identifiers
{
    
    public class UniqueScriptableObject : ScriptableObject
    {
        [ReadOnly] public string Id;
        
        protected virtual string Prefix => "OBJ";
        
        private void OnValidate()
        {
            EnsureId();
            EnsureCustomIDValidation();
        }

        private void EnsureId()
        {
            if (IdentifierUtility.HasPrefix(Id, Prefix))
                return;

            Id = IdentifierUtility.CreateId(Prefix);
        }

        protected virtual void EnsureCustomIDValidation()
        {
        }
    }
}