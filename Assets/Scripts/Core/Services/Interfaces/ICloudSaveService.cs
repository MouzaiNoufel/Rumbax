using System;
using Rumbax.Data;

namespace Rumbax.Core.Services
{
    /// <summary>
    /// Interface for cloud save operations.
    /// </summary>
    public interface ICloudSaveService
    {
        bool IsAuthenticated { get; }
        bool IsSyncing { get; }
        
        event Action OnSyncComplete;
        event Action<string> OnSyncFailed;
        event Action<PlayerData, PlayerData> OnConflictDetected;
        
        void Initialize();
        void SignIn(Action<bool> callback);
        void SignOut();
        void SaveToCloud(PlayerData data);
        void LoadFromCloud(Action<PlayerData> callback);
        void ResolveConflict(bool useCloud);
    }
}
