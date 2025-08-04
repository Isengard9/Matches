using UnityEditor;
using UnityEngine;
using MiniGames.Match3.Core;

[CustomEditor(typeof(GridData))]
public class GridDataEditor : Editor
{
    private int selectedCellRow = -1;
    private int selectedCellCol = -1;

    public override void OnInspectorGUI()
    {
        GridData gridData = (GridData)target;

        gridData.GridSize = (GridSize)EditorGUILayout.EnumPopup("Grid Size", gridData.GridSize);
        int size = (int)gridData.GridSize;

        if (gridData.Cells == null || gridData.Cells.GetLength(0) != size)
            gridData.Cells = new CellData[size, size];

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Grid Layout", EditorStyles.boldLabel);

        // Calculate cell size based on inspector width
        float inspectorWidth = EditorGUIUtility.currentViewWidth - 40; // padding for margins
        float cellSize = Mathf.Min(inspectorWidth / size, 15); // maximum 15px
        cellSize = Mathf.Max(cellSize, 10); // minimum 10px

        for (int row = 0; row < size; row++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int col = 0; col < size; col++)
            {
                if (gridData.Cells[row, col] == null)
                    gridData.Cells[row, col] = new CellData();

                Rect rect = GUILayoutUtility.GetRect(cellSize, cellSize);

                // Background color
                Color bgColor = gridData.Cells[row, col].Piece != null ?
                    gridData.Cells[row, col].Piece.Color : Color.white;
                EditorGUI.DrawRect(rect, bgColor);

                // Draw sprite
                if (gridData.Cells[row, col].Piece?.Sprite != null)
                {
                    Rect spriteRect = new Rect(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4);
                    GUI.DrawTexture(spriteRect, gridData.Cells[row, col].Piece.Sprite.texture);
                }

                // Generate unique ID for each cell
                int cellId = row * size + col;

                // Handle click event
                if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                {
                    selectedCellRow = row;
                    selectedCellCol = col;
                    EditorGUIUtility.ShowObjectPicker<PieceSO>(
                        gridData.Cells[row, col].Piece, false, "", cellId);
                    Event.current.Use();
                }

                // Handle object picker result
                if (Event.current.commandName == "ObjectSelectorUpdated" &&
                    EditorGUIUtility.GetObjectPickerControlID() == cellId)
                {
                    if (selectedCellRow == row && selectedCellCol == col)
                    {
                        gridData.Cells[row, col].Piece = EditorGUIUtility.GetObjectPickerObject() as PieceSO;
                        GUI.changed = true;
                    }
                }

                // Draw cell border
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), Color.black);
                EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - 1, rect.width, 1), Color.black);
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), Color.black);
                EditorGUI.DrawRect(new Rect(rect.x + rect.width - 1, rect.y, 1, rect.height), Color.black);
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUI.changed)
            EditorUtility.SetDirty(gridData);
    }
}