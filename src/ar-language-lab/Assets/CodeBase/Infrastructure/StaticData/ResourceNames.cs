using System;
using CodeBase.Gameplay.Lessons;
using CodeBase.Gameplay.Lessons.Tasks;
using CodeBase.Infrastructure.ProjectResourcesProvider;

namespace CodeBase.Infrastructure.StaticData
{
    public static class ResourceNames
    {
        //Declare resources [type], [location in resources folder]
        private static ResourceName[] resources =
        {
            new (typeof(TaskResolversSettings), "Gameplay"),
            new (typeof(GameLessons), "Gameplay"),
        };

        public static string GetLocation<TResource>() where TResource : IResource
        {
            var location = string.Empty;
            foreach (var resource in resources)
            {
                if (resource.Type == typeof(TResource))
                    location = resource.Location;
            }

            if (string.IsNullOrEmpty(location))
                throw new NullReferenceException($"The is no path for resource with type {typeof(TResource)}.");

            return location;
        }
    }
}
