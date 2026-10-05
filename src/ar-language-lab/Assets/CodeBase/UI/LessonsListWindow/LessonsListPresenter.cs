using CodeBase.Gameplay.Lessons;
using CodeBase.Gameplay.Lessons.Saves;
using CodeBase.Infrastructure.GameStateMachineService.StateMachine;
using CodeBase.Infrastructure.GameStateMachineService.States;
using CodeBase.Infrastructure.ProjectResourcesProvider;
using CodeBase.Infrastructure.SaveLoad;
using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using R3;

namespace CodeBase.UI.LessonsListWindow
{
    public class LessonsListPresenter : PresenterBase
    {
        private readonly ISaveService _saveService;
        private readonly IGameStateMachine _gameStateMachine;

        private GameLessons _model;
        private LessonsListView _view;
        private readonly LessonsGameDataProvider _lessonGameDataProvider;

        public LessonsListPresenter(GameLessons model, LessonsListView view, 
            ISaveService saveService, 
            IGameStateMachine gameStateMachine,
            IProjectResourcesProvider resourcesProvider) : base(view)
        {
            _model = model;
            _view = view;
        
            _saveService = saveService;
            _gameStateMachine = gameStateMachine;
            _lessonGameDataProvider = new LessonsGameDataProvider(saveService, resourcesProvider);

            _view.Initialize(_model.Lessons, _lessonGameDataProvider, OnLessonSelected);
            _view.CloseButton.OnClickAsObservable()
                .Subscribe(_ => DisposeAsync())
                .AddTo(_compositeDisposable);
        }

        public void OnLessonSelected(string lessonID)
        {
            var lessonProgress = _lessonGameDataProvider.GetProgressById(lessonID);
            if (lessonProgress == null || lessonProgress.IsOpen == false)
                return;
            
            if (lessonProgress != null && lessonProgress.IsComplete) 
                _lessonGameDataProvider.ClearCompletedLessonData(lessonID); 
            
            _lessonGameDataProvider.SetSelectedLessonID(lessonID);
            _gameStateMachine.Enter<EnterGameplaySceneState>();
        }

        protected override void ClearInstance()
        {
            _model = null;
            _view = null;
            
            base.ClearInstance();
        }
    }
}