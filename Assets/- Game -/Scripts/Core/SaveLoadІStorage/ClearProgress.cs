#if UNITY_EDITOR
using UnityEditor;

public class ClearProgress
{
    [MenuItem("Tools/" + ProjectSettings.TITLE + "/Clear Progress")]
    public static void Clear()
    {
        ProgressSaveStorage.Clear();
    }
}
#endif
