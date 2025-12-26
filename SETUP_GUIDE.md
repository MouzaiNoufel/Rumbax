# Rumbax Unity Project Setup Guide

This document provides step-by-step instructions to set up the Rumbax Unity project from scratch.

## Step 1: Create Unity Project

1. Open Unity Hub
2. Click "New Project"
3. Select "2D" template
4. Name: "Rumbax"
5. Location: Choose your project folder
6. Click "Create Project"

## Step 2: Import TextMeshPro

1. Window > Package Manager
2. Find "TextMeshPro" in Unity Registry
3. Click "Install"
4. When prompted, click "Import TMP Essentials"

## Step 3: Project Settings

### Quality Settings
1. Edit > Project Settings > Quality
2. Add/modify quality levels:
   - Low: Mobile-optimized settings
   - Medium: Default for most devices
   - High: For flagship devices

### Player Settings
1. Edit > Project Settings > Player
2. Configure:
   - Company Name
   - Product Name: Rumbax
   - Default Icon (add your icon)
   - Splash Image (customize or disable)

### Android Settings
1. In Player Settings, select Android tab
2. Other Settings:
   - Package Name: com.yourcompany.rumbax
   - Minimum API Level: 24
   - Target API Level: 34
   - Scripting Backend: IL2CPP
   - API Compatibility Level: .NET Standard 2.1
   - Target Architectures: ARMv7, ARM64

### Graphics Settings
1. Edit > Project Settings > Graphics
2. For mobile optimization:
   - Use URP (Universal Render Pipeline) for better performance
   - Or use Built-in Render Pipeline for simplicity

## Step 4: Create Folder Structure

Create the following folders in Assets:

```
Assets/
├── Art/
│   ├── Sprites/
│   │   ├── Defenders/
│   │   ├── Enemies/
│   │   ├── Projectiles/
│   │   ├── UI/
│   │   └── Environment/
│   └── Animations/
├── Audio/
│   ├── Music/
│   └── SFX/
├── Prefabs/
│   ├── Defenders/
│   ├── Enemies/
│   ├── Projectiles/
│   ├── VFX/
│   └── UI/
├── Scenes/
├── ScriptableObjects/
│   ├── Configs/
│   └── Audio/
└── Scripts/
    (Already created by code generation)
```

## Step 5: Create Scenes

### 1. SplashScene
- Create: File > New Scene
- Save as: Assets/Scenes/SplashScene.unity
- Add to Build Settings (index 0)

### 2. MainMenuScene
- Create: File > New Scene
- Save as: Assets/Scenes/MainMenuScene.unity
- Add to Build Settings (index 1)

### 3. GameScene
- Create: File > New Scene
- Save as: Assets/Scenes/GameScene.unity
- Add to Build Settings (index 2)

## Step 6: Scene Setup

### MainMenuScene Setup

1. Create Canvas:
   - Right-click Hierarchy > UI > Canvas
   - Set Canvas Scaler to "Scale With Screen Size"
   - Reference Resolution: 1080x1920

2. Create UI elements:
   - Background image
   - Title text (TextMeshPro)
   - Play button
   - Shop button
   - Settings button
   - Daily Challenges button
   - Achievements button

3. Add MainMenuUI component to Canvas

### GameScene Setup

1. Create Game Manager:
   - Create empty GameObject: "GameManager"
   - Add GameManager component
   - Add AudioService component
   - Add VFXManager component

2. Create Grid:
   - Create empty GameObject: "GridManager"
   - Add GridManager component
   - Create 5x5 grid of cell GameObjects

3. Create Wave System:
   - Create empty GameObject: "WaveManager"
   - Add WaveManager component
   - Add EnemyManager component

4. Create UI Canvas:
   - Add GameHUD component
   - Create health bar, currency display, wave info

5. Create Enemy Path:
   - Create empty GameObjects for path waypoints
   - Assign to EnemyManager

## Step 7: Create Prefabs

### Defender Prefab
1. Create: Sprite with DefenderConfig
2. Add components:
   - SpriteRenderer
   - Defender script
   - Collider2D (trigger)
3. Configure attack range, damage, etc.
4. Save to Prefabs/Defenders/

### Enemy Prefab
1. Create: Sprite with enemy appearance
2. Add components:
   - SpriteRenderer
   - Enemy script
   - Collider2D
3. Configure health, speed, etc.
4. Save to Prefabs/Enemies/

### Projectile Prefab
1. Create: Small sprite for projectile
2. Add components:
   - SpriteRenderer
   - Projectile script
   - Rigidbody2D (kinematic)
   - Collider2D (trigger)
3. Save to Prefabs/Projectiles/

## Step 8: Create ScriptableObjects

### GameConfig
1. Right-click Assets/ScriptableObjects/Configs
2. Create > Rumbax > Game Config
3. Configure balancing values

### DefenderConfig
1. Create one for each defender type
2. Set damage, attack speed, range, etc.

### EnemyConfig
1. Create one for each enemy type
2. Set health, speed, reward values

### AudioLibrary
1. Create > Rumbax > Audio Library
2. Assign audio clips to slots

## Step 9: Import SDKs

### AdMob (Google Mobile Ads)
1. Download: https://github.com/googleads/googleads-mobile-unity/releases
2. Assets > Import Package > Custom Package
3. Select downloaded .unitypackage
4. Configure with your AdMob App ID

### Unity IAP
1. Window > Package Manager
2. Find "In App Purchasing" under Unity Registry
3. Install
4. Window > Services
5. Enable In-App Purchasing

### Firebase
1. Download: https://firebase.google.com/docs/unity/setup
2. Import packages:
   - FirebaseAnalytics.unitypackage
   - FirebaseAuth.unitypackage
   - FirebaseFirestore.unitypackage
   - FirebaseCrashlytics.unitypackage
3. Add google-services.json to Assets folder

### Google Play Games
1. Download: https://github.com/playgameservices/play-games-plugin-for-unity
2. Import package
3. Configure with Play Console credentials

## Step 10: Configure Services

### AdMob Configuration
1. After import, find GoogleMobileAdsSettings
2. Set App ID from AdMob console
3. Update AdMobService.cs with your ad unit IDs

### IAP Products
1. Open IAPService.cs
2. Update product IDs to match Play Console
3. Test with sandbox accounts

### Firebase Configuration
1. Create project at firebase.google.com
2. Add Android app with your package name
3. Download google-services.json
4. Place in Assets folder

## Step 11: Testing

### Editor Testing
1. Use Unity Editor for basic gameplay testing
2. Simulate touch input with mouse

### Device Testing
1. File > Build Settings > Android
2. Enable "Development Build"
3. Build and Run
4. Test on multiple devices

### Testing Checklist
- [ ] Defenders spawn correctly
- [ ] Merge mechanic works
- [ ] Enemies follow path
- [ ] Waves progress properly
- [ ] Currency updates correctly
- [ ] Save/load works
- [ ] Ads display (test mode)
- [ ] IAP flows work (sandbox)
- [ ] Audio plays correctly
- [ ] UI scales on different screens

## Step 12: Build for Release

1. Create keystore (first time only):
   - Player Settings > Publishing Settings
   - Create New Keystore
   - Store securely!

2. Build Settings:
   - Build App Bundle: Checked
   - Create symbols.zip: Checked

3. Build:
   - File > Build Settings
   - Build

4. Upload to Play Console for testing

## Troubleshooting

### Script Compilation Errors
- Ensure all namespace references are correct
- Check for missing using statements
- Verify service locator registrations

### Missing References
- Re-assign references in Inspector
- Check prefab connections
- Verify ScriptableObject links

### Android Build Fails
- Update Gradle version
- Check SDK/NDK paths
- Verify minimum API level compatibility

### Firebase Errors
- Verify google-services.json is correct
- Check package name matches Firebase config
- Update Firebase SDK if needed

## Next Steps

After basic setup:
1. Create art assets (sprites, animations)
2. Add sound effects and music
3. Create VFX particles
4. Balance gameplay values
5. Test thoroughly on devices
6. Prepare Play Store listing
7. Submit for review
