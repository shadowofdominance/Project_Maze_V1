#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Keeps this tool out of player builds and exposes maze controls directly in the Unity Editor.
public class MazeTool : EditorWindow
{
    private enum MazeDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    private MazeGenerator _mazeGenerator;
    private MazeDifficulty _difficulty = MazeDifficulty.Medium;
    private int _easySize = 8;
    private int _mediumSize = 12;
    private int _hardSize = 18;
    private int _sizeIncreaseStep = 2;
    private int _resetWidth = 10;
    private int _resetHeight = 10;

    [MenuItem("Window/Maze/Maze Tool")]
    public static void ShowWindow()
    {
        // Reuses the same editor window instance and gives it a readable title in the Window menu.
        GetWindow<MazeTool>("Maze Tool");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Maze Generator", EditorStyles.boldLabel);
        _mazeGenerator = (MazeGenerator)EditorGUILayout.ObjectField("Target", _mazeGenerator, typeof(MazeGenerator), true);

        if (_mazeGenerator == null)
        {
            // The rest of the tool depends on a scene instance to mutate, so stop here until one is assigned.
            EditorGUILayout.HelpBox("Assign a MazeGenerator from the scene to control the maze.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        DrawDifficultyControls();

        EditorGUILayout.Space();
        DrawSizeControls();

        EditorGUILayout.Space();
        DrawResetControls();
    }

    private void DrawDifficultyControls()
    {
        EditorGUILayout.LabelField("Difficulty", EditorStyles.boldLabel);

        _difficulty = (MazeDifficulty)EditorGUILayout.EnumPopup("Preset", _difficulty);
        _easySize = Mathf.Max(1, EditorGUILayout.IntField("Easy Size", _easySize));
        _mediumSize = Mathf.Max(1, EditorGUILayout.IntField("Medium Size", _mediumSize));
        _hardSize = Mathf.Max(1, EditorGUILayout.IntField("Hard Size", _hardSize));

        if (GUILayout.Button("Apply Difficulty"))
        {
            // Applies the currently selected preset by converting it into a square maze size.
            ApplyDifficulty(_difficulty);
        }
    }

    private void DrawSizeControls()
    {
        EditorGUILayout.LabelField("Maze Actions", EditorStyles.boldLabel);

        _sizeIncreaseStep = Mathf.Max(1, EditorGUILayout.IntField("Increase Step", _sizeIncreaseStep));

        if (GUILayout.Button("Reload Maze"))
        {
            RegenerateMaze();
        }

        if (GUILayout.Button("Increase Maze Size"))
        {
            // Register the state before mutating so the editor undo stack can restore the previous maze settings.
            Undo.RecordObject(_mazeGenerator, "Increase Maze Size");
            _mazeGenerator.IncreaseMazeSize(_sizeIncreaseStep);
            MarkGeneratorDirty();
        }
    }

    private void DrawResetControls()
    {
        EditorGUILayout.LabelField("Reset", EditorStyles.boldLabel);

        _resetWidth = Mathf.Max(1, EditorGUILayout.IntField("Reset Width", _resetWidth));
        _resetHeight = Mathf.Max(1, EditorGUILayout.IntField("Reset Height", _resetHeight));

        if (GUILayout.Button("Reset Maze"))
        {
            // Records the change so resetting dimensions can also be undone from the editor.
            Undo.RecordObject(_mazeGenerator, "Reset Maze");
            _mazeGenerator.ResetMaze(_resetWidth, _resetHeight);
            MarkGeneratorDirty();
        }
    }

    private void ApplyDifficulty(MazeDifficulty difficulty)
    {
        Undo.RecordObject(_mazeGenerator, "Set Maze Difficulty");
        int size = GetMazeSize(difficulty);
        // Difficulty presets map to a single size and are applied as a square maze.
        _mazeGenerator.SetMazeSize(size, size);
        MarkGeneratorDirty();
    }

    private void RegenerateMaze()
    {
        Undo.RecordObject(_mazeGenerator, "Reload Maze");
        _mazeGenerator.GenerateNewMaze();
        MarkGeneratorDirty();
    }

    private int GetMazeSize(MazeDifficulty difficulty)
    {
        switch (difficulty)
        {
            case MazeDifficulty.Easy:
                return _easySize;
            case MazeDifficulty.Medium:
                return _mediumSize;
            case MazeDifficulty.Hard:
                return _hardSize;
            default:
                return _mediumSize;
        }
    }

    private void MarkGeneratorDirty()
    {
        // Marks the component as modified so Unity saves the serialized changes made by this editor tool.
        EditorUtility.SetDirty(_mazeGenerator);

        if (!Application.isPlaying)
        {
            // In edit mode, also flag the scene itself so the user is prompted to save the maze changes.
            EditorSceneManager.MarkSceneDirty(_mazeGenerator.gameObject.scene);
        }
    }
}
#endif
