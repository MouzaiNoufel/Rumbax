using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Rumbax.Systems
{
    /// <summary>
    /// Tutorial System - Guided tutorial for new players.
    /// Professional mobile game feature with step-by-step instructions.
    /// </summary>
    [Serializable]
    public class TutorialStep
    {
        public string id;
        public string title;
        public string message;
        public TutorialHighlightType highlightType;
        public string targetName;
        public Vector2 targetPosition;
        public bool requiresAction;
        public TutorialAction requiredAction;
        public float autoAdvanceDelay;
        public bool showArrow;
        public ArrowDirection arrowDirection;
    }

    public enum TutorialHighlightType
    {
        None,
        Button,
        Area,
        ScreenCenter,
        Custom
    }

    public enum TutorialAction
    {
        None,
        TapButton,
        TapGrid,
        PlaceDefender,
        Merge,
        StartWave,
        CollectCoin,
        KillEnemy,
        Wait
    }

    public enum ArrowDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public class TutorialSystem : MonoBehaviour
    {
        public static TutorialSystem Instance { get; private set; }

        private List<TutorialStep> _tutorialSteps = new List<TutorialStep>();
        private int _currentStepIndex = -1;
        private bool _tutorialActive = false;
        private bool _tutorialCompleted = false;
        private bool _waitingForAction = false;

        // UI Elements
        private Canvas _tutorialCanvas;
        private GameObject _overlayPanel;
        private GameObject _messagePanel;
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _messageText;
        private Button _nextButton;
        private Button _skipButton;
        private GameObject _highlightFrame;
        private GameObject _arrowIndicator;
        private GameObject _maskOverlay;

        // Events
        public event Action OnTutorialStarted;
        public event Action OnTutorialCompleted;
        public event Action OnTutorialSkipped;
        public event Action<TutorialStep> OnStepChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeTutorial();
                LoadProgress();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeTutorial()
        {
            _tutorialSteps.Clear();

            // Welcome
            AddStep("welcome", "Welcome to Rumbax!", 
                "Welcome to the ultimate merge defense game! Let's learn how to play.",
                TutorialHighlightType.ScreenCenter, "", TutorialAction.None, 0f, false);

            // Explain grid
            AddStep("grid_intro", "The Battle Grid",
                "This is your defense grid. You'll place defenders here to protect your base.",
                TutorialHighlightType.Area, "GridContainer", TutorialAction.None, 0f, false);

            // Spawn button
            AddStep("spawn_button", "Summon Defenders",
                "Tap the SUMMON button to prepare a defender for placement.",
                TutorialHighlightType.Button, "SpawnButton", TutorialAction.TapButton, 0f, true, ArrowDirection.Down);

            // Place defender
            AddStep("place_defender", "Place Your Defender",
                "Now tap on any empty cell to place your defender!",
                TutorialHighlightType.Area, "GridContainer", TutorialAction.PlaceDefender, 0f, true, ArrowDirection.Up);

            // Explain defender
            AddStep("defender_intro", "Your Defender",
                "Great! This is a Tier 1 defender. It will automatically attack enemies that come near.",
                TutorialHighlightType.None, "", TutorialAction.None, 2f, false);

            // Spawn another
            AddStep("spawn_second", "Summon Another",
                "Summon one more defender. Having multiple defenders is key to survival!",
                TutorialHighlightType.Button, "SpawnButton", TutorialAction.PlaceDefender, 0f, true, ArrowDirection.Down);

            // Explain merging
            AddStep("merge_intro", "The Power of Merging",
                "When you have two defenders of the same tier, you can merge them to create a stronger one!",
                TutorialHighlightType.ScreenCenter, "", TutorialAction.None, 0f, false);

            // How to merge
            AddStep("merge_how", "How to Merge",
                "Tap one defender, then tap another of the same tier to merge them together.",
                TutorialHighlightType.Area, "GridContainer", TutorialAction.Merge, 0f, true, ArrowDirection.Up);

            // Start wave
            AddStep("start_wave", "Start the Battle!",
                "Ready? Tap START WAVE to begin the enemy assault!",
                TutorialHighlightType.Button, "StartWaveButton", TutorialAction.StartWave, 0f, true, ArrowDirection.Down);

            // Enemy path
            AddStep("enemy_path", "Enemy Path",
                "Enemies will march along the red path. Don't let them reach the end!",
                TutorialHighlightType.Area, "PathLane", TutorialAction.None, 3f, true, ArrowDirection.Down);

            // Kill enemies
            AddStep("combat", "Defend!",
                "Your defenders will automatically attack. Watch them destroy the enemies!",
                TutorialHighlightType.None, "", TutorialAction.KillEnemy, 0f, false);

            // Coins
            AddStep("coins_intro", "Earn Coins",
                "Killing enemies earns you coins! Use them to summon more defenders.",
                TutorialHighlightType.Custom, "", TutorialAction.None, 2f, true, ArrowDirection.Up);

            // Combo system
            AddStep("combo_intro", "Combo System",
                "Kill enemies quickly to build combos! Higher combos mean more rewards.",
                TutorialHighlightType.ScreenCenter, "", TutorialAction.None, 0f, false);

            // Power-ups
            AddStep("powerup_intro", "Power-Ups",
                "Sometimes power-ups will appear. Tap them to collect powerful bonuses!",
                TutorialHighlightType.ScreenCenter, "", TutorialAction.None, 0f, false);

            // Fever mode
            AddStep("fever_intro", "Fever Mode",
                "Fill the fever bar by getting combos. Fever mode doubles your rewards!",
                TutorialHighlightType.Custom, "FeverBar", TutorialAction.None, 0f, true, ArrowDirection.Up);

            // Ultimate
            AddStep("ultimate_intro", "Ultimate Ability",
                "Charge your ultimate by killing enemies. Use it to clear the screen!",
                TutorialHighlightType.Button, "UltimateButton", TutorialAction.None, 0f, true, ArrowDirection.Down);

            // Complete
            AddStep("complete", "Tutorial Complete!",
                "You're ready to defend! Remember: Summon, Merge, Survive!\n\nGood luck, commander! 🎮",
                TutorialHighlightType.ScreenCenter, "", TutorialAction.None, 0f, false);
        }

        private void AddStep(string id, string title, string message, 
            TutorialHighlightType highlight, string target, TutorialAction action,
            float autoAdvance, bool showArrow, ArrowDirection direction = ArrowDirection.Down)
        {
            _tutorialSteps.Add(new TutorialStep
            {
                id = id,
                title = title,
                message = message,
                highlightType = highlight,
                targetName = target,
                requiresAction = action != TutorialAction.None,
                requiredAction = action,
                autoAdvanceDelay = autoAdvance,
                showArrow = showArrow,
                arrowDirection = direction
            });
        }

        public bool IsTutorialCompleted() => _tutorialCompleted;
        public bool IsTutorialActive() => _tutorialActive;
        public TutorialStep GetCurrentStep() => _currentStepIndex >= 0 && _currentStepIndex < _tutorialSteps.Count 
            ? _tutorialSteps[_currentStepIndex] : null;

        public void StartTutorial()
        {
            if (_tutorialCompleted || _tutorialActive) return;

            _tutorialActive = true;
            _currentStepIndex = -1;
            CreateTutorialUI();
            NextStep();
            OnTutorialStarted?.Invoke();
        }

        public void SkipTutorial()
        {
            _tutorialActive = false;
            _tutorialCompleted = true;
            HideTutorialUI();
            SaveProgress();
            OnTutorialSkipped?.Invoke();
        }

        private void CreateTutorialUI()
        {
            // Find existing canvas or create one
            Canvas existingCanvas = FindObjectOfType<Canvas>();
            
            GameObject tutorialUIObj = new GameObject("TutorialUI");
            _tutorialCanvas = tutorialUIObj.AddComponent<Canvas>();
            _tutorialCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _tutorialCanvas.sortingOrder = 1000;
            
            CanvasScaler scaler = tutorialUIObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            
            tutorialUIObj.AddComponent<GraphicRaycaster>();

            // Dark overlay
            _overlayPanel = CreatePanel("Overlay", _tutorialCanvas.transform, Vector2.zero, Vector2.one, 
                new Color(0f, 0f, 0f, 0.7f));

            // Message panel
            _messagePanel = CreatePanel("MessagePanel", _tutorialCanvas.transform, 
                new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), 
                new Color(0.1f, 0.12f, 0.18f, 0.98f));
            RectTransform msgRect = _messagePanel.GetComponent<RectTransform>();
            msgRect.sizeDelta = new Vector2(900, 300);

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(_messagePanel.transform, false);
            _titleText = titleObj.AddComponent<TextMeshProUGUI>();
            _titleText.fontSize = 36;
            _titleText.fontStyle = FontStyles.Bold;
            _titleText.color = new Color(1f, 0.9f, 0.3f);
            _titleText.alignment = TextAlignmentOptions.Center;
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.7f);
            titleRect.anchorMax = new Vector2(1, 1f);
            titleRect.offsetMin = new Vector2(20, 0);
            titleRect.offsetMax = new Vector2(-20, -20);

            // Message
            GameObject msgObj = new GameObject("Message");
            msgObj.transform.SetParent(_messagePanel.transform, false);
            _messageText = msgObj.AddComponent<TextMeshProUGUI>();
            _messageText.fontSize = 26;
            _messageText.color = Color.white;
            _messageText.alignment = TextAlignmentOptions.Center;
            RectTransform textRect = msgObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0.25f);
            textRect.anchorMax = new Vector2(1, 0.7f);
            textRect.offsetMin = new Vector2(30, 0);
            textRect.offsetMax = new Vector2(-30, 0);

            // Next button
            _nextButton = CreateButton("Next", "NEXT →", _messagePanel.transform,
                new Vector2(0.75f, 0.1f), new Color(0.2f, 0.6f, 0.3f));
            _nextButton.onClick.AddListener(NextStep);

            // Skip button
            _skipButton = CreateButton("Skip", "SKIP", _messagePanel.transform,
                new Vector2(0.25f, 0.1f), new Color(0.4f, 0.4f, 0.4f));
            _skipButton.onClick.AddListener(SkipTutorial);

            // Highlight frame
            _highlightFrame = new GameObject("HighlightFrame");
            _highlightFrame.transform.SetParent(_tutorialCanvas.transform, false);
            Image highlightImg = _highlightFrame.AddComponent<Image>();
            highlightImg.color = new Color(1f, 0.9f, 0.3f, 0.3f);
            highlightImg.raycastTarget = false;
            RectTransform highlightRect = _highlightFrame.GetComponent<RectTransform>();
            highlightRect.sizeDelta = new Vector2(200, 200);
            _highlightFrame.SetActive(false);

            // Arrow indicator
            _arrowIndicator = new GameObject("Arrow");
            _arrowIndicator.transform.SetParent(_tutorialCanvas.transform, false);
            TextMeshProUGUI arrowText = _arrowIndicator.AddComponent<TextMeshProUGUI>();
            arrowText.text = "👆";
            arrowText.fontSize = 60;
            arrowText.alignment = TextAlignmentOptions.Center;
            RectTransform arrowRect = _arrowIndicator.GetComponent<RectTransform>();
            arrowRect.sizeDelta = new Vector2(100, 100);
            _arrowIndicator.SetActive(false);
        }

        private GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            
            Image img = panel.AddComponent<Image>();
            img.color = color;
            
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            return panel;
        }

        private Button CreateButton(string name, string text, Transform parent, Vector2 anchorPos, Color color)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            
            Image img = btnObj.AddComponent<Image>();
            img.color = color;
            
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;
            
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.anchorMin = anchorPos - new Vector2(0.15f, 0.05f);
            rect.anchorMax = anchorPos + new Vector2(0.15f, 0.05f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 24;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return btn;
        }

        public void NextStep()
        {
            _currentStepIndex++;
            
            if (_currentStepIndex >= _tutorialSteps.Count)
            {
                CompleteTutorial();
                return;
            }

            ShowStep(_tutorialSteps[_currentStepIndex]);
        }

        private void ShowStep(TutorialStep step)
        {
            _titleText.text = step.title;
            _messageText.text = step.message;

            // Handle highlight
            _highlightFrame.SetActive(step.highlightType != TutorialHighlightType.None);
            
            // Handle arrow
            if (step.showArrow)
            {
                _arrowIndicator.SetActive(true);
                TextMeshProUGUI arrowText = _arrowIndicator.GetComponent<TextMeshProUGUI>();
                arrowText.text = step.arrowDirection switch
                {
                    ArrowDirection.Up => "👆",
                    ArrowDirection.Down => "👇",
                    ArrowDirection.Left => "👈",
                    ArrowDirection.Right => "👉",
                    _ => "👆"
                };
                StartCoroutine(AnimateArrow());
            }
            else
            {
                _arrowIndicator.SetActive(false);
            }

            // Handle required action
            _waitingForAction = step.requiresAction;
            _nextButton.interactable = !step.requiresAction;

            // Auto advance
            if (step.autoAdvanceDelay > 0)
            {
                StartCoroutine(AutoAdvance(step.autoAdvanceDelay));
            }

            OnStepChanged?.Invoke(step);
        }

        private IEnumerator AnimateArrow()
        {
            RectTransform arrowRect = _arrowIndicator.GetComponent<RectTransform>();
            Vector2 basePos = arrowRect.anchoredPosition;

            while (_arrowIndicator.activeSelf)
            {
                float offset = Mathf.Sin(Time.time * 5f) * 20f;
                arrowRect.anchoredPosition = basePos + Vector2.up * offset;
                yield return null;
            }
        }

        private IEnumerator AutoAdvance(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_tutorialActive && !_waitingForAction)
            {
                NextStep();
            }
        }

        public void OnActionCompleted(TutorialAction action)
        {
            if (!_tutorialActive || !_waitingForAction) return;

            TutorialStep currentStep = GetCurrentStep();
            if (currentStep != null && currentStep.requiredAction == action)
            {
                _waitingForAction = false;
                _nextButton.interactable = true;
                
                // Small delay then auto-advance
                StartCoroutine(AutoAdvanceAfterAction());
            }
        }

        private IEnumerator AutoAdvanceAfterAction()
        {
            yield return new WaitForSeconds(0.5f);
            NextStep();
        }

        private void CompleteTutorial()
        {
            _tutorialActive = false;
            _tutorialCompleted = true;
            HideTutorialUI();
            SaveProgress();
            OnTutorialCompleted?.Invoke();
        }

        private void HideTutorialUI()
        {
            if (_tutorialCanvas != null)
            {
                Destroy(_tutorialCanvas.gameObject);
            }
        }

        private void SaveProgress()
        {
            PlayerPrefs.SetInt("TutorialCompleted", _tutorialCompleted ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            _tutorialCompleted = PlayerPrefs.GetInt("TutorialCompleted", 0) == 1;
        }

        public void ResetTutorial()
        {
            _tutorialCompleted = false;
            _currentStepIndex = -1;
            SaveProgress();
        }

        // Helper to check if should show tutorial
        public bool ShouldShowTutorial()
        {
            if (_tutorialCompleted) return false;
            if (SettingsSystem.Instance != null && !SettingsSystem.Instance.ShowTutorialHints) return false;
            return true;
        }
    }
}
