using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Rumbax.Systems;
using System.Collections;
using System.Collections.Generic;

namespace Rumbax.Testing
{
    /// <summary>
    /// Professional Main Menu - Integrates all game systems.
    /// Complete mobile game main menu with heroes, shop, gacha, achievements, and more.
    /// </summary>
    public class ProfessionalMainMenu : MonoBehaviour
    {
        private Canvas _canvas;
        
        // Panels
        private GameObject _mainPanel;
        private GameObject _playPanel;
        private GameObject _heroesPanel;
        private GameObject _shopPanel;
        private GameObject _gachaPanel;
        private GameObject _dailyRewardsPanel;
        private GameObject _achievementsPanel;
        private GameObject _settingsPanel;
        private GameObject _skillTreePanel;
        
        // Currency
        private int _coins;
        private int _gems;
        private TextMeshProUGUI _coinsText;
        private TextMeshProUGUI _gemsText;
        private TextMeshProUGUI _playerLevelText;

        // Animation
        private float _bgAnimTime;
        private List<GameObject> _bgParticles = new List<GameObject>();

        private void Start()
        {
            InitializeSystems();
            LoadPlayerData();
            SetupCamera();
            SetupCanvas();
            CreateAnimatedBackground();
            CreateMainMenu();
            CreatePlayPanel();
            CreateHeroesPanel();
            CreateShopPanel();
            CreateGachaPanel();
            CreateDailyRewardsPanel();
            CreateAchievementsPanel();
            CreateSettingsPanel();
            CreateSkillTreePanel();
            
            ShowPanel(_mainPanel);
            CheckDailyReward();
        }

        private void Update()
        {
            AnimateBackground();
            UpdateCurrencyDisplay();
        }

        private void InitializeSystems()
        {
            // Initialize all game systems if not already present
            if (HeroSystem.Instance == null)
            {
                new GameObject("HeroSystem").AddComponent<HeroSystem>();
            }
            if (DailyRewardsSystem.Instance == null)
            {
                new GameObject("DailyRewardsSystem").AddComponent<DailyRewardsSystem>();
            }
            if (AchievementSystem.Instance == null)
            {
                new GameObject("AchievementSystem").AddComponent<AchievementSystem>();
            }
            if (SkillTreeSystem.Instance == null)
            {
                new GameObject("SkillTreeSystem").AddComponent<SkillTreeSystem>();
            }
            if (GameModeSystem.Instance == null)
            {
                new GameObject("GameModeSystem").AddComponent<GameModeSystem>();
            }
            if (GachaSystem.Instance == null)
            {
                new GameObject("GachaSystem").AddComponent<GachaSystem>();
            }
            if (SettingsSystem.Instance == null)
            {
                new GameObject("SettingsSystem").AddComponent<SettingsSystem>();
            }
            if (VFXSystem.Instance == null)
            {
                new GameObject("VFXSystem").AddComponent<VFXSystem>();
            }
            if (TutorialSystem.Instance == null)
            {
                new GameObject("TutorialSystem").AddComponent<TutorialSystem>();
            }
            if (SimpleAudioManager.Instance == null)
            {
                new GameObject("AudioManager").AddComponent<SimpleAudioManager>();
            }
        }

        private void LoadPlayerData()
        {
            _coins = PlayerPrefs.GetInt("Coins", 1000);
            _gems = PlayerPrefs.GetInt("Gems", 50);
        }

        private void SavePlayerData()
        {
            PlayerPrefs.SetInt("Coins", _coins);
            PlayerPrefs.SetInt("Gems", _gems);
            PlayerPrefs.Save();
        }

        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }
            cam.backgroundColor = new Color(0.02f, 0.03f, 0.06f);
            cam.orthographic = true;
            cam.orthographicSize = 5;
        }

        private void SetupCanvas()
        {
            GameObject canvasObj = new GameObject("MenuCanvas");
            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        private void CreateAnimatedBackground()
        {
            // Floating particles in background
            for (int i = 0; i < 15; i++)
            {
                GameObject particle = new GameObject($"BgParticle_{i}");
                particle.transform.SetParent(_canvas.transform, false);
                particle.transform.SetAsFirstSibling();

                Image img = particle.AddComponent<Image>();
                img.color = new Color(0.3f, 0.5f, 0.8f, Random.Range(0.05f, 0.15f));
                img.raycastTarget = false;

                RectTransform rect = particle.GetComponent<RectTransform>();
                rect.sizeDelta = Vector2.one * Random.Range(50f, 200f);
                rect.anchoredPosition = new Vector2(
                    Random.Range(-600f, 600f),
                    Random.Range(-1000f, 1000f));

                _bgParticles.Add(particle);
            }
        }

        private void AnimateBackground()
        {
            _bgAnimTime += Time.deltaTime;
            
            for (int i = 0; i < _bgParticles.Count; i++)
            {
                if (_bgParticles[i] == null) continue;
                
                RectTransform rect = _bgParticles[i].GetComponent<RectTransform>();
                float speed = 10f + (i * 3f);
                float xOffset = Mathf.Sin(_bgAnimTime * 0.2f + i) * 30f;
                
                rect.anchoredPosition += new Vector2(xOffset * Time.deltaTime, speed * Time.deltaTime);
                
                // Wrap around
                if (rect.anchoredPosition.y > 1100f)
                {
                    rect.anchoredPosition = new Vector2(
                        Random.Range(-600f, 600f),
                        -1100f);
                }
            }
        }

        private void CreateMainMenu()
        {
            _mainPanel = CreateBasePanel("MainPanel");

            // === TOP BAR ===
            GameObject topBar = CreateTopBar(_mainPanel.transform);

            // === LOGO ===
            CreateLabel("⚔️ RUMBAX ⚔️", new Vector2(0, 650), 64, Color.white, _mainPanel.transform, FontStyles.Bold);
            CreateLabel("MERGE  •  DEFEND  •  CONQUER", new Vector2(0, 580), 22, 
                new Color(0.6f, 0.8f, 1f), _mainPanel.transform);

            // === HERO SHOWCASE ===
            CreateHeroShowcase(new Vector2(0, 380), _mainPanel.transform);

            // === MAIN BUTTONS ===
            CreateGlowButton("▶  PLAY", new Vector2(0, 120), new Vector2(380, 90),
                new Color(0.2f, 0.8f, 0.4f), _mainPanel.transform, () => ShowPanel(_playPanel));

            // === BUTTON GRID ===
            float btnY = -30f;
            float spacing = 130f;
            
            CreateMenuButton("🦸", "HEROES", new Vector2(-160, btnY), _mainPanel.transform, 
                () => ShowPanel(_heroesPanel));
            CreateMenuButton("🎰", "SUMMON", new Vector2(0, btnY), _mainPanel.transform, 
                () => ShowPanel(_gachaPanel));
            CreateMenuButton("📈", "UPGRADES", new Vector2(160, btnY), _mainPanel.transform, 
                () => ShowPanel(_skillTreePanel));

            btnY -= spacing;
            CreateMenuButton("🏆", "ACHIEVE", new Vector2(-160, btnY), _mainPanel.transform, 
                () => ShowPanel(_achievementsPanel));
            CreateMenuButton("🎁", "DAILY", new Vector2(0, btnY), _mainPanel.transform, 
                () => ShowPanel(_dailyRewardsPanel), HasDailyReward());
            CreateMenuButton("🛒", "SHOP", new Vector2(160, btnY), _mainPanel.transform, 
                () => ShowPanel(_shopPanel));

            // === BOTTOM NAV ===
            CreateBottomNav(_mainPanel.transform);

            // === VERSION ===
            CreateLabel("v1.2.0 Professional", new Vector2(0, -520), 16, 
                new Color(0.3f, 0.35f, 0.4f), _mainPanel.transform);
        }

        private void CreatePlayPanel()
        {
            _playPanel = CreateBasePanel("PlayPanel");
            
            CreatePanelHeader("🎮 SELECT MODE", _playPanel.transform, () => ShowPanel(_mainPanel));

            float startY = 400f;
            float spacing = 150f;

            if (GameModeSystem.Instance != null)
            {
                var modes = GameModeSystem.Instance.GetAllModes();
                int index = 0;
                
                foreach (var mode in modes)
                {
                    float y = startY - (index * spacing);
                    CreateGameModeCard(mode, new Vector2(0, y), _playPanel.transform);
                    index++;
                    
                    if (index > 4) break; // Show first 5 modes
                }
            }
            else
            {
                // Fallback if system not ready
                CreateGlowButton("🎮 CLASSIC MODE", new Vector2(0, 300), new Vector2(400, 80),
                    new Color(0.3f, 0.7f, 0.4f), _playPanel.transform, StartGame);
                CreateGlowButton("♾️ ENDLESS MODE", new Vector2(0, 180), new Vector2(400, 80),
                    new Color(0.5f, 0.3f, 0.8f), _playPanel.transform, StartGame);
                CreateGlowButton("⏱️ TIME ATTACK", new Vector2(0, 60), new Vector2(400, 80),
                    new Color(1f, 0.5f, 0.2f), _playPanel.transform, StartGame);
            }

            _playPanel.SetActive(false);
        }

        private void CreateHeroesPanel()
        {
            _heroesPanel = CreateBasePanel("HeroesPanel");
            
            CreatePanelHeader("🦸 HEROES", _heroesPanel.transform, () => ShowPanel(_mainPanel));

            // Hero stats summary
            int unlockedCount = HeroSystem.Instance?.GetUnlockedHeroes().Count ?? 1;
            int totalCount = HeroSystem.Instance?.GetAllHeroes().Count ?? 15;
            CreateLabel($"Collected: {unlockedCount}/{totalCount}", new Vector2(0, 500), 24, 
                new Color(0.7f, 0.75f, 0.8f), _heroesPanel.transform);

            // Create scrollable hero grid (simplified - showing first 6)
            if (HeroSystem.Instance != null)
            {
                var heroes = HeroSystem.Instance.GetAllHeroes();
                int row = 0, col = 0;
                
                foreach (var hero in heroes)
                {
                    float x = -150f + (col * 160f);
                    float y = 350f - (row * 200f);
                    
                    CreateHeroCard(hero, new Vector2(x, y), _heroesPanel.transform);
                    
                    col++;
                    if (col >= 3)
                    {
                        col = 0;
                        row++;
                    }
                    
                    if (row >= 3) break; // Show 3x3 grid
                }
            }

            _heroesPanel.SetActive(false);
        }

        private void CreateShopPanel()
        {
            _shopPanel = CreateBasePanel("ShopPanel");
            
            CreatePanelHeader("🛒 SHOP", _shopPanel.transform, () => ShowPanel(_mainPanel));

            // Currency display
            CreateLabel($"💰 {_coins:N0}  |  💎 {_gems:N0}", new Vector2(0, 520), 28, 
                new Color(0.7f, 0.8f, 0.9f), _shopPanel.transform);

            // Coins section
            CreateLabel("─── COINS ───", new Vector2(0, 430), 24, new Color(1f, 0.85f, 0.2f), _shopPanel.transform);

            CreateShopItem("💰 1,000", "💎 20", new Vector2(-140, 340), _shopPanel.transform, 
                () => BuyCoinsWithGems(1000, 20));
            CreateShopItem("💰 5,000", "💎 80", new Vector2(140, 340), _shopPanel.transform, 
                () => BuyCoinsWithGems(5000, 80));
            CreateShopItem("💰 20,000", "💎 280", new Vector2(-140, 220), _shopPanel.transform, 
                () => BuyCoinsWithGems(20000, 280));
            CreateShopItem("💰 100,000", "💎 1200", new Vector2(140, 220), _shopPanel.transform, 
                () => BuyCoinsWithGems(100000, 1200));

            // Gems section
            CreateLabel("─── GEMS ───", new Vector2(0, 100), 24, new Color(0.4f, 0.9f, 1f), _shopPanel.transform);

            CreateShopItem("💎 60", "$0.99", new Vector2(-140, 10), _shopPanel.transform, null);
            CreateShopItem("💎 300", "$4.99", new Vector2(140, 10), _shopPanel.transform, null);
            CreateShopItem("💎 800", "$9.99", new Vector2(-140, -110), _shopPanel.transform, null);
            CreateShopItem("💎 2000", "$19.99", new Vector2(140, -110), _shopPanel.transform, null);

            // Special offers
            CreateLabel("─── SPECIAL ───", new Vector2(0, -230), 24, new Color(1f, 0.5f, 0.8f), _shopPanel.transform);
            
            CreateSpecialOffer("🎁 STARTER PACK", "1000💰 + 100💎 + Hero", "$4.99",
                new Vector2(0, -320), _shopPanel.transform);

            _shopPanel.SetActive(false);
        }

        private void CreateGachaPanel()
        {
            _gachaPanel = CreateBasePanel("GachaPanel");
            
            CreatePanelHeader("🎰 SUMMON", _gachaPanel.transform, () => ShowPanel(_mainPanel));

            // Featured banner
            CreateLabel("✨ FEATURED BANNER ✨", new Vector2(0, 480), 32, 
                new Color(1f, 0.8f, 0.3f), _gachaPanel.transform, FontStyles.Bold);

            // Banner card
            GameObject bannerCard = CreatePanel("BannerCard", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 280), new Vector2(450, 250), new Color(0.15f, 0.1f, 0.25f, 0.95f));
            bannerCard.transform.SetParent(_gachaPanel.transform, false);

            CreateLabel("🐉 Dragon Knight", new Vector2(0, 60), 28, 
                new Color(1f, 0.8f, 0.2f), bannerCard.transform, FontStyles.Bold);
            CreateLabel("LEGENDARY", new Vector2(0, 20), 18, 
                new Color(1f, 0.6f, 0.2f), bannerCard.transform);
            CreateLabel("Rate Up: 5%", new Vector2(0, -20), 16, 
                new Color(0.7f, 0.7f, 0.8f), bannerCard.transform);

            // Pity counter
            int pity = GachaSystem.Instance?.GetPityCount("legendary_up") ?? 0;
            CreateLabel($"Pity: {pity}/80", new Vector2(0, -60), 16, 
                new Color(0.6f, 0.8f, 0.6f), bannerCard.transform);

            // Summon buttons
            CreateGlowButton("SUMMON x1  💎150", new Vector2(0, 50), new Vector2(350, 60),
                new Color(0.6f, 0.3f, 0.8f), _gachaPanel.transform, () => DoSummon(1));
            CreateGlowButton("SUMMON x10  💎1350", new Vector2(0, -30), new Vector2(350, 60),
                new Color(0.7f, 0.2f, 0.6f), _gachaPanel.transform, () => DoSummon(10));

            // Rates info
            CreateLabel("Drop Rates:", new Vector2(0, -130), 20, Color.white, _gachaPanel.transform);
            CreateLabel("★ Common 35% | ★★ Rare 25% | ★★★ Epic 20%", new Vector2(0, -165), 14, 
                new Color(0.5f, 0.55f, 0.6f), _gachaPanel.transform);
            CreateLabel("★★★★ Legendary 15% | ★★★★★ Mythic 5%", new Vector2(0, -190), 14, 
                new Color(0.5f, 0.55f, 0.6f), _gachaPanel.transform);

            // Lucky wheel section
            CreateLabel("─── LUCKY WHEEL ───", new Vector2(0, -260), 24, 
                new Color(0.2f, 0.9f, 0.5f), _gachaPanel.transform);
            
            int spins = DailyRewardsSystem.Instance?.GetLuckySpins() ?? 0;
            CreateLabel($"Free Spins: {spins}", new Vector2(0, -300), 18, 
                new Color(0.6f, 0.7f, 0.6f), _gachaPanel.transform);
            
            CreateGlowButton("🎡 SPIN WHEEL", new Vector2(0, -370), new Vector2(300, 60),
                new Color(0.2f, 0.7f, 0.4f), _gachaPanel.transform, SpinWheel);

            _gachaPanel.SetActive(false);
        }

        private void CreateDailyRewardsPanel()
        {
            _dailyRewardsPanel = CreateBasePanel("DailyRewardsPanel");
            
            CreatePanelHeader("🎁 DAILY REWARDS", _dailyRewardsPanel.transform, () => ShowPanel(_mainPanel));

            int streak = DailyRewardsSystem.Instance?.GetCurrentStreak() ?? 0;
            int currentDay = DailyRewardsSystem.Instance?.GetCurrentDay() ?? 1;
            
            CreateLabel($"🔥 Day {currentDay} Streak: {streak}", new Vector2(0, 500), 28, 
                new Color(1f, 0.7f, 0.2f), _dailyRewardsPanel.transform);

            // Show week of rewards
            if (DailyRewardsSystem.Instance != null)
            {
                var rewards = DailyRewardsSystem.Instance.GetAllRewards();
                int startDay = ((currentDay - 1) / 7) * 7;
                
                for (int i = 0; i < 7; i++)
                {
                    int dayIndex = startDay + i;
                    if (dayIndex >= rewards.Count) break;
                    
                    var reward = rewards[dayIndex];
                    float x = -225f + (i * 75f);
                    float y = 350f;
                    
                    CreateDailyRewardCard(reward, dayIndex + 1, currentDay, new Vector2(x, y), 
                        _dailyRewardsPanel.transform);
                }
            }

            // Claim button
            bool canClaim = DailyRewardsSystem.Instance?.CanClaimToday() ?? false;
            Color claimColor = canClaim ? new Color(0.2f, 0.8f, 0.3f) : new Color(0.3f, 0.3f, 0.35f);
            
            Button claimBtn = CreateGlowButton(canClaim ? "CLAIM REWARD" : "COME BACK TOMORROW", 
                new Vector2(0, 180), new Vector2(350, 70), claimColor, _dailyRewardsPanel.transform, ClaimDailyReward);
            claimBtn.interactable = canClaim;

            // Reward preview
            var todayReward = DailyRewardsSystem.Instance?.GetTodaysReward();
            if (todayReward != null)
            {
                string desc = DailyRewardsSystem.Instance.GetRewardDescription(todayReward);
                CreateLabel($"Today's Reward: {desc}", new Vector2(0, 100), 22, 
                    new Color(0.9f, 0.85f, 0.5f), _dailyRewardsPanel.transform);
            }

            _dailyRewardsPanel.SetActive(false);
        }

        private void CreateAchievementsPanel()
        {
            _achievementsPanel = CreateBasePanel("AchievementsPanel");
            
            CreatePanelHeader("🏆 ACHIEVEMENTS", _achievementsPanel.transform, () => ShowPanel(_mainPanel));

            int completed = AchievementSystem.Instance?.GetCompletedCount() ?? 0;
            int total = AchievementSystem.Instance?.GetTotalCount() ?? 50;
            int points = AchievementSystem.Instance?.GetTotalPoints() ?? 0;
            
            CreateLabel($"Completed: {completed}/{total}  |  Points: {points}", 
                new Vector2(0, 500), 22, new Color(0.7f, 0.75f, 0.8f), _achievementsPanel.transform);

            // Progress bar
            float progress = AchievementSystem.Instance?.GetCompletionPercentage() ?? 0f;
            CreateProgressBar(new Vector2(0, 440), 400f, progress / 100f, 
                new Color(1f, 0.8f, 0.2f), _achievementsPanel.transform);

            // Category tabs (simplified)
            CreateLabel("─ COMBAT ─", new Vector2(-180, 370), 18, 
                new Color(1f, 0.4f, 0.4f), _achievementsPanel.transform);
            CreateLabel("─ COLLECTION ─", new Vector2(0, 370), 18, 
                new Color(0.4f, 0.8f, 1f), _achievementsPanel.transform);
            CreateLabel("─ PROGRESS ─", new Vector2(180, 370), 18, 
                new Color(0.4f, 1f, 0.5f), _achievementsPanel.transform);

            // Show sample achievements
            if (AchievementSystem.Instance != null)
            {
                var achievements = AchievementSystem.Instance.GetAchievementsByCategory(AchievementCategoryType.Combat);
                float y = 280f;
                
                foreach (var ach in achievements)
                {
                    CreateAchievementCard(ach, new Vector2(0, y), _achievementsPanel.transform);
                    y -= 90f;
                    if (y < -200f) break;
                }
            }

            _achievementsPanel.SetActive(false);
        }

        private void CreateSettingsPanel()
        {
            _settingsPanel = CreateBasePanel("SettingsPanel");
            
            CreatePanelHeader("⚙️ SETTINGS", _settingsPanel.transform, () => ShowPanel(_mainPanel));

            float startY = 450f;
            float spacing = 80f;

            // Audio
            CreateLabel("─── AUDIO ───", new Vector2(0, startY), 22, 
                new Color(0.6f, 0.65f, 0.7f), _settingsPanel.transform);
            CreateSettingToggle("🎵 Music", new Vector2(0, startY - spacing), 
                SettingsSystem.Instance?.MusicEnabled ?? true, _settingsPanel.transform, 
                (v) => SettingsSystem.Instance?.SetMusicEnabled(v));
            CreateSettingToggle("🔊 Sound Effects", new Vector2(0, startY - spacing * 2), 
                SettingsSystem.Instance?.SFXEnabled ?? true, _settingsPanel.transform,
                (v) => SettingsSystem.Instance?.SetSFXEnabled(v));

            // Gameplay
            CreateLabel("─── GAMEPLAY ───", new Vector2(0, startY - spacing * 3.5f), 22, 
                new Color(0.6f, 0.65f, 0.7f), _settingsPanel.transform);
            CreateSettingToggle("📳 Haptic Feedback", new Vector2(0, startY - spacing * 4.5f), 
                SettingsSystem.Instance?.HapticFeedback ?? true, _settingsPanel.transform,
                (v) => SettingsSystem.Instance?.SetHapticFeedback(v));
            CreateSettingToggle("💢 Screen Shake", new Vector2(0, startY - spacing * 5.5f), 
                SettingsSystem.Instance?.ScreenShakeEnabled ?? true, _settingsPanel.transform,
                (v) => SettingsSystem.Instance?.SetScreenShakeEnabled(v));
            CreateSettingToggle("📝 Damage Numbers", new Vector2(0, startY - spacing * 6.5f), 
                SettingsSystem.Instance?.ShowDamageNumbers ?? true, _settingsPanel.transform,
                (v) => SettingsSystem.Instance?.SetShowDamageNumbers(v));

            // Reset button
            CreateGlowButton("RESET PROGRESS", new Vector2(0, -380), new Vector2(300, 55),
                new Color(0.6f, 0.2f, 0.2f), _settingsPanel.transform, ResetProgress);

            _settingsPanel.SetActive(false);
        }

        private void CreateSkillTreePanel()
        {
            _skillTreePanel = CreateBasePanel("SkillTreePanel");
            
            CreatePanelHeader("📈 UPGRADES", _skillTreePanel.transform, () => ShowPanel(_mainPanel));

            int totalPoints = SkillTreeSystem.Instance?.GetTotalSkillPoints() ?? 0;
            CreateLabel($"Skill Points: {totalPoints}", new Vector2(0, 500), 24, 
                new Color(0.5f, 0.9f, 0.6f), _skillTreePanel.transform);

            // Category tabs
            CreateLabel("⚔️ ATK", new Vector2(-180, 420), 20, 
                new Color(1f, 0.4f, 0.4f), _skillTreePanel.transform);
            CreateLabel("🛡️ DEF", new Vector2(-60, 420), 20, 
                new Color(0.4f, 0.6f, 1f), _skillTreePanel.transform);
            CreateLabel("💰 ECO", new Vector2(60, 420), 20, 
                new Color(1f, 0.85f, 0.2f), _skillTreePanel.transform);
            CreateLabel("⚡ ULT", new Vector2(180, 420), 20, 
                new Color(0.8f, 0.4f, 1f), _skillTreePanel.transform);

            // Show attack skills
            if (SkillTreeSystem.Instance != null)
            {
                var skills = SkillTreeSystem.Instance.GetSkillsByCategory(SkillCategory.Attack);
                float y = 320f;
                
                foreach (var skill in skills)
                {
                    CreateSkillCard(skill, new Vector2(0, y), _skillTreePanel.transform);
                    y -= 100f;
                    if (y < -200f) break;
                }
            }

            _skillTreePanel.SetActive(false);
        }

        // === UI HELPER METHODS ===

        private GameObject CreateBasePanel(string name)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(_canvas.transform, false);
            
            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.02f, 0.03f, 0.06f, 1f);
            bg.raycastTarget = true;
            
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            
            return panel;
        }

        private GameObject CreateTopBar(Transform parent)
        {
            GameObject topBar = CreatePanel("TopBar", new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -60), new Vector2(-40, 100), new Color(0.05f, 0.07f, 0.12f, 0.95f));
            topBar.transform.SetParent(parent, false);

            // Coins
            CreateLabel("💰", new Vector2(80, 0), 32, Color.white, topBar.transform);
            _coinsText = CreateLabel(_coins.ToString("N0"), new Vector2(180, 0), 26, 
                new Color(1f, 0.85f, 0.2f), topBar.transform);

            // Gems
            CreateLabel("💎", new Vector2(320, 0), 32, Color.white, topBar.transform);
            _gemsText = CreateLabel(_gems.ToString("N0"), new Vector2(400, 0), 26, 
                new Color(0.4f, 0.9f, 1f), topBar.transform);

            // Settings
            CreateIconButton("⚙️", new Vector2(-80, 0), topBar.transform, () => ShowPanel(_settingsPanel));

            return topBar;
        }

        private void CreatePanelHeader(string title, Transform parent, System.Action backAction)
        {
            // Back button
            CreateIconButton("←", new Vector2(80, -60), parent, backAction);
            
            // Title
            CreateLabel(title, new Vector2(0, -60), 36, Color.white, parent, FontStyles.Bold);
            
            // Currency bar
            CreateLabel($"💰 {_coins:N0}  |  💎 {_gems:N0}", new Vector2(0, -120), 22, 
                new Color(0.6f, 0.65f, 0.7f), parent);
        }

        private void CreateHeroShowcase(Vector2 position, Transform parent)
        {
            var selectedHero = HeroSystem.Instance?.GetSelectedHero();
            if (selectedHero == null) return;

            GameObject showcase = CreatePanel("HeroShowcase", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                position, new Vector2(320, 180), new Color(0.1f, 0.12f, 0.18f, 0.9f));
            showcase.transform.SetParent(parent, false);

            Color rarityColor = HeroSystem.RarityColors[selectedHero.rarity];
            
            CreateLabel(HeroSystem.ClassIcons[selectedHero.heroClass], new Vector2(-80, 20), 48, 
                selectedHero.primaryColor, showcase.transform);
            CreateLabel(selectedHero.name, new Vector2(40, 40), 26, rarityColor, showcase.transform, FontStyles.Bold);
            CreateLabel(HeroSystem.RarityNames[selectedHero.rarity], new Vector2(40, 5), 16, 
                rarityColor, showcase.transform);
            CreateLabel($"Power: {HeroSystem.Instance.GetTotalPower(selectedHero):N0}", new Vector2(40, -30), 18, 
                new Color(0.7f, 0.75f, 0.8f), showcase.transform);
        }

        private void CreateMenuButton(string icon, string label, Vector2 position, Transform parent, 
            System.Action onClick, bool hasNotification = false)
        {
            GameObject btn = CreatePanel("MenuBtn", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                position, new Vector2(130, 110), new Color(0.1f, 0.12f, 0.18f, 0.9f));
            btn.transform.SetParent(parent, false);
            
            Button button = btn.AddComponent<Button>();
            button.targetGraphic = btn.GetComponent<Image>();
            button.onClick.AddListener(() => {
                SimpleAudioManager.Instance?.PlayClick();
                onClick?.Invoke();
            });

            CreateLabel(icon, new Vector2(0, 15), 36, Color.white, btn.transform);
            CreateLabel(label, new Vector2(0, -30), 14, new Color(0.6f, 0.65f, 0.7f), btn.transform);

            if (hasNotification)
            {
                GameObject notif = new GameObject("Notification");
                notif.transform.SetParent(btn.transform, false);
                Image notifImg = notif.AddComponent<Image>();
                notifImg.color = new Color(1f, 0.3f, 0.3f);
                RectTransform notifRect = notif.GetComponent<RectTransform>();
                notifRect.anchoredPosition = new Vector2(45, 40);
                notifRect.sizeDelta = new Vector2(20, 20);
            }
        }

        private void CreateGameModeCard(GameModeData mode, Vector2 position, Transform parent)
        {
            Color cardColor = mode.isUnlocked ? mode.color : new Color(0.2f, 0.2f, 0.25f, 0.9f);
            
            GameObject card = CreatePanel("ModeCard", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                position, new Vector2(450, 120), cardColor);
            card.transform.SetParent(parent, false);

            if (mode.isUnlocked)
            {
                Button btn = card.AddComponent<Button>();
                btn.targetGraphic = card.GetComponent<Image>();
                btn.onClick.AddListener(() => {
                    SimpleAudioManager.Instance?.PlayClick();
                    GameModeSystem.Instance?.SelectMode(mode.type);
                    StartGame();
                });
            }

            CreateLabel(mode.icon, new Vector2(-160, 0), 40, Color.white, card.transform);
            CreateLabel(mode.name.ToUpper(), new Vector2(20, 20), 24, Color.white, card.transform, FontStyles.Bold);
            CreateLabel(mode.description.Length > 40 ? mode.description.Substring(0, 40) + "..." : mode.description, 
                new Vector2(20, -15), 14, new Color(0.8f, 0.85f, 0.9f), card.transform);

            if (!mode.isUnlocked)
            {
                CreateLabel($"🔒 {GameModeSystem.Instance?.GetModeUnlockText(mode) ?? "Locked"}", 
                    new Vector2(0, -40), 12, new Color(0.5f, 0.5f, 0.55f), card.transform);
            }
            else if (mode.highWave > 0)
            {
                CreateLabel($"Best: Wave {mode.highWave}", new Vector2(150, 0), 14, 
                    new Color(0.9f, 0.85f, 0.4f), card.transform);
            }
        }

        private void CreateHeroCard(HeroData hero, Vector2 position, Transform parent)
        {
            Color cardColor = hero.isUnlocked 
                ? HeroSystem.RarityColors[hero.rarity] * 0.3f 
                : new Color(0.1f, 0.1f, 0.12f, 0.9f);
            
            GameObject card = CreatePanel("HeroCard", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                position, new Vector2(140, 180), cardColor);
            card.transform.SetParent(parent, false);

            if (hero.isUnlocked)
            {
                Button btn = card.AddComponent<Button>();
                btn.targetGraphic = card.GetComponent<Image>();
                btn.onClick.AddListener(() => {
                    SimpleAudioManager.Instance?.PlayClick();
                    HeroSystem.Instance?.SelectHero(hero.id);
                });
            }

            string icon = hero.isUnlocked ? HeroSystem.ClassIcons[hero.heroClass] : "🔒";
            CreateLabel(icon, new Vector2(0, 35), 42, Color.white, card.transform);
            
            CreateLabel(hero.isUnlocked ? hero.name : "???", new Vector2(0, -20), 16, 
                hero.isUnlocked ? HeroSystem.RarityColors[hero.rarity] : Color.gray, card.transform);
            
            if (hero.isUnlocked)
            {
                int stars = (int)hero.rarity + 1;
                string starStr = new string('★', stars);
                CreateLabel(starStr, new Vector2(0, -50), 12, 
                    HeroSystem.RarityColors[hero.rarity], card.transform);
                
                CreateLabel($"Lv.{hero.level}", new Vector2(0, -70), 12, 
                    new Color(0.6f, 0.65f, 0.7f), card.transform);
            }
        }

        private void CreateDailyRewardCard(DailyRewardData reward, int day, int currentDay, 
            Vector2 position, Transform parent)
        {
            bool isClaimed = day < currentDay || (day == currentDay && !DailyRewardsSystem.Instance.CanClaimToday());
            bool isToday = day == currentDay && DailyRewardsSystem.Instance.CanClaimToday();
            
            Color cardColor = isClaimed ? new Color(0.15f, 0.2f, 0.15f, 0.9f)
                : isToday ? new Color(0.2f, 0.3f, 0.5f, 0.95f)
                : new Color(0.1f, 0.1f, 0.12f, 0.9f);
            
            GameObject card = CreatePanel("DayCard", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                position, new Vector2(65, 90), cardColor);
            card.transform.SetParent(parent, false);

            CreateLabel($"D{day}", new Vector2(0, 28), 14, 
                isToday ? Color.yellow : Color.white, card.transform);
            
            string icon = DailyRewardsSystem.Instance?.GetRewardIcon(reward.type) ?? "?";
            CreateLabel(icon, new Vector2(0, -5), 24, Color.white, card.transform);
            
            CreateLabel(reward.amount.ToString(), new Vector2(0, -30), 12, 
                DailyRewardsSystem.Instance?.GetRewardColor(reward.type) ?? Color.white, card.transform);

            if (isClaimed)
            {
                CreateLabel("✓", new Vector2(0, 0), 32, new Color(0.3f, 0.8f, 0.3f, 0.5f), card.transform);
            }
        }

        private void CreateAchievementCard(AchievementEntry ach, Vector2 position, Transform parent)
        {
            Color cardColor = ach.isCompleted && !ach.isRewardClaimed 
                ? new Color(0.2f, 0.3f, 0.2f, 0.9f)
                : new Color(0.08f, 0.1f, 0.14f, 0.9f);
            
            GameObject card = CreatePanel("AchCard", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                position, new Vector2(450, 75), cardColor);
            card.transform.SetParent(parent, false);

            CreateLabel(ach.icon, new Vector2(-180, 0), 32, 
                AchievementSystem.TierColors[ach.tier], card.transform);
            CreateLabel(ach.name, new Vector2(-30, 12), 18, Color.white, card.transform);
            CreateLabel(ach.description, new Vector2(-30, -12), 12, 
                new Color(0.5f, 0.55f, 0.6f), card.transform);

            // Progress
            float progress = AchievementSystem.Instance?.GetProgressPercentage(ach) ?? 0f;
            CreateProgressBar(new Vector2(100, -25), 150f, progress, 
                AchievementSystem.TierColors[ach.tier], card.transform);

            if (ach.isCompleted && !ach.isRewardClaimed)
            {
                Button claimBtn = CreateGlowButton("CLAIM", new Vector2(180, 0), new Vector2(70, 35),
                    new Color(0.2f, 0.7f, 0.3f), card.transform, () => {
                        AchievementSystem.Instance?.ClaimReward(ach.id, ref _coins, ref _gems);
                        SavePlayerData();
                    });
            }
            else if (ach.isRewardClaimed)
            {
                CreateLabel("✓", new Vector2(180, 0), 28, new Color(0.4f, 0.7f, 0.4f), card.transform);
            }
        }

        private void CreateSkillCard(SkillNode skill, Vector2 position, Transform parent)
        {
            Color cardColor = new Color(0.08f, 0.1f, 0.14f, 0.9f);
            
            GameObject card = CreatePanel("SkillCard", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                position, new Vector2(450, 85), cardColor);
            card.transform.SetParent(parent, false);

            CreateLabel(skill.icon, new Vector2(-180, 0), 32, 
                SkillTreeSystem.CategoryColors[skill.category], card.transform);
            CreateLabel(skill.name, new Vector2(-30, 18), 20, Color.white, card.transform);
            CreateLabel(SkillTreeSystem.Instance?.GetSkillDescription(skill) ?? skill.description, 
                new Vector2(-30, -8), 12, new Color(0.5f, 0.55f, 0.6f), card.transform);
            
            CreateLabel($"Lv.{skill.currentLevel}/{skill.maxLevel}", new Vector2(-30, -28), 12, 
                new Color(0.6f, 0.8f, 0.6f), card.transform);

            if (skill.currentLevel < skill.maxLevel)
            {
                int cost = SkillTreeSystem.Instance?.GetUpgradeCost(skill.id) ?? 0;
                string costStr = skill.usesGems ? $"💎{cost}" : $"💰{cost}";
                bool canAfford = SkillTreeSystem.Instance?.CanUpgrade(skill.id, _coins, _gems) ?? false;
                
                Color btnColor = canAfford ? new Color(0.2f, 0.6f, 0.3f) : new Color(0.3f, 0.3f, 0.35f);
                Button upgradeBtn = CreateGlowButton(costStr, new Vector2(170, 0), new Vector2(80, 40),
                    btnColor, card.transform, () => {
                        if (SkillTreeSystem.Instance?.TryUpgrade(skill.id, ref _coins, ref _gems) ?? false)
                        {
                            SavePlayerData();
                            SimpleAudioManager.Instance?.PlayUpgrade();
                        }
                    });
                upgradeBtn.interactable = canAfford;
            }
            else
            {
                CreateLabel("MAX", new Vector2(170, 0), 18, new Color(1f, 0.85f, 0.2f), card.transform);
            }
        }

        private void CreateProgressBar(Vector2 position, float width, float fillAmount, Color color, Transform parent)
        {
            // Background
            GameObject bg = CreatePanel("ProgressBg", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                position, new Vector2(width, 12), new Color(0.1f, 0.1f, 0.12f, 0.9f));
            bg.transform.SetParent(parent, false);

            // Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(bg.transform, false);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = color;
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(fillAmount, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
        }

        private void CreateBottomNav(Transform parent)
        {
            GameObject nav = CreatePanel("BottomNav", new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 40), new Vector2(-40, 80), new Color(0.05f, 0.07f, 0.1f, 0.95f));
            nav.transform.SetParent(parent, false);

            // Social buttons
            CreateIconButton("👥", new Vector2(-120, 0), nav.transform, null);
            CreateIconButton("📊", new Vector2(-40, 0), nav.transform, null);
            CreateIconButton("❓", new Vector2(40, 0), nav.transform, null);
            CreateIconButton("📢", new Vector2(120, 0), nav.transform, null);
        }

        private Button CreateGlowButton(string text, Vector2 position, Vector2 size, Color color, 
            Transform parent, System.Action onClick)
        {
            GameObject btn = CreatePanel("GlowBtn", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                position, size, color);
            btn.transform.SetParent(parent, false);

            // Glow effect
            GameObject glow = new GameObject("Glow");
            glow.transform.SetParent(btn.transform, false);
            glow.transform.SetAsFirstSibling();
            Image glowImg = glow.AddComponent<Image>();
            glowImg.color = new Color(color.r, color.g, color.b, 0.3f);
            glowImg.raycastTarget = false;
            RectTransform glowRect = glow.GetComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.sizeDelta = new Vector2(10, 10);
            glowRect.anchoredPosition = new Vector2(0, -3);

            Button button = btn.AddComponent<Button>();
            button.targetGraphic = btn.GetComponent<Image>();
            button.onClick.AddListener(() => {
                SimpleAudioManager.Instance?.PlayClick();
                onClick?.Invoke();
            });

            CreateLabel(text, Vector2.zero, 22, Color.white, btn.transform, FontStyles.Bold);

            return button;
        }

        private void CreateShopItem(string item, string price, Vector2 position, Transform parent, System.Action onClick)
        {
            GameObject card = CreatePanel("ShopItem", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                position, new Vector2(200, 90), new Color(0.08f, 0.1f, 0.14f, 0.9f));
            card.transform.SetParent(parent, false);

            if (onClick != null)
            {
                Button btn = card.AddComponent<Button>();
                btn.targetGraphic = card.GetComponent<Image>();
                btn.onClick.AddListener(() => {
                    SimpleAudioManager.Instance?.PlayClick();
                    onClick();
                });
            }

            CreateLabel(item, new Vector2(0, 15), 18, Color.white, card.transform);
            CreateLabel(price, new Vector2(0, -20), 16, new Color(0.5f, 0.9f, 0.5f), card.transform);
        }

        private void CreateSpecialOffer(string title, string content, string price, Vector2 position, Transform parent)
        {
            GameObject card = CreatePanel("SpecialOffer", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                position, new Vector2(420, 80), new Color(0.25f, 0.15f, 0.3f, 0.95f));
            card.transform.SetParent(parent, false);

            Button btn = card.AddComponent<Button>();
            btn.targetGraphic = card.GetComponent<Image>();

            CreateLabel(title, new Vector2(-80, 15), 20, new Color(1f, 0.8f, 0.3f), card.transform, FontStyles.Bold);
            CreateLabel(content, new Vector2(-80, -15), 14, new Color(0.7f, 0.75f, 0.8f), card.transform);
            CreateLabel(price, new Vector2(160, 0), 22, new Color(0.4f, 1f, 0.5f), card.transform, FontStyles.Bold);
        }

        private void CreateSettingToggle(string label, Vector2 position, bool initialValue, 
            Transform parent, System.Action<bool> onChanged)
        {
            GameObject row = CreatePanel("SettingRow", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                position, new Vector2(400, 50), new Color(0.08f, 0.1f, 0.14f, 0.9f));
            row.transform.SetParent(parent, false);

            CreateLabel(label, new Vector2(-80, 0), 20, Color.white, row.transform);

            // Toggle button
            GameObject toggleBtn = CreatePanel("Toggle", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(140, 0), new Vector2(60, 30), 
                initialValue ? new Color(0.2f, 0.7f, 0.3f) : new Color(0.3f, 0.3f, 0.35f));
            toggleBtn.transform.SetParent(row.transform, false);

            TextMeshProUGUI toggleText = CreateLabel(initialValue ? "ON" : "OFF", Vector2.zero, 16, 
                Color.white, toggleBtn.transform);

            bool currentValue = initialValue;
            Button btn = toggleBtn.AddComponent<Button>();
            btn.targetGraphic = toggleBtn.GetComponent<Image>();
            btn.onClick.AddListener(() => {
                currentValue = !currentValue;
                toggleBtn.GetComponent<Image>().color = currentValue 
                    ? new Color(0.2f, 0.7f, 0.3f) : new Color(0.3f, 0.3f, 0.35f);
                toggleText.text = currentValue ? "ON" : "OFF";
                SimpleAudioManager.Instance?.PlayClick();
                onChanged?.Invoke(currentValue);
            });
        }

        // === COMMON UI HELPERS ===

        private GameObject CreatePanel(string name, Vector2 anchorMin, Vector2 anchorMax, 
            Vector2 position, Vector2 size, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(_canvas.transform, false);
            
            Image img = panel.AddComponent<Image>();
            img.color = color;
            
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            
            return panel;
        }

        private TextMeshProUGUI CreateLabel(string text, Vector2 position, int size, Color color, 
            Transform parent, FontStyles style = FontStyles.Normal)
        {
            GameObject obj = new GameObject("Label");
            obj.transform.SetParent(parent, false);
            
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(500, 50);
            
            return tmp;
        }

        private void CreateIconButton(string icon, Vector2 position, Transform parent, System.Action onClick)
        {
            GameObject btn = new GameObject("IconBtn");
            btn.transform.SetParent(parent, false);
            
            Image bg = btn.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.18f, 0.25f, 0.9f);
            
            Button button = btn.AddComponent<Button>();
            button.targetGraphic = bg;
            if (onClick != null)
            {
                button.onClick.AddListener(() => {
                    SimpleAudioManager.Instance?.PlayClick();
                    onClick();
                });
            }
            
            RectTransform rect = btn.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(50, 50);

            CreateLabel(icon, Vector2.zero, 28, Color.white, btn.transform);
        }

        // === ACTIONS ===

        private void ShowPanel(GameObject panel)
        {
            _mainPanel?.SetActive(false);
            _playPanel?.SetActive(false);
            _heroesPanel?.SetActive(false);
            _shopPanel?.SetActive(false);
            _gachaPanel?.SetActive(false);
            _dailyRewardsPanel?.SetActive(false);
            _achievementsPanel?.SetActive(false);
            _settingsPanel?.SetActive(false);
            _skillTreePanel?.SetActive(false);

            panel?.SetActive(true);
        }

        private void StartGame()
        {
            SimpleAudioManager.Instance?.PlayWaveStart();
            
            // Check if should show tutorial
            if (TutorialSystem.Instance != null && TutorialSystem.Instance.ShouldShowTutorial())
            {
                // Will start tutorial in gameplay scene
            }
            
            SceneManager.LoadScene("GameplayScene");
        }

        private void CheckDailyReward()
        {
            if (DailyRewardsSystem.Instance != null && DailyRewardsSystem.Instance.CanClaimToday())
            {
                // Could show popup here
                Debug.Log("[MainMenu] Daily reward available!");
            }
        }

        private bool HasDailyReward()
        {
            return DailyRewardsSystem.Instance?.CanClaimToday() ?? false;
        }

        private void ClaimDailyReward()
        {
            if (DailyRewardsSystem.Instance == null) return;
            
            var reward = DailyRewardsSystem.Instance.ClaimReward();
            if (reward != null)
            {
                DailyRewardsSystem.Instance.ApplyReward(reward, ref _coins, ref _gems);
                SavePlayerData();
                SimpleAudioManager.Instance?.PlaySuccess();
                
                // Refresh panel
                ShowPanel(_dailyRewardsPanel);
            }
        }

        private void DoSummon(int count)
        {
            if (GachaSystem.Instance == null) return;
            
            int cost = 150 * count;
            if (_gems < cost)
            {
                SimpleAudioManager.Instance?.PlayDefeat();
                return;
            }

            var results = GachaSystem.Instance.MultiPull("legendary_up", count, ref _gems);
            if (results != null)
            {
                SavePlayerData();
                SimpleAudioManager.Instance?.PlayVictory();
                // Could show summon animation/results
            }
        }

        private void SpinWheel()
        {
            if (GachaSystem.Instance == null || !GachaSystem.Instance.CanSpin())
            {
                SimpleAudioManager.Instance?.PlayDefeat();
                return;
            }

            var result = GachaSystem.Instance.SpinWheel();
            if (result != null)
            {
                GachaSystem.Instance.ApplyWheelReward(result, ref _coins, ref _gems);
                SavePlayerData();
                SimpleAudioManager.Instance?.PlaySuccess();
            }
        }

        private void BuyCoinsWithGems(int coins, int gemCost)
        {
            if (_gems < gemCost)
            {
                SimpleAudioManager.Instance?.PlayDefeat();
                return;
            }

            _gems -= gemCost;
            _coins += coins;
            SavePlayerData();
            SimpleAudioManager.Instance?.PlayCoin();
        }

        private void ResetProgress()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void UpdateCurrencyDisplay()
        {
            if (_coinsText != null) _coinsText.text = _coins.ToString("N0");
            if (_gemsText != null) _gemsText.text = _gems.ToString("N0");
        }
    }
}
