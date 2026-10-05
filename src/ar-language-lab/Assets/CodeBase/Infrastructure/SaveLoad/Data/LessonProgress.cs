namespace CodeBase.Infrastructure.SaveLoad.Data
{
    public class LessonProgress
    {
        public string LessonId;
        public string LastCompletedTaskId;
        public bool IsComplete;
        public bool IsOpen;

        public void Clear()
        {
            LastCompletedTaskId = null;
            IsComplete = false;
            IsOpen = false;
        }
    }
}