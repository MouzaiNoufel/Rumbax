using System;
using System.Collections.Generic;

namespace Rumbax.Core.Services
{
    /// <summary>
    /// Interface for achievement system.
    /// </summary>
    public interface IAchievementService
    {
        event Action<string> OnAchievementUnlocked;
        
        void Initialize();
        void UnlockAchievement(string achievementId);
        void IncrementAchievement(string achievementId, int steps);
        void GetAchievements(Action<List<AchievementData>> callback);
        void ShowAchievementsUI();
        bool IsUnlocked(string achievementId);
        
        // Additional methods for UI
        int GetUnlockedCount();
        int GetTotalCount();
        int GetUnclaimedCount();
        List<Systems.Achievement> GetAllAchievements();
        List<Systems.Achievement> GetAchievementsByCategory(Systems.AchievementCategory category);
        bool ClaimReward(string achievementId);
    }

    [Serializable]
    public class AchievementData
    {
        public string Id;
        public string Name;
        public string Description;
        public bool IsUnlocked;
        public float Progress;
        public int CurrentSteps;
        public int TotalSteps;
    }
}
