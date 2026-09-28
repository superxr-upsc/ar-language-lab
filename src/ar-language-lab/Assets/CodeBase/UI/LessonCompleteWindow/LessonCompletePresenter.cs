using CodeBase.Gameplay.Lessons;
using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using R3;

namespace CodeBase.UI.LessonCompleteWindow
{
    public class LessonCompletePresenter : PresenterBase
    {
        private readonly ILessonManagementService _lessonManagementService;

        public LessonCompletePresenter(LessonCompleteData model, LessonCompleteView viewBase,
            ILessonManagementService lessonManagementService)
            : base(viewBase)
        {
            _lessonManagementService = lessonManagementService;
            viewBase.ContinueButton
                .OnClickAsObservable()
                .Subscribe(_ => LoadMainMenu())
                .AddTo(_compositeDisposable);
        }

        private void LoadMainMenu() => 
            _lessonManagementService.LoadMainMenu();
    }
}