using CodeBase.Gameplay.ARObjects;
using CodeBase.Gameplay.Lessons.Tasks;
using Cysharp.Threading.Tasks;

namespace CodeBase.Gameplay.Lessons
{
    public interface ILessonManagementService
    {
        UniTask SetupLesson();
        void SetPerspective(Perspective perspective);
        Perspective GetPerspective();
        void CleanupLesson();
        void StartAudioLesson();
        void StartLesson();
        void LoadMainMenu();
        ARObjectBase GetObject(ARObjectConfig selectedObjectConfig);
    }
}