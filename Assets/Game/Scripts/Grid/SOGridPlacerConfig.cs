using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects / Grid Placer/ Config", fileName = "Grid Configuration")]
public class SOGridPlacerConfig : ScriptableObject
{
    public GameObject[] prefabs;
    public float cellSize = 1f;
    public bool RandomYRotation = true;
    public float ScaleMin = 1f;
    public float ScaleMax = 1f;
    [Header("Placement Settings")]
    public float placementYOffset = 0.5f;


    public GameObject GetRandomPrefab()
    {
        if (prefabs == null || prefabs.Length == 0) return null;
        return prefabs[Random.Range(0, prefabs.Length)];
    }
}
