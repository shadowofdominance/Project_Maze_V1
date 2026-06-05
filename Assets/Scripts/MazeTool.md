# MazeTool.cs Explanation

`MazeTool.cs` defines a custom Unity Editor window for controlling a maze generator from inside the Unity Editor. It is wrapped in `#if UNITY_EDITOR`, so this script is only included while working in the editor and is not compiled into the final game build.

## Main Purpose

The tool gives you a menu option at `Window > Maze > Maze Tool`. Opening that menu shows a small editor window where you can assign a `MazeGenerator` object from the scene and then control the maze without manually changing values in the Inspector.

It can:

- Apply difficulty presets.
- Reload or regenerate the current maze.
- Increase the maze size.
- Reset the maze to a specific width and height.
- Mark the scene as changed so Unity knows the edits should be saved.

## Editor Window Setup

The class inherits from `EditorWindow`, which is Unity's base class for custom editor windows.

```csharp
public class MazeTool : EditorWindow
```

The `[MenuItem("Window/Maze/Maze Tool")]` attribute adds the tool to Unity's top menu. When selected, `ShowWindow()` opens or focuses the editor window and gives it the title `Maze Tool`.

## Difficulty Presets

The script defines an internal enum:

```csharp
private enum MazeDifficulty
{
    Easy,
    Medium,
    Hard
}
```

This controls which preset is selected in the editor window. The default values are:

- Easy: `8`
- Medium: `12`
- Hard: `18`

These values are used as square maze dimensions. For example, choosing `Medium` applies a `12 x 12` maze.

The user can also edit the preset sizes directly in the tool window.

## Target MazeGenerator

The field below stores the scene object that this tool controls:

```csharp
private MazeGenerator _mazeGenerator;
```

Inside `OnGUI()`, the tool draws an object field where you assign a `MazeGenerator` from the scene. If no generator is assigned, the window shows a help message and stops drawing the rest of the controls.

This is important because every button depends on calling methods on a real `MazeGenerator` instance.

## OnGUI()

`OnGUI()` is the main drawing method for the editor window. Unity calls it whenever the window needs to render or process input.

The method does three main things:

1. Shows the `MazeGenerator` target field.
2. Stops early if no target is assigned.
3. Draws the difficulty, maze action, and reset sections.

The sections are split into helper methods:

- `DrawDifficultyControls()`
- `DrawSizeControls()`
- `DrawResetControls()`

This keeps the editor UI easier to read and maintain.

## Difficulty Controls

`DrawDifficultyControls()` draws:

- A dropdown for `Easy`, `Medium`, or `Hard`.
- Editable integer fields for each preset size.
- An `Apply Difficulty` button.

The size fields use `Mathf.Max(1, value)` so the maze size can never go below `1`.

When `Apply Difficulty` is clicked, the tool calls:

```csharp
ApplyDifficulty(_difficulty);
```

That method finds the selected preset size and sends it to the maze generator using:

```csharp
_mazeGenerator.SetMazeSize(size, size);
```

## Maze Actions

`DrawSizeControls()` contains the controls for changing or rebuilding the maze.

It has an `Increase Step` field, which controls how much bigger the maze should become when increasing its size. Like the preset sizes, this value is clamped to at least `1`.

The `Reload Maze` button calls:

```csharp
RegenerateMaze();
```

That method calls:

```csharp
_mazeGenerator.GenerateNewMaze();
```

This regenerates the maze layout without changing its current dimensions.

The `Increase Maze Size` button calls:

```csharp
_mazeGenerator.IncreaseMazeSize(_sizeIncreaseStep);
```

This tells the generator to expand the maze by the configured step amount.

## Reset Controls

`DrawResetControls()` lets you enter a custom reset width and height.

The default reset dimensions are:

- Width: `10`
- Height: `10`

Both values are clamped to at least `1`.

When `Reset Maze` is clicked, the tool calls:

```csharp
_mazeGenerator.ResetMaze(_resetWidth, _resetHeight);
```

This resets the maze to the exact dimensions entered in the editor window.

## Undo Support

Before changing the maze generator, the script usually calls:

```csharp
Undo.RecordObject(_mazeGenerator, "Action Name");
```

This registers the current state with Unity's Undo system. Because of this, actions like applying difficulty, increasing maze size, reloading, or resetting can be undone through the editor.

## Saving Editor Changes

After changing the maze generator, the tool calls:

```csharp
MarkGeneratorDirty();
```

That method does two things:

1. Calls `EditorUtility.SetDirty(_mazeGenerator)` so Unity knows the component has changed.
2. If the game is not currently playing, calls `EditorSceneManager.MarkSceneDirty(...)` so Unity knows the scene has unsaved changes.

This helps make sure editor-time maze changes are not silently lost.

## How It Connects To MazeGenerator

`MazeTool.cs` does not generate the maze by itself. Instead, it acts as a user interface for the `MazeGenerator` script.

It relies on these `MazeGenerator` methods:

- `SetMazeSize(width, height)`
- `GenerateNewMaze()`
- `IncreaseMazeSize(step)`
- `ResetMaze(width, height)`

So the actual maze-building logic lives in `MazeGenerator.cs`, while `MazeTool.cs` provides convenient editor buttons and fields for calling that logic.

## Summary

`MazeTool.cs` is a Unity Editor helper tool. It creates a custom window that lets you control a selected `MazeGenerator` from the Unity menu. It supports difficulty presets, maze regeneration, size increases, custom resets, undo support, and scene dirty marking so changes can be saved properly.
