using CodeBase.Infrastructure.SaveLoad;
using CodeBase.Infrastructure.SaveLoad.Data;

namespace CodeBase.Gameplay.Lessons.Saves
{
    public class LessonsGameDataProvider
    {
        private readonly ISaveService _saveService;
        private readonly LessonsSaveData _lessonsSaveData;
    
        public LessonsGameDataProvider(ISaveService saveService)
        {
            _saveService = saveService;
            _lessonsSaveData = saveService.SaveData.Lessons;
        }

        public LessonProgress[] GetAllProgress() => 
            _lessonsSaveData.Progress.ToArray();

        public bool IsLessonComplete(string lessonId)
        {
            var lessonProgress = GetProgressById(lessonId);
            return lessonProgress != null && lessonProgress.IsComplete;
        }

        public string GetLastCompletedTaskId(string lessonId)
        {
            var lessonProgress = GetProgressById(lessonId);
            return lessonProgress != null ? lessonProgress.LastCompletedTaskId : string.Empty;
        }

        public LessonProgress GetProgressById(string lessonId)
        {
            foreach (var lessonProgress in _lessonsSaveData.Progress)
            {
                if (lessonProgress.LessonId == lessonId)
                    return lessonProgress;
            }
            return null;
        }

        public void SaveCompletedTask(string currentLessonId, string taskId)
        {
            var lessonProgress = GetProgressById(currentLessonId);
            if (lessonProgress != null)
            {
                lessonProgress.LastCompletedTaskId = taskId;
            }
            else
            {
                var newLessonProgress = new LessonProgress
                {
                    LessonId = currentLessonId,
                    LastCompletedTaskId = taskId,
                    IsComplete = false
                };
            
                _lessonsSaveData.Progress.Add(newLessonProgress);
            }
        
            _saveService.MarkDirty();
        }

        public void SaveCompletedLesson(string currentLessonId)
        {
            var lessonProgress = GetProgressById(currentLessonId);
            if (lessonProgress != null)
            {
                lessonProgress.IsComplete = true;
            }
            else
            {
                var newLessonProgress = new LessonProgress
                {
                    LessonId = currentLessonId,
                    LastCompletedTaskId = string.Empty,
                    IsComplete = true
                };
            
                _lessonsSaveData.Progress.Add(newLessonProgress);
            }
        
            _saveService.MarkDirty();
        }
    }
}