using System;

namespace Rumbax.Core.Services
{
    /// <summary>
    /// Interface for managing in-game currencies.
    /// </summary>
    public interface ICurrencyService
    {
        long Coins { get; }
        int Gems { get; }
        
        event Action<long> OnCoinsChanged;
        event Action<int> OnGemsChanged;
        
        void AddCoins(long amount);
        void AddGems(int amount);
        bool SpendCoins(long amount);
        bool SpendGems(int amount);
        bool CanAffordCoins(long amount);
        bool CanAffordGems(int amount);
    }
}
