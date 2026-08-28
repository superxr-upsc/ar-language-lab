using CodeBase.Infrastructure.GameFactory;

namespace CodeBase.Gameplay.Lessons.Tasks.Extensions
{
    public static class TaskExtensions
    {
        public static TaskResolverBase ToResolver(this TaskType taskType, TaskData taskData, IGameFactory gameFactory)
        {
            return taskType switch
            {
                TaskType.None => null,
                TaskType.FindObject => gameFactory.Create<FindObjectTask>(taskData),
                TaskType.PlaceObjectsInHierarchy => gameFactory.Create<PlaceObjectsInHierarchyTask>(taskData),
                TaskType.PlaceObjectNearToAnother => gameFactory.Create<PlaceObjectNearToAnotherTask>(taskData),
                _ => null
            };
        }

        public static TaskResolverBase[] ToResolvers(this TaskData[] tasksData, IGameFactory gameFactory)
        {
            var tasks = new TaskResolverBase[tasksData.Length];
            for (var index = 0; index < tasksData.Length; index++)
            {
                var taskData = tasksData[index];
                tasks[index] = taskData.Type.ToResolver(taskData, gameFactory);
            }
            
            return tasks;
        }
    }
}