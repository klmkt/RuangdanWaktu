# Copilot Instructions for `RuangdanWaktu`

## Build, test, and lint

Unity version is pinned to **2023.2.22f1** (`ProjectSettings\ProjectVersion.txt`).

There is no repository-defined build script or lint command checked in.

Use Unity Editor/Build Settings for player builds. Build settings currently reference `Assets/Scenes/SampleScene.unity`, while the committed gameplay scene is `Assets/Scenes/Testing.unity`, so verify scene list before producing player builds.

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

- **Rendering stack:** URP is enabled in `ProjectSettings\GraphicsSettings.asset` and points to `Assets\Settings\UniversalRP.asset`, which uses the 2D renderer in `Assets\Settings\Renderer2D.asset`. `Assets\Scenes\Testing.unity` includes a `Global Light 2D`.
- **Scene topology:** The committed scene is `Assets\Scenes\Testing.unity`. It wires a manually draggable analog clock (`AnalogClock` + `HourHand`/`MinuteHand`) together with a separate platformer player (`player` + `PlayerMovement2D`) and static collision platforms.
- **Code responsibilities (`Assets\Scripts`):**
  - `AnalogClock` rotates/repositions hand transforms around `clockCenter`, supports drag input, and can optionally follow `TimeManager`.
  - `TimeManager` is the in-game time source (`dayDuration` mapped to 24h/60m) and provides formatted `Clock24Hour()` / `Clock12Hour()` strings.
  - `DigitalClock` writes the formatted time to a `UnityEngine.UI.Text` component.
  - `PlayerMovement2D` handles 2D movement/jumping with a Rigidbody2D cast-based grounded check.
- **Current scene wiring detail:** `Testing.unity` uses `AnalogClock` and `PlayerMovement2D`; `TimeManager`/`DigitalClock` are present in code but not currently attached in the scene.
- **Dependency source of truth:** Unity packages are managed in `Packages\manifest.json` (includes URP, 2D feature set, Test Framework, Timeline, uGUI, Visual Scripting).

## Key conventions in this repository

- **Clock hand rotation contract:** `AnalogClock` uses negative time-derived angles (`hourAngle = -GetHour() * 30`, `minuteAngle = -GetMinutes() * 6`) and applies them in `FixedUpdate`. Keep angle sign/direction consistent when changing time or drag behavior.
- **Clock hierarchy + physics contract:** `HourHand` and `MinuteHand` are child transforms of the clock center and use `Rigidbody2D` (`m_BodyType: 1`, `m_UseFullKinematicContacts: 1`) plus `BoxCollider2D`. `AnalogClock` prefers `MovePosition`/`MoveRotation` when bodies exist; preserve this setup to avoid jitter and broken interaction.
- **Reference-first, fallback-second pattern:** scripts expect serialized references but use `FindObjectOfType<TimeManager>()` as a fallback in `Awake` (`AnalogClock`, `DigitalClock`). Keep explicit scene references when possible; fallback is convenience, not primary wiring.
- **Input/update split:** input state is collected in `Update` and physics is applied in `FixedUpdate` (`AnalogClock`, `PlayerMovement2D`). Follow this split for new movement/interaction logic.
- **Unity asset/version-control expectations:** keep `.meta` files committed; Unity YAML/JSON files follow `.gitattributes` merge/diff settings; Unity-generated folders (`Library`, `Temp`, `Logs`, `UserSettings`, `Build*`) and generated solution/project files are ignored by `.gitignore`.
