using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace CodeBase.Infrastructure.Localization
{
    public class LocalizationService : ILocalizationService
    {
        public const string DefaultLocaleCode = "en";
        public const string DefaultStringTableName = "BaseStringTableCollection";
        public const string DefaultAssetTableName = "AssetLocalisationTableCollection";

        public bool IsInitialized => _isInitialized;
        public string CurrentLocaleCode => _currentLocale?.Identifier.Code;
        public IReadOnlyList<string> AvailableLocaleCodes => _availableLocaleCodes;

        public event Action<string> LocaleChanged;

        private readonly Dictionary<string, StringTable> _stringTablesCache = new();
        private readonly Dictionary<string, AssetTable> _assetTablesCache = new();
        private readonly List<string> _availableLocaleCodes = new();

        private bool _isInitialized;
        private Locale _currentLocale;
        private UniTaskCompletionSource _initializeSource;

        public async UniTask InitializeAsync(string localeCode = null)
        {
            if (_isInitialized)
                return;

            if (_initializeSource != null)
            {
                await _initializeSource.Task;
                return;
            }

            _initializeSource = new UniTaskCompletionSource();

            try
            {
                await LocalizationSettings.InitializationOperation.ToUniTask();

                CacheAvailableLocales();

                var requestedLocaleCode = string.IsNullOrWhiteSpace(localeCode)
                    ? DefaultLocaleCode
                    : localeCode;

                await SetLocaleInternalAsync(requestedLocaleCode);

                _isInitialized = true;
                _initializeSource.TrySetResult();
            }
            catch (Exception exception)
            {
                _initializeSource.TrySetException(exception);
                throw;
            }
            finally
            {
                _initializeSource = null;
            }
        }

        public async UniTask SetLocaleAsync(string localeCode)
        {
            if (!_isInitialized)
            {
                await InitializeAsync(localeCode);
                return;
            }

            await SetLocaleInternalAsync(localeCode);
        }

        public async UniTask<string> GetStringAsync(string key, string tableName = DefaultStringTableName, params object[] arguments)
        {
            await EnsureInitializedAsync();

            var table = await GetStringTableAsync(tableName, _currentLocale);
            var entry = table.GetEntry(key);

            if (entry == null)
            {
                throw new KeyNotFoundException(
                    $"String key '{key}' was not found in table '{tableName}' for locale '{CurrentLocaleCode}'.");
            }

            return arguments == null || arguments.Length == 0
                ? entry.GetLocalizedString()
                : entry.GetLocalizedString(arguments);
        }

        public async UniTask<TAsset> GetAssetAsync<TAsset>(string key, string tableName = DefaultAssetTableName)
            where TAsset : UnityEngine.Object
        {
            await EnsureInitializedAsync();

            var table = await GetAssetTableAsync(tableName, _currentLocale);
            var entry = table.GetEntry(key);

            if (entry == null)
            {
                throw new KeyNotFoundException(
                    $"Asset key '{key}' was not found in table '{tableName}' for locale '{CurrentLocaleCode}'.");
            }

            var handle = LocalizationSettings.AssetDatabase.GetLocalizedAssetAsync<TAsset>(tableName, key, _currentLocale);
            await handle.ToUniTask();

            return handle.Result;
        }

        private async UniTask EnsureInitializedAsync()
        {
            if (_isInitialized)
                return;

            await InitializeAsync();
        }

        private void CacheAvailableLocales()
        {
            _availableLocaleCodes.Clear();

            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
                _availableLocaleCodes.Add(locale.Identifier.Code);
        }

        private async UniTask SetLocaleInternalAsync(string localeCode)
        {
            if (string.IsNullOrWhiteSpace(localeCode))
                localeCode = DefaultLocaleCode;

            var locale = FindLocale(localeCode);
            if (locale == null)
            {
                throw new KeyNotFoundException(
                    $"Locale '{localeCode}' is not available. Available locales: {string.Join(", ", _availableLocaleCodes)}.");
            }

            if (_currentLocale == locale)
                return;

            LocalizationSettings.SelectedLocale = locale;
            _currentLocale = locale;

            ClearTablesCache();

            await UniTask.Yield();
            LocaleChanged?.Invoke(_currentLocale.Identifier.Code);
        }

        private Locale FindLocale(string localeCode)
        {
            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
            {
                if (string.Equals(locale.Identifier.Code, localeCode, StringComparison.OrdinalIgnoreCase))
                    return locale;
            }

            return null;
        }

        private async UniTask<StringTable> GetStringTableAsync(string tableName, Locale locale)
        {
            var cacheKey = BuildCacheKey(tableName, locale);
            if (_stringTablesCache.TryGetValue(cacheKey, out var table))
                return table;

            var handle = LocalizationSettings.StringDatabase.GetTableAsync(tableName, locale);
            await handle.ToUniTask();

            table = handle.Result;
            if (table == null)
            {
                throw new KeyNotFoundException(
                    $"String table '{tableName}' was not found for locale '{locale.Identifier.Code}'.");
            }

            _stringTablesCache[cacheKey] = table;
            return table;
        }

        private async UniTask<AssetTable> GetAssetTableAsync(string tableName, Locale locale)
        {
            var cacheKey = BuildCacheKey(tableName, locale);
            if (_assetTablesCache.TryGetValue(cacheKey, out var table))
                return table;

            var handle = LocalizationSettings.AssetDatabase.GetTableAsync(tableName, locale);
            await handle.ToUniTask();

            table = handle.Result;
            if (table == null)
            {
                throw new KeyNotFoundException(
                    $"Asset table '{tableName}' was not found for locale '{locale.Identifier.Code}'.");
            }

            _assetTablesCache[cacheKey] = table;
            return table;
        }

        private static string BuildCacheKey(string tableName, Locale locale)
        {
            return $"{tableName}:{locale.Identifier.Code}";
        }

        private void ClearTablesCache()
        {
            _stringTablesCache.Clear();
            _assetTablesCache.Clear();
        }
    }
}