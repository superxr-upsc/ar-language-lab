using System.Collections.Generic;
using System.Linq;
using CodeBase.Infrastructure.StaticData;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeBase.Infrastructure.ProjectResourcesProvider
{
    public class ProjectResourcesProvider: IProjectResourcesProvider
    {
        public TResource LoadResource<TResource>(string path) where TResource : Object, IResource
        {
            return Resources.LoadAll<TResource>(path).FirstOrDefault();
        }
        
        public TResource LoadResource<TResource>() where TResource : Object, IResource
        {
            return LoadResources<TResource>().FirstOrDefault();
        }
        
        public IEnumerable<TResource> LoadResources<TResource>() where TResource : Object, IResource
        {
            var path = ResourceNames.GetLocation<TResource>();
            return Resources.LoadAll<TResource>(path);
        }
        
        public void ReleaseResource(Object resource)
        {
            Resources.UnloadAsset(resource);
        }
    }
}