using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Rumbax.Testing
{
    /// <summary>
    /// Modern Main Menu with all navigation options.
    /// </summary>
    public class MainMenuTest : MonoBehaviour
    {
        private Canvas _canvas;
        private GameObject _mainPanel;
        private GameObject _settingsPanel;
        private GameObject _shopPanel;
        private GameObject _dailyRewardsPanel;
        
        // Player data (simulated)
        private int _coins = 500;
        private int _gems = 25;
        private int _currentDay = 1;
        private bool _dailyRewardClaimed = false;

        private TextMeshProUGUI _coinsText;
        private TextMeshProUGUI _gemsText;

        private void Start()
        {
            SetupCamera();
            SetupCanvas();
            CreateMainMenu();
            CreateSettingsPanel();
            CreateShopPanel();
            CreateDailyRewardsPanel();
            
            ShowPanel(_mainPanel);
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
            cam.backgroundColor = new Color(0.03f, 0.05f, 0.1f);
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

            // Ensure EventSystem
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        private void CreateMainMenu()
        {
            _mainPanel = CreatePanel("MainPanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // Background gradient effect
            Image bg = _mainPanel.GetComponent<Image>();
            bg.color = new Color(0.02f, 0.04f, 0.08f, 1f);

            // === TOP BAR - Currency Display ===
            GameObject topBar = CreatePanel("TopBar", new Vector2(0, 1), new Vector2(1, 1), 
                new Vector2(0, -60), new Vector2(-40, 100), new Color(0.05f, 0.08f, 0.12f, 0.95f));
            topBar.transform.SetParent(_mainPanel.transform, false);

            // Coins
            GameObject coinIcon = CreateIcon("💰", new Vector2(100, 0), topBar.transform);
            _coinsText = CreateLabel(_coins.ToString(), new Vector2(180, 0), 28, new Color(1f, 0.85f, 0.2f), topBar.transform);

            // Gems  
            GameObject gemIcon = CreateIcon("💎", new Vector2(320, 0), topBar.transform);
            _gemsText = CreateLabel(_gems.ToString(), new Vector2(400, 0), 28, new Color(0.4f, 0.9f, 1f), topBar.transform);

            // Settings button (top right)
            CreateIconButton("⚙️", new Vector2(-80, 0), topBar.transform, () => ShowPanel(_settingsPanel));

            // === LOGO / TITLE ===
            CreateLabel("RUMBAX", new Vector2(0, 600), 72, Color.white, _mainPanel.transform, FontStyles.Bold);
            CreateLabel("MERGE DEFENSE", new Vector2(0, 520), 32, new Color(0.5f, 0.8f, 1f), _mainPanel.transform);

            // === MAIN BUTTONS ===
            // Play Button - Large and prominent
            CreateMainButton("▶  PLAY", new Vector2(0, 200), new Vector2(400, 100),
                new Color(0.2f, 0.75f, 0.35f), _mainPanel.transform, OnPlayClicked);

            // Secondary buttons row
            CreateSecondaryButton("🎁 DAILY", new Vector2(-150, 50), _mainPanel.transform, () => ShowPanel(_dailyRewardsPanel));
            CreateSecondaryButton("🛒 SHOP", new Vector2(150, 50), _mainPanel.transform, () => ShowPanel(_shopPanel));

            // Bottom buttons
            CreateSecondaryButton("🏆 LEADERBOARD", new Vector2(-150, -80), _mainPanel.transform, OnLeaderboardClicked);
            CreateSecondaryButton("🎯 ACHIEVEMENTS", new Vector2(150, -80), _mainPanel.transform, OnAchievementsClicked);

            // === BOTTOM INFO ===
            CreateLabel("v1.0.0", new Vector2(0, -450), 18, new Color(0.4f, 0.45f, 0.5f), _mainPanel.transform);
        }

        private void CreateSettingsPanel()
        {
            _settingsPanel = CreatePanel("SettingsPanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _settingsPanel.GetComponent<Image>().color = new Color(0.02f, 0.04f, 0.08f, 0.98f);

            // Header
            CreateLabel("⚙️ SETTINGS", new Vector2(0, 600), 48, Color.white, _settingsPanel.transform);

            // Settings container
            GameObject container = CreatePanel("Container", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 100), new Vector2(500, 500), new Color(0.08f, 0.1f, 0.15f, 0.9f));
            container.transform.SetParent(_settingsPanel.transform, false);

            // Music toggle
            CreateSettingRow("🎵 Music", new Vector2(0, 150), container.transform, true);
            
            // SFX toggle
            CreateSettingRow("🔊 Sound Effects", new Vector2(0, 50), container.transform, true);
            
            // Vibration toggle
            CreateSettingRow("📳 Vibration", new Vector2(0, -50), container.transform, true);
            
            // Notifications toggle
            CreateSettingRow("🔔 Notifications", new Vector2(0, -150), container.transform, false);

            // Back button
            CreateMainButton("← BACK", new Vector2(0, -350), new Vector2(300, 70),
                new Color(0.4f, 0.4f, 0.5f), _settingsPanel.transform, () => ShowPanel(_mainPanel));

            _settingsPanel.SetActive(false);
        }

        private void CreateShopPanel()
        {
            _shopPanel = CreatePanel("ShopPanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _shopPanel.GetComponent<Image>().color = new Color(0.02f, 0.04f, 0.08f, 0.98f);

            // Header
            CreateLabel("🛒 SHOP", new Vector2(0, 650), 48, Color.white, _shopPanel.transform);

            // Currency display
            CreateLabel($"💰 {_coins}  |  💎 {_gems}", new Vector2(0, 580), 28, 
                new Color(0.7f, 0.75f, 0.8f), _shopPanel.transform);

            // === COINS SECTION ===
            CreateLabel("COINS", new Vector2(0, 480), 32, new Color(1f, 0.85f, 0.2f), _shopPanel.transform);

            CreateShopItem("💰 500 Coins", "💎 10", new Vector2(-150, 380), _shopPanel.transform, () => BuyWithGems(500, 10));
            CreateShopItem("💰 1500 Coins", "💎 25", new Vector2(150, 380), _shopPanel.transform, () => BuyWithGems(1500, 25));
            CreateShopItem("💰 5000 Coins", "💎 75", new Vector2(-150, 260), _shopPanel.transform, () => BuyWithGems(5000, 75));
            CreateShopItem("💰 15000 Coins", "💎 200", new Vector2(150, 260), _shopPanel.transform, () => BuyWithGems(15000, 200));

            // === GEMS SECTION ===
            CreateLabel("GEMS", new Vector2(0, 140), 32, new Color(0.4f, 0.9f, 1f), _shopPanel.transform);

            CreateShopItem("💎 50 Gems", "$0.99", new Vector2(-150, 40), _shopPanel.transform, () => BuyWithMoney(50, 0.99f));
            CreateShopItem("💎 150 Gems", "$2.99", new Vector2(150, 40), _shopPanel.transform, () => BuyWithMoney(150, 2.99f));
            CreateShopItem("💎 500 Gems", "$7.99", new Vector2(-150, -80), _shopPanel.transform, () => BuyWithMoney(500, 7.99f));
            CreateShopItem("💎 1500 Gems", "$19.99", new Vector2(150, -80), _shopPanel.transform, () => BuyWithMoney(1500, 19.99f));

            // === SPECIAL OFFERS ===
            CreateLabel("✨ SPECIAL", new Vector2(0, -200), 32, new Color(1f, 0.6f, 0.2f), _shopPanel.transform);
            
            CreateSpecialOffer("🎁 STARTER PACK", "500💰 + 50💎 + No Ads", "$4.99", 
                new Vector2(0, -300), _shopPanel.transform);

            // Back button
            CreateMainButton("← BACK", new Vector2(0, -450), new Vector2(300, 70),
                new Color(0.4f, 0.4f, 0.5f), _shopPanel.transform, () => ShowPanel(_mainPanel));

            _shopPanel.SetActive(false);
        }

        private void CreateDailyRewardsPanel()
        {
            _dailyRewardsPanel = CreatePanel("DailyRewardsPanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            _dailyRewardsPanel.GetComponent<Image>().color = new Color(0.02f, 0.04f, 0.08f, 0.98f);

            // Header
            CreateLabel("🎁 DAILY REWARDS", new Vector2(0, 650), 42, Color.white, _dailyRewardsPanel.transform);
            CreateLabel($"Day {_currentDay} Streak!", new Vector2(0, 590), 24, 
                new Color(0.5f, 1f, 0.6f), _dailyRewardsPanel.transform);

            // 7-day reward grid
            string[] rewards = { "50💰", "100💰", "5💎", "200💰", "10💎", "500💰", "25💎" };
            
            for (int i = 0; i < 7; i++)
            {
                int day = i + 1;
                float x = (i % 4 - 1.5f) * 130;
                float y = 400 - (i / 4) * 180;
                
                bool isClaimed = day < _currentDay;
                bool isToday = day == _currentDay;
                bool isLocked = day > _currentDay;

                CreateDayReward(day, rewards[i], new Vector2(x, y), isClaimed, isToday, isLocked, 
                    _dailyRewardsPanel.transform);
            }

            // Claim button
            Button claimBtn = CreateMainButton(_dailyRewardClaimed ? "✓ CLAIMED" : "🎁 CLAIM TODAY", 
                new Vector2(0, -100), new Vector2(350, 80),
                _dailyRewardClaimed ? new Color(0.3f, 0.35f, 0.4f) : new Color(0.9f, 0.5f, 0.1f), 
                _dailyRewardsPanel.transform, OnClaimDaily);
            claimBtn.interactable = !_dailyRewardClaimed;

            // Back button
            CreateMainButton("← BACK", new Vector2(0, -220), new Vector2(300, 70),
                new Color(0.4f, 0.4f, 0.5f), _dailyRewardsPanel.transform, () => ShowPanel(_mainPanel));

            _dailyRewardsPanel.SetActive(false);
        }

        // === HELPER METHODS ===

        private GameObject CreatePanel(string name, Vector2 anchorMin, Vector2 anchorMax, 
            Vector2 pos, Vector2 size, Color? color = null)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(_canvas.transform, false);
            
            Image img = panel.AddComponent<Image>();
            img.color = color ?? Color.clear;
            
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            
            return panel;
        }

        private TextMeshProUGUI CreateLabel(string text, Vector2 pos, int size, Color color, 
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
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(600, 80);
            return tmp;
        }

        private GameObject CreateIcon(string emoji, Vector2 pos, Transform parent)
        {
            GameObject obj = new GameObject("Icon");
            obj.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = emoji;
            tmp.fontSize = 32;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(50, 50);
            return obj;
        }

        private Button CreateIconButton(string emoji, Vector2 pos, Transform parent, UnityEngine.Events.UnityAction onClick)
        {
            GameObject obj = new GameObject("IconButton");
            obj.transform.SetParent(parent, false);
            
            Image bg = obj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.18f, 0.25f, 0.9f);
            
            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(onClick);
            
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 0.5f);
            rect.anchorMax = new Vector2(1, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(70, 70);

            // Icon
            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(obj.transform, false);
            TextMeshProUGUI tmp = icon.AddComponent<TextMeshProUGUI>();
            tmp.text = emoji;
            tmp.fontSize = 32;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.sizeDelta = Vector2.zero;

            return btn;
        }

        private Button CreateMainButton(string text, Vector2 pos, Vector2 size, Color color, 
            Transform parent, UnityEngine.Events.UnityAction onClick)
        {
            GameObject obj = new GameObject("MainButton");
            obj.transform.SetParent(parent, false);
            
            Image bg = obj.AddComponent<Image>();
            bg.color = color;
            
            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(onClick);
            
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            // Glow effect
            GameObject glow = new GameObject("Glow");
            glow.transform.SetParent(obj.transform, false);
            glow.transform.SetAsFirstSibling();
            Image glowImg = glow.AddComponent<Image>();
            glowImg.color = new Color(color.r, color.g, color.b, 0.3f);
            RectTransform glowRect = glow.GetComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.sizeDelta = new Vector2(8, 8);
            glowRect.anchoredPosition = new Vector2(0, -4);

            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size.y > 80 ? 36 : 24;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return btn;
        }

        private Button CreateSecondaryButton(string text, Vector2 pos, Transform parent, 
            UnityEngine.Events.UnityAction onClick)
        {
            return CreateMainButton(text, pos, new Vector2(200, 60), 
                new Color(0.15f, 0.2f, 0.3f), parent, onClick);
        }

        private void CreateSettingRow(string label, Vector2 pos, Transform parent, bool defaultOn)
        {
            GameObject row = new GameObject("SettingRow");
            row.transform.SetParent(parent, false);
            RectTransform rowRect = row.AddComponent<RectTransform>();
            rowRect.anchoredPosition = pos;
            rowRect.sizeDelta = new Vector2(400, 60);

            // Label
            CreateLabel(label, new Vector2(-80, 0), 22, Color.white, row.transform);

            // Toggle
            GameObject toggle = new GameObject("Toggle");
            toggle.transform.SetParent(row.transform, false);
            Image toggleBg = toggle.AddComponent<Image>();
            toggleBg.color = defaultOn ? new Color(0.2f, 0.7f, 0.3f) : new Color(0.3f, 0.3f, 0.35f);
            
            Button toggleBtn = toggle.AddComponent<Button>();
            toggleBtn.targetGraphic = toggleBg;
            
            bool isOn = defaultOn;
            toggleBtn.onClick.AddListener(() => {
                isOn = !isOn;
                toggleBg.color = isOn ? new Color(0.2f, 0.7f, 0.3f) : new Color(0.3f, 0.3f, 0.35f);
            });
            
            RectTransform toggleRect = toggle.GetComponent<RectTransform>();
            toggleRect.anchoredPosition = new Vector2(150, 0);
            toggleRect.sizeDelta = new Vector2(80, 40);
        }

        private void CreateShopItem(string item, string price, Vector2 pos, Transform parent, 
            UnityEngine.Events.UnityAction onClick)
        {
            GameObject container = new GameObject("ShopItem");
            container.transform.SetParent(parent, false);
            
            Image bg = container.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.12f, 0.18f, 0.95f);
            
            Button btn = container.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(onClick);
            
            RectTransform rect = container.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(180, 100);

            // Item name
            CreateLabel(item, new Vector2(0, 15), 18, Color.white, container.transform);
            
            // Price
            CreateLabel(price, new Vector2(0, -25), 20, new Color(0.5f, 1f, 0.6f), container.transform, FontStyles.Bold);
        }

        private void CreateSpecialOffer(string title, string desc, string price, Vector2 pos, Transform parent)
        {
            GameObject container = new GameObject("SpecialOffer");
            container.transform.SetParent(parent, false);
            
            Image bg = container.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.12f, 0.05f, 0.95f);
            
            Button btn = container.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() => Debug.Log("Buy special offer!"));
            
            RectTransform rect = container.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(450, 90);

            CreateLabel(title, new Vector2(-100, 10), 22, new Color(1f, 0.8f, 0.4f), container.transform, FontStyles.Bold);
            CreateLabel(desc, new Vector2(-100, -20), 14, new Color(0.7f, 0.7f, 0.75f), container.transform);
            CreateLabel(price, new Vector2(160, 0), 28, new Color(0.5f, 1f, 0.6f), container.transform, FontStyles.Bold);
        }

        private void CreateDayReward(int day, string reward, Vector2 pos, bool claimed, bool isToday, 
            bool locked, Transform parent)
        {
            GameObject container = new GameObject($"Day{day}");
            container.transform.SetParent(parent, false);
            
            Color bgColor = claimed ? new Color(0.15f, 0.4f, 0.2f, 0.9f) 
                : isToday ? new Color(0.4f, 0.3f, 0.1f, 0.95f)
                : new Color(0.1f, 0.12f, 0.18f, 0.8f);
            
            Image bg = container.AddComponent<Image>();
            bg.color = bgColor;
            
            RectTransform rect = container.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(110, 140);

            CreateLabel($"Day {day}", new Vector2(0, 40), 18, Color.white, container.transform);
            CreateLabel(reward, new Vector2(0, 0), 22, 
                claimed ? new Color(0.5f, 0.5f, 0.5f) : new Color(1f, 0.85f, 0.2f), 
                container.transform, FontStyles.Bold);
            
            if (claimed)
                CreateLabel("✓", new Vector2(0, -40), 28, new Color(0.3f, 0.8f, 0.4f), container.transform);
            else if (locked)
                CreateLabel("🔒", new Vector2(0, -40), 24, new Color(0.5f, 0.5f, 0.55f), container.transform);
        }

        private void ShowPanel(GameObject panel)
        {
            _mainPanel.SetActive(panel == _mainPanel);
            _settingsPanel.SetActive(panel == _settingsPanel);
            _shopPanel.SetActive(panel == _shopPanel);
            _dailyRewardsPanel.SetActive(panel == _dailyRewardsPanel);
        }

        // === BUTTON HANDLERS ===

        private void OnPlayClicked()
        {
            Debug.Log("[MainMenu] Starting game...");
            // Load game scene - for now just load scene index 0 or find GameScene
            if (SceneManager.sceneCountInBuildSettings > 1)
            {
                SceneManager.LoadScene(1);
            }
            else
            {
                // If only one scene, just start gameplay in current scene
                gameObject.SetActive(false);
                GameObject gameplay = new GameObject("GameplayTest");
                gameplay.AddComponent<GameplayTest>();
            }
        }

        private void OnLeaderboardClicked()
        {
            Debug.Log("[MainMenu] Leaderboard clicked - Coming soon!");
        }

        private void OnAchievementsClicked()
        {
            Debug.Log("[MainMenu] Achievements clicked - Coming soon!");
        }

        private void OnClaimDaily()
        {
            if (!_dailyRewardClaimed)
            {
                _dailyRewardClaimed = true;
                _coins += 50 * _currentDay;
                Debug.Log($"[MainMenu] Claimed daily reward: {50 * _currentDay} coins!");
                UpdateCurrency();
                ShowPanel(_dailyRewardsPanel); // Refresh panel
            }
        }

        private void BuyWithGems(int coins, int gemCost)
        {
            if (_gems >= gemCost)
            {
                _gems -= gemCost;
                _coins += coins;
                Debug.Log($"[MainMenu] Bought {coins} coins for {gemCost} gems!");
                UpdateCurrency();
            }
            else
            {
                Debug.Log("[MainMenu] Not enough gems!");
            }
        }

        private void BuyWithMoney(int gems, float price)
        {
            // In real app, this would trigger IAP
            Debug.Log($"[MainMenu] IAP: Buy {gems} gems for ${price}");
            _gems += gems; // Simulate purchase for testing
            UpdateCurrency();
        }

        private void UpdateCurrency()
        {
            if (_coinsText != null) _coinsText.text = _coins.ToString();
            if (_gemsText != null) _gemsText.text = _gems.ToString();
        }
    }
}
