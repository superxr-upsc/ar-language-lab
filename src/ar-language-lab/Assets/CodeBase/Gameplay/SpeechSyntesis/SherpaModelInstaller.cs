using System;
using System.IO;
using System.IO.Compression;
using CodeBase.Common.LoggerService;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace CodeBase.Gameplay.SpeechSyntesis
{
    public sealed class SherpaModelInstaller
    {
        private const string ModelId =
            "vits-piper-en_GB-sweetbbak-amy";

        private const string StreamingAssetZip =
            "sherpa-models/vits-piper-en_GB-sweetbbak-amy.zip";

        private const string ModelRelativePath =
            "sherpa-onnx/models/speech-synthesis/vits-piper-en_GB-sweetbbak-amy";

        private const string InstallMarker =
            ".installed";

        public string ModelPath =>
            Path.Combine(
                Application.persistentDataPath,
                ModelRelativePath);

        public async UniTask EnsureInstalledAsync()
        {
            #if UNITY_EDITOR
                return;
            #endif
            
            GameLogger.Log(
                $"[SherpaInstaller] Checking model: {ModelId}");

            if (IsInstalled())
            {
                GameLogger.Log(
                    $"[SherpaInstaller] Model already installed: {ModelPath}");

                return;
            }

            GameLogger.Log(
                "[SherpaInstaller] Model is not installed. Starting installation...");

            await InstallAsync();

            GameLogger.Log(
                $"[SherpaInstaller] Model installation completed: {ModelPath}");
        }

        private bool IsInstalled()
        {
            var markerPath = Path.Combine(
                ModelPath,
                InstallMarker);

            return Directory.Exists(ModelPath) &&
                   File.Exists(markerPath);
        }

        private async UniTask InstallAsync()
        {
            var destinationRoot = Path.Combine(
                Application.persistentDataPath,
                "sherpa-onnx",
                "models",
                "speech-synthesis");

            var finalModelPath = Path.Combine(
                destinationRoot,
                ModelId);

            var temporaryModelPath = Path.Combine(
                destinationRoot,
                $"{ModelId}.tmp");

            var zipPath = Path.Combine(
                Application.temporaryCachePath,
                $"{ModelId}.zip");

            try
            {
                Directory.CreateDirectory(destinationRoot);

                // Remove leftovers from previous interrupted installation.
                if (Directory.Exists(temporaryModelPath))
                {
                    GameLogger.Log(
                        "[SherpaInstaller] Removing incomplete temporary installation...");

                    Directory.Delete(
                        temporaryModelPath,
                        true);
                }

                // Remove temporary ZIP from previous attempt.
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }

                // Download/copy ZIP from StreamingAssets to writable storage.
                await DownloadZipAsync(zipPath);

                GameLogger.Log(
                    $"[SherpaInstaller] Extracting model to temporary directory: {temporaryModelPath}");

                Directory.CreateDirectory(temporaryModelPath);

                ExtractZip(
                    zipPath,
                    temporaryModelPath);

                GameLogger.Log(
                    "[SherpaInstaller] Validating extracted model...");

                ValidateModelDirectory(
                    temporaryModelPath);

                // Remove old model if it somehow exists.
                if (Directory.Exists(finalModelPath))
                {
                    GameLogger.Log(
                        "[SherpaInstaller] Removing existing incomplete model...");

                    Directory.Delete(
                        finalModelPath,
                        true);
                }

                // Move fully extracted model into final location.
                Directory.Move(
                    temporaryModelPath,
                    finalModelPath);

                // Only now mark the model as successfully installed.
                var markerPath = Path.Combine(
                    finalModelPath,
                    InstallMarker);

                File.WriteAllText(
                    markerPath,
                    DateTime.UtcNow.ToString("O"));

                GameLogger.Log(
                    $"[SherpaInstaller] Model successfully installed: {finalModelPath}");
            }
            catch (Exception exception)
            {
                GameLogger.LogError(
                    $"[SherpaInstaller] Installation failed: {exception}");

                // Remove incomplete temporary model.
                if (Directory.Exists(temporaryModelPath))
                {
                    try
                    {
                        Directory.Delete(
                            temporaryModelPath,
                            true);
                    }
                    catch (Exception cleanupException)
                    {
                        GameLogger.LogError(
                            $"[SherpaInstaller] Failed to cleanup temporary model: {cleanupException}");
                    }
                }

                throw;
            }
            finally
            {
                // ZIP is no longer needed after extraction.
                if (File.Exists(zipPath))
                {
                    try
                    {
                        File.Delete(zipPath);

                        GameLogger.Log(
                            "[SherpaInstaller] Temporary ZIP deleted.");
                    }
                    catch (Exception exception)
                    {
                        GameLogger.LogWarning(
                            $"[SherpaInstaller] Failed to delete temporary ZIP: {exception.Message}");
                    }
                }
            }
        }

        private async UniTask DownloadZipAsync(
            string destinationPath)
        {
            var sourcePath =
                Path.Combine(
                    Application.streamingAssetsPath,
                    StreamingAssetZip);

            GameLogger.Log(
                $"[SherpaInstaller] Reading model from StreamingAssets:");
            
            GameLogger.Log(
                sourcePath);

            using var request =
                UnityWebRequest.Get(sourcePath);

            request.downloadHandler =
                new DownloadHandlerFile(destinationPath);

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new IOException(
                    $"Failed to read Sherpa model ZIP. " +
                    $"Path: {sourcePath}. " +
                    $"Error: {request.error}");
            }

            if (!File.Exists(destinationPath))
            {
                throw new FileNotFoundException(
                    "Sherpa model ZIP was downloaded but file does not exist.",
                    destinationPath);
            }

            var fileInfo =
                new FileInfo(destinationPath);

            if (fileInfo.Length == 0)
            {
                throw new InvalidDataException(
                    "Sherpa model ZIP is empty.");
            }

            GameLogger.Log(
                $"[SherpaInstaller] ZIP copied successfully. " +
                $"Size: {fileInfo.Length / 1024f / 1024f:F2} MB");
        }

        private static void ExtractZip(
            string zipPath,
            string destinationPath)
        {
            using var archive =
                ZipFile.OpenRead(zipPath);

            var destinationRoot =
                Path.GetFullPath(destinationPath);

            foreach (var entry in archive.Entries)
            {
                var fullPath =
                    Path.GetFullPath(
                        Path.Combine(
                            destinationRoot,
                            entry.FullName));

                // Protect against Zip Slip attacks.
                if (!fullPath.StartsWith(
                        destinationRoot + Path.DirectorySeparatorChar,
                        StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        $"Unsafe ZIP entry detected: {entry.FullName}");
                }

                // Directory entry.
                if (string.IsNullOrEmpty(entry.Name))
                {
                    Directory.CreateDirectory(
                        fullPath);

                    continue;
                }

                var directory =
                    Path.GetDirectoryName(fullPath);

                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(
                        directory);
                }

                entry.ExtractToFile(
                    fullPath,
                    overwrite: true);
            }
        }

        private static void ValidateModelDirectory(
            string modelPath)
        {
            if (!Directory.Exists(modelPath))
            {
                throw new DirectoryNotFoundException(
                    $"Extracted model directory does not exist: {modelPath}");
            }

            var files =
                Directory.GetFiles(
                    modelPath,
                    "*",
                    SearchOption.AllDirectories);

            if (files.Length == 0)
            {
                throw new InvalidDataException(
                    $"Extracted model directory is empty: {modelPath}");
            }

            GameLogger.Log(
                $"[SherpaInstaller] Found {files.Length} files in model directory.");
        }
    }
}