# Copilot Instructions for `RuangdanWaktu`

## Build, test, and lint

Unity version is pinned to **2023.2.22f1** (`ProjectSettings\ProjectVersion.txt`).

There is no repository-defined build script or lint command checked in.

Use Unity Editor/Build Settings for player builds. Scene order is defined in `ProjectSettings\EditorBuildSettings.asset` (currently `Assets\A_Fauzan\A_Scenes\A_MainMenu.unity`, `Assets\Scenes\Testing.unity`, and `Assets\A_Fauzan\A_Scenes\A_PauseMenu.unity`), so verify the list before producing player builds.

Use Unity Editor CLI from the project root:

```powershell
# Run EditMode + PlayMode tests
& "<UNITY_EDITOR_PATH>\Unity.exe" -batchmode -nographics -projectPath . -runTests -testPlatform all -testResults .\Logs\test-results.xml -quit

# Run a single EditMode test by full name (or namespace/class filter)
& "<UNITY_EDITOR_PATH>\Unity.exe" -batchmode -nographics -projectPath . -runTests -testPlatform editmode -testFilter "Namespace.ClassName.TestName" -testResults .\Logs\single-test-results.xml -quit

# Run a single PlayMode test by full name (or namespace/class filter)
& "<UNITY_EDITOR_PATH>\Unity.exe" -batchmode -nographics -projectPath . -runTests -testPlatform playmode -testFilter "Namespace.ClassName.TestName" -testResults .\Logs\single-playmode-test-results.xml -quit
```

## High-level architecture

- **Rendering stack:** URP is enabled in `ProjectSettings\GraphicsSettings.asset` and points to `Assets\Settings\UniversalRP.asset`, which uses the 2D renderer in `Assets\Settings\Renderer2D.asset`. Global URP settings live in `Assets\UniversalRenderPipelineGlobalSettings.asset`.
- **Scene topology:** Build settings include `Assets\A_Fauzan\A_Scenes\A_MainMenu.unity` (menu UI), `Assets\Scenes\Testing.unity` (gameplay), and `Assets\A_Fauzan\A_Scenes\A_PauseMenu.unity` (pause UI). Gameplay wires a manually draggable analog clock (`AnalogClock` + `HourHand`/`MinuteHand`) together with a platformer player (`PlayerMovement2D`) and static collision platforms.
- **Code responsibilities (`Assets\Scripts`):**
  - `AnalogClock` rotates/repositions hand transforms around `clockCenter`, supports drag input, and can optionally follow `TimeManager`.
  - `TimeManager` is the in-game time source (`dayDuration` mapped to 24h/60m) and provides formatted `Clock24Hour()` / `Clock12Hour()` strings.
  - `DigitalClock` writes the formatted time to a `UnityEngine.UI.Text` component.
  - `PlayerMovement2D` handles 2D movement/jumping with a Rigidbody2D cast-based grounded check.
- **Time/movement bridge:** `PlayerMovementTimeBridge` can drive `TimeManager` from player X movement and toggles `TimeManager` auto-advance accordingly.
- **Menu/pause/audio layer:** `MainMenuManager` drives scene loads and settings, `SettingsUIManager` animates settings panels with DOTween, `PauseManager` freezes gameplay and toggles UI with DOTween, and `AudioManager` is a `DontDestroyOnLoad` singleton for music/SFX/dialog.
- **Dependency source of truth:** Unity packages are managed in `Packages\manifest.json` (includes URP, 2D feature set, Test Framework, Timeline, uGUI, Visual Scripting).

## Key conventions in this repository

- **Clock hand rotation contract:** `AnalogClock` computes angles as `TwelveOClockAngle - (time * degrees)` (90° is up in Unity 2D) and applies them in `FixedUpdate`. Keep the 90° offset and sign consistent when changing time or drag behavior.
- **Clock hierarchy + physics contract:** `HourHand` and `MinuteHand` are child transforms of the clock center and use `Rigidbody2D` (`m_BodyType: 1`, `m_UseFullKinematicContacts: 1`) plus `BoxCollider2D`. `AnalogClock` prefers `MovePosition`/`MoveRotation` when bodies exist; preserve this setup to avoid jitter and broken interaction.
- **Reference-first, fallback-second pattern:** scripts expect serialized references but use `FindFirstObjectByType<T>()` as a fallback in `Awake` (`AnalogClock`, `DigitalClock`, `PlayerMovementTimeBridge`, `PauseManager`). Keep explicit scene references when possible; fallback is convenience, not primary wiring.
- **Input/update split:** input state is collected in `Update` and physics is applied in `FixedUpdate` (`AnalogClock`, `PlayerMovement2D`). Follow this split for new movement/interaction logic.
- **Pause UI animation while paused:** `PauseManager` sets `Time.timeScale = 0` and uses DOTween sequences with `SetUpdate(true)` so UI animations still run; keep this pattern when changing pause transitions.
- **Unity asset/version-control expectations:** keep `.meta` files committed; Unity YAML/JSON files follow `.gitattributes` merge/diff settings; Unity-generated folders (`Library`, `Temp`, `Logs`, `UserSettings`, `Build*`) and generated solution/project files are ignored by `.gitignore`.
