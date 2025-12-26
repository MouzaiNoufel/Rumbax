# Rumbax - Merge Defense Mobile Game

A modern, high-quality Android mobile game built with Unity. Rumbax is a Merge Defense game combining idle/merge mechanics with strategic tower defense gameplay.

## Game Overview

### Genre
- Casual / Strategy / Idle / Tower Defense Hybrid

### Core Mechanics
- **Merge System**: Combine matching defenders to create higher-level units
- **Tower Defense**: Place defenders on a grid to stop enemy waves
- **Idle Progression**: Earn offline rewards based on play time
- **Wave-based Combat**: Survive increasingly difficult enemy waves

### Features
- Dual currency system (Coins + Premium Gems)
- Daily challenges with streak rewards
- Achievement system with Google Play Games integration
- Leaderboards for competitive play
- Cloud save with Firebase
- Complete monetization (Ads, IAP, Subscriptions)

## Project Structure

```
Assets/
├── Scripts/
│   ├── Audio/           # Audio system and sound management
│   ├── Core/            # Core architecture (GameManager, ServiceLocator, EventBus)
│   ├── Data/            # Data models and configurations
│   ├── Firebase/        # Firebase integration (Analytics, Cloud Save, Remote Config)
│   ├── Gameplay/        # Game mechanics (Grid, Defenders, Enemies, Waves)
│   ├── Monetization/    # AdMob and IAP services
│   ├── Systems/         # Game systems (Achievements, Challenges, Leaderboards)
│   ├── UI/              # User interface panels and components
│   └── VFX/             # Visual effects and animations
├── Prefabs/             # Game prefabs (create manually in Unity)
├── Scenes/              # Game scenes (create manually in Unity)
├── Audio/               # Audio clips (add your audio files)
└── Art/                 # Sprites and textures (add your art assets)
```

## Architecture

### Design Patterns
- **Service Locator**: Centralized dependency injection
- **Event Bus**: Decoupled publish-subscribe communication
- **Object Pooling**: Efficient memory management for VFX and enemies
- **Singleton**: GameManager for global state

### Key Components

#### Core Services
- `GameManager`: Main game state and lifecycle management
- `ServiceLocator`: Service registry for dependency injection
- `EventBus`: Event-driven communication system

#### Gameplay
- `GridManager`: 5x5 grid with drag-drop support
- `DefenderSpawner`: Spawns and manages defenders
- `WaveManager`: Wave progression and enemy spawning
- `LevelController`: Level orchestration

#### Monetization
- `AdMobService`: Banner, interstitial, and rewarded ads
- `IAPService`: In-app purchases and subscriptions

#### Firebase
- `FirebaseAnalyticsService`: Event tracking
- `FirebaseCloudService`: Cloud save with conflict resolution
- `FirebaseRemoteConfigService`: Remote configuration

## Setup Instructions

### Prerequisites
- Unity 2022.3 LTS or newer
- Android Build Support module
- TextMeshPro package
- Visual Studio or VS Code

### Initial Setup

1. Open the project in Unity
2. Import required packages:
   - TextMeshPro (via Package Manager)
   - Google Mobile Ads Unity Plugin
   - Unity IAP
   - Firebase Unity SDK
   - Google Play Games Plugin for Unity

### Package Installation

#### Google Mobile Ads (AdMob)
1. Download from: https://github.com/googleads/googleads-mobile-unity/releases
2. Import the .unitypackage
3. Configure App ID in `Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings`

#### Unity IAP
1. Window > Package Manager > Unity IAP
2. Enable In-App Purchasing in Services window

#### Firebase SDK
1. Download from: https://firebase.google.com/docs/unity/setup
2. Import: FirebaseAnalytics, FirebaseAuth, FirebaseFirestore, FirebaseCrashlytics
3. Add `google-services.json` to Assets folder

#### Google Play Games
1. Download from: https://github.com/playgameservices/play-games-plugin-for-unity
2. Import the .unitypackage
3. Configure with your Play Console credentials

### Scene Setup

Create the following scenes:

1. **SplashScene**: Loading and initialization
2. **MainMenuScene**: Main menu with play, shop, settings
3. **GameScene**: Main gameplay scene
4. **LoadingScene**: Scene transitions

### Prefab Setup

Create prefabs for:
- Defenders (6 types with 10 levels each)
- Enemies (Normal, Fast, Tank, Boss variants)
- Projectiles (Basic, Explosive, Piercing)
- Grid Cells
- UI Panels

## Build Configuration

### Android Settings

1. **Player Settings** (Edit > Project Settings > Player):
   - Company Name: Your company
   - Product Name: Rumbax
   - Package Name: com.yourcompany.rumbax
   - Minimum API Level: 24 (Android 7.0)
   - Target API Level: 34 (Android 14)
   - Scripting Backend: IL2CPP
   - Target Architectures: ARMv7, ARM64

2. **Keystore**:
   - Create a keystore for release builds
   - Store securely - you cannot update without it

### Build Process

1. File > Build Settings
2. Select Android platform
3. Build Settings:
   - Build App Bundle (Google Play): Checked
   - Create symbols.zip: Checked (for crash reports)
4. Build

### Testing

- Use Unity Remote for quick testing
- Build debug APK for device testing
- Use internal testing track on Play Console

## Play Store Submission

### Requirements Checklist

- [ ] App Bundle built and signed
- [ ] Privacy Policy URL
- [ ] App screenshots (phone and tablet)
- [ ] Feature graphic (1024x500)
- [ ] App icon (512x512)
- [ ] Short description (80 chars)
- [ ] Full description (4000 chars)
- [ ] Content rating questionnaire
- [ ] Target audience declaration
- [ ] Data safety form

### AdMob Setup

1. Create AdMob account
2. Create app in AdMob console
3. Create ad units:
   - Banner (bottom of screen)
   - Interstitial (level complete)
   - Rewarded (bonus rewards)
4. Replace test ad unit IDs with production IDs

### IAP Products

Create in Play Console:
- `remove_ads`: One-time purchase
- `subscription_monthly`: Monthly subscription
- `subscription_yearly`: Yearly subscription
- `gems_100`: 100 gems package
- `gems_500`: 500 gems package
- `gems_1000`: 1000 gems package
- `starter_pack`: Starter bundle

## Configuration Files

### Test Ad Unit IDs (Replace for Production)

```
Banner: ca-app-pub-3940256099942544/6300978111
Interstitial: ca-app-pub-3940256099942544/1033173712
Rewarded: ca-app-pub-3940256099942544/5224354917
```

### Firebase Configuration

Place `google-services.json` in:
- `Assets/` folder for Android
- Configure in Firebase Console

## Troubleshooting

### Common Issues

1. **Firebase initialization fails**
   - Check google-services.json is in Assets folder
   - Verify package name matches Firebase config

2. **Ads not showing**
   - Test mode should work immediately
   - Production ads need app approval

3. **IAP not working**
   - Ensure app is uploaded to Play Console (even closed testing)
   - Use test accounts configured in Play Console

4. **Build errors**
   - Update Gradle version in Player Settings
   - Ensure all SDKs are compatible versions

## Version History

- v0.1.0: Initial development version
  - Core architecture
  - Gameplay mechanics
  - UI system
  - Monetization integration
  - Firebase integration
  - Achievements and challenges
  - Audio and VFX systems

## License

Proprietary - All rights reserved.

## Support

For development questions, refer to:
- Unity Documentation: https://docs.unity3d.com
- Firebase Unity Guide: https://firebase.google.com/docs/unity/setup
- AdMob Unity Guide: https://developers.google.com/admob/unity/quick-start
- Play Games Services: https://developers.google.com/games/services