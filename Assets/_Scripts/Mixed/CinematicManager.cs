using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

/// <summary>
/// MenuManager Description
/// </summary>
public class CinematicManager : MonoBehaviour, ILevelManager
{
    #region Attributes

    private bool enabledScript = false;
    public int indexCinematicBack = 0;
    public bool canSkip = true;

    public FrequencyCoolDown coolDownButton;
    #endregion

    #region Initialization
    /// <summary>
    /// est appelé depuis le GameManager depuis l'interface
    /// à l'initialisation...
    /// </summary>
    public void InitScene()
    {
        enabledScript = true;

        SoundManager.Instance.PlaySound("Stop_all");
        coolDownButton.StartCoolDown();

        Invoke("Quit", 12);
    }

    #endregion

    #region Core

    /// <summary>
    /// ici lance le jeu, il est chargé !
    /// </summary>
    [FoldoutGroup("Debug"), Button("Play")]
    public void Play()
    {
        if (!coolDownButton.IsReady())
            return;
        enabledScript = false;

        GameManager.Instance.SceneManagerLocal.PlayNext();
    }

    [FoldoutGroup("Debug"), Button("Quit")]
    public void Quit()
    {
        if (!coolDownButton.IsReady())
            return;

        enabledScript = false;
        Debug.Log("ci quit ???");
        SoundManager.Instance.PlaySound("Stop_all");
        //SoundManager.Instance.PlaySound("Play_Music_Menu", true);

        GameManager.Instance.SceneManagerLocal.PlayIndex(indexCinematicBack);

    }

    public void InputLevel()
    {
        if (PlayerConnected.Instance.getPlayer(-1).GetButtonDown("Escape")
           || PlayerConnected.Instance.getButtonDownFromAnyGamePad("Back"))
        {
            Quit();
        }
        if (PlayerConnected.Instance.getPlayer(-1).GetButtonDown("Restart")
            || PlayerConnected.Instance.getButtonDownFromAnyGamePad("Restart"))
        {
            Play();
        }
    }

    #endregion

    #region Unity ending functions
    private void Update()
    {
        if (!enabledScript)
            return;

        if (!canSkip)
            return;

        InputLevel();
    }
    #endregion
}
