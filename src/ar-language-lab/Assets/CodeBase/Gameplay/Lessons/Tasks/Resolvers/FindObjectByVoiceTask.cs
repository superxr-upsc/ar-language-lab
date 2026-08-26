using CodeBase.Gameplay.SpeechSyntesis;
using CodeBase.Infrastructure.Localization;

namespace CodeBase.Gameplay.Lessons.Tasks
{
    public class FindObjectByVoiceTask : TaskResolverBase
    {
        private readonly ILocalizationService _localization;
        private readonly ITTSService _ttsService;

        public FindObjectByVoiceTask(TaskData taskData, ILocalizationService localization, ITTSService ttsService) : base(taskData)
        {
            _localization = localization;
            _ttsService = ttsService;
        }

        public override void Run()
        {
            _taskData.
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void CheckTaskCompletion()
        {
            base.CheckTaskCompletion();
        }

        protected override void CompleteTask()
        {
            base.CompleteTask();
        }
    }
}