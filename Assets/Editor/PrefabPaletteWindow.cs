using UnityEngine;
using UnityEditor;

public class PrefabPaletteWindow : EditorWindow
{
    private SOGridPlacerConfig config;
    private int selectedIndex = -1;
    private Vector2 scroll;
    private bool useRandom = true;

    [MenuItem("Tools/Grid Placer/Prefab Palette")]
    public static void Open()
    {
        GetWindow<PrefabPaletteWindow>("Prefab Palette");
    }

    private void OnGUI()
    {
        config = EditorGUILayout.ObjectField("Config", config, typeof(SOGridPlacerConfig), false) as SOGridPlacerConfig;
        if (config == null) return;

        useRandom = EditorGUILayout.Toggle("Random Selection", useRandom);
        GridPlacerEditorState.useRandom = useRandom;
        
        EditorGUILayout.Space(5);
        scroll = EditorGUILayout.BeginScrollView(scroll);

        int columns = 4;
        float size = 64;

        var prefabs = config.prefabs;
        if (prefabs == null) return;

        int rows = Mathf.CeilToInt(prefabs.Length / (float)columns);

        for (int y = 0; y < rows; y++)
        {
            EditorGUILayout.BeginHorizontal();

            for (int x = 0; x < columns; x++)
            {
                int index = y * columns + x;
                if (index >= prefabs.Length) break;

                var prefab = prefabs[index];
                if (prefab == null)
                {
                    GUILayout.Box("Missing", GUILayout.Width(size), GUILayout.Height(size));
                    continue;
                }

                Texture2D preview = AssetPreview.GetAssetPreview(prefab);
                GUIContent content = new GUIContent(preview ?? EditorGUIUtility.ObjectContent(prefab, typeof(GameObject)).image, prefab.name);

                GUIStyle style = GUI.skin.button;
                if (selectedIndex == index && !useRandom)
                    GUI.backgroundColor = Color.green;

                if (GUILayout.Button(content, style, GUILayout.Width(size), GUILayout.Height(size)))
                {
                    selectedIndex = index;
                    config.selectedIndex = index;
                    GridPlacerEditorState.GetSelectedPrefab(config);
                }
                
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }
}