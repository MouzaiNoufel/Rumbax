using Rumbax.Data;

namespace Rumbax.Core.Services
{
    /// <summary>
    /// Interface for save/load game data operations.
    /// </summary>
    public interface ISaveService
    {
        void SaveGame();
        void LoadGame();
        void DeleteSave();
        PlayerData GetPlayerData();
        void UpdatePlayerData(PlayerData data);
        bool HasSaveData();
    }
}
