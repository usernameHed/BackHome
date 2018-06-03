using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

/// <summary>
/// MenuManager Description
/// </summary>
public class StartManager : MonoBehaviour, ILevelManager
{
    #region Attributes

    private bool enabledScript = false;
    #endregion

    #region Initialization
    /// <summary>
    /// est appelé depuis le GameManager depuis l'interface
    /// à l'initialisation...
    /// </summary>
    public void InitScene()
    {
        enabledScript = true;
        Debug.Log("play start");
        SoundManager.Instance.PlaySound("Play_ambiance");
        Cursor.visible = true;
    }

    #endregion

    #region Core

    /// <summary>
    /// ici lance le jeu, il est chargé !
    /// </summary>
    [FoldoutGroup("Debug"), Button("Play")]
    public void Play()
    {
        
        
        GameManager.Instance.SceneManagerLocal.PlayNext();
    }

    public void Quit()
    {

    }

    public void InputLevel()
    {

    }

    #endregion

    #region Unity ending functions

    #endregion
}
