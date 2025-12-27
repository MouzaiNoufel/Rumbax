using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Rumbax.Systems;
using System.Collections;
using System.Collections.Generic;

namespace Rumbax.Testing
{
    /// <summary>
    /// Professional Gameplay Integration - Connects all systems to gameplay.
    /// Manages gameplay events and their effects on achievements, quests, VFX, etc.
    /// </summary>
    public class GameplayIntegration : MonoBehaviour
    {
        public static GameplayIntegration Instance { get; private set; }

        // Session stats
        private int _sessionKills;
        private int _sessionBossKills;
        private int _sessionMerges;
        private int _sessionCoins;
        private int _sessionWave;
        private int _sessionHighestCombo;
        private int _currentCombo;
        private float _comboTimer;

        // Settings cache
        private float _damageMultiplier = 1f;
        private float _coinMultiplier = 1f;
        private float _critChanceBonus = 0f;
        private float _critDamageBonus = 0f;

        // Game state
        private bool _feverModeActive;
        private bool _ultimateReady;
        private float _ultimateCharge;
        private float _feverMultiplier = 2f;

        // UI references
        private TextMeshProUGUI _comboText;
        private TextMeshProUGUI _waveText;
        private TextMeshProUGUI _killsText;
        private TextMeshProUGUI _coinsText;
        private GameObject _feverIndicator;
        private GameObject _ultimateButton;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            CacheModifiers();
            SubscribeToEvents();
        }

        private void Update()
        {
            UpdateComboTimer();
            UpdateUltimateCharge();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void CacheModifiers()
        {
            // Combine all multipliers from different systems
            float baseDamage = 1f;
            float baseCoin = 1f;
            float baseCrit = 0.05f;
            float baseCritDmg = 1.5f;

            // Skill Tree bonuses
            if (SkillTreeSystem.Instance != null)
            {
                baseDamage *= SkillTreeSystem.Instance.GetDamageMultiplier();
                baseCoin *= SkillTreeSystem.Instance.GetCoinMultiplier();
                baseCrit += SkillTreeSystem.Instance.GetCritChanceBonus();
                baseCritDmg = SkillTreeSystem.Instance.GetCritDamageMultiplier();
            }

            // Prestige bonuses
            if (PrestigeSystem.Instance != null)
            {
                baseDamage *= PrestigeSystem.Instance.GetDamageMultiplier();
                baseCoin *= PrestigeSystem.Instance.GetCoinMultiplier();
                baseCritDmg += PrestigeSystem.Instance.GetCritDamageBonus() / 100f;
            }

            // Hero bonuses
            if (HeroSystem.Instance != null)
            {
                var hero = HeroSystem.Instance.GetSelectedHero();
                if (hero != null)
                {
                    baseDamage *= (hero.baseDamage / 100f);
                    // Additional hero-specific bonuses could be applied here
                }
            }

            // Game mode modifiers
            if (GameModeSystem.Instance != null)
            {
                var modeData = GameModeSystem.Instance.GetCurrentModeData();
                if (modeData != null)
                {
                    baseCoin *= modeData.coinMultiplier;
                }
            }

            _damageMultiplier = baseDamage;
            _coinMultiplier = baseCoin;
            _critChanceBonus = baseCrit;
            _critDamageBonus = baseCritDmg;

            Debug.Log($"[Integration] Damage: {_damageMultiplier:F2}x, Coins: {_coinMultiplier:F2}x, Crit: {_critChanceBonus:P0}");
        }

        private void SubscribeToEvents()
        {
            // Achievement events
            if (AchievementSystem.Instance != null)
            {
                AchievementSystem.Instance.OnAchievementCompleted += OnAchievementCompleted;
            }

            // Quest events
            if (QuestSystem.Instance != null)
            {
                QuestSystem.Instance.OnQuestCompleted += OnQuestCompleted;
            }

            // Battle Pass events
            if (BattlePassSystem.Instance != null)
            {
                BattlePassSystem.Instance.OnLevelUp += OnBattlePassLevelUp;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (AchievementSystem.Instance != null)
            {
                AchievementSystem.Instance.OnAchievementCompleted -= OnAchievementCompleted;
            }

            if (QuestSystem.Instance != null)
            {
                QuestSystem.Instance.OnQuestCompleted -= OnQuestCompleted;
            }

            if (BattlePassSystem.Instance != null)
            {
                BattlePassSystem.Instance.OnLevelUp -= OnBattlePassLevelUp;
            }
        }

        // === COMBAT EVENTS ===

        public void OnEnemyKilled(Vector3 position, int baseCoins = 10, bool isBoss = false, int damage = 0)
        {
            _sessionKills++;
            _currentCombo++;
            _comboTimer = 3f; // Reset combo timer

            if (_currentCombo > _sessionHighestCombo)
                _sessionHighestCombo = _currentCombo;

            // Calculate final coins
            float coins = baseCoins * _coinMultiplier;
            if (_feverModeActive) coins *= _feverMultiplier;
            coins *= (1f + _currentCombo * 0.01f); // Combo bonus
            int finalCoins = Mathf.RoundToInt(coins);
            _sessionCoins += finalCoins;

            // VFX
            if (VFXSystem.Instance != null)
            {
                VFXSystem.Instance.OnEnemyDeath(position);
                VFXSystem.Instance.ShowFloatingText(position, $"+{finalCoins}", Color.yellow, 0.8f);
                
                if (_currentCombo > 0 && _currentCombo % 10 == 0)
                {
                    VFXSystem.Instance.ShowCenterText($"{_currentCombo}x COMBO!", new Color(1f, 0.5f, 0f), 1.5f);
                }
            }

            // Sound
            SimpleAudioManager.Instance?.PlayHit();
            if (_currentCombo >= 10 && _currentCombo % 10 == 0)
            {
                SimpleAudioManager.Instance?.PlaySuccess();
            }

            // Update systems
            AchievementSystem.Instance?.OnEnemyKilled();
            QuestSystem.Instance?.OnEnemyKilled();
            AchievementSystem.Instance?.OnComboReached(_currentCombo);
            QuestSystem.Instance?.OnComboReached(_currentCombo);

            // Ultimate charge
            _ultimateCharge = Mathf.Min(100f, _ultimateCharge + 2f);
            if (_ultimateCharge >= 100f && !_ultimateReady)
            {
                _ultimateReady = true;
                VFXSystem.Instance?.ShowCenterText("⚡ ULTIMATE READY!", new Color(1f, 0.8f, 0.2f), 1.5f);
            }

            // Boss-specific
            if (isBoss)
            {
                _sessionBossKills++;
                AchievementSystem.Instance?.OnBossKilled();
                QuestSystem.Instance?.OnBossKilled();
                VFXSystem.Instance?.OnEnemyDeath(position, true);
                SimpleAudioManager.Instance?.PlayVictory();
            }
        }

        public void OnMerge(Vector3 position, int newTier)
        {
            _sessionMerges++;

            // VFX
            VFXSystem.Instance?.OnMerge(position, newTier);
            if (newTier >= 5)
            {
                VFXSystem.Instance?.ShowCenterText($"TIER {newTier}!", new Color(0.8f, 0.3f, 1f), 1.2f);
            }

            // Sound
            SimpleAudioManager.Instance?.PlayMerge();

            // Update systems
            AchievementSystem.Instance?.OnMerge();
            QuestSystem.Instance?.OnMerge();
        }

        public void OnWaveCompleted(int wave)
        {
            _sessionWave = wave;

            // Record to prestige system
            PrestigeSystem.Instance?.RecordWave(wave);

            // VFX
            VFXSystem.Instance?.OnWaveComplete(wave);
            VFXSystem.Instance?.ShowCenterText($"WAVE {wave} COMPLETE!", Color.cyan, 2f);

            // Sound
            SimpleAudioManager.Instance?.PlayWaveStart();

            // Update systems
            AchievementSystem.Instance?.OnWaveReached(wave);
            QuestSystem.Instance?.OnWaveReached(wave);

            // Grant battle pass XP
            int xpGained = 10 + wave * 2;
            BattlePassSystem.Instance?.AddXP(xpGained);

            // Check fever mode
            if (wave % 5 == 0 && !_feverModeActive)
            {
                StartCoroutine(ActivateFeverMode(10f));
            }
        }

        public void OnDamageTaken(float damage, Vector3 position)
        {
            // Break combo on damage
            if (_currentCombo > 5)
            {
                VFXSystem.Instance?.ShowFloatingText(position, "COMBO LOST!", Color.red, 1f);
            }
            _currentCombo = 0;

            // VFX
            if (SettingsSystem.Instance?.ScreenShakeEnabled ?? true)
            {
                VFXSystem.Instance?.ScreenShake(0.3f, 0.2f);
            }

            // Sound
            SimpleAudioManager.Instance?.PlayDefeat();
        }

        public void OnUltimateActivated(Vector3 position)
        {
            if (!_ultimateReady) return;

            _ultimateReady = false;
            _ultimateCharge = 0f;

            // Massive VFX
            VFXSystem.Instance?.OnUltimate();
            VFXSystem.Instance?.FlashScreen(new Color(1f, 0.9f, 0.5f, 0.5f), 0.5f);
            VFXSystem.Instance?.ScreenShake(0.8f, 0.5f);
            VFXSystem.Instance?.ShowCenterText("⚡ ULTIMATE! ⚡", new Color(1f, 0.8f, 0.2f), 2f);

            // Sound
            SimpleAudioManager.Instance?.PlayVictory();

            // Update systems
            AchievementSystem.Instance?.OnUltimateUsed();
        }

        public void OnSkillUsed()
        {
            QuestSystem.Instance?.OnSkillUsed();
        }

        public void OnCoinsCollected(int amount)
        {
            int boostedAmount = Mathf.RoundToInt(amount * _coinMultiplier);
            _sessionCoins += boostedAmount;

            QuestSystem.Instance?.OnCoinsCollected(boostedAmount);
        }

        public void OnCoinsSpent(int amount)
        {
            QuestSystem.Instance?.OnCoinsSpent(amount);
        }

        public void OnUpgrade()
        {
            QuestSystem.Instance?.OnUpgrade();
        }

        // === GAME FLOW ===

        public void OnGameStart()
        {
            _sessionKills = 0;
            _sessionBossKills = 0;
            _sessionMerges = 0;
            _sessionCoins = 0;
            _sessionWave = PrestigeSystem.Instance?.GetStartingWave() ?? 1;
            _sessionHighestCombo = 0;
            _currentCombo = 0;
            _ultimateCharge = 0;
            _ultimateReady = false;

            CacheModifiers();

            // Check if tutorial needed
            if (TutorialSystem.Instance != null && TutorialSystem.Instance.ShouldShowTutorial())
            {
                TutorialSystem.Instance.StartTutorial();
            }

            Debug.Log("[Integration] Game started");
        }

        public void OnGameEnd(bool victory)
        {
            // Record to quest system
            QuestSystem.Instance?.OnGameCompleted();

            // VFX
            if (victory)
            {
                VFXSystem.Instance?.FlashScreen(new Color(0.5f, 1f, 0.5f, 0.3f), 1f);
                VFXSystem.Instance?.ShowCenterText("VICTORY!", Color.green, 3f);
                SimpleAudioManager.Instance?.PlayVictory();
            }
            else
            {
                VFXSystem.Instance?.ShowCenterText($"GAME OVER\nWave {_sessionWave}", Color.red, 3f);
                SimpleAudioManager.Instance?.PlayDefeat();
            }

            // Grant battle pass XP based on performance
            int totalXP = _sessionWave * 5 + _sessionKills / 10 + _sessionBossKills * 20;
            BattlePassSystem.Instance?.AddXP(totalXP);

            Debug.Log($"[Integration] Game ended - Wave: {_sessionWave}, Kills: {_sessionKills}, Coins: {_sessionCoins}");
        }

        // === FEVER MODE ===

        private IEnumerator ActivateFeverMode(float duration)
        {
            _feverModeActive = true;
            _feverMultiplier = 2f;

            VFXSystem.Instance?.ShowCenterText("🔥 FEVER MODE! 🔥", new Color(1f, 0.3f, 0f), 2f);
            VFXSystem.Instance?.FlashScreen(new Color(1f, 0.5f, 0f, 0.3f), 0.5f);
            SimpleAudioManager.Instance?.PlaySuccess();

            yield return new WaitForSeconds(duration);

            _feverModeActive = false;
            VFXSystem.Instance?.ShowCenterText("Fever ended", Color.gray, 1f);
        }

        // === COMBO SYSTEM ===

        private void UpdateComboTimer()
        {
            if (_currentCombo > 0)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0f)
                {
                    if (_currentCombo >= 10)
                    {
                        VFXSystem.Instance?.ShowCenterText($"Combo: {_currentCombo}x", Color.gray, 1f);
                    }
                    _currentCombo = 0;
                }
            }
        }

        // === ULTIMATE ===

        private void UpdateUltimateCharge()
        {
            // Passive charge
            if (!_ultimateReady)
            {
                _ultimateCharge = Mathf.Min(100f, _ultimateCharge + Time.deltaTime * 0.5f);
            }
        }

        // === EVENT HANDLERS ===

        private void OnAchievementCompleted(AchievementEntry achievement)
        {
            VFXSystem.Instance?.ShowCenterText($"🏆 {achievement.name}", 
                AchievementSystem.TierColors[achievement.tier], 2f);
            SimpleAudioManager.Instance?.PlaySuccess();
        }

        private void OnQuestCompleted(Quest quest)
        {
            VFXSystem.Instance?.ShowCenterText($"✓ {quest.name}", Color.green, 1.5f);
            SimpleAudioManager.Instance?.PlaySuccess();
        }

        private void OnBattlePassLevelUp(int newLevel)
        {
            VFXSystem.Instance?.ShowCenterText($"Battle Pass Lv.{newLevel}!", 
                new Color(1f, 0.8f, 0.2f), 2f);
            VFXSystem.Instance?.FlashScreen(new Color(1f, 0.9f, 0.5f, 0.3f), 0.5f);
            SimpleAudioManager.Instance?.PlayVictory();
        }

        // === DAMAGE CALCULATION ===

        public int CalculateDamage(int baseDamage)
        {
            float damage = baseDamage * _damageMultiplier;

            // Fever bonus
            if (_feverModeActive)
            {
                damage *= 1.5f;
            }

            // Combo bonus
            damage *= (1f + _currentCombo * 0.005f);

            // Critical hit
            bool isCrit = Random.value < _critChanceBonus;
            if (isCrit)
            {
                damage *= _critDamageBonus;
            }

            return Mathf.RoundToInt(damage);
        }

        public bool IsCriticalHit()
        {
            return Random.value < _critChanceBonus;
        }

        // === GETTERS ===

        public int CurrentCombo => _currentCombo;
        public int SessionKills => _sessionKills;
        public int SessionWave => _sessionWave;
        public int SessionCoins => _sessionCoins;
        public bool IsFeverActive => _feverModeActive;
        public bool IsUltimateReady => _ultimateReady;
        public float UltimateCharge => _ultimateCharge;
        public float DamageMultiplier => _damageMultiplier;
        public float CoinMultiplier => _coinMultiplier;
    }
}
