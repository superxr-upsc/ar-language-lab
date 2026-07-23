using CodeBase.Infrastructure.ProjectResourcesProvider;
using CodeBase.Infrastructure.StaticData;
using CodeBase.Infrastructure.WindowsManagement.MVPBase;

namespace CodeBase.Infrastructure.WindowsManagement
{
    public interface IWindowsManagementService
    {
        TPresenter CreateWindow<TPresenter, TView, TModel>(UILayer layer, TModel model)
            where TPresenter : PresenterBase
            where TView : ViewBase, IResource
            where TModel : ModelBase;
    }
}