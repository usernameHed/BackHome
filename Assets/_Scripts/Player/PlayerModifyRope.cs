using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

/// <summary>
/// WrapMode Description
/// trigger handle
/// </summary>
public class PlayerModifyRope : MonoBehaviour
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip("particule minimum"), SerializeField]
    private int minParticle = 10;
    [FoldoutGroup("GamePlay"), Tooltip("particule maximum"), SerializeField]
    private int maxParticle = 50;
    [FoldoutGroup("GamePlay"), Tooltip("tensité max où on ne peux plus diminuer la corde"), SerializeField]
    private float maxTensityForLess = 1.5f;
    [FoldoutGroup("GamePlay"), Tooltip("angle différence pour ajouter/supprimer au joystick"), SerializeField]
    private float diffAngleForUpAndDown = 5f;
    [FoldoutGroup("GamePlay"), Tooltip("peut-on modifier quand on est grippé ?"), SerializeField]
    private bool canModifyWhenIAmGripped = false;
    [FoldoutGroup("GamePlay"), Tooltip("peut-on modifier quand on est grippé ?"), SerializeField]
    private bool canModifyWhenOtherIsGripped = true;
    [FoldoutGroup("GamePlay"), Tooltip("peut-on modifier quand on est grippé ?"), SerializeField]
    private bool always = true;


    [FoldoutGroup("GamePlay"), Tooltip("force d'aggripation au mur..."), SerializeField]
    private float speedChange = 5f;

    
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerInput playerInput;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private RopeHandler ropeHandler;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayersManager playerManager;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerController playerController;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerGrip playerGrip;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private WorldCollision worldCollision;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerPenduleMove playerPenduleMove;

    [FoldoutGroup("Debug"), Tooltip("ici dès qu'on peut modifier la rope une fois, on a le droit pendant X seconde de continuer, même si l arope n'est plus tense"), SerializeField]
    private FrequencyCoolDown timeWhenWeCanModifyRope;  //0.3

    private bool stopAction = false;    //le joueur est-il stopé ?
    private Vector3 holdDirRope;
    #endregion

    #region Initialization
    private void OnEnable()
    {
        EventManager.StartListening(GameData.Event.GameOver, StopAction);
        InitValue();
    }

    private void InitValue()
    {

    }
    #endregion

    #region Core
    /// <summary>
    /// renvoi vrai ou faux si on a le droit de sauter (selon le hold)
    /// </summary>
    /// <returns></returns>
    public bool CanModifyHandle()
    {
        if (canModifyWhenIAmGripped && playerGrip.Gripped)
        {
            //ici on est soit même grippé
            return (true);
        }            
        if (canModifyWhenOtherIsGripped && playerManager.IsOtherIsGripped(playerController.IdPlayer))
        {
            //ici l'autre est grippé
            return (true);
        }
        return (always);
    }

    /// <summary>
    /// ici action du grip à chaque fixedUpdate
    /// </summary>
    private void ModifyRopeTriggerHandle()
    {
        if (!CanModifyHandle())
            return;

        if (playerInput.ModyfyRopeRemoveDownInput > 0)
        {
            RemoveRopeParticle(true);
        }
        else if (playerInput.ModyfyRopeAddDownInput > 0)
        {
            AddRopeParticle(true);
        }
    }

    private void AddRopeParticle(bool vibration = false)
    {
        if (ropeHandler.ParticleInRope < maxParticle)
        {
            //on peut ajouter
            ropeHandler.ChangeParticleInRope(true, speedChange, vibration);
        }
    }
    private void RemoveRopeParticle(bool vibration = false)
    {
        if (ropeHandler.ParticleInRope > minParticle && ropeHandler.actualTensity < maxTensityForLess)
        {
            //on peut supprimer
            ropeHandler.ChangeParticleInRope(false, speedChange, vibration);
        }
    }

    /// <summary>
    /// peut on modifier en up / down ?
    /// </summary>
    /// <returns></returns>
    private bool CanModifyUpDown()
    {
        //si on n'appuis sur rien
        if (playerInput.Horiz == 0 && playerInput.Verti == 0)
            return (false);

        //si on est pas grounded... NE PAS AUTORISER
        if (!worldCollision.IsGroundedSafe()/* && timeWhenWeCanModifyRope.IsReady()*/)
        {
            if (!timeWhenWeCanModifyRope.IsReady())
            {
                Debug.Log("ici on peut encore modifier, alors qu'on est en l'air");
            }
            else
            {
                return (false);
            }
        }
        if (worldCollision.IsGroundedSafe() && worldCollision.IsOnFloor()/* && timeWhenWeCanModifyRope.IsReady()*/)
        {
            return (false);
        }

        //ici on peut modifier en temps normal... maintenant test la tension, et l'angle de l'input
        if (!ropeHandler.IsTenseForAirMove && timeWhenWeCanModifyRope.IsReady())
            return (false);

        //ne pas changé si on est grippé
        if (playerGrip.Gripped)
            return (false);

        //si l'autre n'est pas grippé, ne rien faire
        if (!playerManager.IsOtherIsGripped(playerController.IdPlayer))
            return (false);

        //ici les conditions sont bonnes !
        return (true);
    }

    /// <summary>
    /// ajout / enleve avec l'input du joueur up / down
    /// </summary>
    private void ModifyRopeUpDown()
    {
        if (!CanModifyUpDown())
            return;

        Vector3 dirRope = Vector3.zero;

        if (!ropeHandler.IsTenseForAirMove)
        {
            //ici on reprend avec l'angle d'avant de la rope
            dirRope = holdDirRope;
        }
        else
        {
            dirRope = ropeHandler.GetVectorFromPlayer(playerController.IdPlayer);
            //dirRope = playerPenduleMove.GetDirRopeFromPivot();
            holdDirRope = dirRope;
            timeWhenWeCanModifyRope.StartCoolDown();
        }

        

        Vector3 dirRopeInverse = -dirRope;
        Vector3 dirInput = playerInput.GetDirInput();
        float angleRope = QuaternionExt.GetAngleFromVector(dirRope);
        float angleRopeInverse = QuaternionExt.GetAngleFromVector(dirRopeInverse);
        float angleInput = QuaternionExt.GetAngleFromVector(dirInput);

        Debug.DrawRay(transform.position, dirRope, Color.green, 1f);
        Debug.DrawRay(transform.position, dirInput, Color.red, 1f);

        float diffAngleRopeNormal;
        if (QuaternionExt.IsAngleCloseToOtherByAmount(angleInput, angleRope, diffAngleForUpAndDown, out diffAngleRopeNormal))
        {
            //Debug.Log("delete");
            RemoveRopeParticle();
        }
        else if (QuaternionExt.IsAngleCloseToOtherByAmount(angleInput, angleRopeInverse, diffAngleForUpAndDown, out diffAngleRopeNormal))
        {
            //Debug.Log("add");
            AddRopeParticle();
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

    private void Update()
    {
        ModifyRopeTriggerHandle();
        //ModifyRopeUpDown();
    }

    /// <summary>
    /// désactiver
    /// </summary>
    private void OnDisable()
    {
        EventManager.StopListening(GameData.Event.GameOver, StopAction);
    }
    #endregion
}
