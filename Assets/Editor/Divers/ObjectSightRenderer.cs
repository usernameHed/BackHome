using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectSight))]
public class ObjectSightRenderer : Editor
{
	#region Core

    void OnSceneGUI()
    {
		ObjectSight sight = (ObjectSight) target;

        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(Screen.width - 100, Screen.height - 80, 90, 50));

        GUILayout.EndArea();
        Handles.EndGUI();

        Handles.color = new Color(1, 1, 1, 0.2f);
		Handles.DrawSolidArc(sight.transform.position, -sight.transform.forward, Quaternion.AngleAxis(90 - sight.SightFov / 2, -sight.transform.forward) * -sight.transform.right, sight.SightFov, sight.SightRange);
    }

	#endregion
}