using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// ClampToCamera Description
/// </summary>
public class ClampToCamera : MonoBehaviour
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip("Clamp la position du player aux borders de la caméra"), SerializeField]
    private float borderMin = 0.1f;
    [FoldoutGroup("GamePlay"), Tooltip("Clamp la position du player aux borders de la caméra"), SerializeField]
    private float borderMax = 0.9f;

    [Tooltip("opti fps"), SerializeField]
	private FrequencyTimer updateTimer;

    private Camera cam;
    private bool enabledScript = true;

    #endregion

    #region Initialization
    private void OnEnable()
    {
        EventManager.StartListening(GameData.Event.GameOver, StopAction);
    }

    private void Awake()
    {
        cam = GameManager.Instance.CameraMain;
        enabledScript = true;
        updateTimer.Reset();
        updateTimer.Ready();
    }
    #endregion

    #region Core
    /// <summary>
    /// stop les action du player...
    /// </summary>
    private void StopAction()
    {
        enabledScript = false;
        Debug.Log("stop action du joueur");
    }
    /// <summary>
    /// clamp la position du player dans la caméra !
    /// </summary>
    private void ClampPlayer()
    {
        Vector3 pos = cam.WorldToViewportPoint(transform.position);
        pos.x = Mathf.Clamp(pos.x, borderMin, borderMax);
        pos.y = Mathf.Clamp(pos.y, borderMin, borderMax);
        transform.position = cam.ViewportToWorldPoint(pos);
    }
    #endregion

    #region Unity ending functions

    private void FixedUpdate()
    {
        if (updateTimer.Ready() && enabledScript)
            ClampPlayer();
    }

    private void OnDisable()
    {
        EventManager.StopListening(GameData.Event.GameOver, StopAction);
    }

    #endregion
}
