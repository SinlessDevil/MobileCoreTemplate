# 🧩 Unity Zenject Template

A clean, modular Unity template built on [Zenject](https://github.com/modesttree/Zenject), designed for scalable game projects. Includes core services, state machine, tool integrations, URP setup, and a full test suite.
### ✨ Features
- **🧠 State Machine (Game Flow)** — `Bootstrap → Load → Menu → Game → Win/Lose`  
- **🏗️ Factory Services** — UI, Game, Widget, and Scene instantiation  
- **📊 Static Data System** — Loadable config data for sounds, levels, etc.  
- **📦 Save System Toolkit** — PlayerPrefs / JSON / XML + Editor tool  
- **🔊 Audio & Vibration Kit** — Music, 2D/3D sounds, haptics with pooling  
- **🧪 Built-in Test ToolKit** — Scene/prefab/enum/guid validation  
- **🎨 URP + Toony Colors Pro** — Stylized rendering preset  
- **🐞 SRDebugger** — Integrated in-game debug UI  
- **📁 Modular Structure** — Clean separation into Code, Plugins, Tests, Resources

### 🧪 Test Coverage
**EditMode:**
- `GuidDuplicationTest` — detects duplicate `.meta` GUIDs  
- `ResourcesPrefabValidationTests` — checks for missing scripts in `Resources` prefabs  
- `SceneValidationTests` — verifies scenes for:
  - missing scripts  
  - missing prefab links  
  - null fields on serializable components  
- `Enum Tests` — ensures enum-to-data mapping is valid  
- `LevelService Tests` — validates level selection and local progress logic  
- `StorageService Tests` — checks key-value consistency  

**PlayMode:**
- `WidgetProvider Tests` — tests prefab resolution and instantiation

### 📦 Included Tools
- [🔧 CI GitHub Action](https://github.com/SinlessDevil/CI)
- [💾 SaveSystemToolkit](https://github.com/SinlessDevil/SaveSystemToolkit)
- [🎵 AudioVibrationKit](https://github.com/SinlessDevil/AudioVibrationKit)
- [🧪 TestToolKit](https://github.com/SinlessDevil/TestToolKit)

### ✅ Entry Point
The `BootstrapInstaller` (via `Zenject`) wires up:
- Core services (UI, Level, SaveLoad, Storage, Audio)
- Game StateMachine & States
- Static Data loaders
- Coroutine runner and loading curtain

### 📏 Architecture Overview
The project is structured with a modular architecture, using **Zenject** as the Dependency Injection framework. It is designed to support mid-core and hybrid-casual projects out of the box. Below is an overview of the core systems and services:

### 📆 Core Components
* **🌀 Game State Machine**
  A flexible state machine with separate classes for each game state:
  * `BootstrapState`, `LoadProgressState`, `LoadMenuState`, `LoadLevelState`, `GameLoopState`, etc.
* **🏠 Game/UI Factories**
  Centralized services for creating gameplay entities and UI windows.
* **📁 Static Data System**
  Configurable data loading system using ScriptableObjects. Includes support for gameplay configuration and audio/vibration settings.
* **🧠 Services Layer**
  Decoupled services for all game-related logic:
  * **Level Management** (LevelService)
  * **Save/Load Progress** (UnifiedSaveLoadFacade)
  * **UI & Windows** (WindowService)
  * **Input Handling** (InputService)
  * **Random Generator** (RandomService)
  * **Time Management** (TimeService)
  * **Widget System** (WidgetProvider)
  * **Storage Logic** (StorageService)
* **🎮 Win/Lose System**
  Includes `WinService` and `LoseService` for handling end-of-level logic.
* **🚪 Coroutine and Loading Curtain**
  Mono-based services used for async logic and visual transitions.
