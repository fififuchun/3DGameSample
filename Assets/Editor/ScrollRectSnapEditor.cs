using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ScrollRectSnap))]
public sealed class ScrollRectSnapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}