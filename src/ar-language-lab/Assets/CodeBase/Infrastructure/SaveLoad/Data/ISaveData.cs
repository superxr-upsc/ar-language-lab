namespace CodeBase.Infrastructure.SaveLoad.Data
{
    public interface ISaveData
    {
        int Version { get; set; }
        SettingsSaveData Settings { get; }
        LessonsSaveData Lessons { get; }
    }
}

