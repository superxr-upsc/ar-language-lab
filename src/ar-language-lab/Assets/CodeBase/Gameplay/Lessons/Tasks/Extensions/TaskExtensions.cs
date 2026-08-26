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
                TaskType.FindObjectByVoice => gameFactory.Create<FindObjectByVoiceTask>(taskData),
                TaskType.FindObjectByText => gameFactory.Create<FindObjectByTextTask>(taskData),
                TaskType.FindObjectByColor => gameFactory.Create<FindObjectByColorTask>(taskData),
                TaskType.PlaceObjectsInHierarchy => gameFactory.Create<PlaceObjectsInHierarchyTask>(taskData),
                TaskType.PlaceObjectNextToAnother => gameFactory.Create<PlaceObjectNextToAnotherTask>(taskData),
                TaskType.PlaceObjectNearToAnotherObjectSide => gameFactory.Create<PlaceObjectNearToAnotherObjectSideTask>(taskData),
                TaskType.SelectObjectsByDescription => gameFactory.Create<SelectObjectByDescriptionTask>(taskData),
                TaskType.SelectObjectForQuestion => gameFactory.Create<SelectObjectForQuestionTask>(taskData),
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