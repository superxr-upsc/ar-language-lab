using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using R3;

namespace CodeBase.Infrastructure.Loading.UI
{
    public class LoadingScreenPresenter : PresenterBase
    {
        private readonly LoadingScreenData _model;
        private readonly LoadingScreenView _view;

        public LoadingScreenPresenter(LoadingScreenData model, LoadingScreenView view) : base(view)
        {
            _model = model;
            _view = view;

            _model.CurrentProgress
                .Subscribe(SetNewProgress)
                .AddTo(_compositeDisposable);
            
            _model.CurrentProgressStep
                .Subscribe(SetNewProgress)
                .AddTo(_compositeDisposable);
        }
        
        private void SetNewProgress(string progress) => 
            _view.UpdateProgressText(progress);

        private void SetNewProgress(float progress) => 
            _view.UpdateProgressBar(progress);
        
        
    }
}