using System;
using UnityEngine;
using Rumbax.Core.Events;

namespace Rumbax.Core.Services
{
    /// <summary>
    /// Service for managing in-game currencies (coins and gems).
    /// </summary>
    public class CurrencyService : ICurrencyService
    {
        private long _coins;
        private int _gems;

        public long Coins => _coins;
        public int Gems => _gems;

        public event Action<long> OnCoinsChanged;
        public event Action<int> OnGemsChanged;

        public CurrencyService()
        {
            LoadFromPlayerData();
        }

        private void LoadFromPlayerData()
        {
            if (ServiceLocator.TryGet<ISaveService>(out var saveService))
            {
                var data = saveService.GetPlayerData();
                if (data != null)
                {
                    _coins = data.Coins;
                    _gems = data.Gems;
                }
            }
        }

        public void AddCoins(long amount)
        {
            if (amount <= 0) return;

            var oldAmount = _coins;
            _coins += amount;
            
            UpdateSaveData();
            OnCoinsChanged?.Invoke(_coins);
            
            ServiceLocator.Get<IEventBus>()?.Publish(
                new CurrencyChangedEvent(Events.CurrencyType.Coins, oldAmount, _coins));
        }

        public void AddGems(int amount)
        {
            if (amount <= 0) return;

            var oldAmount = _gems;
            _gems += amount;
            
            UpdateSaveData();
            OnGemsChanged?.Invoke(_gems);
            
            ServiceLocator.Get<IEventBus>()?.Publish(
                new CurrencyChangedEvent(Events.CurrencyType.Gems, oldAmount, _gems));
        }

        public bool SpendCoins(long amount)
        {
            if (amount <= 0 || _coins < amount) return false;

            var oldAmount = _coins;
            _coins -= amount;
            
            UpdateSaveData();
            OnCoinsChanged?.Invoke(_coins);
            
            ServiceLocator.Get<IEventBus>()?.Publish(
                new CurrencyChangedEvent(Events.CurrencyType.Coins, oldAmount, _coins));
            
            return true;
        }

        public bool SpendGems(int amount)
        {
            if (amount <= 0 || _gems < amount) return false;

            var oldAmount = _gems;
            _gems -= amount;
            
            UpdateSaveData();
            OnGemsChanged?.Invoke(_gems);
            
            ServiceLocator.Get<IEventBus>()?.Publish(
                new CurrencyChangedEvent(Events.CurrencyType.Gems, oldAmount, _gems));
            
            return true;
        }

        public bool CanAffordCoins(long amount)
        {
            return _coins >= amount;
        }

        public bool CanAffordGems(int amount)
        {
            return _gems >= amount;
        }

        private void UpdateSaveData()
        {
            if (ServiceLocator.TryGet<ISaveService>(out var saveService))
            {
                var data = saveService.GetPlayerData();
                if (data != null)
                {
                    data.Coins = _coins;
                    data.Gems = _gems;
                }
            }
        }

        /// <summary>
        /// Set currency values directly (for loading saved data).
        /// </summary>
        public void SetCurrencies(long coins, int gems)
        {
            _coins = coins;
            _gems = gems;
            OnCoinsChanged?.Invoke(_coins);
            OnGemsChanged?.Invoke(_gems);
        }
    }
}
