using UnityEngine;

namespace Rumbax.Testing
{
    /// <summary>
    /// Master bootstrap that sets up the complete game with all systems.
    /// Attach this to an empty GameObject in your main scene.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool _startWithMainMenu = true;
        [SerializeField] private bool _enableAudio = true;
        [SerializeField] private bool _enableVibration = true;

        private void Awake()
        {
            // Set up application settings
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // Initialize audio system
            if (_enableAudio && SimpleAudioManager.Instance == null)
            {
                GameObject audioManager = new GameObject("AudioManager");
                audioManager.AddComponent<SimpleAudioManager>();
            }

            UnityEngine.Debug.Log("[GameBootstrap] Game initialized successfully");
        }

        private void Start()
        {
            if (_startWithMainMenu)
            {
                InitializeMainMenu();
            }
            else
            {
                InitializeGameplay();
            }
        }

        private void InitializeMainMenu()
        {
            // Create main menu
            GameObject menuObj = new GameObject("MainMenuTest");
            menuObj.AddComponent<MainMenuTest>();
        }

        private void InitializeGameplay()
        {
            // Create gameplay
            GameObject gameObj = new GameObject("GameplayTest");
            gameObj.AddComponent<GameplayTest>();
        }

        public static void TriggerVibration(float duration = 0.1f)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            if (SystemInfo.supportsVibration)
            {
                Handheld.Vibrate();
            }
            #endif
        }
    }
}
