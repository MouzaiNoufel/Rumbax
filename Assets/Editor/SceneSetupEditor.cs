#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Rumbax.Editor
{
    /// <summary>
    /// Quick scene setup utilities for Rumbax.
    /// Use menu: Rumbax → Setup Scenes
    /// </summary>
    public class SceneSetupEditor : EditorWindow
    {
        [MenuItem("Rumbax/Setup Scenes/Setup Main Menu Scene", false, 200)]
        public static void SetupMainMenuScene()
        {
            // Open or create scene
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenuScene.unity", OpenSceneMode.Single);
            
            // Clear existing objects
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name != "Main Camera" && go.name != "Directional Light")
                {
                    DestroyImmediate(go);
                }
            }

            // Setup camera
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCamera = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                camObj.AddComponent<AudioListener>();
            }
            mainCamera.transform.position = new Vector3(0, 0, -10);
            mainCamera.backgroundColor = new Color(0.05f, 0.08f, 0.12f);
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 5;

            // Add game bootstrap
            GameObject bootstrap = new GameObject("GameBootstrap");
            var bootstrapScript = bootstrap.AddComponent<Rumbax.Testing.GameBootstrap>();
            
            // Mark scene dirty
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[SceneSetup] Main Menu Scene configured successfully!");
            EditorUtility.DisplayDialog("Scene Setup", "Main Menu Scene configured!\n\nGameBootstrap added with Main Menu mode.", "OK");
        }

        [MenuItem("Rumbax/Setup Scenes/Setup Game Scene", false, 201)]
        public static void SetupGameScene()
        {
            // Open or create scene
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity", OpenSceneMode.Single);
            
            // Clear existing objects
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name != "Main Camera" && go.name != "Directional Light")
                {
                    DestroyImmediate(go);
                }
            }

            // Setup camera
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCamera = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                camObj.AddComponent<AudioListener>();
            }
            mainCamera.transform.position = new Vector3(0, 0, -10);
            mainCamera.backgroundColor = new Color(0.05f, 0.08f, 0.12f);
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 5;

            // Add gameplay test
            GameObject gameplayObj = new GameObject("GameplayTest");
            gameplayObj.AddComponent<Rumbax.Testing.GameplayTest>();

            // Mark scene dirty
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[SceneSetup] Game Scene configured successfully!");
            EditorUtility.DisplayDialog("Scene Setup", "Game Scene configured!\n\nGameplayTest added for gameplay testing.", "OK");
        }

        [MenuItem("Rumbax/Setup Scenes/Configure Build Settings", false, 210)]
        public static void ConfigureBuildSettings()
        {
            var scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenuScene.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/SplashScene.unity", true)
            };

            EditorBuildSettings.scenes = scenes;

            Debug.Log("[SceneSetup] Build settings configured with all scenes!");
            EditorUtility.DisplayDialog("Build Settings", 
                "Build settings configured!\n\n" +
                "Scenes in build:\n" +
                "0. MainMenuScene\n" +
                "1. GameScene\n" +
                "2. SplashScene", "OK");
        }

        [MenuItem("Rumbax/Quick Play/Play Main Menu", false, 50)]
        public static void PlayMainMenu()
        {
            if (EditorApplication.isPlaying) return;
            
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenuScene.unity");
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Rumbax/Quick Play/Play Game Scene", false, 51)]
        public static void PlayGameScene()
        {
            if (EditorApplication.isPlaying) return;
            
            EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");
            EditorApplication.isPlaying = true;
        }
    }
}
#endif
