using CodeBase.Gameplay.Lessons.Tasks.Resolvers;
using CodeBase.Infrastructure.GameFactory;

namespace CodeBase.Gameplay.Lessons.Tasks.Extensions
{
    public static class TaskExtensions
    {
        public static TaskResolverBase ToResolver(this TaskType taskType, TaskData taskData, IGameFactory gameFactory, Perspective perspective)
        {
            return taskType switch
            {
                TaskType.None => null,
                TaskType.FindObject => gameFactory.Create<FindObjectTask>(taskData, perspective),
                TaskType.PlaceObjectsInHierarchy => gameFactory.Create<PlaceObjectsInHierarchyTask>(taskData, perspective),
                TaskType.PlaceObjectNearToAnother => gameFactory.Create<PlaceObjectNearToAnotherTask>(taskData, perspective),
                _ => null
            };
        }

        public static TaskResolverBase[] ToResolvers(this TaskData[] tasksData, IGameFactory gameFactory, Perspective perspective)
        {
            var tasks = new TaskResolverBase[tasksData.Length];
            for (var index = 0; index < tasksData.Length; index++)
            {
                var taskData = tasksData[index];
                tasks[index] = taskData.Type.ToResolver(taskData, gameFactory, perspective);
            }
            
            return tasks;
        }
    }
}