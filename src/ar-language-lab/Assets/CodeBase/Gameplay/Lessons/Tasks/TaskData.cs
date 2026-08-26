using System;
using CodeBase.Gameplay.ARObjects;
using CodeBase.Gameplay.Identifiers;

namespace CodeBase.Gameplay.Lessons.Tasks
{
    [Serializable]
    public class TaskData : UniqueObject
    {
        public TaskType Type;
        public string DescriptionLocalizationKey;

        public ARObjectConfig[] TargetObjects;
        public ARObjectConfig[] SecondaryTargetObjects;
        
        
        
        public TaskData() : base(IdentifierUtility.TaskDataPrefix)
        {
            
        }
    }
}