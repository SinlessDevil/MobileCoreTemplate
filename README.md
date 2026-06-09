# MobileCoreTemplate — Unity Mobile Game Template

A production-ready Unity template for mobile games (Android / iOS) built on Zenject dependency injection and Unity Addressables. Provides a complete infrastructure layer so development can begin from game logic rather than boilerplate.

---

## Requirements

| Requirement | Version |
|---|---|
| Unity | 2023.2.22f1 LTS or newer |
| Target platforms | Android, iOS |
| Render pipeline | URP |
| .NET | Standard 2.1 |

---

## Architecture

The project follows a layered architecture with strict dependency direction: Infrastructure → Services → Game Logic.

```
Infrastructure
  BootstrapInstaller        — Zenject root composition, service registration
  StateMachine              — Game flow controller (Bootstrap → Menu → Level → GameLoop)
  SceneLoader               — Async scene transitions via Addressables
  LoadingCurtain            — Visual transition overlay

Services
  AssetProvider             — Addressables asset cache (temp + persistence)
  AssetPreloaderService     — Bundle download size check and dependency preload
  AssetPreloaderConductor   — Background preload scheduling by player level
  UIFactory / GameFactory   — Prefab instantiation via Zenject IInstantiator
  WindowService             — Window lifecycle via AssetReference
  StaticDataService         — ScriptableObject config loader
  SaveLoadFacade            — Unified save/load (PlayerPrefs / JSON)
  LevelService              — Level progression and completion
  TimeService               — Pause, resume, elapsed time
  SoundService              — 2D/3D audio with pooling
  VibrationService          — Haptic feedback
  WidgetProvider            — UI widget pooling
  RandomService             — Seeded random utility
  InputService              — Input abstraction
```

---

## Game Flow

```
BootstrapState
  └─ LoadProgressState       (load save, version check)
       └─ LoadMenuState      (clean asset cache, load Menu scene, setup UI)
            └─ LoadLevelState (clean asset cache, load Game scene, setup HUD)
                 └─ GameLoopState
                      ├─ WinState
                      └─ LoseState
```

State transitions are handled through `IStateMachine<IGameState>`. Each state is injected via Zenject — no service locator, no static references.

---

## Asset Management

### AssetProvider

Central Addressables cache with two independent stores:

| Cache | Cleared by | Use case |
|---|---|---|
| Temp (`_completedHandles`) | `CleanUp()` on scene change | Scene-specific assets: HUD, windows, level objects |
| Persistence (`_completedPersistenceHandles`) | Never | Assets needed across all scenes |

Loading an asset that is already cached returns immediately without an Addressables request.

### Scene Lifecycle

`SceneLoader` stores the handle of the currently loaded Addressable scene. Before each new scene load, the previous handle is released via `Addressables.UnloadSceneAsync()`. This prevents handle accumulation across scene transitions.

### Window System

`WindowConfig` stores an `AssetReferenceGameObject` per window type. Windows are loaded on demand through `AssetProvider` (temp cache) and released automatically when `CleanUp()` is called on scene transition. No window prefab is loaded until the window is first opened.

---

## Project Structure

```
Assets/
  Code/
    Infrastructure/         — State machine, SceneLoader, LoadingCurtain, Installers
    Services/
      AssetProvider/        — IAssetProvider, AssetProvider
      AssetPreloader/       — IAssetPreloaderService, AssetPreloaderService
      PreloaderConductor/   — AssetPreloaderConductor
      Factories/            — Factory base, UIFactory, GameFactory
      StaticData/           — IStaticDataService, StaticDataService
      PersistenceProgress/  — PlayerData, LoadingData, progress models
      SaveLoad/             — ISaveLoadFacade, PlayerPrefs/JSON backends
      Levels/               — LevelService
      Window/               — IWindowService, WindowService
      Providers/Widgets/    — WidgetProvider, widget pooling
      Finish/               — WinService, LoseService
    StaticData/             — ScriptableObject definitions (configs, window configs)
    UI/                     — HUD, Menu, Window prefab scripts
  Resources/
    StaticData/             — ScriptableObject assets (loaded synchronously at boot)
  Tests/
    EditMode/               — Structural validation tests
    PlayMode/               — Runtime integration tests
```

---

## Addressables Setup

All runtime-loaded assets are organized into Addressable groups:

| Group | Contents |
|---|---|
| `Hud` | GameHud, MenuHud, LoadingCurtain |
| `UI` | UiRoot, Widget, ItemLevel, window prefabs |
| `Game` | Game scene |
| `Menu` | Menu scene |

Only the `Initial` scene is listed in Build Settings. All other scenes are loaded via `Addressables.LoadSceneAsync()`.

To iterate without rebuilding bundles, set Addressables **Play Mode Script** to `Use Asset Database` in the Addressables Groups window.

---

## Getting Started

1. Clone the repository
2. Open in Unity 2022.3 LTS or newer
3. Open `Assets/Scenes/Initial.unity`
4. In the Addressables Groups window (Window → Asset Management → Addressables → Groups), build local content: **Build → New Build → Default Build Script**
5. Enter Play Mode

---

## Static Data Configuration

ScriptableObject assets are loaded from `Resources/StaticData/` at startup via `StaticDataService`. Key configs:

| Asset | Path | Purpose |
|---|---|---|
| `GameConfig` | `StaticData/Balance/GameConfig` | Core game parameters |
| `Balance` | `StaticData/Balance/Balance` | Numeric balance values |
| `WindowsStaticData` | `StaticData/WindowsStaticData` | Window type → AssetReference mapping |
| `Chapters` | `StaticData/Chapters` | Level chapter definitions |
| `PreloadConfig` | `StaticData/PreloadConfig` | CDN group preload schedule by player level |

---

## Save System

Two backends, selected per call via `SaveMethod` enum:

| Backend | Class | Use case |
|---|---|---|
| PlayerPrefs | `PrefsSaveLoadService` | Lightweight progress, settings |
| JSON | `JsonSaveLoadService` | Complex serializable data |

Progress is accessed through `IPersistenceProgressService` and persisted via `ISaveLoadFacade`.

---

## Testing

### EditMode

| Test class | What it validates |
|---|---|
| `GuidDuplicationTest` | No duplicate `.meta` GUIDs in the project |
| `ResourcesPrefabValidationTests` | No missing scripts on prefabs in `Resources/` |
| `SceneValidationTests` | No missing scripts, prefab links, or null serialized fields in scenes |
| `LevelService Tests` | Level selection and local progress logic |
| `StorageService Tests` | Key-value storage consistency |

### PlayMode

| Test class | What it validates |
|---|---|
| `WidgetProviderPlayModeTest` | Widget pool reuse — same instance returned after release |

---

## Third-Party Dependencies

| Package | Source | Purpose |
|---|---|---|
| Zenject | UPM | Dependency injection |
| UniTask | UPM | Zero-allocation async/await |
| Unity Addressables | UPM | Asset and scene loading |
| TextMeshPro | UPM | UI text rendering |
| UI Particle (Coffee) | UPM | Particle effects on UI canvas |

---
