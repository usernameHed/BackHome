using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using System;
using System.Collections.Generic;

/// <summary>
/// LevelManager Description
/// </summary>
public class LevelManager : MonoBehaviour, ILevelManager
{
    #region Attributes
    
    [FoldoutGroup("Debug"), Tooltip("gere le temps avant de pouvoir faire Restart"), SerializeField]
    private FrequencyTimer coolDownRestart;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayersManager playersManager;

    private bool gameOver = false;
    private bool enabledScript = true;
    #endregion

    #region Initialization

    private void OnEnable()
    {
        EventManager.StartListening(GameData.Event.TryToEnd, StopAction);
    }

    private void Awake()
    {
        coolDownRestart.Ready();
    }

    /// <summary>
    /// est appelé depuis le GameManager depuis l'interface
    /// à l'initialisation... 
    /// </summary>
    public void InitScene()
    {
        enabledScript = true;
        gameOver = false;
        LevelInit();
    }
    #endregion

    #region Core
    
    public void LevelInit()
    {
        Debug.Log("level init");
        

        //SoundManager.GetSingleton.playSound("Play_vent");
        if (!ScoreManager.Instance.Data.GetRestart())
        {
            Debug.Log("play jouer le vent pour la première fois");
            //ici on vient de jouer
            //SoundManager.Instance.PlaySound("Play_ambiance");
            SoundManager.Instance.PlaySound("Play_BackHome");
            Debug.Log("la musique se lance qu'une fois ??");
        }
        else
        {
            Debug.Log("play on a restart, ne PAS lancer de musique encore...");
            //ici on vient de restart
            //SoundManager.GetSingleton.playSound("Play_vent");
        }
    }

    //ici game over...
    private void StopAction()
    {
        if (!enabledScript)
            return;
        if (gameOver)
            return;
        if (!playersManager.AreBothDied())
            return;


        gameOver = true;
        Debug.Log("gameOver !");
        EventManager.TriggerEvent(GameData.Event.GameOver);

        Invoke("Restart", 0.3f);
        //Restart();
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
            Restart();
        }
    }

    public void Play()
    {

    }

    /// <summary>
    /// restart le jeu
    /// </summary>
    [Button("Restart")]
    public void Restart()
    {
        if (!enabledScript)
            return;
        if (!coolDownRestart.Ready())
            return;

        Debug.Log("restart, garder la musique !");

        enabledScript = false;

        ObjectsPooler.Instance.desactiveEveryOneForTransition();
        ObjectsPoolerLocal.Instance.desactiveEveryOneForTransition();
        //LevelInit();
        ScoreManager.Instance.Data.SetRestart(true);
        

        //GameManager.GetSingleton.RestartGame(true);
        GameManager.Instance.SceneManagerLocal.PlayNext();
    }

    [Button("Quit")]
    public void Quit()
    {
        if (!enabledScript)
            return;
        if (!coolDownRestart.Ready())
            return;

        Debug.Log("play stop all");
        //SoundManager.Instance.PlaySound("Stop_ambiance", true);
        SoundManager.Instance.PlaySound("Stop_all");

        enabledScript = false;

        ObjectsPooler.Instance.desactiveEveryOneForTransition();
        ObjectsPoolerLocal.Instance.desactiveEveryOneForTransition();

        ScoreManager.Instance.Data.SetRestart(false);

        GameManager.Instance.SceneManagerLocal.PlayPrevious(false);


    }

    #endregion

    #region Unity ending functions
    private void Update()
    {
        InputLevel();
    }

    private void OnDisable()
    {
        EventManager.StopListening(GameData.Event.TryToEnd, StopAction);
    }
    #endregion
}
