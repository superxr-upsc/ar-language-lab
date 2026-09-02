using System.Collections.Generic;

namespace CodeBase.Infrastructure.SaveLoad.Data
{
    public class LessonsSaveData
    {
        public string SelectedLessonID { get; set; }
        public List<LessonProgress> Progress = new();
    }
}