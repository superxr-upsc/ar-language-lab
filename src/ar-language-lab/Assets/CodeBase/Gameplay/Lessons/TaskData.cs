using System;
using CodeBase.Gameplay.Identifiers;
using CodeBase.Gameplay.Lessons.Tasks;

namespace CodeBase.Gameplay.Lessons
{
    [Serializable]
    public class TaskData : UniqueObject
    {
        public TaskType Type;

        public TaskData() : base(IdentifierUtility.TaskDataPrefix)
        {
            
        }
    }
}