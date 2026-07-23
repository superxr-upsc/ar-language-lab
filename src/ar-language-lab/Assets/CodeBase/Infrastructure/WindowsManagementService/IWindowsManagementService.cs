using CodeBase.Infrastructure.ProjectResourcesProvider;
using CodeBase.Infrastructure.StaticData;
using CodeBase.Infrastructure.WindowsManagementService.MVPBase;

namespace CodeBase.Infrastructure.WindowsManagementService
{
    public interface IWindowsManagementService
    {
        TPresenter CreateWindow<TPresenter, TView, TModel>(UILayer layer, TModel model)
            where TPresenter : PresenterBase
            where TView : ViewBase, IResource
            where TModel : ModelBase;
    }
}