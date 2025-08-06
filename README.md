# Project Matches

Welcome to the official repository for Project Matches! This document provides a comprehensive guide to the project's architecture, development workflows, and core mechanics.

## Table of Contents
- [Project Overview](#project-overview)
- [Core Architecture](#core-architecture)
  - [Manager System](#manager-system)
  - [Event System](#event-system)
- [Level Creation Workflow](#level-creation-workflow)
- [Mini-Game Mechanics](#mini-game-mechanics)
  - [Match-3](#match-3)
  - [Runner](#runner)
- [Getting Started](#getting-started)

## Project Overview

Project Matches is a versatile Unity-based game framework designed to support various mini-games, starting with a Match-3 and a Runner game. The architecture is highly modular, making it easy to extend and add new features or game modes.

## Core Architecture

The project is built upon a robust and modular architecture to ensure scalability and maintainability.

### Manager System

The core of the framework is the **Manager System**. A central `ManagerContainer` is responsible for initializing and providing access to all other managers. This ensures a clean separation of concerns and a single point of access for core functionalities.

The main managers are:
- **`EventManager`**: Handles the communication between different parts of the game using a publish-subscribe pattern.
- **`LevelManager`**: Manages the lifecycle of levels, including loading, starting, and finishing them.
- **`UIManager`**: Controls the user interface, including screens, pop-ups, and HUD elements.
- **`SaveLoadManager`**: Manages game data persistence, saving and loading player progress.

### Level System

The `LevelManager` orchestrates the entire level flow, from loading data to managing player progression.

-   **Level Container**: It loads all level data from a `LevelsContainerSO` asset found in the `Resources` folder. This asset acts as a central repository for all levels in the game.
-   **Lifecycle Management**: It handles the complete lifecycle of a level. It interacts with the `SaveLoadManager` to get the `currentLevelIndex` and load the appropriate level. It provides methods like `StartLevel`, `EndLevel`, and `LoadNextLevel` to control the game flow.
-   **Event Publishing**: It is a key event publisher that informs other systems about the level's state:
    -   `LevelLoadedEvent`: Published after a level's data is loaded. The `UIManager` listens to this to show the start panel.
    -   `LevelStartedEvent`: Published when the gameplay for the current level officially begins.
    -   `LevelEndedEvent`: Published when the level is successfully completed.
    -   `LevelFailedEvent`: Published when the player fails the level.
    -   `LevelUnloadedEvent`: Published when a level's resources are unloaded before loading the next one.

### UI System

The `UIManager` controls the entire UI flow of the game. It operates as a state machine, managing different UI panels based on the current game state.

-   **Panel Controllers:** The manager holds references to various panel controllers, such as `StartPanelController`, `EndPanelController`, and game-specific panels (e.g., `Match3InGamePanelController`).
-   **State-Driven:** It uses a `UIState` enum (e.g., `StartPanel`, `EndPanel`, `InGame`) to determine which panel should be active.
-   **Event-Driven:** The `UIManager` subscribes to core game events from the `EventManager` (`LevelLoadedEvent`, `LevelEndedEvent`, `LevelFailedEvent`). When an event is published, the `UIManager` automatically transitions to the appropriate UI state, showing and hiding panels as needed. For example, when a `LevelLoadedEvent` is received, it shows the start panel. When a `LevelEndedEvent` is received, it displays the end-game panel.

### Save/Load System

The `SaveLoadManager` is responsible for all data persistence in the game. It automatically loads user data when the game starts and provides methods to save it.

-   **Data Model:** Player progress is stored in the `UserSaveData` class.
-   **Storage:** The data is serialized into a JSON file and stored in the device's persistent data path (`Application.persistentDataPath`). This ensures that player data is not lost between sessions.

### Event System

The `EventManager` allows for decoupled communication between different systems.

**Subscribing to an Event:**

To listen for an event, you need to subscribe to it using its type. The `EventManager` will call your provided method whenever the event is published.

```csharp
// Example of subscribing to a 'GameStartEvent'
private void OnEnable()
{
    ManagerContainer.Instance.GetManager<EventManager>().Subscribe<GameStartEvent>(OnGameStarted);
}

private void OnDisable()
{
    ManagerContainer.Instance.GetManager<EventManager>().Unsubscribe<GameStartEvent>(OnGameStarted);
}

private void OnGameStarted(GameStartEvent gameEvent)
{
    // Handle the event
    Debug.Log("Game has started!");
}
```

**Publishing an Event:**

To publish an event, simply create an instance of your event class and pass it to the `Publish` method.

```csharp
// Example of publishing a 'GameStartEvent'
var gameStartEvent = new GameStartEvent();
ManagerContainer.Instance.GetManager<EventManager>().Publish(gameStartEvent);
```

## Level Creation Workflow

Levels are created using custom Editor windows, which streamline the design process.

1.  **Open the Level Editor:** Navigate to `Matches > Match3 Level Creator` or `Matches > Runner Level Creator` in the Unity menu, depending on the desired game type.
2.  **Design the Level:** Use the editor's interface to define the level's properties, such as the grid layout for a Match-3 game or the track segments for a Runner game.
3.  **Save the Level:** The editor will serialize the level data into a `ScriptableObject` asset.
4.  **Add to Level Container:** To make the level accessible in the game, you must add the newly created level asset to the `LevelContainer` `ScriptableObject`. This container holds a list of all available levels. The `LevelManager` uses this container to load levels by their index.

## Mini-Game Mechanics

The framework is designed to support different mini-games.

### Match-3

-   **Grid System:** The Match-3 game uses a grid system defined in the level data. Each cell in the grid can contain a game piece. The `GridManager` (or a similar class) is responsible for managing the state of the grid.
-   **Input Handling:** Input is typically handled via the `InputManager`, which detects player swipes or clicks on the grid. These inputs are then translated into game actions, like swapping pieces.
-   **Matching Logic:** After a player's move, the `GridManager` scans the grid for horizontal and vertical matches of three or more identical pieces. When a match is identified, the pieces are removed from the grid. The pieces above then fall down to fill the empty spaces, which can potentially create new matches (cascades). This process repeats until no new matches are formed.

### Runner

-   **Game Progression:** The Runner game progresses by sequentially spawning track segments. The `LevelManager` loads the level data, which defines the order and type of segments to be used. Player movement and game speed are managed by a dedicated `PlayerController` and `GameManager` for the runner mode.

## Third-Party Assets

- **Free Casual Buttons Pack**: This project includes the [Free Casual Buttons Pack](https://assetstore.unity.com/packages/2d/gui/free-casual-buttons-pack-307406) from the Unity Asset Store to provide placeholder UI elements for visualization purposes.

## Getting Started

1.  Clone the repository to your local machine.
2.  Open the project in Unity Hub.
3.  The main scene (e.g., `MainMenu` or `Core`) should be located in the `Assets/Scenes` folder. Open it to get started.
4.  Press Play in the Unity Editor to run the game.

## Known Issues and Future Improvements

-   **Empty Level Container:** The system currently does not gracefully handle cases where the `LevelsContainer` is empty. It fails silently without providing a clear error or a fallback mechanism.
-   **Rendering Issues in Runner Game:** The project was initially configured for a 2D rendering pipeline. As a result, the 3D Runner mini-game has some lighting and material rendering artifacts that need to be addressed by adjusting the render pipeline settings.
-   **Lack of Unit Tests:** The project currently lacks unit tests. While the architecture is modular, some components are tightly coupled with Unity's lifecycle, making standard unit testing challenging. Future work could involve refactoring critical parts for better testability.
-   **Animation System:** Animations are primarily handled using Coroutines. Migrating to a dedicated tweening library like **DoTween** could result in cleaner, more performant, and more maintainable animation code.
