using UnityEngine;

public static class TransformExtensions
{
    public static void DestroyAllChildren(this Transform parent)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            while (parent.childCount > 0)
                Object.DestroyImmediate(parent.GetChild(0).gameObject);
        }
        else
#endif
        {
            foreach (Transform child in parent)
                Object.Destroy(child.gameObject);
        }
    }
}