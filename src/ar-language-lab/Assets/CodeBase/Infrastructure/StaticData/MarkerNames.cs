namespace CodeBase.Infrastructure.StaticData
{
    public static class MarkerNames
    {
        private static MarkerName[] All = new MarkerName[]
        {
            new MarkerName(0, "ar-marker-1"),
            new MarkerName(1, "ar-marker-2")
        };

        public static string GetNameAtIndex(int index)
        {
            if (index < 0 || index >= All.Length)
                return string.Empty;
            
            return All[index].Name;
        }
    }
}