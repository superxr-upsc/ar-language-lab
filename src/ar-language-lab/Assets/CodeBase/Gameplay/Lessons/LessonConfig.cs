using System.Collections.Generic;
using CodeBase.Gameplay.Identifiers;
using UnityEngine;

namespace CodeBase.Gameplay.Lessons
{
    [CreateAssetMenu(fileName = "LessonConfig", menuName = "AR/LessonConfig")]
    public class LessonConfig : UniqueScriptableObject
    {
        protected override string Prefix => IdentifierUtility.LessonConfigPrefix;
        public TaskData[] Tasks;
        
        //Validation Task ID for task data 
        protected override void EnsureCustomIDValidation()
        {
            if (Tasks == null || Tasks.Length == 0)
                return;

            var usedIds = new HashSet<string>();

            for (int i = 0; i < Tasks.Length; i++)
            {
                var task = Tasks[i];
                var id = task.Id;
                var isValidId = IdentifierUtility.HasPrefix(id, IdentifierUtility.TaskDataPrefix);

                if (!isValidId || !usedIds.Add(id))
                {
                    task.Id = IdentifierUtility.CreateId(IdentifierUtility.TaskDataPrefix);
                    usedIds.Add(task.Id);
                    Tasks[i] = task;
                }
            }
        }
    }
}