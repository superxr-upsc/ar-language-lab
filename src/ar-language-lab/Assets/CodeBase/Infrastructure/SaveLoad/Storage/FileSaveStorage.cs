using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Infrastructure.SaveLoad.Storage
{
    public class FileSaveStorage : ISaveStorage
    {
        private readonly string _rootDirectory;
        
        private const string SaveFileName = "game_data";

        public FileSaveStorage(string rootFolderName = "saves")
        {
            _rootDirectory = Path.Combine(Application.persistentDataPath, rootFolderName);
            Directory.CreateDirectory(_rootDirectory);
        }

        public UniTask<bool> ExistsAsync()
        {
            var path = GetAbsolutePath();
            return UniTask.FromResult(File.Exists(path));
        }

        public async UniTask<string> ReadAsync()
        {
            var path = GetAbsolutePath();

            if (!File.Exists(path))
                return null;

            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        public async UniTask WriteAsync(string payload)
        {
            var path = GetAbsolutePath();
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(payload ?? string.Empty);
        }

        public UniTask DeleteAsync()
        {
            var path = GetAbsolutePath();
            if (File.Exists(path))
                File.Delete(path);

            return UniTask.CompletedTask;
        }

        public string GetAbsolutePath()
        {
            if (string.IsNullOrWhiteSpace(SaveFileName))
                throw new ArgumentException("Save key cannot be null or empty.", nameof(SaveFileName));

            var fileName = SanitizeFileName(SaveFileName) + ".json";
            return Path.Combine(_rootDirectory, fileName);
        }

        private static string SanitizeFileName(string fileName)
        {
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(invalidChar, '_');

            return fileName;
        }
    }
}


