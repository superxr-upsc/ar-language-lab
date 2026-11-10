using CodeBase.Gameplay.Lessons;
using CodeBase.Gameplay.Lessons.Saves;
using CodeBase.Gameplay.Lessons.Tasks;
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
        private readonly ILessonManagementService _lessonManagementService;
        private readonly LessonsGameDataProvider _lessonGameDataProvider;

        private GameLessons _model;
        private LessonsListView _view;
        

        public LessonsListPresenter(GameLessons model, LessonsListView view, 
            ISaveService saveService, 
            IGameStateMachine gameStateMachine,
            IProjectResourcesProvider resourcesProvider,
            ILessonManagementService lessonManagementService) : base(view)
        {
            _model = model;
            _view = view;
        
            _saveService = saveService;
            _gameStateMachine = gameStateMachine;
            _lessonManagementService = lessonManagementService;
            _lessonGameDataProvider = new LessonsGameDataProvider(saveService, resourcesProvider);

            _view.Initialize(_model.Lessons, _lessonGameDataProvider, _lessonManagementService.GetPerspective(), OnLessonSelected);
            
            _view.CloseButton.OnClickAsObservable()
                .Subscribe(_ => DisposeAsync())
                .AddTo(_compositeDisposable);
            
            _view.PerspectiveDropDown.OnValueChangedAsObservable()
                .Subscribe(OnPerspectiveChanged)
                .AddTo(_compositeDisposable);

            #if WITHOUT_PERSPECTIVE
            HidePerspectiveSelection();
            #endif
        }

        private void HidePerspectiveSelection()
        {
            _view.PerspectiveDropDown.gameObject.SetActive(false);
        }

        private void OnPerspectiveChanged(int index)
        {
            var selectedPerspective = (Perspective)index;
            _lessonManagementService.SetPerspective(selectedPerspective);
        }

        private void OnLessonSelected(string lessonID)
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