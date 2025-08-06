using UnityEngine;
using UnityEditor;
using System.IO;
using MiniGames.Match3.Data;
using MiniGames.Match3.Core;
using Core;
using MiniGames.Data;

namespace MiniGames.Match3.Editor
{
    public class Match3LevelEditorWindow : EditorWindow
    {
        private GridSize selectedGridSize = GridSize.Small;
        private GridDataSO currentGridData;
        private PieceSO[,] gridPieces;
        private PieceSO[] availablePieces;

        private int maxSwapAttempts = 10;
        private int targetScore = 20;
        private int pieceScore = 1;
        private int bombPieceScore = 3;
        private int rowColumnPieceScore = 2;

        private Vector2 scrollPosition;
        private string levelName = "Match3_1";

        // Piece selection popup variables
        private bool showPieceSelection = false;
        private int selectedGridX = -1;
        private int selectedGridY = -1;
        private Vector2 pieceScrollPosition;

        [MenuItem("Matches/Match3 Level Creator")]
        public static void ShowWindow()
        {
            GetWindow<Match3LevelEditorWindow>("Match3 Level Editor");
        }

        private void OnEnable()
        {
            LoadAvailablePieces();
            CreateNewGrid();
            SetNextLevelName();
        }

        private void SetNextLevelName()
        {
            string basePath = "Assets/Resources/Data/Level";

            if (!AssetDatabase.IsValidFolder(basePath))
            {
                AssetDatabase.CreateFolder("Assets/Resources/Data", "Level");
            }

            int nextNumber = 1;
            string[] existingFolders = AssetDatabase.GetSubFolders(basePath);

            foreach (string folderPath in existingFolders)
            {
                string folderName = Path.GetFileName(folderPath);
                if (folderName.StartsWith("Match3_"))
                {
                    string numberPart = folderName.Substring(7); // Get part after "Match3_"
                    if (int.TryParse(numberPart, out int number))
                    {
                        if (number >= nextNumber)
                        {
                            nextNumber = number + 1;
                        }
                    }
                }
            }

            levelName = $"Match3_{nextNumber}";
        }

        private void LoadAvailablePieces()
        {
            availablePieces = Resources.LoadAll<PieceSO>("Data/Match3Objects");
        }

        private void OnGUI()
        {
            // Set background color
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark gray

            // Background box for entire window
            GUI.Box(new Rect(0, 0, position.width, position.height), "");

            GUI.backgroundColor = originalColor;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Match3 Level Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawGridSizeSelection();
            DrawLevelSettings();
            DrawGrid();
            DrawCreateButton();

            EditorGUILayout.EndScrollView();
        }

        private void DrawGridSizeSelection()
        {
            EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);

            GridSize newSize = (GridSize)EditorGUILayout.EnumPopup("Grid Size", selectedGridSize);
            if (newSize != selectedGridSize)
            {
                selectedGridSize = newSize;
                CreateNewGrid();
                SetNextLevelName(); // Update name when grid size changes
            }

            EditorGUILayout.Space();
        }

        private void DrawLevelSettings()
        {
            EditorGUILayout.LabelField("Level Settings", EditorStyles.boldLabel);

            levelName = EditorGUILayout.TextField("Level Name", levelName);
            maxSwapAttempts = EditorGUILayout.IntField("Max Swap Attempts", maxSwapAttempts);
            targetScore = EditorGUILayout.IntField("Target Score", targetScore);

            EditorGUILayout.LabelField("Score Settings", EditorStyles.boldLabel);
            pieceScore = EditorGUILayout.IntField("Piece Score", pieceScore);
            bombPieceScore = EditorGUILayout.IntField("Bomb Piece Score", bombPieceScore);
            rowColumnPieceScore = EditorGUILayout.IntField("Row/Column Piece Score", rowColumnPieceScore);

            EditorGUILayout.Space();
        }

        private void DrawGrid()
        {
            EditorGUILayout.LabelField("Grid Layout", EditorStyles.boldLabel);

            int gridSize = (int)selectedGridSize;

            for (int y = gridSize - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < gridSize; x++)
                {
                    DrawGridButton(x, y);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
        }

        private void DrawGridButton(int x, int y)
        {
            PieceSO piece = gridPieces[x, y];

            if (piece != null)
            {
                // Draw button with sprite and color if piece exists
                GUI.backgroundColor = piece.Color;

                GUIContent buttonContent = new GUIContent();
                if (piece.Sprite != null)
                {
                    buttonContent.image = piece.Sprite.texture;
                    buttonContent.text = "";
                }
                else
                {
                    buttonContent.text = piece.name;
                }

                if (GUILayout.Button(buttonContent, GUILayout.Width(60), GUILayout.Height(60)))
                {
                    OpenPieceSelection(x, y);
                }
            }
            else
            {
                // Empty button
                GUI.backgroundColor = Color.gray;
                if (GUILayout.Button("Empty", GUILayout.Width(60), GUILayout.Height(60)))
                {
                    OpenPieceSelection(x, y);
                }
            }

            GUI.backgroundColor = Color.white;
        }

        private void OpenPieceSelection(int x, int y)
        {
            PieceSelectionWindow.ShowWindow(availablePieces, x, y, (selectedPiece) =>
            {
                gridPieces[x, y] = selectedPiece;
                Repaint();
            });
        }

        private void DrawCreateButton()
        {
            if (GUILayout.Button("Create Match3 Level", GUILayout.Height(40)))
            {
                CreateLevel();
            }
        }

        private void CreateNewGrid()
        {
            int gridSize = (int)selectedGridSize;
            gridPieces = new PieceSO[gridSize, gridSize];

            currentGridData = ScriptableObject.CreateInstance<GridDataSO>();
        }

        private void CreateLevel()
        {
            if (string.IsNullOrEmpty(levelName))
            {
                EditorUtility.DisplayDialog("Error", "Level name cannot be empty!", "OK");
                return;
            }

            // Create folder
            string folderPath = $"Assets/Resources/Data/Level/{levelName}";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/Resources/Data/Level", levelName);
            }

            // Create GridDataSO
            CreateGridData(folderPath);

            // Create ScoreData
            Match3ScoreData scoreData = CreateScoreData(folderPath);

            // Create Match3DataSO
            Match3DataSO match3Data = CreateMatch3Data(folderPath, scoreData);

            // Create LevelDataSO
            CreateLevelData(folderPath, match3Data);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", $"Level '{levelName}' created successfully!", "OK");
            
            // Set next level name for the next creation
            SetNextLevelName();
        }

        private void CreateGridData(string folderPath)
        {
            currentGridData.GridSize = selectedGridSize;

            int gridSize = (int)selectedGridSize;
            CellData[,] cells = new CellData[gridSize, gridSize];

            // Transfer data from gridPieces to cells array
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    cells[x, y] = new CellData() { Piece = gridPieces[x, y] };
                }
            }

            // Assign to Cells property (this will automatically set serializableCells)
            currentGridData.Cells = cells;

            AssetDatabase.CreateAsset(currentGridData, $"{folderPath}/{levelName}_GridData.asset");
        }

        private Match3ScoreData CreateScoreData(string folderPath)
        {
            Match3ScoreData scoreData = ScriptableObject.CreateInstance<Match3ScoreData>();
            scoreData.defaultPieceScore = pieceScore;
            scoreData.bombPieceScore = bombPieceScore;
            scoreData.rowColumnPieceScore = rowColumnPieceScore;

            AssetDatabase.CreateAsset(scoreData, $"{folderPath}/{levelName}_ScoreData.asset");
            return scoreData;
        }

        private Match3DataSO CreateMatch3Data(string folderPath, Match3ScoreData scoreData)
        {
            Match3DataSO match3Data = ScriptableObject.CreateInstance<Match3DataSO>();
            match3Data.girdDataSo = currentGridData;
            match3Data.scoreData = scoreData;
            match3Data.maxSwapAttempts = maxSwapAttempts;
            match3Data.TargetScore = targetScore;
            match3Data.LevelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Match3/Match3Base.prefab");

            // Assign CameraData
            string cameraPath = $"Config/Match3_{(int)selectedGridSize}X{(int)selectedGridSize}_Camera.asset";
            var cameraData = AssetDatabase.LoadAssetAtPath<CameraDataSO>(cameraPath);
            if (cameraData != null)
            {
                // Assign camera data if the field exists in Match3DataSO
                 match3Data.CameraData= cameraData;
                Debug.Log($"Camera data loaded successfully: {cameraPath}");
            }
            else
            {
                Debug.LogWarning($"Camera data not found at path: {cameraPath}");
            }

            AssetDatabase.CreateAsset(match3Data, $"{folderPath}/{levelName}_Data.asset");
            return match3Data;
        }

        private void CreateLevelData(string folderPath, Match3DataSO match3Data)
        {
            LevelDataSO levelData = ScriptableObject.CreateInstance<LevelDataSO>();
            levelData.Level = match3Data;

            AssetDatabase.CreateAsset(levelData, $"{folderPath}/{levelName}_LevelData.asset");
        }
    }
}