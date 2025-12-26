using System.Collections.Generic;

namespace Rumbax.Core.Services
{
    /// <summary>
    /// Interface for analytics tracking.
    /// </summary>
    public interface IAnalyticsService
    {
        void Initialize();
        void LogEvent(string eventName);
        void LogEvent(string eventName, Dictionary<string, object> parameters);
        void LogLevelStart(int level);
        void LogLevelComplete(int level, int score, float duration);
        void LogLevelFail(int level, string reason);
        void LogPurchase(string productId, decimal amount, string currency);
        void LogAdWatched(string adType, string placement);
        void SetUserProperty(string property, string value);
    }
}
