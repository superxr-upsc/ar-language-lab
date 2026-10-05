using System.Linq;
using CodeBase.Infrastructure.ProjectResourcesProvider;
using CodeBase.Infrastructure.SaveLoad;
using CodeBase.Infrastructure.SaveLoad.Data;

namespace CodeBase.Gameplay.Lessons.Saves
{
    public class LessonsGameDataProvider
    {
        private readonly ISaveService _saveService;
        private readonly IProjectResourcesProvider _projectResourcesProvider;
        private readonly LessonsSaveData _lessonsSaveData;
        private readonly GameLessons _config;

        public LessonsGameDataProvider(ISaveService saveService,
            IProjectResourcesProvider projectResourcesProvider)
        {
            _saveService = saveService;
            _projectResourcesProvider = projectResourcesProvider;
            _lessonsSaveData = saveService.SaveData.Lessons;

            _config = _projectResourcesProvider.LoadResource<GameLessons>();
        }

        public LessonProgress[] GetAllProgress() => 
            _lessonsSaveData.Progress.ToArray();

        public bool IsLessonComplete(string lessonId)
        {
            var lessonProgress = GetProgressById(lessonId);
            return lessonProgress != null && lessonProgress.IsComplete;
        }

        public string GetNextLessonId(string lessonId)
        {
            var nextLesson = _config.Lessons
                .SkipWhile(lesson => lesson.Id != lessonId)
                .Skip(1)
                .FirstOrDefault();
            
            return nextLesson != null ? nextLesson.Id : string.Empty;
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

            OpenNextLesson(currentLessonId);
        
            _saveService.MarkDirty();
        }

        private void OpenNextLesson(string currentLessonId)
        {
            var nextLessonId = GetNextLessonId(currentLessonId);
            if (string.IsNullOrEmpty(nextLessonId))
            {
                return;
            }
            
            var lessonProgress = GetProgressById(nextLessonId);
            if (lessonProgress != null)
            {
                lessonProgress.IsOpen = true;
            }
            else
            {
                var newLessonProgress = new LessonProgress
                {
                    LessonId = nextLessonId,
                    LastCompletedTaskId = string.Empty,
                    IsComplete = false,
                    IsOpen = true
                };
                
            
                _lessonsSaveData.Progress.Add(newLessonProgress);
            }
        }

        public void SetSelectedLessonID(string lessonID)
        {
            _lessonsSaveData.SelectedLessonID = lessonID;
            _saveService.MarkDirty();
        }

        public void ClearCompletedLessonData(string lessonID)
        {
            var lessonProgress = GetProgressById(lessonID);
            if (lessonProgress == null)
                return;
            
            lessonProgress.Clear();
            lessonProgress.IsOpen = true;
            _saveService.MarkDirty();
        }
    }
}