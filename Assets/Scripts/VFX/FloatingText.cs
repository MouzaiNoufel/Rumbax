using UnityEngine;
using TMPro;

namespace Rumbax.VFX
{
    /// <summary>
    /// Floating text that animates upward and fades out.
    /// Used for damage numbers, coin pickups, etc.
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshPro _textMesh;
        [SerializeField] private TextMeshProUGUI _textMeshUI;

        [Header("Animation Settings")]
        [SerializeField] private float _floatSpeed = 2f;
        [SerializeField] private float _duration = 1f;
        [SerializeField] private float _fadeStartTime = 0.5f;
        [SerializeField] private AnimationCurve _scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Optional Effects")]
        [SerializeField] private bool _randomHorizontalOffset = true;
        [SerializeField] private float _horizontalRange = 0.5f;
        [SerializeField] private float _scaleMultiplier = 1.5f;

        private float _timer;
        private Vector3 _startPosition;
        private Vector3 _direction;
        private Color _startColor;
        private float _startScale;
        private bool _isPlaying;

        private void Awake()
        {
            if (_textMesh == null)
            {
                _textMesh = GetComponent<TextMeshPro>();
            }
            if (_textMeshUI == null)
            {
                _textMeshUI = GetComponent<TextMeshProUGUI>();
            }
        }

        /// <summary>
        /// Show floating text with default settings.
        /// </summary>
        public void Show(string text, Color color)
        {
            SetText(text);
            SetColor(color);
            Play();
        }

        /// <summary>
        /// Show damage number.
        /// </summary>
        public void ShowDamage(int damage, bool isCritical = false)
        {
            string text = damage.ToString();
            Color color = isCritical ? Color.yellow : Color.white;
            
            if (isCritical)
            {
                text = damage + "!";
                _scaleMultiplier = 2f;
            }
            else
            {
                _scaleMultiplier = 1.5f;
            }

            Show(text, color);
        }

        /// <summary>
        /// Show coin pickup.
        /// </summary>
        public void ShowCoin(int amount)
        {
            string text = "+" + amount;
            Show(text, new Color(1f, 0.85f, 0f)); // Gold color
        }

        /// <summary>
        /// Show gem pickup.
        /// </summary>
        public void ShowGem(int amount)
        {
            string text = "+" + amount;
            Show(text, new Color(0.5f, 0.8f, 1f)); // Gem blue color
        }

        /// <summary>
        /// Show heal amount.
        /// </summary>
        public void ShowHeal(int amount)
        {
            string text = "+" + amount;
            Show(text, Color.green);
        }

        /// <summary>
        /// Show status text.
        /// </summary>
        public void ShowStatus(string status, Color color)
        {
            Show(status, color);
        }

        private void SetText(string text)
        {
            if (_textMesh != null)
            {
                _textMesh.text = text;
            }
            if (_textMeshUI != null)
            {
                _textMeshUI.text = text;
            }
        }

        private void SetColor(Color color)
        {
            _startColor = color;
            
            if (_textMesh != null)
            {
                _textMesh.color = color;
            }
            if (_textMeshUI != null)
            {
                _textMeshUI.color = color;
            }
        }

        private void Play()
        {
            _timer = 0f;
            _startPosition = transform.position;
            _startScale = transform.localScale.x;
            _isPlaying = true;

            // Random horizontal offset
            if (_randomHorizontalOffset)
            {
                float xOffset = Random.Range(-_horizontalRange, _horizontalRange);
                _direction = new Vector3(xOffset, 1f, 0f).normalized;
            }
            else
            {
                _direction = Vector3.up;
            }

            // Apply initial scale pop
            transform.localScale = Vector3.one * _startScale * _scaleMultiplier;

            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (!_isPlaying) return;

            _timer += Time.deltaTime;
            float t = _timer / _duration;

            if (t >= 1f)
            {
                Complete();
                return;
            }

            // Move upward
            transform.position = _startPosition + _direction * (_floatSpeed * _timer);

            // Scale animation (pop then shrink)
            float scaleT = _scaleCurve.Evaluate(t);
            float currentScale = Mathf.Lerp(_startScale * _scaleMultiplier, _startScale * 0.5f, scaleT);
            transform.localScale = Vector3.one * currentScale;

            // Fade out
            if (_timer > _fadeStartTime)
            {
                float fadeT = (_timer - _fadeStartTime) / (_duration - _fadeStartTime);
                Color fadeColor = _startColor;
                fadeColor.a = Mathf.Lerp(1f, 0f, fadeT);
                
                if (_textMesh != null)
                {
                    _textMesh.color = fadeColor;
                }
                if (_textMeshUI != null)
                {
                    _textMeshUI.color = fadeColor;
                }
            }
        }

        private void Complete()
        {
            _isPlaying = false;
            gameObject.SetActive(false);
            
            // Reset for pooling
            transform.localScale = Vector3.one * _startScale;
        }
    }

    /// <summary>
    /// Spawns and manages floating text instances.
    /// </summary>
    public class FloatingTextSpawner : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private GameObject _floatingTextPrefab;
        [SerializeField] private int _poolSize = 20;

        [Header("Settings")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private bool _useWorldSpace = true;

        private static FloatingTextSpawner _instance;
        public static FloatingTextSpawner Instance => _instance;

        private System.Collections.Generic.Queue<FloatingText> _pool = new System.Collections.Generic.Queue<FloatingText>();
        private Transform _poolParent;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            InitializePool();
        }

        private void InitializePool()
        {
            _poolParent = new GameObject("FloatingTextPool").transform;
            _poolParent.SetParent(transform);

            for (int i = 0; i < _poolSize; i++)
            {
                CreateInstance();
            }
        }

        private FloatingText CreateInstance()
        {
            if (_floatingTextPrefab == null)
            {
                Debug.LogError("[FloatingTextSpawner] No prefab assigned");
                return null;
            }

            GameObject obj;
            
            if (_useWorldSpace)
            {
                obj = Instantiate(_floatingTextPrefab, _poolParent);
            }
            else if (_canvas != null)
            {
                obj = Instantiate(_floatingTextPrefab, _canvas.transform);
            }
            else
            {
                obj = Instantiate(_floatingTextPrefab, _poolParent);
            }

            FloatingText text = obj.GetComponent<FloatingText>();
            if (text == null)
            {
                text = obj.AddComponent<FloatingText>();
            }

            obj.SetActive(false);
            _pool.Enqueue(text);
            return text;
        }

        private FloatingText GetFromPool()
        {
            if (_pool.Count > 0)
            {
                FloatingText text = _pool.Dequeue();
                _pool.Enqueue(text);
                return text;
            }
            
            return CreateInstance();
        }

        /// <summary>
        /// Spawn damage text at position.
        /// </summary>
        public static void SpawnDamage(Vector3 position, int damage, bool isCritical = false)
        {
            if (Instance == null) return;
            
            FloatingText text = Instance.GetFromPool();
            if (text != null)
            {
                text.transform.position = position;
                text.ShowDamage(damage, isCritical);
            }
        }

        /// <summary>
        /// Spawn coin pickup text.
        /// </summary>
        public static void SpawnCoin(Vector3 position, int amount)
        {
            if (Instance == null) return;
            
            FloatingText text = Instance.GetFromPool();
            if (text != null)
            {
                text.transform.position = position;
                text.ShowCoin(amount);
            }
        }

        /// <summary>
        /// Spawn gem pickup text.
        /// </summary>
        public static void SpawnGem(Vector3 position, int amount)
        {
            if (Instance == null) return;
            
            FloatingText text = Instance.GetFromPool();
            if (text != null)
            {
                text.transform.position = position;
                text.ShowGem(amount);
            }
        }

        /// <summary>
        /// Spawn heal text.
        /// </summary>
        public static void SpawnHeal(Vector3 position, int amount)
        {
            if (Instance == null) return;
            
            FloatingText text = Instance.GetFromPool();
            if (text != null)
            {
                text.transform.position = position;
                text.ShowHeal(amount);
            }
        }

        /// <summary>
        /// Spawn custom text.
        /// </summary>
        public static void SpawnText(Vector3 position, string message, Color color)
        {
            if (Instance == null) return;
            
            FloatingText text = Instance.GetFromPool();
            if (text != null)
            {
                text.transform.position = position;
                text.Show(message, color);
            }
        }
    }
}
