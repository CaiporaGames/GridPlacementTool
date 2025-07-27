using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public static class GridPlacerEditorState
{
    public static GameObject selectedPrefab = null;
    public static bool useRandom = true;

    public static GameObject GetSelectedPrefab(SOGridPlacerConfig config)
    {
        if (config == null || config.prefabs == null || config.prefabs.Length == 0)
            return null;

        if (useRandom)
        {
            selectedPrefab = config.GetRandomPrefab();
            return selectedPrefab;
        }
        else
        {
            if (selectedPrefab != null)
            {
                return selectedPrefab;
            }
            else
            {
                selectedPrefab = config.GetRandomPrefab();
                return selectedPrefab;
            }
        }
    }
}

#endif