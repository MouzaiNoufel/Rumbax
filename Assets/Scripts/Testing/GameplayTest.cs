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

        private void Start()
        {
            SetupCamera();
            SetupUI();
            SetupGrid();
            SetupEnemyPath();
            
            _coins = startingCoins;
            _gameRunning = false;
            UpdateUI();
            
            Debug.Log("[GameplayTest] Ready! Click 'Spawn Defender' then 'Start Wave'");
        }

        private void Update()
        {
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
            cam.backgroundColor = new Color(0.08f, 0.1f, 0.15f);
            cam.orthographic = true;
            cam.orthographicSize = 6;
            cam.transform.position = new Vector3(2, 0, -10);
            
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
            // Create canvas
            GameObject canvasObj = new GameObject("GameCanvas");
            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // Top bar with stats
            CreatePanel(new Vector2(0, -50), new Vector2(1000, 100), new Color(0.1f, 0.1f, 0.15f, 0.9f));
            
            _coinsText = CreateText($"Coins: {_coins}", new Vector2(-300, -50), 32, Color.yellow);
            _livesText = CreateText($"Lives: {_lives}", new Vector2(0, -50), 32, Color.red);
            _waveText = CreateText($"Wave: {_wave}", new Vector2(300, -50), 32, Color.cyan);

            // Score below top bar
            _scoreText = CreateText("Score: 0", new Vector2(0, -120), 28, Color.white);

            // Bottom panel with buttons
            CreatePanel(new Vector2(0, 280), new Vector2(1000, 180), new Color(0.1f, 0.1f, 0.15f, 0.8f));
            
            // Buttons - positioned at bottom
            _spawnButton = CreateButton($"SPAWN (${defenderCost})", new Vector2(-200, 320), OnSpawnDefender);
            _startWaveButton = CreateButton("START WAVE", new Vector2(200, 320), OnStartWave);

            // Instructions above buttons
            CreateText("Click SPAWN, then click an empty cell to place defender", new Vector2(0, 240), 20, Color.white);
            CreateText("Click two same-tier defenders to MERGE them!", new Vector2(0, 210), 18, new Color(0.5f, 1f, 0.5f));
        }

        private void SetupGrid()
        {
            _gridCells = new GameObject[gridWidth, gridHeight];
            _defenders = new TestDefender[gridWidth, gridHeight];

            float startX = -(gridWidth - 1) * cellSize / 2f;
            float startY = -2f;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3 pos = new Vector3(startX + x * cellSize, startY + y * cellSize, 0);
                    
                    GameObject cell = new GameObject($"Cell_{x}_{y}");
                    cell.transform.position = pos;
                    
                    SpriteRenderer sr = cell.AddComponent<SpriteRenderer>();
                    sr.sprite = SpriteGenerator.CreateGridCell(64);
                    sr.sortingOrder = 0;
                    cell.transform.localScale = Vector3.one * cellSize * 0.95f;

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
            // Enemies come from right, go to left
            _enemySpawnPoint = new Vector3(8, -1f, 0);
            _enemyEndPoint = new Vector3(-6, -1f, 0);

            // Visual path indicator
            for (int i = 0; i < 10; i++)
            {
                GameObject pathDot = new GameObject($"PathDot_{i}");
                pathDot.transform.position = Vector3.Lerp(_enemySpawnPoint, _enemyEndPoint, i / 9f);
                SpriteRenderer sr = pathDot.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteGenerator.CreateSquare(8, new Color(0.3f, 0.2f, 0.2f, 0.5f));
                sr.sortingOrder = -1;
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
            
            _startWaveButton.interactable = false;
            Debug.Log($"[GameplayTest] Wave {_wave} started! {_enemiesToSpawn} enemies incoming!");
        }

        private void SpawnEnemy()
        {
            GameObject enemyObj = new GameObject($"Enemy_{_enemiesThisWave - _enemiesToSpawn}");
            enemyObj.transform.position = _enemySpawnPoint;

            TestEnemy enemy = enemyObj.AddComponent<TestEnemy>();
            int type = Random.Range(0, Mathf.Min(3, _wave)); // More enemy types in later waves
            int health = 20 + _wave * 10 + type * 15;
            float speed = 1f + (type == 2 ? 0.5f : 0); // Fast enemies
            enemy.Initialize(type, health, speed);

            _enemies.Add(enemy);
        }

        public void OnEnemyKilled(TestEnemy enemy, int reward)
        {
            _coins += reward;
            _score += reward * 2;
            _enemies.Remove(enemy);
            UpdateUI();
        }

        private void WaveComplete()
        {
            _wave++;
            _gameRunning = false;
            _startWaveButton.interactable = true;
            _coins += 20 + _wave * 5; // Wave bonus
            UpdateUI();
            
            Debug.Log($"[GameplayTest] Wave complete! Bonus coins awarded. Next: Wave {_wave}");
        }

        private void GameOver()
        {
            _gameRunning = false;
            Debug.Log($"[GameplayTest] GAME OVER! Final Score: {_score}");
            
            // Create game over panel
            CreatePanel(new Vector2(0, 0), new Vector2(400, 300), new Color(0.1f, 0.1f, 0.1f, 0.95f));
            CreateText("GAME OVER", new Vector2(0, 50), 48, Color.red);
            CreateText($"Score: {_score}", new Vector2(0, 0), 32, Color.white);
            CreateText($"Waves Survived: {_wave - 1}", new Vector2(0, -40), 24, Color.gray);
            CreateButton("Restart", new Vector2(0, -100), () => UnityEngine.SceneManagement.SceneManager.LoadScene(0));
        }

        private void UpdateUI()
        {
            _coinsText.text = $"Coins: {_coins}";
            _livesText.text = $"Lives: {_lives}";
            _waveText.text = $"Wave: {_wave}";
            _scoreText.text = $"Score: {_score}";
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

        // UI Helpers
        private TextMeshProUGUI CreateText(string text, Vector2 pos, int size, Color color)
        {
            GameObject obj = new GameObject("Text");
            obj.transform.SetParent(_canvas.transform, false);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(600, 60);
            return tmp;
        }

        private Button CreateButton(string text, Vector2 pos, UnityEngine.Events.UnityAction action)
        {
            GameObject obj = new GameObject("Button");
            obj.transform.SetParent(_canvas.transform, false);
            Image img = obj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.5f, 0.2f);
            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(action);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(220, 50);

            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(obj.transform, false);
            TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 20;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform txtRect = txtObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = Vector2.zero;

            return btn;
        }

        private void CreatePanel(Vector2 pos, Vector2 size, Color color)
        {
            GameObject obj = new GameObject("Panel");
            obj.transform.SetParent(_canvas.transform, false);
            Image img = obj.AddComponent<Image>();
            img.color = color;
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
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
                _attackTimer = _attackCooldown;
            }
        }

        private void Attack(TestEnemy target)
        {
            // Create projectile
            GameObject proj = new GameObject("Projectile");
            proj.transform.position = transform.position;
            SpriteRenderer sr = proj.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreateProjectile(SpriteGenerator.DefenderColors[Tier - 1]);
            sr.sortingOrder = 10;

            Projectile p = proj.AddComponent<Projectile>();
            p.Initialize(target, _damage);
        }
    }

    /// <summary>
    /// Simple projectile.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        private TestEnemy _target;
        private int _damage;
        private float _speed = 10f;

        public void Initialize(TestEnemy target, int damage)
        {
            _target = target;
            _damage = damage;
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
                _target.TakeDamage(_damage);
                Destroy(gameObject);
            }
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
        
        private float _speed;
        private SpriteRenderer _sr;
        private GameplayTest _game;

        public void Initialize(int type, int health, float speed)
        {
            Type = type;
            Health = health;
            MaxHealth = health;
            _speed = speed;

            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = SpriteGenerator.CreateEnemy(type);
            _sr.sortingOrder = 3;
            transform.localScale = Vector3.one * (type == 1 ? 1f : 0.7f); // Tanks are bigger

            _game = FindObjectOfType<GameplayTest>();

            // Health bar
            GameObject healthBar = new GameObject("HealthBar");
            healthBar.transform.SetParent(transform);
            healthBar.transform.localPosition = new Vector3(0, 0.6f, 0);
            SpriteRenderer hbSr = healthBar.AddComponent<SpriteRenderer>();
            hbSr.sprite = SpriteGenerator.CreateSquare(32, Color.green);
            hbSr.sortingOrder = 4;
            healthBar.transform.localScale = new Vector3(1f, 0.1f, 1f);
        }

        public void UpdateMovement(Vector3 target)
        {
            Vector3 dir = (target - transform.position).normalized;
            transform.position += dir * _speed * Time.deltaTime;
        }

        public void TakeDamage(int damage)
        {
            Health -= damage;
            
            // Update health bar
            Transform hb = transform.Find("HealthBar");
            if (hb != null)
            {
                float healthPercent = (float)Health / MaxHealth;
                hb.localScale = new Vector3(healthPercent, 0.1f, 1f);
                hb.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.red, Color.green, healthPercent);
            }

            if (Health <= 0)
            {
                int reward = 5 + Type * 3;
                _game.OnEnemyKilled(this, reward);
                Destroy(gameObject);
            }
        }
    }
}
