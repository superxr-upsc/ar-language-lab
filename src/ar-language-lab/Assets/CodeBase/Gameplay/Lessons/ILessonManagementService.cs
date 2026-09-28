using CodeBase.Gameplay.ARObjects;

namespace CodeBase.Gameplay.Lessons
{
    public interface ILessonManagementService
    {
        void SetupLesson();
        void CleanupLesson();
        void StartLesson();
        void LoadMainMenu();
        ARObjectBase GetObject(ARObjectConfig selectedObjectConfig);
    }
}