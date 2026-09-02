using System.Collections.Generic;
using CodeBase.Gameplay.ARObjects;
using CodeBase.Gameplay.Identifiers;
using CodeBase.Gameplay.Lessons.Tasks;
using CodeBase.Infrastructure.ProjectResourcesProvider;
using UnityEngine;

namespace CodeBase.Gameplay.Lessons
{
    [CreateAssetMenu(fileName = "LessonConfig", menuName = "AR/LessonConfig")]
    public class LessonConfig : UniqueScriptableObject, IResource
    {
        protected override string Prefix => IdentifierUtility.LessonConfigPrefix;

        public Sprite LessonIcon;
        
        public string LessonNameKey;
        public string LessonDescriptionKey;
        
        
        public ARObjectConfig[] ObjectsToUse;
        public TaskData[] Tasks;

        public int GetCompletedTasksPercent(string lastCompletedTask)
        {
            var completedTasks = 0;
            foreach (var taskData in Tasks)    
            {
                if (taskData.Id !=  lastCompletedTask)
                    continue;
                
                completedTasks++;
            }
            
            return (int)((float)completedTasks / Tasks.Length * 100);
        }

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