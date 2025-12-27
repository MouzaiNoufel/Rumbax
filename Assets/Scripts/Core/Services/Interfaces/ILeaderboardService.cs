using System;
using System.Collections.Generic;

namespace Rumbax.Core.Services
{
    /// <summary>
    /// Interface for leaderboard operations.
    /// </summary>
    public interface ILeaderboardService
    {
        void Initialize();
        void SubmitScore(string leaderboardId, long score, Action<bool> callback = null);
        void GetTopScores(string leaderboardId, int count, Action<List<LeaderboardEntry>> callback);
        void GetPlayerScore(string leaderboardId, Action<LeaderboardEntry> callback);
        void ShowLeaderboardUI(string leaderboardId = null);
        
        // Additional methods for UI
        void LoadLeaderboard(Systems.LeaderboardType type, Action<List<LeaderboardEntry>> callback = null);
        string GetPlayerDisplayName();
        void GetPlayerRank(Systems.LeaderboardType type, Action<int> callback);
        string GetPlayerId();
    }

    [Serializable]
    public class LeaderboardEntry
    {
        public string PlayerId;
        public string PlayerName;
        public long Score;
        public int Rank;
        public string AvatarUrl;
    }
}
