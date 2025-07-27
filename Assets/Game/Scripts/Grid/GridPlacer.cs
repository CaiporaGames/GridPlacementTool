using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

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

        var prefab = config.prefabs[config.selectedIndex]; // You can also call GetRandomPrefab() here
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

    public async UniTask SaveAsync(ISaveService saveService, SaveType key)
    {
        // Step 1: Load existing save file if it exists
        GridLayeredSaveData allData = await saveService.LoadAsync<GridLayeredSaveData>(key);
        if (allData == null)
            allData = new GridLayeredSaveData();

        // Step 2: Generate this layer's data
        var saveData = new GridPlacerSaveData();

        foreach (var kvp in placedObjects)
        {
            var obj = kvp.Value;
            if (obj == null) continue;
    #if UNITY_EDITOR
            var prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(obj);
    #else
            var prefabSource = obj;
    #endif
            int prefabIndex = System.Array.IndexOf(config.prefabs, prefabSource);
            if (prefabIndex < 0) continue;

            saveData.objects.Add(new PlacedObjectData
            {
                cell = kvp.Key,
                prefabIndex = prefabIndex,
                rotation = obj.transform.rotation.eulerAngles,
                scale = obj.transform.localScale
            });
        }

        // Step 3: Replace or add this layer into the full save
        allData.layers[config.layerName] = saveData;

        // Step 4: Save the full structure back
        await saveService.SaveAsync(key, allData);
    }



    public async UniTask LoadAsync(ISaveService saveService, SaveType key)
    {
        var data = await saveService.LoadAsync<GridLayeredSaveData>(key);
        if (data == null || !data.layers.TryGetValue(config.layerName, out var layerData)) return;

        // Clear old
        foreach (var go in placedObjects.Values)
            DestroyImmediate(go);
        placedObjects.Clear();

        foreach (var entry in layerData.objects)
        {
            var prefab = config.prefabs[entry.prefabIndex];
    #if UNITY_EDITOR
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, transform);
    #else
            var go = Instantiate(prefab, transform);
    #endif
            float height = GetPrefabHeight(prefab);
            go.transform.position = GetWorldFromCell(entry.cell) + Vector3.up * (height / 2f);
            go.transform.rotation = Quaternion.Euler(entry.rotation);
            go.transform.localScale = entry.scale;

            placedObjects[entry.cell] = go;
        }
    }
}