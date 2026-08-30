namespace CodeBase.Infrastructure.SaveLoad.Data
{
    public class SaveData : ISaveData
    {
        public int Version { get; set; } = 1;

        public SettingsSaveData Settings { get; set; } = new SettingsSaveData();
        public LessonsSaveData Lessons { get; set; } = new LessonsSaveData();
    }
}

