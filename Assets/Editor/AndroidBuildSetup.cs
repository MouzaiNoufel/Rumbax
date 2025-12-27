#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using System.IO;

namespace Rumbax.Editor
{
    /// <summary>
    /// Editor utility for configuring Android build settings for Rumbax.
    /// Use menu: Rumbax → Configure Android Build
    /// </summary>
    public class AndroidBuildSetup : EditorWindow
    {
        private string _packageName = "com.rumbax.mergedefense";
        private string _productName = "Rumbax - Merge Defense";
        private string _companyName = "Rumbax Games";
        private int _versionCode = 1;
        private string _versionName = "1.0.0";
        private int _minApiLevel = 24; // Android 7.0
        private int _targetApiLevel = 34; // Android 14
        private bool _useIL2CPP = true;
        private bool _splitBinary = false;

        [MenuItem("Rumbax/Configure Android Build", false, 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<AndroidBuildSetup>("Android Build Setup");
            window.minSize = new Vector2(400, 500);
            window.LoadCurrentSettings();
        }

        [MenuItem("Rumbax/Build Android APK", false, 101)]
        public static void BuildAPK()
        {
            string path = EditorUtility.SaveFilePanel(
                "Save APK",
                "",
                "Rumbax.apk",
                "apk");

            if (string.IsNullOrEmpty(path)) return;

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = GetEnabledScenes(),
                locationPathName = path,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                EditorUtility.DisplayDialog("Build Complete", 
                    $"APK built successfully!\nLocation: {path}", "OK");
                EditorUtility.RevealInFinder(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Build Failed", 
                    $"Build failed with {report.summary.totalErrors} errors.", "OK");
            }
        }

        [MenuItem("Rumbax/Build Android AAB (Play Store)", false, 102)]
        public static void BuildAAB()
        {
            // Enable AAB format
            EditorUserBuildSettings.buildAppBundle = true;

            string path = EditorUtility.SaveFilePanel(
                "Save AAB",
                "",
                "Rumbax.aab",
                "aab");

            if (string.IsNullOrEmpty(path)) return;

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = GetEnabledScenes(),
                locationPathName = path,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);

            // Reset to APK for development
            EditorUserBuildSettings.buildAppBundle = false;

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                EditorUtility.DisplayDialog("Build Complete", 
                    $"AAB built successfully!\nLocation: {path}", "OK");
                EditorUtility.RevealInFinder(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Build Failed", 
                    $"Build failed with {report.summary.totalErrors} errors.", "OK");
            }
        }

        private static string[] GetEnabledScenes()
        {
            var scenes = EditorBuildSettings.scenes;
            var enabledScenes = new System.Collections.Generic.List<string>();

            foreach (var scene in scenes)
            {
                if (scene.enabled)
                {
                    enabledScenes.Add(scene.path);
                }
            }

            // If no scenes are configured, add current scene
            if (enabledScenes.Count == 0)
            {
                string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
                if (!string.IsNullOrEmpty(currentScene))
                {
                    enabledScenes.Add(currentScene);
                }
            }

            return enabledScenes.ToArray();
        }

        private void LoadCurrentSettings()
        {
            _packageName = PlayerSettings.applicationIdentifier;
            _productName = PlayerSettings.productName;
            _companyName = PlayerSettings.companyName;
            _versionName = PlayerSettings.bundleVersion;
            _versionCode = PlayerSettings.Android.bundleVersionCode;
            _minApiLevel = (int)PlayerSettings.Android.minSdkVersion;
            _targetApiLevel = (int)PlayerSettings.Android.targetSdkVersion;
            _useIL2CPP = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP;
            _splitBinary = PlayerSettings.Android.splitApplicationBinary;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("🎮 Rumbax Android Build Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            // App Info Section
            EditorGUILayout.LabelField("App Information", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            _productName = EditorGUILayout.TextField("Product Name", _productName);
            _companyName = EditorGUILayout.TextField("Company Name", _companyName);
            _packageName = EditorGUILayout.TextField("Package Name", _packageName);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(10);

            // Version Section
            EditorGUILayout.LabelField("Version", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            _versionName = EditorGUILayout.TextField("Version Name", _versionName);
            _versionCode = EditorGUILayout.IntField("Version Code", _versionCode);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(10);

            // API Levels Section
            EditorGUILayout.LabelField("API Levels", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            _minApiLevel = EditorGUILayout.IntSlider("Min API Level", _minApiLevel, 21, 34);
            EditorGUILayout.LabelField($"  (Android {GetAndroidVersionName(_minApiLevel)})", EditorStyles.miniLabel);
            _targetApiLevel = EditorGUILayout.IntSlider("Target API Level", _targetApiLevel, _minApiLevel, 34);
            EditorGUILayout.LabelField($"  (Android {GetAndroidVersionName(_targetApiLevel)})", EditorStyles.miniLabel);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(10);

            // Build Settings Section
            EditorGUILayout.LabelField("Build Settings", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            _useIL2CPP = EditorGUILayout.Toggle("Use IL2CPP (Required for Store)", _useIL2CPP);
            _splitBinary = EditorGUILayout.Toggle("Split APK", _splitBinary);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(20);

            // Apply Button
            if (GUILayout.Button("Apply Settings", GUILayout.Height(40)))
            {
                ApplySettings();
            }

            EditorGUILayout.Space(10);

            // Build Buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Build APK (Testing)", GUILayout.Height(35)))
            {
                BuildAPK();
            }
            if (GUILayout.Button("Build AAB (Play Store)", GUILayout.Height(35)))
            {
                BuildAAB();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Quick Setup Buttons
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create App Icons"))
            {
                CreatePlaceholderIcons();
            }
            if (GUILayout.Button("Setup Keystore"))
            {
                EditorApplication.ExecuteMenuItem("Edit/Project Settings...");
            }
            EditorGUILayout.EndHorizontal();

            // Status
            EditorGUILayout.Space(15);
            EditorGUILayout.HelpBox(
                "📱 Current Target: Android\n" +
                $"🔧 Scripting Backend: {(PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP ? "IL2CPP ✓" : "Mono (Not recommended)")}\n" +
                $"📦 Package: {PlayerSettings.applicationIdentifier}",
                MessageType.Info);
        }

        private void ApplySettings()
        {
            // Switch to Android if needed
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }

            // Apply settings
            PlayerSettings.productName = _productName;
            PlayerSettings.companyName = _companyName;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, _packageName);
            PlayerSettings.bundleVersion = _versionName;
            PlayerSettings.Android.bundleVersionCode = _versionCode;
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)_minApiLevel;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)_targetApiLevel;

            // Scripting backend
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, 
                _useIL2CPP ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);

            // Architecture (ARM64 required for Play Store)
            if (_useIL2CPP)
            {
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            }

            // Split APK
            PlayerSettings.Android.splitApplicationBinary = _splitBinary;

            // Other recommended settings
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            // Graphics
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { 
                UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3,
                UnityEngine.Rendering.GraphicsDeviceType.Vulkan 
            });

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Settings Applied", 
                "Android build settings have been configured successfully!", "OK");

            UnityEngine.Debug.Log("[AndroidBuildSetup] All settings applied successfully");
        }

        private void CreatePlaceholderIcons()
        {
            string iconsPath = "Assets/Icons";
            if (!AssetDatabase.IsValidFolder(iconsPath))
            {
                AssetDatabase.CreateFolder("Assets", "Icons");
            }

            // Create icon sizes
            int[] sizes = { 48, 72, 96, 144, 192, 512 };
            
            foreach (int size in sizes)
            {
                Texture2D icon = CreateIconTexture(size);
                byte[] pngData = icon.EncodeToPNG();
                string path = $"{iconsPath}/icon_{size}x{size}.png";
                File.WriteAllBytes(path, pngData);
                DestroyImmediate(icon);
            }

            AssetDatabase.Refresh();

            // Set the 192x192 as the default icon
            string mainIconPath = $"{iconsPath}/icon_192x192.png";
            var importer = AssetImporter.GetAtPath(mainIconPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }

            EditorUtility.DisplayDialog("Icons Created", 
                $"Placeholder icons created in {iconsPath}\n" +
                "Don't forget to replace them with real artwork!", "OK");
        }

        private Texture2D CreateIconTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color primaryColor = new Color(0.2f, 0.6f, 0.35f); // Green
            Color accentColor = new Color(0.95f, 0.85f, 0.25f); // Gold

            // Create rounded rectangle with gradient
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float cornerRadius = size * 0.18f;
                    float dist = GetRoundedRectDistance(x, y, size, size, cornerRadius);

                    if (dist <= 0)
                    {
                        // Inside the rounded rect
                        float gradientY = (float)y / size;
                        Color bgColor = Color.Lerp(
                            new Color(0.15f, 0.5f, 0.3f),
                            new Color(0.08f, 0.3f, 0.15f),
                            gradientY);

                        // Add R logo in center
                        float centerX = size / 2f;
                        float centerY = size / 2f;
                        float letterSize = size * 0.5f;

                        float dx = Mathf.Abs(x - centerX);
                        float dy = Mathf.Abs(y - centerY);

                        // Simple R shape check
                        bool isLetter = false;
                        float normalX = (x - centerX + letterSize/2) / letterSize;
                        float normalY = (y - centerY + letterSize/2) / letterSize;

                        if (normalX >= 0 && normalX <= 1 && normalY >= 0 && normalY <= 1)
                        {
                            // Left vertical bar
                            if (normalX < 0.35f) isLetter = true;
                            // Top horizontal
                            else if (normalY < 0.25f && normalX < 0.85f) isLetter = true;
                            // Middle horizontal
                            else if (normalY > 0.4f && normalY < 0.55f && normalX < 0.85f) isLetter = true;
                            // Top loop right
                            else if (normalY < 0.5f && normalX > 0.65f && normalX < 0.85f) isLetter = true;
                            // Diagonal leg
                            else if (normalY > 0.5f && normalX > 0.35f)
                            {
                                float expectedX = 0.35f + (normalY - 0.5f) * 1.0f;
                                if (Mathf.Abs(normalX - expectedX) < 0.2f) isLetter = true;
                            }
                        }

                        if (isLetter)
                        {
                            tex.SetPixel(x, y, accentColor);
                        }
                        else
                        {
                            tex.SetPixel(x, y, bgColor);
                        }
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        private float GetRoundedRectDistance(float x, float y, float width, float height, float radius)
        {
            float halfWidth = width / 2f;
            float halfHeight = height / 2f;
            float centerX = halfWidth;
            float centerY = halfHeight;

            float dx = Mathf.Abs(x - centerX) - (halfWidth - radius);
            float dy = Mathf.Abs(y - centerY) - (halfHeight - radius);

            if (dx <= 0 && dy <= 0)
                return Mathf.Max(dx, dy);

            float cornerDist = Mathf.Sqrt(Mathf.Max(0, dx) * Mathf.Max(0, dx) + 
                                          Mathf.Max(0, dy) * Mathf.Max(0, dy));
            return cornerDist - radius;
        }

        private string GetAndroidVersionName(int apiLevel)
        {
            return apiLevel switch
            {
                21 => "5.0 Lollipop",
                22 => "5.1 Lollipop",
                23 => "6.0 Marshmallow",
                24 => "7.0 Nougat",
                25 => "7.1 Nougat",
                26 => "8.0 Oreo",
                27 => "8.1 Oreo",
                28 => "9.0 Pie",
                29 => "10",
                30 => "11",
                31 => "12",
                32 => "12L",
                33 => "13",
                34 => "14",
                _ => $"API {apiLevel}"
            };
        }
    }
}
#endif
