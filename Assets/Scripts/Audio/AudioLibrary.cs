using UnityEngine;

namespace Rumbax.Audio
{
    /// <summary>
    /// Scriptable object for audio library containing all game sounds and music.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioLibrary", menuName = "Rumbax/Audio Library")]
    public class AudioLibrary : ScriptableObject
    {
        [Header("UI Sounds")]
        public AudioClip[] ButtonClick;
        public AudioClip[] ButtonBack;
        public AudioClip[] PopupOpen;
        public AudioClip[] PopupClose;
        public AudioClip[] TabSwitch;

        [Header("Currency Sounds")]
        public AudioClip[] CoinCollect;
        public AudioClip[] GemCollect;
        public AudioClip[] Purchase;
        public AudioClip[] NotEnoughFunds;

        [Header("Defender Sounds")]
        public AudioClip[] DefenderSpawn;
        public AudioClip[] DefenderMerge;
        public AudioClip[] DefenderUpgrade;
        public AudioClip[] DefenderAttack;
        public AudioClip[] DefenderSpecial;

        [Header("Combat Sounds")]
        public AudioClip[] ProjectileFire;
        public AudioClip[] ProjectileHit;
        public AudioClip[] EnemyHit;
        public AudioClip[] EnemyDeath;
        public AudioClip[] BossDeath;

        [Header("Game State Sounds")]
        public AudioClip WaveStart;
        public AudioClip WaveComplete;
        public AudioClip LevelComplete;
        public AudioClip LevelFailed;
        public AudioClip Victory;
        public AudioClip GameOver;

        [Header("Reward Sounds")]
        public AudioClip RewardClaim;
        public AudioClip AchievementUnlock;
        public AudioClip ChallengeComplete;
        public AudioClip LevelUp;

        [Header("Special Sounds")]
        public AudioClip[] Countdown;
        public AudioClip Notification;
        public AudioClip Error;

        [Header("Music Tracks")]
        public AudioClip MainMenuMusic;
        public AudioClip GameplayMusic;
        public AudioClip BossFightMusic;
        public AudioClip VictoryMusic;
        public AudioClip GameOverMusic;
        public AudioClip ShopMusic;

        /// <summary>
        /// Get sound clips by type.
        /// </summary>
        public AudioClip[] GetSoundClips(SoundType type)
        {
            switch (type)
            {
                case SoundType.ButtonClick: return ButtonClick;
                case SoundType.ButtonBack: return ButtonBack;
                case SoundType.PopupOpen: return PopupOpen;
                case SoundType.PopupClose: return PopupClose;
                case SoundType.TabSwitch: return TabSwitch;
                case SoundType.CoinCollect: return CoinCollect;
                case SoundType.GemCollect: return GemCollect;
                case SoundType.Purchase: return Purchase;
                case SoundType.NotEnoughFunds: return NotEnoughFunds;
                case SoundType.DefenderSpawn: return DefenderSpawn;
                case SoundType.DefenderMerge: return DefenderMerge;
                case SoundType.DefenderUpgrade: return DefenderUpgrade;
                case SoundType.DefenderAttack: return DefenderAttack;
                case SoundType.DefenderSpecial: return DefenderSpecial;
                case SoundType.ProjectileFire: return ProjectileFire;
                case SoundType.ProjectileHit: return ProjectileHit;
                case SoundType.EnemyHit: return EnemyHit;
                case SoundType.EnemyDeath: return EnemyDeath;
                case SoundType.BossDeath: return BossDeath;
                case SoundType.WaveStart: return WaveStart != null ? new[] { WaveStart } : null;
                case SoundType.WaveComplete: return WaveComplete != null ? new[] { WaveComplete } : null;
                case SoundType.LevelComplete: return LevelComplete != null ? new[] { LevelComplete } : null;
                case SoundType.LevelFailed: return LevelFailed != null ? new[] { LevelFailed } : null;
                case SoundType.Victory: return Victory != null ? new[] { Victory } : null;
                case SoundType.GameOver: return GameOver != null ? new[] { GameOver } : null;
                case SoundType.RewardClaim: return RewardClaim != null ? new[] { RewardClaim } : null;
                case SoundType.AchievementUnlock: return AchievementUnlock != null ? new[] { AchievementUnlock } : null;
                case SoundType.ChallengeComplete: return ChallengeComplete != null ? new[] { ChallengeComplete } : null;
                case SoundType.LevelUp: return LevelUp != null ? new[] { LevelUp } : null;
                case SoundType.Countdown: return Countdown;
                case SoundType.Notification: return Notification != null ? new[] { Notification } : null;
                case SoundType.Error: return Error != null ? new[] { Error } : null;
                default: return null;
            }
        }

        /// <summary>
        /// Get music clip by type.
        /// </summary>
        public AudioClip GetMusicClip(MusicType type)
        {
            switch (type)
            {
                case MusicType.MainMenu: return MainMenuMusic;
                case MusicType.Gameplay: return GameplayMusic;
                case MusicType.BossFight: return BossFightMusic;
                case MusicType.Victory: return VictoryMusic;
                case MusicType.GameOver: return GameOverMusic;
                case MusicType.Shop: return ShopMusic;
                default: return null;
            }
        }
    }
}
