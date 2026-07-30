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
        public bool IsInitialized => _isInitialized;
        public string CurrentLocaleCode => CurrentLocale?.Identifier.Code;
        public Locale CurrentLocale => LocalizationSettings.SelectedLocale;
        public IReadOnlyList<string> AvailableLocaleCodes => _availableLocaleCodes;

        public event Action<string> LocaleChanged;

        private readonly Dictionary<string, StringTable> _stringTablesCache = new();
        private readonly Dictionary<string, AssetTable> _assetTablesCache = new();
        private readonly List<string> _availableLocaleCodes = new();

        private bool _isInitialized;
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

                var requestedLocaleCode = GetRequestedLocaleCode(localeCode);

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

        public async UniTask<string> GetStringAsync(string key, string tableName = LocalizationConsts.DefaultStringTableName, params object[] arguments)
        {
            await EnsureInitializedAsync();

            var table = await GetStringTableAsync(tableName, CurrentLocale);
            var entry = table.GetEntry(key);

            if (entry == null)
            {
                throw new KeyNotFoundException(
                    $"String key '{key}' was not found in table '{tableName}' for locale '{CurrentLocale}'.");
            }

            return arguments == null || arguments.Length == 0
                ? entry.GetLocalizedString()
                : entry.GetLocalizedString(arguments);
        }

        public async UniTask<TAsset> GetAssetAsync<TAsset>(string key, string tableName = LocalizationConsts.DefaultAssetTableName)
            where TAsset : UnityEngine.Object
        {
            await EnsureInitializedAsync();

            var table = await GetAssetTableAsync(tableName, CurrentLocale);
            var entry = table.GetEntry(key);

            if (entry == null)
            {
                throw new KeyNotFoundException(
                    $"Asset key '{key}' was not found in table '{tableName}' for locale '{CurrentLocale}'.");
            }

            var handle = LocalizationSettings.AssetDatabase.GetLocalizedAssetAsync<TAsset>(tableName, key, CurrentLocale);
            await handle.ToUniTask();

            return handle.Result;
        }

        private async UniTask EnsureInitializedAsync()
        {
            if (_isInitialized)
                return;

            await InitializeAsync();
        }

        private async UniTask SetLocaleInternalAsync(string localeCode)
        {
            if (string.IsNullOrWhiteSpace(localeCode))
                localeCode = LocalizationConsts.DefaultLocaleCode;

            var locale = FindLocale(localeCode);
            if (locale == null)
            {
                throw new KeyNotFoundException(
                    $"Locale '{localeCode}' is not available. Available locales: {string.Join(", ", _availableLocaleCodes)}.");
            }

            if (CurrentLocale == locale)
                return;

            LocalizationSettings.SelectedLocale = locale;

            ClearTablesCache();

            await UniTask.Yield();
            LocaleChanged?.Invoke(CurrentLocale.Identifier.Code);
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

        private Locale FindLocale(string localeCode)
        {
            foreach (var availableLocale in _availableLocaleCodes)
            {
                if (string.Equals(availableLocale, localeCode, StringComparison.OrdinalIgnoreCase))
                    return LocalizationSettings.AvailableLocales.GetLocale(availableLocale);
            }

            return null;
        }

        private void CacheAvailableLocales()
        {
            _availableLocaleCodes.Clear();

            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
                _availableLocaleCodes.Add(locale.Identifier.Code);
        }

        private string GetRequestedLocaleCode(string localeCode)
        {
            return string.IsNullOrWhiteSpace(localeCode)
                ? LocalizationConsts.DefaultLocaleCode
                : localeCode;
        }

        private string BuildCacheKey(string tableName, Locale locale)
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