using CodeBase.Gameplay.ARObjects;

namespace CodeBase.Gameplay.Lessons
{
    public interface ILessonManagementService
    {
        void SetupLesson();
        void CleanupLesson();
        void StartLesson();
        ARObjectBase GetObject(ARObjectConfig selectedObjectConfig);
    }
}