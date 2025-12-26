using System;

namespace Rumbax.Core.Services
{
    /// <summary>
    /// Interface for remote configuration management.
    /// </summary>
    public interface IRemoteConfigService
    {
        bool IsInitialized { get; }
        
        event Action OnConfigFetched;
        event Action<string> OnConfigFetchFailed;
        
        void Initialize();
        void FetchConfig(Action<bool> callback = null);
        string GetString(string key, string defaultValue = "");
        int GetInt(string key, int defaultValue = 0);
        float GetFloat(string key, float defaultValue = 0f);
        bool GetBool(string key, bool defaultValue = false);
        T GetJson<T>(string key, T defaultValue = default);
    }
}
