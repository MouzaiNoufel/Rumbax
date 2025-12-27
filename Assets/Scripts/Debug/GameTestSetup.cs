using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Rumbax.Testing
{
    /// <summary>
    /// Temporary test script to visualize the game without art assets.
    /// Attach this to a GameObject in the scene to auto-generate test visuals.
    /// </summary>
    public class GameTestSetup : MonoBehaviour
    {
        [Header("Auto Setup")]
        [SerializeField] private bool autoSetup = true;
        
        private Canvas _canvas;
        private Camera _camera;

        private void Start()
        {
            if (autoSetup)
            {
                SetupTestScene();
            }
        }

        private void SetupTestScene()
        {
            Debug.Log("[TestSetup] Setting up test scene...");
            
            // Setup camera
            SetupCamera();
            
            // Setup UI
            SetupUI();
            
            // Create test grid visual
            CreateTestGrid();
            
            Debug.Log("[TestSetup] Test scene ready!");
        }

        private void SetupCamera()
        {
            _camera = Camera.main;
            if (_camera == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                _camera = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }
            
            _camera.backgroundColor = new Color(0.1f, 0.15f, 0.2f); // Dark blue-gray
            _camera.orthographic = true;
            _camera.orthographicSize = 5;
        }

        private void SetupUI()
        {
            // Find or create canvas
            _canvas = FindObjectOfType<Canvas>();
            if (_canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                _canvas = canvasObj.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Create title text
            CreateText("Rumbax - Merge Defense", new Vector2(0, 200), 48, Color.white);
            
            // Create instruction text
            CreateText("Game is running!\nCode compiled successfully.", new Vector2(0, 100), 24, Color.green);
            
            // Create info panel
            CreateText("This is a test scene.\nArt assets need to be added in Unity Editor.", new Vector2(0, 0), 18, Color.yellow);
            
            // Create spawn button
            CreateButton("Spawn Test Defender", new Vector2(0, -100), OnSpawnClicked);
            
            // Create stats text
            CreateText("Coins: 100 | Gems: 10 | Level: 1", new Vector2(0, -200), 20, Color.cyan);
        }

        private void CreateText(string text, Vector2 position, int fontSize, Color color)
        {
            GameObject textObj = new GameObject("Text_" + text.Substring(0, Mathf.Min(10, text.Length)));
            textObj.transform.SetParent(_canvas.transform, false);
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            
            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(800, 100);
        }

        private void CreateButton(string text, Vector2 position, UnityEngine.Events.UnityAction onClick)
        {
            GameObject buttonObj = new GameObject("Button_" + text);
            buttonObj.transform.SetParent(_canvas.transform, false);
            
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.6f, 0.2f);
            
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);
            
            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(300, 60);
            
            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 24;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }

        private void CreateTestGrid()
        {
            // Create a simple visual grid
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    CreateGridCell(x, y);
                }
            }
        }

        private void CreateGridCell(int x, int y)
        {
            GameObject cell = new GameObject($"Cell_{x}_{y}");
            cell.transform.position = new Vector3(x - 2, y - 3.5f, 0);
            
            SpriteRenderer sr = cell.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite();
            sr.color = new Color(0.3f, 0.3f, 0.4f, 0.8f);
            cell.transform.localScale = Vector3.one * 0.9f;
        }

        private Sprite CreateSquareSprite()
        {
            Texture2D tex = new Texture2D(64, 64);
            Color[] colors = new Color[64 * 64];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.white;
            }
            tex.SetPixels(colors);
            tex.Apply();
            
            return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
        }

        private int _defenderCount = 0;
        
        private void OnSpawnClicked()
        {
            Debug.Log("[TestSetup] Spawning test defender!");
            
            // Create a simple defender visual
            GameObject defender = new GameObject($"TestDefender_{_defenderCount++}");
            defender.transform.position = new Vector3(Random.Range(-2f, 2f), Random.Range(-3.5f, -1.5f), 0);
            
            SpriteRenderer sr = defender.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite();
            sr.color = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f));
            sr.sortingOrder = 10;
            defender.transform.localScale = Vector3.one * 0.7f;
            
            // Add some animation
            defender.AddComponent<TestDefenderAnimation>();
        }
    }

    public class TestDefenderAnimation : MonoBehaviour
    {
        private float _time;
        
        private void Update()
        {
            _time += Time.deltaTime;
            transform.localScale = Vector3.one * (0.7f + Mathf.Sin(_time * 3f) * 0.05f);
        }
    }
}
