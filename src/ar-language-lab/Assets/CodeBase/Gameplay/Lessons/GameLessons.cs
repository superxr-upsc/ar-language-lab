using System.Linq;
using CodeBase.Infrastructure.ProjectResourcesProvider;
using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using UnityEngine;

namespace CodeBase.Gameplay.Lessons
{
    [CreateAssetMenu(fileName = "GameLessons", menuName = "AR/GameLessons")]
    public class GameLessons : ScriptableObject, IResource, IModel
    {
        public LessonConfig[] Lessons;

        public LessonConfig GetLesson(string lessonsSelectedLessonID)
        {
            foreach (var lesson in Lessons)
            {
                if (lesson.Id == lessonsSelectedLessonID)
                    return lesson;
            }

            Debug.LogError($"Lesson with ID {lessonsSelectedLessonID} not found. Returning the first lesson as a fallback.");
            return Lessons.FirstOrDefault();
        }
    }
}