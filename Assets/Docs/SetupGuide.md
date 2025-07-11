# AdventureGame Setup Guide

This document walks through creating a local test scene and the basic assets needed to play with the systems included in this repository. All steps can be followed inside the Unity editor.

## 1. Create Data Assets

Use the custom editor windows under the **Adventure** menu to generate the ScriptableObject databases.

1. **Stats Database**
   - Open `Adventure/Stats Editor`.
   - Click **Create Database** to make `StatsDatabase.asset` in `Assets/Data`.
   - Add entries for every stat your game needs (e.g. HP, Attack, Defense).
2. **Ability Database**
   - Open `Adventure/Ability Editor` and create `AbilityDatabase.asset`.
   - Define abilities with damage, healing and status effect fields.
3. **Class Database**
   - Open `Adventure/Class Editor` to create `ClassDatabase.asset`.
   - Add classes and assign values for the stats from the Stats Database.
4. Optional databases like `RoomDatabase`, `EnemyDatabase` and `ShopDatabase` can be created from their respective editors.

All databases are loaded automatically through `DataManager` when placed in the `Assets/Data` folder.

## 2. Build Prefabs

Prefab examples live under `Assets/Prefabs/Rooms`. Create additional room prefabs by attaching the following components:

1. **RoomBuilder** – generates the floor and walls. Configure room size and tileset path as desired.
2. **RoomPrefab** – references the `Door` components for each side of the room. These will be enabled based on dungeon generation.
3. **Door** (optional) – doors can be locked and animated. Ensure a `Collider` and optional `Animator` are present.

Place finished prefabs in `Assets/Prefabs/Rooms` so the `GameManager` can spawn them.

## 3. Scene Hierarchy

Create a new scene and add the following objects:

1. **GameManager**
   - Add the `GameManager` component.
   - Assign a `DungeonGenerator` instance to the `generator` field.
   - Reference the `BattleManager` and `SessionManager` components.
   - Populate the `roomPrefabs` list with your room prefabs.
2. **DungeonGenerator** – attach the `DungeonGenerator` script. Adjust width, height and other generation settings.
3. **DataManager** – drag your data assets into their fields so runtime systems can access them.
4. **BattleManager**
   - Requires an `ATBManager` component (either on the same object or a child).
   - Handles combat turns and ability execution.
5. **UI Canvas**
   - Add a canvas to the scene and attach `UIManager`.
   - Create child objects for `BattleUI` and `SkillsUI` and connect their serialized fields:
     - `BattleManager` reference.
     - Slider prefab for `BattleUI` and button prefab for `SkillsUI`.

Organising these under an empty "Managers" GameObject keeps the hierarchy tidy.

## 4. Running the Test

Play the scene and call `GameManager.StartNewGame` from another script or through the inspector to generate a dungeon. Use the on‑screen skill buttons during combat. Databases and prefabs can be adjusted without code changes.

## 5. Menu and Lobby Scenes

Create two additional scenes called **MainMenu** and **Lobby**. These use
TextMeshPro for all text elements so ensure **TMP Essentials** are imported from
the package manager before creating UI objects.

1. **MainMenu Scene**
   - Add a `Canvas` with a `MainMenuUI` component.
   - Create buttons for New Game, Load Game, Tutorial and High Scores and assign
     them to the corresponding fields on `MainMenuUI` in the inspector.
   - Reference a `GameSetupUI` object for the new game flow and a
     `SessionManager` for loading saved sessions.

2. **Lobby Scene**
   - Add a `Canvas` with a `LobbyUI` component and optional class selection
     panel.
   - Create refresh, create and start buttons and connect them to the serialized
     fields on `LobbyUI`.
   - Set the lobby item prefab and content container for listing available
     lobbies.

Link the menu's buttons to load the Lobby scene so players can create or join a
session before starting gameplay.

### Game Setup Panel

Before building this UI, import **TMP Essentials** from the package manager so
TextMeshPro components are available. Create a panel containing dropdowns for
**Difficulty** and **Game Type**, plus Confirm and Cancel buttons. Attach the
`GameSetupUI` script and assign your `GameManager` and `LobbyManager` objects in
the inspector.

### Class Selection Panel

Add another panel with a dropdown for classes, a `TMP_Text` field to display the
stats, an image for the class portrait and a Confirm button. Use the
`ClassSelectUI` script, wiring up references to `DataManager` and `GameManager`.

### Lobby Item Prefab

Finally create a prefab consisting of a `Button` with a child `TMP_Text` element.
`LobbyUI` uses this prefab when listing available sessions in the lobby.

Add **MainMenu**, **Lobby** and **SampleScene** to **File → Build Settings** in
that order before testing.

