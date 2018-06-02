using UnityEngine;

/// <summary>
/// Display fps in game
/// </summary>
public class FPSDisplay : MonoBehaviour
{
	#region Attributes

    private float deltaTime = 0.0f;

	#endregion

	#region Core

	// Unity functions

	private void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
    }

	private void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = Color.white;
        float msec = deltaTime * 1000.0f;
        if (deltaTime != 0)
        {
            float fps = 1.0f / deltaTime;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            GUI.Label(rect, text, style);
        }
    }

	#endregion
}