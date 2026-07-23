using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase.Infrastructure.GameFactory;
using CodeBase.Infrastructure.ProjectResourcesProvider;
using CodeBase.Infrastructure.StaticData;
using CodeBase.Infrastructure.WindowsManagement.MVPBase;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure.WindowsManagement
{
    public class WindowsManagementService : MonoBehaviour, IWindowsManagementService
    {
        private const string WindowResourcesPath = "UI/";
        
        [SerializeField] private UILayerInfo[] _layers;

        private Dictionary<UILayer, PresenterBase> _currentOpenedWindows = new ();
        
        private IProjectResourcesProvider _resourcesProvider;
        private IGameFactory _factory;

        [Inject]
        public void Construct(IProjectResourcesProvider resourcesProvider, IGameFactory factory)
        {
            _resourcesProvider = resourcesProvider;
            _factory = factory;
        }

        public TPresenter CreateWindow<TPresenter, TView, TModel>(UILayer layer, TModel model) 
            where TPresenter : PresenterBase 
            where TView : ViewBase, IResource
            where TModel : ModelBase
        {
            CloseAllWindowsOnLayer(layer);

            var viewResource = _resourcesProvider.LoadResource<TView>(WindowResourcesPath);
            if (viewResource is null)
            {
                throw new Exception($"There is no resource {typeof(TView)} in folder Resources/{WindowResourcesPath}");
            }

            var view = _factory.CreatePrefab(viewResource, GetParentByLayer(layer));
            return _factory.Create<TPresenter>(model, view);
        }

        private Transform GetParentByLayer(UILayer layer)
        {
            var layerInfo = _layers.FirstOrDefault(x => x.Layer == layer);
            return layerInfo?.Parent;
        }

        private void CloseAllWindowsOnLayer(UILayer layer)
        {
            if (_currentOpenedWindows.ContainsKey(layer) == false) return;
            var currentWindowOnLayer = _currentOpenedWindows[layer];
            currentWindowOnLayer.Dispose();

            _currentOpenedWindows.Remove(layer);
        }
    }
}