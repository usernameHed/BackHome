using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

/// <summary>
/// MenuManager Description
/// </summary>
public class MenuManager : MonoBehaviour, ILevelManager
{
    #region Attributes

    [FoldoutGroup("Objects"), Tooltip("Debug"), SerializeField]
    private List<Button> buttonsMainMenu;

    [FoldoutGroup("Objects"), Tooltip("Debug"), SerializeField]
    private Toggle simplified;

    public FrequencyCoolDown coolDownButton;

    private bool enabledScript = false;
    #endregion

    #region Initialization
    /// <summary>
    /// est appelé depuis le GameManager depuis l'interface
    /// à l'initialisation...
    /// </summary>
    public void InitScene()
    {
        Cursor.visible = true;
        enabledScript = true;

        SetToggleOnStart();

        ScoreManager.Instance.Data.SetDefault();

        SoundManager.Instance.PlaySound("MenuMusic_Play");
        

        coolDownButton.StartCoolDown();
    }

    private void SetToggleOnStart()
    {
        simplified.isOn = ScoreManager.Instance.Data.GetSimplified();
    }

    private void SetAlternativeControl()
    {
        ScoreManager.Instance.Data.SetSimplified(simplified.isOn);
        Cursor.visible = simplified.isOn;
        PlayerConnected.Instance.enabledVibration = !simplified.isOn;
    }

    #endregion

    #region Core

    /// <summary>
    /// ici lance le jeu, il est chargé !
    /// </summary>
    [FoldoutGroup("Debug"), Button("Play")]
    public void Play()
    {
        Debug.Log("play ici menu");
        //SoundManager.Instance.PlaySound("Play_Music_Menu", true);

        SoundManager.Instance.PlaySound("Stop_all");
        //SoundManager.Instance.PlaySound("MenuMusic_stop");

        ScoreManager.Instance.ResetAll();
        SetAlternativeControl();

        GameManager.Instance.SceneManagerLocal.PlayNext();
    }

    [FoldoutGroup("Debug"), Button("Quit")]
    public void Quit()
    {
        if (!coolDownButton.IsReady())
            return;

        enabledScript = false;
        buttonsMainMenu[1].Select();

        SoundManager.Instance.PlaySound("Stop_all");
        //SoundManager.Instance.PlaySound("Play_Music_Menu", true);

        SceneManagerGlobal.Instance.QuitGame(true);
    }

    public void InputLevel()
    {
        if (PlayerConnected.Instance.getPlayer(-1).GetButtonDown("Escape")
           || PlayerConnected.Instance.getButtonDownFromAnyGamePad("Back"))
        {
            Quit();
        }
    }

    /// <summary>
    /// est appelé pour débug le clique
    /// Quand on clique avec la souris: reselect le premier bouton !
    /// </summary>
    private void DebugMouseCLick()
    {
        if (!enabledScript)
            return;
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            buttonsMainMenu[0].Select();
        }
    }
    #endregion

    #region Unity ending functions
    private void Update()
    {
        if (!enabledScript)
            return;
        InputLevel();
        DebugMouseCLick();
    }
    #endregion
}
