using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

namespace Rumbax.Testing
{
    /// <summary>
    /// Manages all in-game UI panels: HUD, Pause, Victory, GameOver
    /// </summary>
    public class GameUIManager : MonoBehaviour
    {
        public static GameUIManager Instance { get; private set; }

        private Canvas _canvas;
        
        // Panels
        private GameObject _hudPanel;
        private GameObject _pausePanel;
        private GameObject _victoryPanel;
        private GameObject _gameOverPanel;
        
        // HUD elements
        private TextMeshProUGUI _coinsText;
        private TextMeshProUGUI _livesText;
        private TextMeshProUGUI _waveText;
        private TextMeshProUGUI _scoreText;
        private Button _pauseButton;
        private Button _spawnButton;
        private Button _startWaveButton;

        // Game state
        private int _coins = 100;
        private int _gems = 10;
        private int _lives = 10;
        private int _wave = 1;
        private int _score = 0;
        private bool _isPaused = false;

        public event Action<int, int> OnCellClicked;
        public event Action OnSpawnRequested;
        public event Action OnWaveStartRequested;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Initialize(Canvas existingCanvas = null)
        {
            if (existingCanvas != null)
            {
                _canvas = existingCanvas;
            }
            else
            {
                CreateCanvas();
            }

            CreateHUD();
            CreatePausePanel();
            CreateVictoryPanel();
            CreateGameOverPanel();

            ShowHUD();
        }

        private void CreateCanvas()
        {
            GameObject canvasObj = new GameObject("GameUICanvas");
            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        private void CreateHUD()
        {
            _hudPanel = CreatePanel("HUDPanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.clear);

            // === TOP BAR ===
            GameObject topBar = CreatePanel("TopBar", 
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -20), new Vector2(-40, 100),
                new Color(0.05f, 0.08f, 0.12f, 0.95f));
            topBar.transform.SetParent(_hudPanel.transform, false);

            // Pause button (top left)
            _pauseButton = CreateIconButton("⏸️", new Vector2(60, 0), topBar.transform, OnPauseClicked);
            _pauseButton.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.5f);
            _pauseButton.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0.5f);

            // Coins
            CreateIcon("💰", new Vector2(180, 0), topBar.transform);
            _coinsText = CreateLabel(_coins.ToString(), new Vector2(260, 0), 28, 
                new Color(1f, 0.85f, 0.2f), topBar.transform);

            // Lives
            _livesText = CreateLabel($"❤️ {_lives}", new Vector2(0, 0), 28, 
                new Color(1f, 0.4f, 0.4f), topBar.transform);

            // Wave (right side)
            _waveText = CreateLabel($"Wave {_wave}", new Vector2(-100, 0), 28, 
                new Color(0.5f, 0.8f, 1f), topBar.transform);
            _waveText.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0.5f);
            _waveText.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.5f);

            // === SCORE BADGE ===
            GameObject scoreBadge = CreatePanel("ScoreBadge",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -140), new Vector2(180, 45),
                new Color(0.15f, 0.4f, 0.2f, 0.95f));
            scoreBadge.transform.SetParent(_hudPanel.transform, false);
            _scoreText = CreateLabel("0", 28, Color.white, scoreBadge.transform);

            // === BOTTOM ACTION BAR ===
            GameObject bottomBar = CreatePanel("BottomBar",
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 20), new Vector2(-40, 180),
                new Color(0.05f, 0.08f, 0.12f, 0.98f));
            bottomBar.transform.SetParent(_hudPanel.transform, false);

            // Spawn button
            _spawnButton = CreateActionButton("SUMMON $20", new Vector2(-140, 80), bottomBar.transform,
                new Color(0.2f, 0.7f, 0.35f), () => OnSpawnRequested?.Invoke());

            // Start wave button
            _startWaveButton = CreateActionButton("START WAVE", new Vector2(140, 80), bottomBar.transform,
                new Color(0.9f, 0.45f, 0.15f), () => OnWaveStartRequested?.Invoke());

            // Instructions
            CreateLabel("Tap SUMMON → Tap Grid → Merge Same Tiers!", 16,
                new Color(0.55f, 0.6f, 0.65f), bottomBar.transform, new Vector2(0, 150));
        }

        private void CreatePausePanel()
        {
            _pausePanel = CreatePanel("PausePanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0.02f, 0.03f, 0.05f, 0.9f));

            // Pause container
            GameObject container = CreatePanel("Container",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(400, 450),
                new Color(0.08f, 0.1f, 0.15f, 0.98f));
            container.transform.SetParent(_pausePanel.transform, false);

            CreateLabel("⏸️ PAUSED", 42, Color.white, container.transform, new Vector2(0, 150));

            CreateMenuButton("▶️ RESUME", new Vector2(0, 50), container.transform, 
                new Color(0.2f, 0.7f, 0.35f), OnResumeClicked);
            CreateMenuButton("🔄 RESTART", new Vector2(0, -30), container.transform,
                new Color(0.5f, 0.5f, 0.2f), OnRestartClicked);
            CreateMenuButton("🏠 MAIN MENU", new Vector2(0, -110), container.transform,
                new Color(0.4f, 0.4f, 0.5f), OnMainMenuClicked);

            _pausePanel.SetActive(false);
        }

        private void CreateVictoryPanel()
        {
            _victoryPanel = CreatePanel("VictoryPanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0.02f, 0.05f, 0.03f, 0.9f));

            GameObject container = CreatePanel("Container",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(450, 500),
                new Color(0.08f, 0.12f, 0.1f, 0.98f));
            container.transform.SetParent(_victoryPanel.transform, false);

            CreateLabel("🏆 VICTORY!", 48, new Color(1f, 0.85f, 0.2f), container.transform, new Vector2(0, 180));
            
            // Stars
            CreateLabel("⭐⭐⭐", 40, new Color(1f, 0.9f, 0.3f), container.transform, new Vector2(0, 100));

            // Stats
            CreateLabel("Score: 0", 28, Color.white, container.transform, new Vector2(0, 30));
            CreateLabel("Enemies Killed: 0", 22, new Color(0.7f, 0.75f, 0.8f), container.transform, new Vector2(0, -10));
            CreateLabel("Reward: 💰 100", 24, new Color(1f, 0.85f, 0.2f), container.transform, new Vector2(0, -60));

            CreateMenuButton("NEXT LEVEL ▶️", new Vector2(0, -140), container.transform,
                new Color(0.2f, 0.7f, 0.35f), OnNextLevelClicked);
            CreateMenuButton("🏠 MAIN MENU", new Vector2(0, -210), container.transform,
                new Color(0.4f, 0.4f, 0.5f), OnMainMenuClicked);

            _victoryPanel.SetActive(false);
        }

        private void CreateGameOverPanel()
        {
            _gameOverPanel = CreatePanel("GameOverPanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0.05f, 0.02f, 0.02f, 0.9f));

            GameObject container = CreatePanel("Container",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(420, 450),
                new Color(0.12f, 0.08f, 0.08f, 0.98f));
            container.transform.SetParent(_gameOverPanel.transform, false);

            CreateLabel("💀 GAME OVER", 42, new Color(1f, 0.3f, 0.3f), container.transform, new Vector2(0, 150));
            CreateLabel("Score: 0", 32, Color.white, container.transform, new Vector2(0, 70));
            CreateLabel("Waves Survived: 0", 24, new Color(0.6f, 0.65f, 0.7f), container.transform, new Vector2(0, 30));

            CreateMenuButton("🔄 TRY AGAIN", new Vector2(0, -50), container.transform,
                new Color(0.7f, 0.4f, 0.15f), OnRestartClicked);
            CreateMenuButton("🏠 MAIN MENU", new Vector2(0, -130), container.transform,
                new Color(0.4f, 0.4f, 0.5f), OnMainMenuClicked);

            _gameOverPanel.SetActive(false);
        }

        // === UI HELPERS ===

        private GameObject CreatePanel(string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pos, Vector2 size, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(_canvas.transform, false);

            Image img = panel.AddComponent<Image>();
            img.color = color;

            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            return panel;
        }

        private TextMeshProUGUI CreateLabel(string text, int size, Color color, Transform parent, 
            Vector2? pos = null)
        {
            GameObject obj = new GameObject("Label");
            obj.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = pos ?? Vector2.zero;
            rect.sizeDelta = new Vector2(400, 60);
            return tmp;
        }

        private TextMeshProUGUI CreateLabel(string text, Vector2 pos, int size, Color color, Transform parent)
        {
            return CreateLabel(text, size, color, parent, pos);
        }

        private GameObject CreateIcon(string emoji, Vector2 pos, Transform parent)
        {
            GameObject obj = new GameObject("Icon");
            obj.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = emoji;
            tmp.fontSize = 28;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(50, 50);
            return obj;
        }

        private Button CreateIconButton(string emoji, Vector2 pos, Transform parent, 
            UnityEngine.Events.UnityAction onClick)
        {
            GameObject obj = new GameObject("IconButton");
            obj.transform.SetParent(parent, false);

            Image bg = obj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.18f, 0.25f, 0.9f);

            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(onClick);

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(60, 60);

            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(obj.transform, false);
            TextMeshProUGUI tmp = icon.AddComponent<TextMeshProUGUI>();
            tmp.text = emoji;
            tmp.fontSize = 28;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.sizeDelta = Vector2.zero;

            return btn;
        }

        private Button CreateActionButton(string text, Vector2 pos, Transform parent, Color color,
            UnityEngine.Events.UnityAction onClick)
        {
            GameObject obj = new GameObject("ActionButton");
            obj.transform.SetParent(parent, false);

            Image bg = obj.AddComponent<Image>();
            bg.color = color;

            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(onClick);

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(190, 60);

            // Glow
            GameObject glow = new GameObject("Glow");
            glow.transform.SetParent(obj.transform, false);
            glow.transform.SetAsFirstSibling();
            Image glowImg = glow.AddComponent<Image>();
            glowImg.color = new Color(color.r, color.g, color.b, 0.25f);
            RectTransform glowRect = glow.GetComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.sizeDelta = new Vector2(8, 8);
            glowRect.anchoredPosition = new Vector2(0, -3);

            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 20;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return btn;
        }

        private Button CreateMenuButton(string text, Vector2 pos, Transform parent, Color color,
            UnityEngine.Events.UnityAction onClick)
        {
            GameObject obj = new GameObject("MenuButton");
            obj.transform.SetParent(parent, false);

            Image bg = obj.AddComponent<Image>();
            bg.color = color;

            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(onClick);

            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(280, 60);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 22;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return btn;
        }

        // === PUBLIC API ===

        public void UpdateHUD(int coins, int lives, int wave, int score)
        {
            _coins = coins;
            _lives = lives;
            _wave = wave;
            _score = score;

            if (_coinsText != null) _coinsText.text = coins.ToString();
            if (_livesText != null) _livesText.text = $"❤️ {lives}";
            if (_waveText != null) _waveText.text = $"Wave {wave}";
            if (_scoreText != null) _scoreText.text = score.ToString();
        }

        public void SetSpawnButtonInteractable(bool interactable)
        {
            if (_spawnButton != null) _spawnButton.interactable = interactable;
        }

        public void SetWaveButtonInteractable(bool interactable)
        {
            if (_startWaveButton != null) _startWaveButton.interactable = interactable;
        }

        public void ShowHUD()
        {
            _hudPanel.SetActive(true);
            _pausePanel.SetActive(false);
            _victoryPanel.SetActive(false);
            _gameOverPanel.SetActive(false);
            Time.timeScale = 1f;
            _isPaused = false;
        }

        public void ShowPause()
        {
            _pausePanel.SetActive(true);
            Time.timeScale = 0f;
            _isPaused = true;
        }

        public void ShowVictory(int score, int enemiesKilled, int reward)
        {
            _victoryPanel.SetActive(true);
            // Update victory stats would go here
            Time.timeScale = 0f;
        }

        public void ShowGameOver(int score, int wavesSurvived)
        {
            _gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }

        // === BUTTON HANDLERS ===

        private void OnPauseClicked()
        {
            ShowPause();
        }

        private void OnResumeClicked()
        {
            ShowHUD();
        }

        private void OnRestartClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnMainMenuClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(0); // Assumes main menu is scene 0
        }

        private void OnNextLevelClicked()
        {
            Time.timeScale = 1f;
            // Would load next level - for now restart
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
