# Oni Flip

A short 2D puzzle-platformer inspired by VVVVVV. You cannot jump. The only way to move vertically is to **flip gravity**, and you can only do that while standing on a surface. Find a route through five stages, avoid the spikes, grab the key, and reach the goal as fast as you can.

Built with Unity 6 (6000.5.9f1), C#, and the Input System package.

## Controls

| Key | Action |
|---|---|
| A | Move left |
| D | Move right |
| Space | Flip gravity (only while grounded) |
| Esc | Pause menu |
| F12 | Save a screenshot |

## Core loop

1. **Action:** walk and flip gravity to cross a room.
2. **Feedback:** the character mirrors and flips on screen, a sound plays on every flip, and touching a hazard makes the character blink red before the stage restarts.
3. **Progression:** reaching the goal loads the next stage. The HUD shows the current stage, the total time, and the death count.
4. **Repeat:** after the last stage a results screen shows the total time and deaths, so there is a reason to play again and beat your own record.

## Mechanics

- **Gravity flip:** works only when the player's collider is touching ground below it (checked with a collider cast and the surface normal, so walls do not count).
- **Spikes:** touching one starts the death sequence (blink, then restart).
- **Bounce wall:** a solid block that flips gravity when you walk into it.
- **Bounce bar:** a thin trigger line that flips gravity when you pass through it. Crossing it repeatedly makes the player oscillate around it, which is used to travel through spike corridors.
- **Key and goal:** in some stages the goal is locked (dimmed) until the key is collected.
- **Multi-screen stages:** camera boundaries move the camera one screen at a time when the player walks across them.

## Progression and UI

- Five stages, loaded in build order, with a difficulty curve that introduces one new idea per stage.
- Main menu, pause menu (Resume, Restart stage, Main menu), and a results screen.
- HUD with stage number, timer, and death counter. Time and deaths carry across stages and reset when returning to the main menu.

## Screenshot feature

Press **F12** at any time (menu or gameplay). The current frame, including the UI, is saved as a PNG and a confirmation message appears at the bottom of the screen.

Files are saved to the game's persistent data folder, in a `Screenshots` subfolder. On Windows this is:

`%USERPROFILE%\AppData\LocalLow\<company name>\<product name>\Screenshots`

(The company and product names come from Project Settings > Player.) In the Unity Editor you can use **Tools > Open Screenshot Folder**.

## How to run

**Build:** download the Windows build from the project's itch.io page, extract it, and run the `.exe`.

**From source:**

1. Open the project with Unity 6 (6000.5.9f1 or newer in the same line).
2. Open the `Main Menu` scene and press Play.
3. Make sure the scenes are listed in **File > Build Profiles**, with `Main Menu` first and the stages after it in order.

## Project structure

```
Assets/
  Scripts/      Gameplay and UI scripts
  Editor/       Editor-only tools (UI builders, screenshot folder shortcut)
  Scenes/       Main Menu and the five stage scenes
  Prefabs/      Reusable objects (player, goal, and others)
  Sprites/      Character, trap, and goal art
  Tiles/        Tile assets and palettes
  Audio/        Sound effects and background music
```

### Scripts

| Script | Role |
|---|---|
| `PlayerManager` | Movement, gravity flip, ground check, bounce handling, death sequence, freeze on goal |
| `GameManager` | Restarts the stage, loads the next stage, triggers the results screen |
| `GameSession` | Static data that survives scene loads (time, deaths, finished flag) |
| `GameHUD` | Creates and updates the on-screen stage, time, and death display |
| `GameMenus` | Creates the pause menu and the results screen, handles Esc |
| `ScreenshotManager` | F12 capture and on-screen confirmation |
| `CameraManager` | Moves the camera between screens when the player crosses a boundary |
| `Goal` / `KeyPickup` | Goal trigger with optional lock, key that unlocks it |
| `MainMenu` | Start and Quit buttons |

## Design decisions

- **Velocity-based movement:** the player's horizontal speed is set directly while vertical speed is left to the physics engine, so gravity (including negative gravity after a flip) works naturally.
- **Input is read in `Update`, physics in `FixedUpdate`:** a button press lasts one frame, so a flag stores the press until the next physics step.
- **Collider cast for the ground check:** it uses the real collider shape and the surface normal, and works the same in both gravity directions.
- **Self-creating UI:** the HUD, menus, and screenshot message build themselves from code at startup, so they exist in every scene without per-scene setup.
- **Stage order from build settings:** the next stage is chosen by build index, so adding or removing stages only requires editing the build list.

## Credits

All assets used are free to use. Sound sources are listed here:

Based on the tutorial series "VVVVVV-style 2D action game in Unity"
by Jonny_dayo (https://www.youtube.com/@Jonny_dayo).
Sprites and tiles are from that series.

## Expansion ideas

- More stage mechanics: moving platforms, one-way bounce bars, switches that toggle spikes.
- Checkpoints inside long stages.
- Per-stage best times saved to disk.
- Mobile touch controls and a WebGL build with a browser-based screenshot download.
- Gamepad support through the existing Input Actions asset.
