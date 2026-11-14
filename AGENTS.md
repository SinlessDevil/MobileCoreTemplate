# AGENTS.md

This file provides guidance to OpenAI Codex when working with code in this repository.

## Project

MobileCoreTemplate is a Unity game template for Android and iOS. It uses Extenject (the Zenject API), Unity Addressables and URP. Read `README.md` for the architecture. Use the Editor pinned in `ProjectSettings/ProjectVersion.txt` (**6000.5.3f1**) for all Unity operations.

## Git workflow and commit messages

The default branch is `main`. Existing `main-addresable-template-*` and `main-zenject-template-*` branches are versioned template lines. Check the current branch and working tree before changing branches. Use `feature/<short-name>` from the intended base when using the feature-cycle skill. Follow the existing history for commit titles, such as `add:`, `edit:`, `fix:` and `upd:`, with a concise English description.

## Commands

Use Unity CLI with the official `com.unity.pipeline` bridge to verify the open Editor. Pass `--project-path` when targeting this checkout and use `unity status --until-ready`, `unity command editor status` and `unity command console status`. If the bridge is unavailable, verify in the Unity Editor or use batch mode when the project is closed.

- **Run the game:** open `Assets/Scenes/Initial.unity` (the only scene in Build Settings) and enter Play Mode. All other scenes load via Addressables.
- **First-time setup / after touching addressable assets:** *Window → Asset Management → Addressables → Groups* → *Build → New Build → Default Build Script*. To iterate without rebuilding bundles, set **Play Mode Script** to *Use Asset Database*.
- **Tests:** *Window → General → Test Runner* (EditMode + PlayMode tabs), or the project's own *Tools → Tests Tool Window*. Tests live in `Assets/Tests/{EditMode,PlayMode}` under the `Tests.EditMode` / `Tests.PlayMode` asmdefs.
- **Batch-mode tests (CI):** `Unity.exe -runTests -batchmode -projectPath . -testPlatform EditMode -testResults results.xml` (swap `PlayMode` for the other suite). To run one test, add `-testFilter "Namespace.ClassName.TestName"`.

### Editor dev tools (custom menu items)

- *GameEditors → ⚙ Game Config Editor* (`%F3` / Ctrl+F3) — central config editor
- *Tools → Audio Vibration Window/Sound Library*, *Tools → Save Window/...*, *Tools → Text Migration Window*

## Architecture (big picture)

Strict dependency direction: **Infrastructure → Services → Game Logic**. Code namespace root is `Code`; all gameplay/runtime code is in the `Game` asmdef (`Assets/Code/Game.asmdef`), editor code in `GameEditor`.

**Composition root — `BootstrapInstaller` (`Assets/Code/Infrastructure/Installers/`).** A Zenject `MonoInstaller` on the `GameRunner` prefab. `InstallBindings()` wires every service (`Bind*` methods), then `Initialize()` → `BootstrapGame()` enters the state machine at `BootstrapState`. To add a service: register it in the appropriate `Bind*` method here. `BindStaticDataService()` constructs `StaticDataService` and `AudioVibrationStaticDataService`, calls `LoadData()` on each, and binds the loaded instances.

**Game flow — `IStateMachine<IGameState>`.** States are injected via Zenject. The usual menu flow is:
`BootstrapState → LoadProgressState → BootstrapAnalyticState → PreLoadGameState → LoadMenuState → LoadLevelState → GameLoopState` (→ Win/Lose). States are bound in `BindGameStates()` and live in `Infrastructure/StateMachine/Game/States/`. `PreLoadGameState` can enter `LoadLevelState` directly, depending on `GameConfig` and the load type.

**Asset management — `AssetProvider`.** Addressables cache with two stores: a **temp** cache cleared on scene change (`CleanUp()`) and a **persistence** cache that's never cleared. `SceneLoader` releases the previous scene handle before each load. Windows (`WindowConfig` → `AssetReferenceGameObject`) load through the temp cache and free automatically on scene transition.

**Static data.** ScriptableObjects under `Assets/Resources/StaticData/`, loaded synchronously at boot. See the README table for the full asset → path map (`GameConfig`, `Balance`, `WindowsStaticData`, sound and vibration configuration).

**Save system.** `ISaveLoadFacade` (`UnifiedSaveLoadFacade`) dispatches per-call between PlayerPrefs (`PrefsSaveLoadService`) and JSON (`JsonSaveLoadService`) via a `SaveMethod` enum. Progress is read through `IPersistenceProgressService`.

## Development rules

These are the project's standing conventions — follow them for new code unless the user says otherwise.

### Stack
- Third-party plugins in use: **Extenject** (Zenject DI), **UniTask** (async), **DOTween** (tweens), **Addressables** (asset/scene loading), **UI Toolkit** (editor windows only — see UI rules). Check `Packages/manifest.json` and `Assets/Plugins` for the versions and sources used by this checkout.
- Unity **6000.5.3f1** (`ProjectSettings/ProjectVersion.txt`) — write code against this version and the existing architecture, not newer APIs.

### Code style
- **Strict Microsoft C# style.** Private fields are `_camelCase` with a leading underscore; `readonly` wherever possible. Braces on their own lines (Allman). Use explicit types for C# local variables, loop variables and `out` declarations. Do not use `var`. Use expression-bodied members (`=>`) for one-liners (see `LevelService`).
- **Member order inside a type, top to bottom:** constants/`static readonly` → serialized/private fields → properties → events/Actions → constructor (plain or Zenject `Constructor()` init) → public methods → private methods. Keep new code in this order; mirror a neighbouring service.
- **No defensive null-checks.** Do not guard serialized/injected references (`if (_field != null) …`, `x == null → return`) just in case. A missing `.asset`, an unassigned inspector field or a forgotten binding must throw a loud `NullReferenceException` — that's how the user finds the wiring mistake. Write a null-check only when `null` is a real, expected state that the logic must branch on (e.g. "no pooled instance yet → create one", "unbound slot → fall back to the global input adapter").
- **Extensions go in `Assets/Code/Extensions`.** Reusable helper/extension methods belong there as `*Extensions` classes (e.g. `TransformExtensions`, `NumericExtensions`), not scattered inline. Add your own there when it makes sense.

### Shaders

- The quality profile selects **URP 17.5.0 Forward** via `Assets/Resources/URP/URP Asset.asset`. Follow URP conventions for new shaders, including the `UniversalPipeline` tag, `HLSLPROGRAM` and URP `ShaderLibrary` includes. Keep material properties in `UnityPerMaterial` for SRP Batcher compatibility.
- Prefer `half` for colors and masks, `float` where precision matters, and local shader keywords. Keep mobile overdraw, extra passes and variant count in mind. The URP asset disables camera depth texture, opaque texture and HDR, and enables main-light shadows.
- Validate changed shaders in Unity. Use the Frame Debugger or device profiling when the change affects rendering cost.

### Folders (where things live)
- `Assets/Code` — all runtime/editor code.
- `Assets/Tests` — EditMode + PlayMode tests.
- `Assets/Resources/StaticData` — ScriptableObject configs (loaded synchronously at boot).
- `Assets/ResourcesAddresable` — prefabs/scenes spawned & loaded into memory via Addressables.

### Core patterns
- **Entry point = `BootstrapInstaller`** (`Assets/Code/Infrastructure/Installers/`). This is where everything is composed: services, factories, static-data, and the game state machine are all bound here. New service/factory ⇒ register it in the matching `Bind*` method.
- **State machine flow.** The app is driven by a strict state-machine chain (`IStateMachine<IGameState>`): switching screens, leaving menu, entering gameplay, win/lose all go through states in `Infrastructure/StateMachine/Game/States/`. Don't bypass it for navigation/flow.
- **Service-per-feature.** Each feature gets its own interface-first service(s): `IFooService` + `FooService`, injected via Zenject, bound with `Container.BindInterfacesTo<FooService>().AsSingle()`. Per-frame work uses `ITickable`; init that must run at bind time uses eager `Container.Resolve<...>()` in the installer (see `BindAudioService`). Mirror an existing service in `Assets/Code/Services` for structure.
- **Factories.** A complex entity (e.g. the player) gets its own dedicated Zenject factory deriving from `Factory` (`Services/Factories/Factory.cs`) — it loads the prefab via Addressables/`IAssetProvider`, instantiates through `IInstantiator`, and applies the entity's base setup. Async, returns `UniTask`. Trivial entities (a widget, a simple UI element) don't need a dedicated factory — go through a general factory like `UIFactory`. When unsure whether something is "complex enough", ask.
- **OOP / SOLID** are followed strictly — no service locators, no static singletons for runtime logic.
- **Async = UniTask**, not raw `Task` or coroutines, for new code.

### UI (when asked to build UI)
- Stack: **uGUI** (current windows use `RectTransform` + DOTween). Reserve **UI Toolkit** for editor tooling unless told otherwise.
- New UI follows **MVVM** (here VM = a presenter / presentation model):
  - **View** — only displays what's on screen (and may host its own animation), no business logic.
  - **ViewModel (presenter)** — the "conductor": pulls data from the model, computes/calculates presentation-ready values, and feeds the View.
  - **Model** — owns and serves the raw data.
- Scope: apply MVVM to **new UI only**. Leave existing `BaseWindow` / `StateWindow` / `IWindowState` windows (`Assets/Code/UI/Menu/Windows`) as-is — don't migrate them unless explicitly asked.

### Testing
- Unit tests are **optional and per-task**. Add tests when requested or when a specific regression needs coverage. Tests live in `Assets/Tests/{EditMode,PlayMode}`.

### Working in this repo via the filesystem (agent constraints)
- **Edit only code files by default** — `.cs`, plus `.shader`/`.hlsl` under `Assets/Code/Shaders`. You may *read* anything (`.meta`, `.asset`, `.prefab`, `.unity`, `.mat`, `.shadergraph`, `.uxml`/`.uss`, `.json`, configs) to understand the project, but only **edit/create those code files** unless the user explicitly asks you to change another file type. Never silently hand-edit Unity-serialized assets (`.prefab`, `.unity`, `.asset`, `.meta`) — those are owned by the Editor.
- **`.meta` / GUID hygiene.** This is a Unity project edited outside the Editor. Every asset/script has a sibling `.meta` with a unique GUID. Do **not** hand-author `.meta` files or invent GUIDs — Unity generates them on reimport, and `GuidDuplicationTest` fails on duplicates. When renaming/moving/deleting a file, move/delete its `.meta` alongside it. Refresh the Editor import after adding a `.cs` file so Unity generates its `.meta`.
- **Verification.** Check compilation and the Console through Unity Pipeline when it is available. Run relevant Unity tests or Play Mode checks for changes that require them. Report any verification that is still pending.
- **Addresses via `ResourcePath`.** Use the constants in `Code.Services.Factories.ResourcePath` for Addressables/Resources paths instead of inline magic strings. If a needed path isn't there yet, add a constant rather than hardcoding (some legacy paths are still inline — don't multiply them).
- **`Constructor()` init idiom.** Plain C# classes that aren't Zenject constructor-injected use a `public void Constructor(...)` init method (called manually after creation, ~13 places, e.g. `IWindowState.Constructor`). This is the project's init convention, not an actual C# constructor — follow it for similar classes and don't "fix" it into a real ctor.
