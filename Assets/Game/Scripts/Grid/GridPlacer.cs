using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class GridPlacer : MonoBehaviour
{
    [SerializeField] private SOGridPlacerConfig config;
    public SOGridPlacerConfig Config => config;

    [SerializeField] private bool showGrid = true;
    public bool ShowGrid => showGrid;

    private Dictionary<Vector3Int, GameObject> placedObjects = new();
    private GameObject prefab;

    public bool HasObjectAt(Vector3Int cell)
    {
        return placedObjects.ContainsKey(cell);
    }

    public void Place(Vector3 worldPos)
    {
        if (config == null)
        {
            Debug.LogError("Config is null! Make sure SO is assigned.");
            return;
        }

        Vector3Int cell = WorldToGrid(worldPos);

        if (placedObjects.ContainsKey(cell))
        {
            Debug.Log($"Cell {cell} already has an object: {placedObjects[cell].name}");
            return;
        }

#if UNITY_EDITOR
        // Only access GridPlacerEditorState in editor
        prefab = config.GetSelectedPrefab();
#else
        prefab = config.GetRandomPrefab(); // fallback for runtime safety
#endif

        if (!prefab)
        {
            Debug.LogWarning("No prefab returned by GetRandomPrefab(). Check your SO configuration.");
            return;
        }

#if UNITY_EDITOR
        var go = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, transform);
#else
        var go = Instantiate(prefab, transform);
#endif

        if (go == null)
        {
            Debug.LogError("Prefab failed to instantiate. Check if prefab is valid.");
            return;
        }

        float height = GetPrefabHeight(prefab);
        go.transform.position = GridToWorld(cell) + Vector3.up * (height / 2f);

        go.transform.rotation = Quaternion.Euler(0, config.RandomYRotation ? Random.Range(0, 360f) : 0, 0);
        go.transform.localScale = Vector3.one * Random.Range(config.ScaleMin, config.ScaleMax);

        placedObjects[cell] = go;
    }

#if UNITY_EDITOR
    private GameObject GetSelectedPrefabInEditor()
    {
        // Try to get the selected prefab from GridPlacerEditorState
        // This will work if GridPlacerEditorState exists, otherwise fall back to random
        try
        {
            var editorStateType = System.Type.GetType("GridPlacerEditorState");
        Debug.Log("000000000");

            if (editorStateType != null)
            {
        Debug.Log("11111");

                var getSelectedPrefabMethod = editorStateType.GetMethod("GetSelectedPrefab",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                if (getSelectedPrefabMethod != null)
                {
        Debug.Log("000000000");

                    return (GameObject)getSelectedPrefabMethod.Invoke(null, new object[] { config });
                }
            }
        }
        catch (System.Exception)
        {
            // If  or fails, fall back to random
            Debug.Log($"GridPlacerEditorState doesn't exist!");
        }
        // Fallback to random selection
        return config.GetRandomPrefab();
    }
#endif

    public Vector3Int GetCellFromWorld(Vector3 worldPos)
    {
        return Vector3Int.RoundToInt(worldPos / config.cellSize);
    }

    public Vector3 GetWorldFromCell(Vector3Int cell)
    {
        return (Vector3)cell * config.cellSize + Vector3.up * config.placementYOffset;
    }

    public void Erase(Vector3 worldPos)
    {
        Vector3Int cell = WorldToGrid(worldPos);
        if (!placedObjects.TryGetValue(cell, out var go)) return;

#if UNITY_EDITOR
        UnityEditor.Undo.DestroyObjectImmediate(go);
#else
        DestroyImmediate(go);
#endif
        placedObjects.Remove(cell);
    }

    private Vector3Int WorldToGrid(Vector3 pos)
    {
        pos = pos + new Vector3(0, 0, 0);
        return Vector3Int.RoundToInt(pos / config.cellSize);
    }

    private Vector3 GridToWorld(Vector3Int cell)
    {
        return (Vector3)cell * config.cellSize;
    }

#if UNITY_EDITOR
    private GameObject _ghostInstance;

    public void ShowGhostPreview(Vector3Int cell)
    {
        if (config.prefabs == null || config.prefabs.Length == 0) return;

        var prefab = config.prefabs[0]; // You can also call GetRandomPrefab() here
        if (!prefab) return;

        // Create the ghost if it doesn't exist
        if (_ghostInstance == null)
        {
            _ghostInstance = UnityEditor.PrefabUtility.InstantiatePrefab(prefab, transform) as GameObject;
            _ghostInstance.name = "[GhostPreview]";
        }

        SetGhostAppearance(_ghostInstance, placedObjects.ContainsKey(cell));
        float height = GetPrefabHeight(prefab);
        _ghostInstance.transform.position = GetWorldFromCell(cell) + Vector3.up * (height / 2f);

        _ghostInstance.transform.rotation = Quaternion.Euler(0, config.RandomYRotation ? 0 : 0, 0);
        _ghostInstance.transform.localScale = Vector3.one * config.ScaleMin;
    }

    public void HideGhostPreview()
    {
        if (_ghostInstance != null)
        {
            UnityEngine.Object.DestroyImmediate(_ghostInstance);
            _ghostInstance = null;
        }
    }

    private float GetPrefabHeight(GameObject prefab)
    {
        Renderer renderer = prefab.GetComponentInChildren<Renderer>();
        if (renderer != null)
            return renderer.bounds.size.y;
        
        return 1f; // fallback if no renderer
    }

    private void SetGhostAppearance(GameObject go, bool isOccupied)
    {
        foreach (var renderer in go.GetComponentsInChildren<Renderer>())
        {
            var ghostMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            ghostMaterial.color = isOccupied ? new Color(1, 0, 0, 0.3f) : new Color(1, 1, 1, 0.3f);

            renderer.sharedMaterial = ghostMaterial;
        }

        foreach (var collider in go.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }
    }
#endif
}