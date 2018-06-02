using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

/// <summary>
/// WrapMode Description
/// </summary>
public class PlayerGetFat : MonoBehaviour
{
    #region Attributes
    //[FoldoutGroup("GamePlay"), Tooltip("jumper constament en restant appuyé ?"), SerializeField]
    //private bool stayHold = false;
    [FoldoutGroup("GamePlay"), Tooltip("force d'aggripation au mur..."), SerializeField]
    private float forceFat = 3f;
    [FoldoutGroup("GamePlay"), Tooltip("force d'aggripation au mur..."), SerializeField]
    private float drawWhenFat = 10f;

    [FoldoutGroup("GamePlay"), Tooltip("force d'aggripation au mur..."), SerializeField]
    private float boostUpAirMoveWhenFat = 10f;

    [FoldoutGroup("GamePlay"), Tooltip("cooldown du jump (influe sur le mouvements du perso)"), SerializeField]
    private FrequencyCoolDown coolDownFat;

    [FoldoutGroup("Object"), Tooltip("est-on grippé ?"), SerializeField]
    private GameObject display;

    [FoldoutGroup("Debug"), Tooltip("est-on grippé ?"), SerializeField]
    private bool isFat = false;
    public bool IsFat { get { return (isFat); } }

    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private Rigidbody rb;

    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerInput inputPlayer;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerPenduleMove playerAirMove;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private RopeHandler ropeHandler;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerController playerController;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerGrip playerGrip;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerPenduleMove playerPenduleMove;

    private bool stopAction = false;    //le joueur est-il stopé ?
    private float initialDrag;
    #endregion

    #region Initialization
    private void OnEnable()
    {
        EventManager.StartListening(GameData.Event.GameOver, StopAction);
        EventManager.StartListening(GameData.Event.Grounded, GroundedAction);
        InitValue();
    }

    private void InitValue()
    {
        isFat = false;
        initialDrag = rb.drag;
        display.SetActive(false);
    }
    #endregion

    #region Core
    /// <summary>
    /// renvoi vrai ou faux si on a le droit de sauter (selon le hold)
    /// </summary>
    /// <returns></returns>
    public bool CanGrip()
    {
        return (true);
    }

    /// <summary>
    /// aggriper !
    /// </summary>
    /// <returns></returns>
    public bool SetGrip(bool fat)
    {
        if (fat && !CanGrip())
            return (false);

        isFat = fat;
        if (isFat)
        {
            Debug.Log("ici fat");
            display.SetActive(true);

            rb.drag = drawWhenFat;
        }
        else
        {
            display.SetActive(false);
            rb.drag = initialDrag;

            coolDownFat.StartCoolDown();
        }
        return (true);
    }

    /// <summary>
    /// applique un boost su rla force droite / gauche du airMove quand on est fat
    /// </summary>
    /// <returns>renvoi 1 de base, 10 si on est fat</returns>
    public float boostAirMoveWhenFat()
    {
        if (!isFat)
            return (1);
        else
            return (boostUpAirMoveWhenFat);
    }

    /// <summary>
    /// action de grip appelé quand on est grounded
    /// Setup juste le moment de changement
    /// </summary>
    private void FatSetup()
    {
        //si on reste appuyé
        if (inputPlayer.FatInput && !isFat && !playerGrip.Gripped)
        {
            SetGrip(true);
        }
        //si on lache, stopper le gripped
        //(et démarrer le coolDown pour pas réactiver tout de suite après)
        if ((inputPlayer.FatUpInput && isFat) || (!inputPlayer.FatInput && isFat) || (isFat && playerGrip.Gripped))
        {
            SetGrip(false);
        }
    }

    /// <summary>
    /// ici action du grip à chaque fixedUpdate
    /// </summary>
    private void FatAction()
    {
        if (isFat)
        {
            //is on est pas en airMove, just aller en bas
            if (!playerAirMove.IsAirTense)
            {
                rb.AddForce(Vector3.up * -forceFat * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
            else
            {
                //si on est en airMove, aller en direction contraire de la rope
                Vector3 dirPlayer = ropeHandler.GetVectorFromPlayer(playerController.IdPlayer);
                //Vector3 dirPlayer = playerPenduleMove.GetDirRopeFromPivot();
                rb.AddForce(dirPlayer * -forceFat * Time.fixedDeltaTime, ForceMode.Acceleration);
            }

        }
    }

    /// <summary>
    /// désactive le script
    /// </summary>
    private void StopAction()
    {
        stopAction = false;
    }
    #endregion

    #region Unity ending functions
    /// <summary>
    /// cette fonction est appelé par l'event WorldCollision en fixedUpdate
    /// (ici on sait si on est grounded ou pas, et comment)
    /// </summary>
    private void GroundedAction()
    {
        if (stopAction)
            return;

        FatSetup();
    }

    private void FixedUpdate()
    {
        FatAction();
    }

    /// <summary>
    /// désactiver
    /// </summary>
    private void OnDisable()
    {
        EventManager.StopListening(GameData.Event.GameOver, StopAction);
        EventManager.StopListening(GameData.Event.Grounded, GroundedAction);
    }
    #endregion
}
