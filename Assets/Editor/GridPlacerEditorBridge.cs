using UnityEngine;

namespace GridPlacement.Editor
{
    public static class GridPlacerEditorBridge
    {
        public static GameObject GetSelectedPrefab(SOGridPlacerConfig config)
        {
            return GridPlacerEditorState.GetSelectedPrefab(config);
        }
    }
}