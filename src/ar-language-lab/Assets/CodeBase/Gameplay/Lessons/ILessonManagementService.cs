namespace CodeBase.Gameplay.Lessons
{
    public interface ILessonManagementService
    {
        void SetupLesson();
        void CleanupLesson();
        void StartLesson();
    }
}