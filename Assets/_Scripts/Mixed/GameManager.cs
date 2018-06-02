using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System;
using Sirenix.Serialization;

/// <summary>
/// GameManager Description
/// </summary>
public class GameManager : SingletonMono<GameManager>
{
    #region Attributes


    //////////////////////////////////////////////////////////////////////
    [FoldoutGroup("Scenes"), Tooltip("liens du levelManager"), SerializeField]
    private SceneManagerLocal sceneManagerLocal;
    public SceneManagerLocal SceneManagerLocal { set { sceneManagerLocal = value; InitNewScene(); } get { return (sceneManagerLocal); } }

    [FoldoutGroup("Debug"), Tooltip("opti fps"), SerializeField]
	private FrequencyTimer updateTimer;

    [FoldoutGroup("Debug"), Tooltip("Caméra"), SerializeField]
    private GameObject cameraObject;
    public GameObject CameraObject
    {
        get
        {
            if (!cameraObject)
            {
                cameraMain = Camera.main;
                cameraObject = cameraMain.gameObject;
                return (cameraObject);
            }
            return cameraObject;
        }
    }
    public void SetCamera(GameObject cam) { cameraMain = (cam) ? cam.GetComponent<Camera>() : null; }
    private Camera cameraMain;
    public Camera CameraMain { get { return (cameraMain); } }

    #endregion

    #region Initialization

    private void OnEnable()
    {
        EventManager.StartListening(GameData.Event.GamePadConnectionChange, CallChangePhase);
    }

    /// <summary>
    /// initialise les ILevelManagers (il y en a forcément 1 par niveau)
    /// </summary>
    private void InitNewScene()
    {
        if (sceneManagerLocal.LevelManagerScript == null)
        {
            Debug.LogError("no ILevelManager ??");
        }
        //init la caméra de la scene...
        cameraObject = sceneManagerLocal.CameraObject;
        SetCamera(cameraObject);

        //init le level...
        sceneManagerLocal.LevelManagerScript.InitScene();
    }

    /// <summary>
    /// appelé quand les joypad se co/deco.
    /// </summary>
    public void CallChangePhase(bool active, int id)
    {
        //if (sceneManagerLocal.LevelManagerScript != null)
        //    sceneManagerLocal.LevelManagerScript.CallGamePad();
    }

    #endregion

    #region Core
    
    #endregion

    #region Unity ending functions

    private void Update()
    {
        //optimisation des fps
        if (updateTimer.Ready())
        {
            
        }
    }

    private void OnDisable()
    {
        EventManager.StopListening(GameData.Event.GamePadConnectionChange, CallChangePhase);
    }
    #endregion
}
