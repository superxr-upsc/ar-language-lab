using System;

namespace CodeBase.Gameplay.Identifiers
{
    public static class IdentifierUtility
    {
        public const string LessonConfigPrefix = "LC";
        public const string TaskDataPrefix = "TD";
        public const string ObjectConfigPrefix = "OC";

        public static string CreateId(string prefix) =>
            $"{prefix}_{Guid.NewGuid():N}";

        public static bool HasPrefix(string id, string prefix) =>
            !string.IsNullOrWhiteSpace(id) && id.StartsWith(prefix + "_", StringComparison.Ordinal);
    }
}

