# AdventureBot2

AdventureBot2 is a prototype action-adventure game where players take control of an experimental robot exploring a mysterious island. The project is currently in early stages, focusing on core mechanics such as exploration, puzzle solving, and robot upgrades.

## Game Concept
- **Genre:** 3D action-adventure with light puzzle elements.
- **Player Role:** Control a curious robot stranded on an island, uncovering secrets and collecting upgrade parts.
- **Goals:** Explore environments, gather resources, solve environmental puzzles, and unlock new abilities to progress.

## Planned Features
- Open-world island with distinct regions to explore.
- Modular upgrade system for the robot, providing new traversal and combat abilities.
- Quest system with NPCs that guide the player's progression.
- Inventory and crafting mechanics for building tools or upgrades.
- Environmental puzzles that require creative use of acquired abilities.

## Unity Setup
1. Install **Unity Hub** and Unity **2022.3 LTS** (or newer).
2. Clone this repository.
3. Open Unity Hub, select **Open**, and choose the project folder.
4. Allow Unity to import all assets and compile scripts. The first load may take several minutes.

## Build & Play-Testing
- To play-test, press **Play** in the Unity Editor after the project loads. Make sure no compile errors are reported in the Console.
- To create a standalone build:
  1. Go to **File > Build Settings**.
  2. Select your target platform (Windows, macOS, etc.) and click **Build**.
  3. Choose an output directory and wait for Unity to finish building.
- Regularly play-test and iterate on mechanics as new features are added.


## Repository Structure
This repo follows a minimal Unity layout. Key folders include:

- `Assets/` - Main location for game content and scripts.
  - `Scripts/Player` contains `PlayerController.cs`.
  - `Scripts/Systems` holds placeholder systems such as `InventorySystem`,
    `QuestSystem`, `UpgradeSystem`, and `PuzzleManager`.
- `.gitignore` excludes Unity-generated files like `Library/` and build output.

The current scripts are empty templates to compile in Unity. They will be
expanded as gameplay features are implemented.
