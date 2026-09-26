namespace CodeBase.Infrastructure.StaticData
{
    public static class MarkerNames
    {
        private static MarkerName[] All = new MarkerName[]
        {
            new MarkerName(0, "ar-marker-1"),
            new MarkerName(1, "ar-marker-2"),
            new MarkerName(2, "ar-marker-3"),
            new MarkerName(3, "ar-marker-4"),
            new MarkerName(4, "ar-marker-5"),
        };

        public static string GetNameAtIndex(int index)
        {
            if (index < 0 || index >= All.Length)
                return string.Empty;
            
            return All[index].Name;
        }
    }
}