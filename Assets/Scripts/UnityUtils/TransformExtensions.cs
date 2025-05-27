using UnityEngine;

public static class TransformExtensions
{
    /// <summary>
    /// Destroys all children of a given <see cref="Transform"/>.
    ///
    /// In the Editor, this method is faster than destroying children one at a time
    /// because it avoids the overhead of sending a message to the Editor for each
    /// child.
    /// </summary>
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