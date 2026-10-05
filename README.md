<p align="center">
  <img src="https://raw.githubusercontent.com/abdoulrl2028-cloud-Dev/abdoulrl2028-cloud-Dev/main/assets/projects/fps.jpg" alt="FPS 3D in Unity" width="100%">
</p>

# FPS 3D — Unity action game

A playable 3D FPS built in **Unity 6000.0.83f1** with C#.
The player moves through an urban and rural map, fights AI enemies, passes checkpoints, and finishes the mission.

## Open the project

1. Install **Unity Hub** and version **6000.0.83f1**.
2. In Unity Hub, choose **Open** and select the `FPS 3D/FPS 3D` folder.
3. Wait for packages to import and for the scripts to compile.

> On the first open, the project **edits and saves the scene** `Assets/Scenes/Level01.unity` automatically. The menu `FPS → Build FPS Scene` runs the same process by hand. Wait for `[FPS] Scene built successfully!` in the Console.

> **Packages:** `com.unity.ugui` and `com.unity.cloud.gltfast` download through the Package Manager. If the Editor is already open, click the Unity window to refresh the packages.

## Play

1. Open `Assets/Scenes/Level01.unity`.
2. Press **Play**.
3. The HUD (health, ammo, crosshair) and the enemies are already active.

> **Enemy navigation (NavMesh):** the mesh is baked when the scene is built. If you move obstacles, run `FPS → Build FPS Scene` again.

## Controls

| Action | Key |
| --- | --- |
| Move | `W A S D` |
| Sprint | Left `Shift` + WASD |
| Jump | `Space` |
| Look | Mouse |
| Shoot | Left mouse button |
| Weapon 1 (pistol) | `1` |
| Weapon 2 (rifle) | `2` |
| Weapon 3 (shotgun) | `3` |
| Reload | `R` |
| Pause | `Esc` |
| Restart (game over / mission) | `R` |

## Folder layout

```
Assets/
├─ Scripts/
│  ├─ Player/        FpsPlayerController, PlayerDeathHandler, PlayerMovement, PlayerLook, PlayerHealth
│  ├─ Weapons/       WeaponData (SO), WeaponController, WeaponEntry
│  ├─ Enemies/       EnemyAI, EnemyHealth
│  ├─ Systems/       HealthSystem, IDamageable, Checkpoint, GameManager, MusicPlayer
│  └─ UI/            FpsHud, PauseMenu, GameOverMenu, MissionCompleteMenu
├─ Editor/
│  ├─ FpsSceneBuilder.cs     Builds the Level01 scene
│  ├─ FpsCityProps.cs        Places downloaded Poly Haven models
│  ├─ FpsAudioGenerator.cs   Generates music and sound effects (WAV)
│  └─ ProjectScaffolder.cs   Creates scenes, builder settings, and a basic player prefab
├─ Models/Environment/       CC0 Poly Haven models
├─ Prefabs/                  Weapon data and the player prefab
├─ Materials/                Environment, characters, and weapons
├─ Scenes/                   MainMenu, Level01 (main), Level02..07, Test
├─ Textures/
└─ UI/
```

## Main scripts

- **FpsPlayerController** — WASD movement, sprint, jump, gravity, and mouse look.
- **WeaponController / WeaponData** — modular weapons: damage, fire rate, range, magazine, reserve ammo, reload time, and shotgun spread. A new weapon is a new `WeaponData` plus a list entry.
- **HealthSystem** — shared health for the player and enemies (`IDamageable`), with `OnDamaged`, `OnHealed`, and `OnDied`.
- **EnemyAI** — NavMesh patrol, distance and view detection, chase, and melee. The enemy stops when it dies.
- **FpsHud** — health bar, ammo, weapon name, crosshair, and status messages.
- **GameManager** — pause, game over, restart at the last checkpoint, mission complete, and cursor lock.
- **FpsSceneBuilder** — builds the scene and bakes the NavMesh. It runs on the first open.
- **FpsCityProps** — places real 3D models on the street (`FPS → Place Real City Props (Poly Haven)`).

## Add a weapon

1. Create a `WeaponData` asset (`Assets → Create → FPS → Weapon Data`), or run `FPS → Build FPS Scene` to recreate the pistol, rifle, and shotgun.
2. Give it a unique `slot` (1, 2, 3). Keys 1–3 switch weapons.
3. Add a `WeaponEntry` (data, view, and audio source) on the player's `WeaponController`.

## Use your own 3D models

Put Unity-importable files (`.glb` / `.gltf` with glTFast, or `.fbx` / `.obj`) in `Assets/Environment/Imported/`. `FpsCityProps` loads models from that folder. To replace the player or enemy look, parent the model to the player or the enemy capsule and hide the default primitive.

## Asset credits

Street and city models: **Poly Haven** (CC0). Music and effects are generated inside the project.
https://polyhaven.com/
