using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridPlacer))]
public class GridPlacerEditor : Editor
{
    private GridPlacer _placer;
    private bool _isPlacing = true;

    private void OnEnable()
    {
        _placer = (GridPlacer)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor Tools", EditorStyles.boldLabel);

        _isPlacing = GUILayout.Toggle(_isPlacing, _isPlacing ? "Placing Mode (Left Click)" : "Erasing Mode (Right Click)", "Button");

        if (GUILayout.Button("Clear All"))
        {
            Undo.RegisterFullObjectHierarchyUndo(_placer.gameObject, "Clear Grid");
            foreach (Transform child in _placer.transform)
                DestroyImmediate(child.gameObject);
        }
    }

    private void OnSceneGUI()
    {
        Event currentEvent = Event.current;

        // Required for click handling (optional if already works)
        if (currentEvent.type == UnityEngine.EventType.Layout)
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        // Raycast to mouse hit point
        Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 500f))
        {
            // Draw preview cube at snapped grid position
            Vector3Int cell = _placer.GetCellFromWorld(hit.point);
            Vector3 worldPos = _placer.GetWorldFromCell(cell);

            if (_isPlacing)
            {
                // Show ghost preview
                _placer.ShowGhostPreview(cell);
            }
            else
            {
                _placer.HideGhostPreview();
                Handles.color = new Color(1f, 0.5f, 0f, 0.4f); // erase mode fallback
                Handles.DrawWireCube(worldPos, Vector3.one * _placer.Config.cellSize);
            }

            if (currentEvent.type == UnityEngine.EventType.MouseDown && (currentEvent.button == 0 || currentEvent.button == 1))
            {
                Undo.RegisterFullObjectHierarchyUndo(_placer.gameObject, "Grid Place/Erase");

                if (_isPlacing && currentEvent.button == 0)
                    _placer.Place(hit.point);
                else if (!_isPlacing && currentEvent.button == 1)
                    _placer.Erase(hit.point);

                currentEvent.Use();
            }

            if (_placer.ShowGrid)
            {
                DrawGrid(cell);
            }

        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor Tools", EditorStyles.boldLabel);

        // Show grid toggle button
        bool newShowGrid = GUILayout.Toggle(_placer.ShowGrid, _placer.ShowGrid ? "Hide Grid" : "Show Grid", "Button");
        if (newShowGrid != _placer.ShowGrid)
        {
            Undo.RecordObject(_placer, "Toggle Grid Preview");
            typeof(GridPlacer).GetField("showGrid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_placer, newShowGrid);
            EditorUtility.SetDirty(_placer);
        }

    }

    private void DrawGrid(Vector3Int centerCell)
    {
        int size = 3; // grid radius (adjust if needed)
        float cellSize = _placer.Config.cellSize;

        Handles.color = new Color(0.3f, 0.7f, 1f, 0.15f);

        for (int x = -size; x <= size; x++)
        {
            for (int z = -size; z <= size; z++)
            {
                Vector3Int cell = centerCell + new Vector3Int(x, 0, z);
                Vector3 world = _placer.GetWorldFromCell(cell);
                Handles.DrawWireCube(world, Vector3.one * cellSize);
            }
        }
    }


    private void OnDisable()
    {
        _placer.HideGhostPreview();
    }
}
