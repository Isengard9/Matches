using UnityEditor;
using UnityEngine;
using MiniGames.Match3.Data;

[CustomEditor(typeof(GridData))]
public class GridDataEditor : Editor
{
    private int selectedCellRow = -1;
    private int selectedCellCol = -1;

    public override void OnInspectorGUI()
    {
        // Serialized object'i güncelle
        serializedObject.Update();
        
        GridData gridData = (GridData)target;

        // Grid Size'ı doğrudan göster 
        gridData.GridSize = (GridSize)EditorGUILayout.EnumPopup("Grid Size", gridData.GridSize);
        int size = (int)gridData.GridSize;

        EditorGUILayout.Space(5);
    
        // Grid yaratma butonu
        if (GUILayout.Button("Create Grid"))
        {
            // Undo kaydı oluştur
            Undo.RecordObject(gridData, "Create Grid");
            gridData.Cells = new CellData[size, size];
            // Tüm hücreleri initialize et
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    gridData.Cells[row, col] = new CellData();
                }
            }
            EditorUtility.SetDirty(gridData);
        }

        // Grid hiç yoksa otomatik oluştur
        if (gridData.Cells == null)
        {
            Undo.RecordObject(gridData, "Auto Create Grid");
            gridData.Cells = new CellData[size, size];
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    gridData.Cells[row, col] = new CellData();
                }
            }
            EditorUtility.SetDirty(gridData);
        }

        // Grid boyutu uyuşmuyorsa uyarı göster
        if (gridData.Cells != null && gridData.Cells.GetLength(0) != size)
        {
            EditorGUILayout.HelpBox($"Grid size mismatch. Current grid: {gridData.Cells.GetLength(0)}x{gridData.Cells.GetLength(0)}, Selected: {size}x{size}. Click 'Create Grid' to resize.", MessageType.Warning);
        }

        if (gridData.Cells == null) return; // Grid yoksa devam etme

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Grid Layout", EditorStyles.boldLabel);

        // Mevcut grid boyutunu kullan
        int currentSize = gridData.Cells.GetLength(0);

        // Calculate cell size based on inspector width
        float inspectorWidth = EditorGUIUtility.currentViewWidth - 40;
        float cellSize = Mathf.Min(inspectorWidth / currentSize, 30);
        cellSize = Mathf.Max(cellSize, 30);

        // Grid çizimi
        EditorGUI.BeginChangeCheck();
        
        for (int row = 0; row < currentSize; row++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int col = 0; col < currentSize; col++)
            {
                if (gridData.Cells[row, col] == null)
                {
                    gridData.Cells[row, col] = new CellData();
                }

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
                int cellId = row * currentSize + col;

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
                        // Undo kaydı oluştur
                        Undo.RecordObject(gridData, "Change Cell Piece");
                        gridData.Cells[row, col].Piece = EditorGUIUtility.GetObjectPickerObject() as PieceSO;
                        EditorUtility.SetDirty(gridData);
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

        // Eğer değişiklik olduysa dirty flag'i set et
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(gridData);
        }

        // Serialized object'teki değişiklikleri uygula
        serializedObject.ApplyModifiedProperties();
    }
}