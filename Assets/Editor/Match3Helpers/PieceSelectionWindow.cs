using MiniGames.Match3.Data;
using UnityEditor;
using UnityEngine;

namespace MiniGames.Match3.Editor
{
    public class PieceSelectionWindow : EditorWindow
{
    private PieceSO[] availablePieces;
    private Vector2 scrollPosition;
    private System.Action<PieceSO> onPieceSelected;
    private int gridX, gridY;
    private int gridColumns = 5;

    public static void ShowWindow(PieceSO[] pieces, int x, int y, System.Action<PieceSO> onSelected)
    {
        PieceSelectionWindow window = GetWindow<PieceSelectionWindow>("Select Piece");
        window.LoadAllPieces();
        window.gridX = x;
        window.gridY = y;
        window.onPieceSelected = onSelected;
        window.minSize = new Vector2(400, 500);
        window.maxSize = new Vector2(600, 800);
        window.ShowUtility();
    }

    private void LoadAllPieces()
    {
        availablePieces = Resources.LoadAll<PieceSO>("Data/Match3Objects");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField($"Select Piece for Grid ({gridX}, {gridY})", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Asset seçme penceresi
        EditorGUILayout.LabelField("Select from Assets:", EditorStyles.boldLabel);
        PieceSO selectedAsset = (PieceSO)EditorGUILayout.ObjectField("Choose Piece", null, typeof(PieceSO), false);

        if (selectedAsset != null)
        {
            onPieceSelected?.Invoke(selectedAsset);
            Close();
        }

        EditorGUILayout.Space();

        // Empty seçeneği
        if (GUILayout.Button("Set Empty", GUILayout.Height(30)))
        {
            onPieceSelected?.Invoke(null);
            Close();
        }

        EditorGUILayout.Space();

        // Grid şeklinde piece'ler
        EditorGUILayout.LabelField("Available Pieces:", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        if (availablePieces != null && availablePieces.Length > 0)
        {
            int rows = Mathf.CeilToInt((float)availablePieces.Length / gridColumns);
            
            for (int row = 0; row < rows; row++)
            {
                EditorGUILayout.BeginHorizontal();
                
                for (int col = 0; col < gridColumns; col++)
                {
                    int index = row * gridColumns + col;
                    
                    if (index < availablePieces.Length)
                    {
                        PieceSO piece = availablePieces[index];
                        DrawPieceButton(piece);
                    }
                    else
                    {
                        // Boş alan bırak
                        GUILayout.Space(70);
                    }
                }
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);
            }
        }
        else
        {
            EditorGUILayout.LabelField("No pieces found in Resources/Data/Match3Objects", EditorStyles.helpBox);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawPieceButton(PieceSO piece)
    {
        Color originalBgColor = GUI.backgroundColor;
        GUI.backgroundColor = piece.Color;

        GUIContent content = new GUIContent();
        if (piece.Sprite != null)
        {
            content.image = piece.Sprite.texture;
            content.tooltip = piece.name;
        }
        else
        {
            content.text = piece.name.Substring(0, Mathf.Min(piece.name.Length, 6));
            content.tooltip = piece.name;
        }

        if (GUILayout.Button(content, GUILayout.Width(65), GUILayout.Height(65)))
        {
            onPieceSelected?.Invoke(piece);
            Close();
        }

        GUI.backgroundColor = originalBgColor;
    }
}
}