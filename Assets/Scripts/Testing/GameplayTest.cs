using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

namespace Rumbax.Testing
{
    /// <summary>
    /// Full gameplay test with defenders, enemies, merging, and combat.
    /// </summary>
    public class GameplayTest : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private int gridWidth = 5;
        [SerializeField] private int gridHeight = 3;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private float enemySpawnInterval = 2f;
        [SerializeField] private int startingCoins = 100;
        [SerializeField] private int defenderCost = 20;

        // Runtime state
        private int _coins;
        private int _gems = 10;
        private int _score;
        private int _wave = 1;
        private int _lives = 10;
        private bool _gameRunning;
        
        private GameObject[,] _gridCells;
        private TestDefender[,] _defenders;
        private List<TestEnemy> _enemies = new List<TestEnemy>();
        private TestDefender _selectedDefender;
        
        private Canvas _canvas;
        private TextMeshProUGUI _coinsText;
        private TextMeshProUGUI _livesText;
        private TextMeshProUGUI _waveText;
        private TextMeshProUGUI _scoreText;
        private Button _spawnButton;
        private Button _startWaveButton;
        private GameObject _gameOverPanel;

        private float _enemySpawnTimer;
        private int _enemiesThisWave;
        private int _enemiesToSpawn;
        private Vector3 _enemySpawnPoint;
        private Vector3 _enemyEndPoint;

        // New integrated systems
        private SimpleAudioManager _audio;
        private SaveManager _save;
        private bool _isPaused;
        private GameObject _pausePanel;
        private GameObject _victoryPanel;
        private Button _pauseButton;
        private int _enemiesKilled;

        // === NEW EXCITING FEATURES ===
        private float _gameSpeed = 1f;
        private Button _speedButton;
        private TextMeshProUGUI _speedText;
        private int _combo = 0;
        private float _comboTimer = 0f;
        private TextMeshProUGUI _comboText;
        private GameObject _comboPanel;
        private float _feverMeter = 0f;
        private bool _feverMode = false;
        private Image _feverBar;
        private TextMeshProUGUI _feverText;
        private Button _ultimateButton;
        private float _ultimateCharge = 0f;
        private int _killStreak = 0;
        private float _killStreakTimer = 0f;
        private TextMeshProUGUI _killStreakText;
        private List<GameObject> _floatingTexts = new List<GameObject>();
        private Camera _mainCamera;
        
        // Power-ups
        private List<PowerUp> _powerUps = new List<PowerUp>();
        private float _powerUpSpawnTimer = 0f;
        private bool _doubleCoins = false;
        private float _doubleCoinTimer = 0f;
        private bool _rapidFire = false;
        private float _rapidFireTimer = 0f;
        private TextMeshProUGUI _powerUpStatusText;

        private void Start()
        {
            // Initialize audio system
            if (SimpleAudioManager.Instance == null)
            {
                GameObject audioObj = new GameObject("AudioManager");
                _audio = audioObj.AddComponent<SimpleAudioManager>();
            }
            else
            {
                _audio = SimpleAudioManager.Instance;
            }

            // Initialize save system
            if (SaveManager.Instance == null)
            {
                GameObject saveObj = new GameObject("SaveManager");
                _save = saveObj.AddComponent<SaveManager>();
            }
            else
            {
                _save = SaveManager.Instance;
            }

            SetupCamera();
            SetupUI();
            SetupGrid();
            SetupEnemyPath();
            
            // Load saved progress
            if (_save != null && _save.Data != null)
            {
                _coins = _save.Data.coins;
                _gems = _save.Data.gems;
            }
            else
            {
                _coins = startingCoins;
            }
            
            _gameRunning = false;
            UpdateUI();
            
            Debug.Log("[GameplayTest] Ready! Click 'Spawn Defender' then 'Start Wave'");
        }

        private void Update()
        {
            // Check for pause input
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }

            if (_isPaused) return;

            // Update combo timer
            UpdateComboSystem();
            
            // Update kill streak
            UpdateKillStreak();
            
            // Update fever mode
            UpdateFeverMode();
            
            // Update power-ups
            UpdatePowerUps();

            // Handle mouse/touch input for grid cells
            HandleInput();
            
            if (!_gameRunning) return;

            // Spawn enemies
            if (_enemiesToSpawn > 0)
            {
                _enemySpawnTimer -= Time.deltaTime;
                if (_enemySpawnTimer <= 0)
                {
                    SpawnEnemy();
                    _enemySpawnTimer = enemySpawnInterval;
                    _enemiesToSpawn--;
                }
            }

            // Update enemies
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                if (_enemies[i] == null)
                {
                    _enemies.RemoveAt(i);
                    continue;
                }

                _enemies[i].UpdateMovement(_enemyEndPoint);

                // Check if reached end
                if (Vector3.Distance(_enemies[i].transform.position, _enemyEndPoint) < 0.3f)
                {
                    _lives--;
                    Destroy(_enemies[i].gameObject);
                    _enemies.RemoveAt(i);
                    UpdateUI();

                    if (_lives <= 0)
                    {
                        GameOver();
                    }
                }
            }

            // Defenders attack
            foreach (var defender in GetAllDefenders())
            {
                if (defender != null)
                {
                    defender.TryAttack(_enemies);
                }
            }

            // Check wave complete
            if (_enemiesToSpawn <= 0 && _enemies.Count == 0 && _gameRunning)
            {
                WaveComplete();
            }
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
            // Modern dark gradient background
            cam.backgroundColor = new Color(0.05f, 0.07f, 0.12f);
            cam.orthographic = true;
            cam.orthographicSize = 5.5f;
            cam.transform.position = new Vector3(0, 1f, -10);
            
            // Store camera reference
            _mainCamera = cam;
            
            // Add Physics2D Raycaster for mouse clicks on 2D colliders
            if (cam.GetComponent<UnityEngine.EventSystems.Physics2DRaycaster>() == null)
            {
                cam.gameObject.AddComponent<UnityEngine.EventSystems.Physics2DRaycaster>();
            }
            
            // Ensure EventSystem exists
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        private void SetupUI()
        {
            // Create canvas with proper scaling for mobile
            GameObject canvasObj = new GameObject("GameCanvas");
            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); // Portrait mobile
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            // === TOP HUD - Modern Glass Effect ===
            GameObject topPanel = CreateModernPanel("TopHUD", 
                new Vector2(0, 1), new Vector2(0, 1), // Top anchored
                new Vector2(0, -20), new Vector2(-40, 120),
                new Color(0.05f, 0.08f, 0.12f, 0.95f));

            // Currency container (left side)
            _coinsText = CreateModernLabel("💰", _coins.ToString(), 
                new Vector2(80, -60), topPanel.transform, new Color(1f, 0.85f, 0.2f));
            
            // Gems (next to coins)  
            CreateModernLabel("💎", _gems.ToString(),
                new Vector2(220, -60), topPanel.transform, new Color(0.4f, 0.9f, 1f));

            // Wave indicator (right side)
            _waveText = CreateModernLabel("🌊", $"Wave {_wave}",
                new Vector2(-100, -60), topPanel.transform, Color.white, true);

            // === LIVES BAR - Under top HUD ===
            GameObject livesContainer = CreateModernPanel("LivesBar",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -150), new Vector2(300, 50),
                new Color(0.1f, 0.1f, 0.15f, 0.9f));
            
            _livesText = CreateStyledText($"❤️ {_lives} Lives", 24, Color.white, livesContainer.transform);

            // === SCORE - Floating modern badge ===
            GameObject scoreBadge = CreateModernPanel("ScoreBadge",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -210), new Vector2(200, 45),
                new Color(0.15f, 0.4f, 0.15f, 0.95f));
            
            _scoreText = CreateStyledText("0", 28, Color.white, scoreBadge.transform);

            // === BOTTOM ACTION BAR ===
            GameObject bottomPanel = CreateModernPanel("BottomBar",
                new Vector2(0, 0), new Vector2(1, 0), // Bottom anchored full width
                new Vector2(0, 20), new Vector2(-40, 200),
                new Color(0.05f, 0.08f, 0.12f, 0.98f));

            // Spawn button - Modern gradient style
            _spawnButton = CreateModernButton($"SUMMON  ${defenderCost}", 
                new Vector2(-150, 80), bottomPanel.transform,
                new Color(0.2f, 0.7f, 0.3f), new Color(0.15f, 0.5f, 0.2f),
                OnSpawnDefender);

            // Start Wave button
            _startWaveButton = CreateModernButton("START WAVE", 
                new Vector2(150, 80), bottomPanel.transform,
                new Color(0.9f, 0.4f, 0.2f), new Color(0.7f, 0.3f, 0.15f),
                OnStartWave);

            // Tip text
            CreateStyledText("Tap SUMMON → Tap Grid Cell → Merge Same Tiers!", 16, 
                new Color(0.6f, 0.65f, 0.7f), bottomPanel.transform, new Vector2(0, 160));

            // === PAUSE BUTTON (top-left corner) ===
            _pauseButton = CreatePauseButton(topPanel.transform);

            // === SPEED BUTTON (next to pause) ===
            _speedButton = CreateSpeedButton(topPanel.transform);

            // === ULTIMATE BUTTON ===
            _ultimateButton = CreateUltimateButton(bottomPanel.transform);

            // === COMBO DISPLAY ===
            CreateComboDisplay();

            // === FEVER BAR ===
            CreateFeverBar();

            // === KILL STREAK DISPLAY ===
            CreateKillStreakDisplay();

            // === CREATE PAUSE PANEL (hidden) ===
            CreatePausePanel();

            // === CREATE VICTORY PANEL (hidden) ===
            CreateVictoryPanel();
        }

        private Button CreateSpeedButton(Transform parent)
        {
            GameObject btnObj = new GameObject("SpeedButton");
            btnObj.transform.SetParent(parent, false);
            
            Image bg = btnObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.15f, 0.4f, 0.9f);
            
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(CycleGameSpeed);
            
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.anchoredPosition = new Vector2(130, 0);
            rect.sizeDelta = new Vector2(60, 60);
            
            GameObject icon = new GameObject("Text");
            icon.transform.SetParent(btnObj.transform, false);
            _speedText = icon.AddComponent<TextMeshProUGUI>();
            _speedText.text = "1x";
            _speedText.fontSize = 22;
            _speedText.fontStyle = FontStyles.Bold;
            _speedText.alignment = TextAlignmentOptions.Center;
            _speedText.color = Color.white;
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.sizeDelta = Vector2.zero;

            return btn;
        }

        private Button CreateUltimateButton(Transform parent)
        {
            GameObject btnObj = new GameObject("UltimateButton");
            btnObj.transform.SetParent(parent, false);
            
            Image bg = btnObj.AddComponent<Image>();
            bg.color = new Color(0.6f, 0.2f, 0.6f, 0.5f);
            
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(ActivateUltimate);
            btn.interactable = false;
            
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, 80);
            rect.sizeDelta = new Vector2(80, 80);
            
            // Charge bar inside
            GameObject chargeBar = new GameObject("ChargeBar");
            chargeBar.transform.SetParent(btnObj.transform, false);
            Image chargeBg = chargeBar.AddComponent<Image>();
            chargeBg.color = new Color(0.8f, 0.3f, 0.8f, 0.8f);
            chargeBg.type = Image.Type.Filled;
            chargeBg.fillMethod = Image.FillMethod.Radial360;
            chargeBg.fillAmount = 0f;
            RectTransform chargeRect = chargeBar.GetComponent<RectTransform>();
            chargeRect.anchorMin = Vector2.zero;
            chargeRect.anchorMax = Vector2.one;
            chargeRect.sizeDelta = new Vector2(-8, -8);
            
            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI tmp = icon.AddComponent<TextMeshProUGUI>();
            tmp.text = "⚡";
            tmp.fontSize = 36;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.sizeDelta = Vector2.zero;

            return btn;
        }

        private void CreateComboDisplay()
        {
            _comboPanel = CreateModernPanel("ComboPanel",
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-80, 100), new Vector2(140, 80),
                new Color(0.8f, 0.4f, 0.1f, 0.95f));
            
            _comboText = CreateStyledText("", 32, Color.white, _comboPanel.transform);
            _comboPanel.SetActive(false);
        }

        private void CreateFeverBar()
        {
            GameObject feverContainer = CreateModernPanel("FeverBar",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 230), new Vector2(300, 30),
                new Color(0.1f, 0.1f, 0.15f, 0.9f));
            
            // Fill bar
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(feverContainer.transform, false);
            _feverBar = fill.AddComponent<Image>();
            _feverBar.color = new Color(1f, 0.5f, 0f, 0.9f);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(0, 1);
            fillRect.pivot = new Vector2(0, 0.5f);
            fillRect.anchoredPosition = new Vector2(5, 0);
            fillRect.sizeDelta = new Vector2(0, -6);
            
            _feverText = CreateStyledText("FEVER", 16, Color.white, feverContainer.transform);
        }

        private void CreateKillStreakDisplay()
        {
            GameObject streakPanel = CreateModernPanel("KillStreak",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 200), new Vector2(400, 80),
                new Color(0.9f, 0.2f, 0.2f, 0f));
            
            _killStreakText = CreateStyledText("", 48, new Color(1f, 0.9f, 0.2f), streakPanel.transform);
            _killStreakText.fontStyle = FontStyles.Bold | FontStyles.Italic;
        }

        private void CycleGameSpeed()
        {
            if (_gameSpeed == 1f) _gameSpeed = 1.5f;
            else if (_gameSpeed == 1.5f) _gameSpeed = 2f;
            else if (_gameSpeed == 2f) _gameSpeed = 3f;
            else _gameSpeed = 1f;
            
            Time.timeScale = _gameSpeed;
            _speedText.text = $"{_gameSpeed}x";
            
            if (_audio != null) _audio.PlayClick();
        }

        private void UpdateComboSystem()
        {
            if (_comboTimer > 0)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0)
                {
                    _combo = 0;
                    _comboPanel.SetActive(false);
                }
                else
                {
                    // Pulse effect
                    float pulse = 1f + Mathf.Sin(Time.time * 10f) * 0.1f;
                    _comboPanel.transform.localScale = Vector3.one * pulse;
                }
            }
        }

        private void UpdateKillStreak()
        {
            if (_killStreakTimer > 0)
            {
                _killStreakTimer -= Time.deltaTime;
                
                // Fade out
                float alpha = Mathf.Clamp01(_killStreakTimer / 1.5f);
                _killStreakText.color = new Color(1f, 0.9f, 0.2f, alpha);
                
                // Scale effect
                float scale = 1f + (1.5f - _killStreakTimer) * 0.1f;
                _killStreakText.transform.localScale = Vector3.one * scale;
                
                if (_killStreakTimer <= 0)
                {
                    _killStreak = 0;
                    _killStreakText.text = "";
                }
            }
        }

        private void UpdateFeverMode()
        {
            // Update fever bar width
            if (_feverBar != null)
            {
                float targetWidth = (_feverMeter / 100f) * 290f;
                RectTransform fillRect = _feverBar.GetComponent<RectTransform>();
                fillRect.sizeDelta = new Vector2(targetWidth, fillRect.sizeDelta.y);
                
                // Color based on fever level
                if (_feverMode)
                {
                    _feverBar.color = Color.Lerp(Color.red, Color.yellow, Mathf.PingPong(Time.time * 3f, 1f));
                    _feverText.text = "🔥 FEVER MODE! 🔥";
                    _feverText.color = Color.yellow;
                }
                else
                {
                    _feverBar.color = new Color(1f, 0.5f, 0f, 0.9f);
                    _feverText.text = "FEVER";
                    _feverText.color = Color.white;
                }
            }
            
            // Decay fever if not in fever mode
            if (_feverMode)
            {
                _feverMeter -= Time.deltaTime * 10f;
                if (_feverMeter <= 0)
                {
                    _feverMode = false;
                    _feverMeter = 0;
                }
            }
        }

        private void AddCombo()
        {
            _combo++;
            _comboTimer = 2f;
            _comboPanel.SetActive(true);
            _comboText.text = $"x{_combo}";
            
            // Add fever from combo
            _feverMeter = Mathf.Min(100f, _feverMeter + 5f + _combo);
            
            // Check for fever activation
            if (_feverMeter >= 100f && !_feverMode)
            {
                _feverMode = true;
                ShowFloatingText("FEVER MODE!", Color.yellow, Vector3.zero, true);
                if (_audio != null) _audio.PlayUpgrade();
            }
            
            // Add ultimate charge
            _ultimateCharge = Mathf.Min(100f, _ultimateCharge + 2f);
            UpdateUltimateButton();
        }

        private void AddKillStreak()
        {
            _killStreak++;
            _killStreakTimer = 2f;
            
            string streakText = _killStreak switch
            {
                3 => "TRIPLE KILL!",
                5 => "KILLING SPREE!",
                7 => "RAMPAGE!",
                10 => "UNSTOPPABLE!",
                15 => "GODLIKE!",
                20 => "LEGENDARY!",
                _ when _killStreak > 20 => $"x{_killStreak} MASSACRE!",
                _ => ""
            };
            
            if (!string.IsNullOrEmpty(streakText))
            {
                _killStreakText.text = streakText;
                _killStreakText.transform.localScale = Vector3.one * 1.5f;
                
                // Screen shake for big streaks
                if (_killStreak >= 5)
                    StartCoroutine(ScreenShake(0.1f, 0.05f * Mathf.Min(_killStreak, 10)));
            }
        }

        private void UpdateUltimateButton()
        {
            if (_ultimateButton != null)
            {
                _ultimateButton.interactable = _ultimateCharge >= 100f;
                
                // Update charge visual
                Transform chargeBar = _ultimateButton.transform.Find("ChargeBar");
                if (chargeBar != null)
                {
                    Image chargeImg = chargeBar.GetComponent<Image>();
                    chargeImg.fillAmount = _ultimateCharge / 100f;
                }
            }
        }

        private void ActivateUltimate()
        {
            if (_ultimateCharge < 100f) return;
            
            _ultimateCharge = 0f;
            UpdateUltimateButton();
            
            // ULTIMATE: Kill all enemies on screen!
            ShowFloatingText("⚡ ULTIMATE! ⚡", new Color(0.8f, 0.4f, 1f), Vector3.zero, true);
            
            if (_audio != null) _audio.PlayVictory();
            
            StartCoroutine(ScreenShake(0.3f, 0.15f));
            
            // Damage all enemies
            foreach (var enemy in _enemies.ToArray())
            {
                if (enemy != null)
                {
                    enemy.TakeDamage(999);
                }
            }
        }

        private void ShowFloatingText(string text, Color color, Vector3 worldPos, bool centerScreen = false)
        {
            GameObject floatObj = new GameObject("FloatingText");
            floatObj.transform.SetParent(_canvas.transform, false);
            
            TextMeshProUGUI tmp = floatObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = centerScreen ? 48 : 28;
            tmp.color = color;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            
            RectTransform rect = floatObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(500, 100);
            
            if (centerScreen)
            {
                rect.anchoredPosition = Vector2.zero;
            }
            else if (_mainCamera != null)
            {
                Vector2 screenPos = _mainCamera.WorldToScreenPoint(worldPos);
                rect.position = screenPos;
            }
            
            // Animate and destroy
            StartCoroutine(AnimateFloatingText(floatObj, rect));
        }

        private System.Collections.IEnumerator AnimateFloatingText(GameObject obj, RectTransform rect)
        {
            float duration = 1.5f;
            float elapsed = 0f;
            Vector2 startPos = rect.anchoredPosition;
            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            Color startColor = tmp.color;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Move up
                rect.anchoredPosition = startPos + Vector2.up * (t * 100f);
                
                // Fade out
                tmp.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
                
                // Scale pop
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.3f;
                rect.localScale = Vector3.one * scale;
                
                yield return null;
            }
            
            Destroy(obj);
        }

        private System.Collections.IEnumerator ScreenShake(float duration, float magnitude)
        {
            if (_mainCamera == null) yield break;
            
            Vector3 originalPos = _mainCamera.transform.position;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;
                
                _mainCamera.transform.position = new Vector3(
                    originalPos.x + x,
                    originalPos.y + y,
                    originalPos.z);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            _mainCamera.transform.position = originalPos;
        }

        // === POWER-UP SYSTEM ===
        
        private void UpdatePowerUps()
        {
            // Update power-up timers
            if (_doubleCoins)
            {
                _doubleCoinTimer -= Time.deltaTime;
                if (_doubleCoinTimer <= 0)
                {
                    _doubleCoins = false;
                    ShowFloatingText("Double Coins ended!", Color.gray, Vector3.zero, true);
                }
            }
            
            if (_rapidFire)
            {
                _rapidFireTimer -= Time.deltaTime;
                if (_rapidFireTimer <= 0)
                {
                    _rapidFire = false;
                    ShowFloatingText("Rapid Fire ended!", Color.gray, Vector3.zero, true);
                }
            }
            
            UpdatePowerUpStatus();
            
            // Spawn power-ups during waves
            if (_gameRunning)
            {
                _powerUpSpawnTimer -= Time.deltaTime;
                if (_powerUpSpawnTimer <= 0)
                {
                    _powerUpSpawnTimer = Random.Range(8f, 15f);
                    if (Random.value < 0.3f) // 30% chance to spawn
                    {
                        SpawnPowerUp();
                    }
                }
            }
            
            // Update power-ups (movement/collection)
            for (int i = _powerUps.Count - 1; i >= 0; i--)
            {
                if (_powerUps[i] == null)
                {
                    _powerUps.RemoveAt(i);
                    continue;
                }
                
                _powerUps[i].Update();
                
                // Check if collected (clicked)
                if (_powerUps[i].IsCollected)
                {
                    ApplyPowerUp(_powerUps[i].Type);
                    Destroy(_powerUps[i].gameObject);
                    _powerUps.RemoveAt(i);
                }
            }
        }
        
        private void SpawnPowerUp()
        {
            // Random position in play area
            float x = Random.Range(-3f, 3f);
            float y = Random.Range(-1f, 2f);
            
            int type = Random.Range(0, 5);
            
            GameObject powerUpObj = new GameObject($"PowerUp_{type}");
            powerUpObj.transform.position = new Vector3(x, y, -0.5f);
            
            PowerUp powerUp = powerUpObj.AddComponent<PowerUp>();
            powerUp.Initialize(type);
            
            _powerUps.Add(powerUp);
        }
        
        private void ApplyPowerUp(int type)
        {
            if (_audio != null) _audio.PlayUpgrade();
            
            switch (type)
            {
                case 0: // Double Coins
                    _doubleCoins = true;
                    _doubleCoinTimer = 10f;
                    ShowFloatingText("💰 DOUBLE COINS! 💰", Color.yellow, Vector3.zero, true);
                    break;
                    
                case 1: // Rapid Fire
                    _rapidFire = true;
                    _rapidFireTimer = 8f;
                    ShowFloatingText("🔥 RAPID FIRE! 🔥", Color.red, Vector3.zero, true);
                    break;
                    
                case 2: // Instant Coins
                    int bonus = 50 + _wave * 10;
                    _coins += bonus;
                    ShowFloatingText($"+${bonus} BONUS!", Color.green, Vector3.zero, true);
                    UpdateUI();
                    break;
                    
                case 3: // Freeze Enemies
                    StartCoroutine(FreezeEnemies(3f));
                    ShowFloatingText("❄️ FREEZE! ❄️", Color.cyan, Vector3.zero, true);
                    break;
                    
                case 4: // Ultimate Charge
                    _ultimateCharge = Mathf.Min(100f, _ultimateCharge + 50f);
                    UpdateUltimateButton();
                    ShowFloatingText("⚡ +50% ULTIMATE! ⚡", new Color(0.8f, 0.4f, 1f), Vector3.zero, true);
                    break;
            }
        }
        
        private System.Collections.IEnumerator FreezeEnemies(float duration)
        {
            foreach (var enemy in _enemies)
            {
                if (enemy != null)
                {
                    enemy.Freeze(duration);
                }
            }
            yield return new WaitForSeconds(duration);
        }
        
        private void UpdatePowerUpStatus()
        {
            if (_powerUpStatusText == null) return;
            
            string status = "";
            if (_doubleCoins) status += $"💰x2 ({_doubleCoinTimer:F0}s) ";
            if (_rapidFire) status += $"🔥 ({_rapidFireTimer:F0}s) ";
            
            _powerUpStatusText.text = status;
        }
        
        // Public getter for rapid fire status
        public bool IsRapidFireActive() => _rapidFire;
        public bool IsFeverModeActive() => _feverMode;

        private Button CreatePauseButton(Transform parent)
        {
            GameObject btnObj = new GameObject("PauseButton");
            btnObj.transform.SetParent(parent, false);
            
            Image bg = btnObj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.18f, 0.25f, 0.9f);
            
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(TogglePause);
            
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.anchoredPosition = new Vector2(60, 0);
            rect.sizeDelta = new Vector2(60, 60);
            
            // Pause icon
            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI tmp = icon.AddComponent<TextMeshProUGUI>();
            tmp.text = "⏸️";
            tmp.fontSize = 30;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.sizeDelta = Vector2.zero;

            return btn;
        }

        private void CreatePausePanel()
        {
            // Dark overlay
            _pausePanel = CreateModernPanel("PauseOverlay",
                Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero,
                new Color(0.02f, 0.03f, 0.05f, 0.9f));

            // Pause container
            GameObject container = CreateModernPanel("PauseContainer",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(400, 450),
                new Color(0.08f, 0.1f, 0.15f, 0.98f));
            container.transform.SetParent(_pausePanel.transform, false);

            CreateStyledText("⏸️ PAUSED", 42, Color.white, container.transform, new Vector2(0, 150));

            // Resume button
            CreateModernButton("▶️ RESUME", new Vector2(0, 50), container.transform,
                new Color(0.2f, 0.7f, 0.35f), new Color(0.15f, 0.5f, 0.25f),
                TogglePause);

            // Restart button
            CreateModernButton("🔄 RESTART", new Vector2(0, -30), container.transform,
                new Color(0.5f, 0.5f, 0.2f), new Color(0.4f, 0.4f, 0.15f),
                RestartGame);

            // Main menu button
            CreateModernButton("🏠 MAIN MENU", new Vector2(0, -110), container.transform,
                new Color(0.4f, 0.4f, 0.5f), new Color(0.3f, 0.3f, 0.4f),
                GoToMainMenu);

            _pausePanel.SetActive(false);
        }

        private void CreateVictoryPanel()
        {
            _victoryPanel = CreateModernPanel("VictoryOverlay",
                Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero,
                new Color(0.02f, 0.05f, 0.03f, 0.9f));

            GameObject container = CreateModernPanel("VictoryContainer",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(450, 500),
                new Color(0.08f, 0.12f, 0.1f, 0.98f));
            container.transform.SetParent(_victoryPanel.transform, false);

            CreateStyledText("🏆 VICTORY!", 48, new Color(1f, 0.85f, 0.2f), container.transform, new Vector2(0, 180));
            CreateStyledText("⭐⭐⭐", 40, new Color(1f, 0.9f, 0.3f), container.transform, new Vector2(0, 100));

            _victoryPanel.SetActive(false);
        }

        private void TogglePause()
        {
            _isPaused = !_isPaused;
            _pausePanel.SetActive(_isPaused);
            Time.timeScale = _isPaused ? 0f : 1f;
            
            if (_audio != null)
                _audio.PlayClick();
        }

        private void RestartGame()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        private void GoToMainMenu()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        private GameObject CreateModernPanel(string name, Vector2 anchorMin, Vector2 anchorMax, 
            Vector2 anchoredPos, Vector2 sizeDelta, Color bgColor)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(_canvas.transform, false);
            
            Image bg = panel.AddComponent<Image>();
            bg.color = bgColor;
            
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;
            
            // Add subtle outline
            GameObject outline = new GameObject("Outline");
            outline.transform.SetParent(panel.transform, false);
            Image outlineImg = outline.AddComponent<Image>();
            outlineImg.color = new Color(1f, 1f, 1f, 0.08f);
            RectTransform outlineRect = outline.GetComponent<RectTransform>();
            outlineRect.anchorMin = Vector2.zero;
            outlineRect.anchorMax = Vector2.one;
            outlineRect.sizeDelta = new Vector2(2, 2);
            outlineRect.anchoredPosition = Vector2.zero;
            
            return panel;
        }

        private TextMeshProUGUI CreateModernLabel(string icon, string value, Vector2 pos, 
            Transform parent, Color valueColor, bool rightAlign = false)
        {
            GameObject container = new GameObject("Label");
            container.transform.SetParent(parent, false);
            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.anchoredPosition = pos;
            containerRect.sizeDelta = new Vector2(140, 50);
            if (rightAlign)
            {
                containerRect.anchorMin = new Vector2(1, 0.5f);
                containerRect.anchorMax = new Vector2(1, 0.5f);
            }

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(container.transform, false);
            TextMeshProUGUI iconText = iconObj.AddComponent<TextMeshProUGUI>();
            iconText.text = icon;
            iconText.fontSize = 28;
            iconText.alignment = TextAlignmentOptions.Left;
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchoredPosition = new Vector2(-50, 0);
            iconRect.sizeDelta = new Vector2(40, 40);

            // Value
            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(container.transform, false);
            TextMeshProUGUI valueText = valueObj.AddComponent<TextMeshProUGUI>();
            valueText.text = value;
            valueText.fontSize = 26;
            valueText.color = valueColor;
            valueText.fontStyle = FontStyles.Bold;
            valueText.alignment = TextAlignmentOptions.Left;
            RectTransform valueRect = valueObj.GetComponent<RectTransform>();
            valueRect.anchoredPosition = new Vector2(10, 0);
            valueRect.sizeDelta = new Vector2(100, 40);

            return valueText;
        }

        private TextMeshProUGUI CreateStyledText(string text, int size, Color color, 
            Transform parent, Vector2? position = null)
        {
            GameObject obj = new GameObject("Text");
            obj.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = position ?? Vector2.zero;
            rect.sizeDelta = new Vector2(500, 50);
            return tmp;
        }

        private Button CreateModernButton(string text, Vector2 pos, Transform parent,
            Color topColor, Color bottomColor, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject("Button");
            btnObj.transform.SetParent(parent, false);
            
            // Button background
            Image bg = btnObj.AddComponent<Image>();
            bg.color = topColor;
            
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(onClick);
            
            // Button colors for states
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            colors.selectedColor = Color.white;
            btn.colors = colors;
            
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(200, 65);
            
            // Glow/shadow effect
            GameObject glow = new GameObject("Glow");
            glow.transform.SetParent(btnObj.transform, false);
            glow.transform.SetAsFirstSibling();
            Image glowImg = glow.AddComponent<Image>();
            glowImg.color = new Color(topColor.r, topColor.g, topColor.b, 0.3f);
            RectTransform glowRect = glow.GetComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.sizeDelta = new Vector2(10, 10);
            glowRect.anchoredPosition = new Vector2(0, -3);
            
            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
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
            textRect.anchoredPosition = Vector2.zero;

            return btn;
        }

        private void SetupGrid()
        {
            _gridCells = new GameObject[gridWidth, gridHeight];
            _defenders = new TestDefender[gridWidth, gridHeight];

            // Create grid container for organization
            GameObject gridContainer = new GameObject("GridContainer");
            
            // Calculate grid positioning (centered, in middle of screen)
            float totalWidth = (gridWidth - 1) * cellSize;
            float totalHeight = (gridHeight - 1) * cellSize;
            float startX = -totalWidth / 2f;
            float startY = -0.5f; // Centered vertically

            // Create background for grid area
            GameObject gridBg = new GameObject("GridBackground");
            gridBg.transform.SetParent(gridContainer.transform);
            gridBg.transform.position = new Vector3(0, startY + totalHeight / 2f, 1);
            SpriteRenderer bgSr = gridBg.AddComponent<SpriteRenderer>();
            bgSr.sprite = SpriteGenerator.CreateSquare(64, new Color(0.08f, 0.1f, 0.14f, 0.9f));
            bgSr.sortingOrder = -5;
            gridBg.transform.localScale = new Vector3(totalWidth + 1.5f, totalHeight + 1.5f, 1);

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3 pos = new Vector3(startX + x * cellSize, startY + y * cellSize, 0);
                    
                    GameObject cell = new GameObject($"Cell_{x}_{y}");
                    cell.transform.SetParent(gridContainer.transform);
                    cell.transform.position = pos;
                    
                    SpriteRenderer sr = cell.AddComponent<SpriteRenderer>();
                    sr.sprite = SpriteGenerator.CreateModernGridCell(64);
                    sr.sortingOrder = 0;
                    cell.transform.localScale = Vector3.one * cellSize * 0.92f;

                    // Add click detection
                    BoxCollider2D col = cell.AddComponent<BoxCollider2D>();
                    GridCellClick click = cell.AddComponent<GridCellClick>();
                    click.Initialize(x, y, this);

                    _gridCells[x, y] = cell;
                }
            }
        }

        private void SetupEnemyPath()
        {
            // Enemies path - horizontal through middle of screen
            float pathY = -0.5f + (gridHeight - 1) * cellSize + 1.2f; // Above grid
            _enemySpawnPoint = new Vector3(7, pathY, 0);
            _enemyEndPoint = new Vector3(-7, pathY, 0);

            // Create path lane visual
            GameObject pathLane = new GameObject("PathLane");
            pathLane.transform.position = new Vector3(0, pathY, 0.5f);
            SpriteRenderer laneSr = pathLane.AddComponent<SpriteRenderer>();
            laneSr.sprite = SpriteGenerator.CreateSquare(64, new Color(0.25f, 0.12f, 0.12f, 0.6f));
            laneSr.sortingOrder = -3;
            pathLane.transform.localScale = new Vector3(15f, 0.8f, 1);

            // Arrow indicators on path
            for (int i = 0; i < 5; i++)
            {
                GameObject arrow = new GameObject($"Arrow_{i}");
                float t = i / 4f;
                arrow.transform.position = Vector3.Lerp(_enemySpawnPoint, _enemyEndPoint, t) + new Vector3(0, 0, -0.1f);
                SpriteRenderer arrowSr = arrow.AddComponent<SpriteRenderer>();
                arrowSr.sprite = SpriteGenerator.CreateArrow(32, new Color(0.5f, 0.2f, 0.2f, 0.5f));
                arrowSr.sortingOrder = -2;
                arrow.transform.localScale = Vector3.one * 0.4f;
            }
        }

        private bool _wasMouseDown;
        
        private void HandleInput()
        {
            // Simple mouse/touch detection that works with any input system
            bool isMouseDown = false;
            Vector3 mousePos = Vector3.zero;

            // Check for touch first (mobile)
            if (UnityEngine.Input.touchCount > 0)
            {
                Touch touch = UnityEngine.Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    isMouseDown = true;
                    mousePos = touch.position;
                }
            }
            // Fall back to mouse
            else
            {
                // Use GetMouseButton and track state manually
                bool currentMouseDown = UnityEngine.Input.GetMouseButton(0);
                if (currentMouseDown && !_wasMouseDown)
                {
                    isMouseDown = true;
                    mousePos = UnityEngine.Input.mousePosition;
                }
                _wasMouseDown = currentMouseDown;
            }

            if (isMouseDown && Camera.main != null)
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
                worldPos.z = 0;

                // Check each grid cell
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        if (_gridCells[x, y] != null)
                        {
                            float dist = Vector3.Distance(worldPos, _gridCells[x, y].transform.position);
                            if (dist < cellSize * 0.5f)
                            {
                                OnCellClicked(x, y);
                                return;
                            }
                        }
                    }
                }
            }
        }

        public void OnCellClicked(int x, int y)
        {
            if (_defenders[x, y] == null && _pendingSpawn)
            {
                PlaceDefender(x, y);
                _pendingSpawn = false;
            }
            else if (_defenders[x, y] != null)
            {
                // Select for potential merge
                if (_selectedDefender != null && _selectedDefender != _defenders[x, y])
                {
                    TryMerge(_selectedDefender, _defenders[x, y]);
                    _selectedDefender = null;
                }
                else
                {
                    _selectedDefender = _defenders[x, y];
                    Debug.Log($"[GameplayTest] Selected Tier {_selectedDefender.Tier} defender");
                }
            }
        }

        private bool _pendingSpawn;

        private void OnSpawnDefender()
        {
            if (_coins >= defenderCost)
            {
                _pendingSpawn = true;
                Debug.Log("[GameplayTest] Click an empty cell to place defender");
            }
            else
            {
                Debug.Log("[GameplayTest] Not enough coins!");
            }
        }

        private void PlaceDefender(int x, int y)
        {
            _coins -= defenderCost;
            
            Vector3 pos = _gridCells[x, y].transform.position;
            GameObject defObj = new GameObject($"Defender_{x}_{y}");
            defObj.transform.position = pos;

            TestDefender defender = defObj.AddComponent<TestDefender>();
            defender.Initialize(1); // Start at tier 1
            defender.GridX = x;
            defender.GridY = y;

            _defenders[x, y] = defender;
            UpdateUI();
            
            Debug.Log($"[GameplayTest] Placed Tier 1 defender at ({x}, {y})");
        }

        private void TryMerge(TestDefender a, TestDefender b)
        {
            if (a.Tier == b.Tier && a.Tier < 5)
            {
                int newTier = a.Tier + 1;
                int bx = b.GridX, by = b.GridY;
                
                // Remove old defender
                _defenders[a.GridX, a.GridY] = null;
                Destroy(a.gameObject);
                
                // Upgrade the target
                b.Initialize(newTier);
                _score += newTier * 10;
                UpdateUI();
                
                // Play merge sound
                if (_audio != null) _audio.PlayMerge();
                
                Debug.Log($"[GameplayTest] Merged to Tier {newTier}!");
            }
            else if (a.Tier != b.Tier)
            {
                Debug.Log("[GameplayTest] Can only merge same tier defenders!");
            }
            else
            {
                Debug.Log("[GameplayTest] Max tier reached!");
            }
        }

        private void OnStartWave()
        {
            if (_gameRunning) return;
            
            _gameRunning = true;
            _enemiesToSpawn = 3 + _wave * 2;
            _enemiesThisWave = _enemiesToSpawn;
            _enemySpawnTimer = 0.5f;
            
            // Play wave start sound
            if (_audio != null) _audio.PlayWaveStart();
            
            _startWaveButton.interactable = false;
            
            // Play wave start sound
            if (_audio != null) _audio.PlayWaveStart();
            
            Debug.Log($"[GameplayTest] Wave {_wave} started! {_enemiesToSpawn} enemies incoming!");
        }

        private void SpawnEnemy()
        {
            GameObject enemyObj = new GameObject($"Enemy_{_enemiesThisWave - _enemiesToSpawn}");
            enemyObj.transform.position = _enemySpawnPoint;

            TestEnemy enemy = enemyObj.AddComponent<TestEnemy>();
            
            // Determine enemy type with special types
            bool isBoss = (_wave % 5 == 0) && (_enemiesToSpawn == 1); // Boss on last enemy of every 5th wave
            bool isElite = !isBoss && Random.value < 0.1f + (_wave * 0.02f); // 10% + 2% per wave chance of elite
            
            int type;
            int health;
            float speed;
            
            if (isBoss)
            {
                type = 4; // Boss type
                health = 200 + _wave * 50;
                speed = 0.6f;
                ShowFloatingText("⚠️ BOSS INCOMING! ⚠️", Color.red, Vector3.zero, true);
                StartCoroutine(ScreenShake(0.3f, 0.1f));
            }
            else if (isElite)
            {
                type = 3; // Elite type
                health = 50 + _wave * 20;
                speed = 1.0f;
            }
            else
            {
                type = Random.Range(0, Mathf.Min(3, _wave));
                health = 20 + _wave * 10 + type * 15;
                speed = 1.2f + (type == 2 ? 0.6f : 0);
            }
            
            enemy.Initialize(type, health, speed, isBoss, isElite);
            _enemies.Add(enemy);
        }

        public void OnEnemyKilled(TestEnemy enemy, int reward)
        {
            // Fever mode bonus
            int bonusMultiplier = _feverMode ? 2 : 1;
            if (_doubleCoins) bonusMultiplier *= 2; // Double coins power-up
            int comboBonus = _combo > 0 ? _combo * 2 : 0;
            int totalReward = (reward + comboBonus) * bonusMultiplier;
            
            _coins += totalReward;
            _score += totalReward * 2;
            _enemies.Remove(enemy);
            _enemiesKilled++;
            
            // Trigger combo and kill streak
            AddCombo();
            AddKillStreak();
            
            // Show floating damage text
            if (enemy != null)
            {
                string rewardText = $"+${totalReward}";
                if (_combo > 1) rewardText += $" x{_combo}";
                ShowFloatingText(rewardText, new Color(1f, 0.9f, 0.2f), enemy.transform.position);
            }
            
            UpdateUI();
            
            // Play enemy death sound and coin sound
            if (_audio != null)
            {
                _audio.PlayEnemyDeath();
                _audio.PlayCoin();
            }
        }

        private void WaveComplete()
        {
            _wave++;
            _gameRunning = false;
            _startWaveButton.interactable = true;
            _coins += 20 + _wave * 5; // Wave bonus
            UpdateUI();
            
            // Play success sound
            if (_audio != null) _audio.PlaySuccess();
            
            Debug.Log($"[GameplayTest] Wave complete! Bonus coins awarded. Next: Wave {_wave}");
        }

        private void GameOver()
        {
            _gameRunning = false;
            
            // Play defeat sound
            if (_audio != null) _audio.PlayDefeat();
            
            // Save progress
            if (_save != null)
            {
                _save.Data.coins = _coins;
                _save.Data.gems = _gems;
                if (_score > _save.Data.highScore)
                    _save.Data.highScore = _score;
                _save.Data.totalGamesPlayed++;
                _save.Data.totalEnemiesKilled += _enemiesKilled;
                _save.SaveData();
            }
            
            Debug.Log($"[GameplayTest] GAME OVER! Final Score: {_score}");
            
            // Create modern game over overlay
            GameObject overlay = CreateModernPanel("GameOverOverlay",
                new Vector2(0, 0), new Vector2(1, 1),
                Vector2.zero, Vector2.zero,
                new Color(0.02f, 0.03f, 0.05f, 0.9f));
            
            GameObject panel = CreateModernPanel("GameOverPanel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(400, 350),
                new Color(0.1f, 0.12f, 0.18f, 0.98f));
            
            CreateStyledText("GAME OVER", 42, new Color(1f, 0.3f, 0.35f), panel.transform, new Vector2(0, 100));
            CreateStyledText($"Score: {_score}", 36, Color.white, panel.transform, new Vector2(0, 30));
            CreateStyledText($"Waves Survived: {_wave - 1}", 24, new Color(0.6f, 0.65f, 0.7f), panel.transform, new Vector2(0, -20));
            
            CreateModernButton("PLAY AGAIN", new Vector2(0, -100), panel.transform,
                new Color(0.2f, 0.7f, 0.3f), new Color(0.15f, 0.5f, 0.2f),
                () => UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex));
        }

        private void UpdateUI()
        {
            _coinsText.text = _coins.ToString();
            _livesText.text = $"❤️ {_lives} Lives";
            _waveText.text = $"Wave {_wave}";
            _scoreText.text = _score.ToString();
            _spawnButton.interactable = _coins >= defenderCost;
        }

        private List<TestDefender> GetAllDefenders()
        {
            List<TestDefender> list = new List<TestDefender>();
            foreach (var d in _defenders)
            {
                if (d != null) list.Add(d);
            }
            return list;
        }

        // Legacy helpers for backwards compatibility
        private void CreatePanel(Vector2 pos, Vector2 size, Color color)
        {
            CreateModernPanel("Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size, color);
        }
        
        private TextMeshProUGUI CreateText(string text, Vector2 pos, int size, Color color)
        {
            return CreateStyledText(text, size, color, _canvas.transform, pos);
        }
        
        private Button CreateButton(string text, Vector2 pos, UnityEngine.Events.UnityAction action)
        {
            return CreateModernButton(text, pos, _canvas.transform, 
                new Color(0.2f, 0.5f, 0.2f), new Color(0.15f, 0.4f, 0.15f), action);
        }
    }

    /// <summary>
    /// Handles grid cell clicks.
    /// </summary>
    public class GridCellClick : MonoBehaviour
    {
        private int _x, _y;
        private GameplayTest _game;

        public void Initialize(int x, int y, GameplayTest game)
        {
            _x = x;
            _y = y;
            _game = game;
        }

        private void OnMouseDown()
        {
            _game.OnCellClicked(_x, _y);
        }
    }

    /// <summary>
    /// Test defender with shooting capability.
    /// </summary>
    public class TestDefender : MonoBehaviour
    {
        public int Tier { get; private set; }
        public int GridX { get; set; }
        public int GridY { get; set; }

        private SpriteRenderer _sr;
        private float _attackCooldown;
        private float _attackTimer;
        private float _range = 3f;
        private int _damage;

        public void Initialize(int tier)
        {
            Tier = tier;
            _damage = 5 + tier * 5;
            _attackCooldown = Mathf.Max(0.3f, 1f - tier * 0.15f);
            _range = 2.5f + tier * 0.5f;

            if (_sr == null)
                _sr = gameObject.AddComponent<SpriteRenderer>();
            
            _sr.sprite = SpriteGenerator.CreateDefender(tier);
            _sr.sortingOrder = 5;
            transform.localScale = Vector3.one * 0.8f;
        }

        public void TryAttack(List<TestEnemy> enemies)
        {
            // Check for rapid fire power-up
            GameplayTest game = FindObjectOfType<GameplayTest>();
            float cooldownMultiplier = (game != null && game.IsRapidFireActive()) ? 0.5f : 1f;
            
            _attackTimer -= Time.deltaTime;
            if (_attackTimer > 0) return;

            TestEnemy target = null;
            float closestDist = _range;

            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    target = enemy;
                }
            }

            if (target != null)
            {
                Attack(target);
                _attackTimer = _attackCooldown * cooldownMultiplier;
            }
        }

        private void Attack(TestEnemy target)
        {
            // Check for critical hit (10% + 5% per tier)
            bool isCritical = Random.value < (0.1f + Tier * 0.05f);
            int damage = _damage;
            if (isCritical) damage *= 2;
            
            // Play shoot sound
            if (SimpleAudioManager.Instance != null)
                SimpleAudioManager.Instance.PlayShoot();
            
            // Create projectile
            GameObject proj = new GameObject("Projectile");
            proj.transform.position = transform.position;
            SpriteRenderer sr = proj.AddComponent<SpriteRenderer>();
            
            Color projColor = SpriteGenerator.DefenderColors[Tier - 1];
            if (isCritical)
            {
                projColor = Color.yellow; // Critical hits are yellow
                proj.transform.localScale = Vector3.one * 1.5f; // Bigger projectile
            }
            sr.sprite = SpriteGenerator.CreateProjectile(projColor);
            sr.sortingOrder = 10;

            Projectile p = proj.AddComponent<Projectile>();
            p.Initialize(target, damage, isCritical);
        }
    }

    /// <summary>
    /// Simple projectile with critical hit support.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        private TestEnemy _target;
        private int _damage;
        private float _speed = 10f;
        private bool _isCritical;

        public void Initialize(TestEnemy target, int damage, bool isCritical = false)
        {
            _target = target;
            _damage = damage;
            _isCritical = isCritical;
            if (_isCritical) _speed = 15f; // Faster critical projectiles
        }

        private void Update()
        {
            if (_target == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 dir = (_target.transform.position - transform.position).normalized;
            transform.position += dir * _speed * Time.deltaTime;

            if (Vector3.Distance(transform.position, _target.transform.position) < 0.2f)
            {
                // Play hit sound
                if (SimpleAudioManager.Instance != null)
                {
                    SimpleAudioManager.Instance.PlayHit();
                    if (_isCritical)
                        SimpleAudioManager.Instance.PlaySuccess(); // Extra sound for crit
                }
                
                // Show damage number
                ShowDamageNumber();
                
                _target.TakeDamage(_damage);
                Destroy(gameObject);
            }
        }
        
        private void ShowDamageNumber()
        {
            // Find canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;
            
            GameObject dmgObj = new GameObject("DamageNumber");
            dmgObj.transform.SetParent(canvas.transform, false);
            
            TextMeshProUGUI tmp = dmgObj.AddComponent<TextMeshProUGUI>();
            tmp.text = _isCritical ? $"CRIT! {_damage}" : _damage.ToString();
            tmp.fontSize = _isCritical ? 28 : 20;
            tmp.color = _isCritical ? Color.yellow : Color.white;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            
            RectTransform rect = dmgObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(150, 50);
            
            if (Camera.main != null && _target != null)
            {
                Vector2 screenPos = Camera.main.WorldToScreenPoint(_target.transform.position);
                rect.position = screenPos;
            }
            
            // Animate
            StartCoroutine(AnimateDamageNumber(dmgObj, rect, tmp));
        }
        
        private System.Collections.IEnumerator AnimateDamageNumber(GameObject obj, RectTransform rect, TextMeshProUGUI tmp)
        {
            float duration = 0.8f;
            float elapsed = 0f;
            Vector2 startPos = rect.anchoredPosition;
            Color startColor = tmp.color;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                rect.anchoredPosition = startPos + Vector2.up * (t * 80f) + Vector2.right * Random.Range(-5f, 5f);
                tmp.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
                
                yield return null;
            }
            
            Destroy(obj);
        }
    }

    /// <summary>
    /// Test enemy.
    /// </summary>
    public class TestEnemy : MonoBehaviour
    {
        public int Type { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public bool IsBoss { get; private set; }
        public bool IsElite { get; private set; }
        
        private float _speed;
        private SpriteRenderer _sr;
        private GameplayTest _game;
        private float _damageFlashTimer;
        private Color _originalColor;

        public void Initialize(int type, int health, float speed, bool isBoss = false, bool isElite = false)
        {
            Type = type;
            Health = health;
            MaxHealth = health;
            _speed = speed;
            IsBoss = isBoss;
            IsElite = isElite;

            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = SpriteGenerator.CreateEnemy(type);
            _sr.sortingOrder = 3;
            
            // Size based on type
            float size = 0.7f;
            if (type == 1) size = 1f; // Tank
            if (isElite) size = 0.9f;
            if (isBoss) size = 1.5f;
            transform.localScale = Vector3.one * size;
            
            // Color based on type
            if (isBoss)
            {
                _sr.color = new Color(1f, 0.3f, 0.8f); // Purple boss
            }
            else if (isElite)
            {
                _sr.color = new Color(1f, 0.8f, 0.2f); // Golden elite
            }
            _originalColor = _sr.color;

            _game = FindObjectOfType<GameplayTest>();

            // Health bar
            GameObject healthBar = new GameObject("HealthBar");
            healthBar.transform.SetParent(transform);
            healthBar.transform.localPosition = new Vector3(0, 0.6f, 0);
            SpriteRenderer hbSr = healthBar.AddComponent<SpriteRenderer>();
            hbSr.sprite = SpriteGenerator.CreateSquare(32, Color.green);
            hbSr.sortingOrder = 4;
            healthBar.transform.localScale = new Vector3(1f, 0.1f, 1f);
            
            // Boss/Elite indicator
            if (isBoss || isElite)
            {
                GameObject indicator = new GameObject("Indicator");
                indicator.transform.SetParent(transform);
                indicator.transform.localPosition = new Vector3(0, 0.9f, 0);
                SpriteRenderer indSr = indicator.AddComponent<SpriteRenderer>();
                indSr.sprite = SpriteGenerator.CreateSquare(16, isBoss ? Color.magenta : Color.yellow);
                indSr.sortingOrder = 5;
                indicator.transform.localScale = new Vector3(0.3f, 0.15f, 1f);
            }
        }

        private bool _isFrozen;
        private float _freezeTimer;
        private float _baseSpeed;

        private void Update()
        {
            // Handle freeze
            if (_isFrozen)
            {
                _freezeTimer -= Time.deltaTime;
                if (_freezeTimer <= 0)
                {
                    _isFrozen = false;
                    _speed = _baseSpeed;
                    _sr.color = _originalColor;
                }
                return;
            }
            
            // Damage flash effect
            if (_damageFlashTimer > 0)
            {
                _damageFlashTimer -= Time.deltaTime;
                _sr.color = Color.Lerp(_originalColor, Color.white, _damageFlashTimer / 0.1f);
            }
            
            // Boss pulsing effect
            if (IsBoss)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 3f) * 0.1f;
                transform.localScale = Vector3.one * 1.5f * pulse;
            }
        }
        
        public void Freeze(float duration)
        {
            _isFrozen = true;
            _freezeTimer = duration;
            _baseSpeed = _speed;
            _speed = 0f;
            _sr.color = Color.cyan;
        }

        public void UpdateMovement(Vector3 target)
        {
            if (_isFrozen) return;
            
            Vector3 dir = (target - transform.position).normalized;
            transform.position += dir * _speed * Time.deltaTime;
        }

        public void TakeDamage(int damage)
        {
            Health -= damage;
            _damageFlashTimer = 0.1f;
            
            // Update health bar
            Transform hb = transform.Find("HealthBar");
            if (hb != null)
            {
                float healthPercent = (float)Health / MaxHealth;
                hb.localScale = new Vector3(Mathf.Max(0, healthPercent), 0.1f, 1f);
                hb.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.red, Color.green, healthPercent);
            }

            if (Health <= 0)
            {
                int reward = 5 + Type * 3;
                if (IsElite) reward *= 3;
                if (IsBoss) reward *= 10;
                _game.OnEnemyKilled(this, reward);
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Power-up that can be collected for bonuses.
    /// </summary>
    public class PowerUp : MonoBehaviour
    {
        public int Type { get; private set; }
        public bool IsCollected { get; private set; }
        
        private SpriteRenderer _sr;
        private float _bobTimer;
        private Vector3 _startPos;
        private float _lifetime = 10f;
        
        private static readonly string[] PowerUpEmojis = { "💰", "🔥", "💵", "❄️", "⚡" };
        private static readonly Color[] PowerUpColors = {
            new Color(1f, 0.85f, 0.2f),  // Gold - Double Coins
            new Color(1f, 0.3f, 0.1f),   // Red - Rapid Fire
            new Color(0.3f, 0.9f, 0.3f), // Green - Instant Coins
            new Color(0.3f, 0.8f, 1f),   // Cyan - Freeze
            new Color(0.8f, 0.4f, 1f)    // Purple - Ultimate Charge
        };

        public void Initialize(int type)
        {
            Type = type;
            _startPos = transform.position;
            
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = SpriteGenerator.CreateSquare(48, PowerUpColors[type]);
            _sr.sortingOrder = 8;
            transform.localScale = Vector3.one * 0.6f;
            
            // Add collider for clicking
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.size = Vector2.one * 1.5f;
        }

        public void Update()
        {
            _lifetime -= Time.deltaTime;
            if (_lifetime <= 0)
            {
                Destroy(gameObject);
                return;
            }
            
            // Bob up and down
            _bobTimer += Time.deltaTime * 3f;
            transform.position = _startPos + Vector3.up * Mathf.Sin(_bobTimer) * 0.2f;
            
            // Rotate
            transform.Rotate(0, 0, 90f * Time.deltaTime);
            
            // Pulse when about to expire
            if (_lifetime < 3f)
            {
                float pulse = Mathf.PingPong(Time.time * 5f, 1f);
                _sr.color = Color.Lerp(PowerUpColors[Type], Color.white, pulse);
            }
        }

        private void OnMouseDown()
        {
            IsCollected = true;
        }
    }
}
