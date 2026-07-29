using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.Localization
{
    public interface ILocalizationService
    {
        bool IsInitialized { get; }
        string CurrentLocaleCode { get; }
        IReadOnlyList<string> AvailableLocaleCodes { get; }

        event Action<string> LocaleChanged;

        UniTask InitializeAsync(string localeCode = null);
        UniTask SetLocaleAsync(string localeCode);
        UniTask<string> GetStringAsync(string key, string tableName = LocalizationService.DefaultStringTableName, params object[] arguments);
        UniTask<TAsset> GetAssetAsync<TAsset>(string key, string tableName = LocalizationService.DefaultAssetTableName) where TAsset : UnityEngine.Object;
    }
}


