using System;
using Codebase.Systems.CustomAttributes;

namespace CodeBase.Gameplay.Identifiers
{
    [Serializable]
    public class UniqueObject
    {
        [ReadOnly] public string Id;
        private readonly string _prefix = "OBJ";
        
        public UniqueObject(string prefix = "OBJ")
        {
            _prefix = prefix;
            
            EnsureId();
        }
        
        private void EnsureId()
        {
            if (IdentifierUtility.HasPrefix(Id, _prefix))
                return;

            Id = IdentifierUtility.CreateId(_prefix);
        }
    }
}