using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using WorldCollisionNamespace;

[RequireComponent(typeof(Rigidbody))]
public class PlayerAutoAirMove : MonoBehaviour
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip("boost initial du saut"), SerializeField]
    private float initialBoost = 2f;
    [FoldoutGroup("GamePlay"), Tooltip("force constante du saut"), SerializeField]
    private float jumpBoost = 1.0f;
    private float currentJumpBoost = 0;
    [FoldoutGroup("GamePlay"), Tooltip("force inverse sur la rope"), SerializeField]
    private float removeForceOverTime = 2f;


    [FoldoutGroup("GamePlay"), Tooltip("force inverse sur la rope"), SerializeField]
    private float forceOnRopeBoost = 1.0f;
    private float currentForceOnRopeBoost = 0;

    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private Vector3 previousControllerJumpDir = Vector3.zero;

    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerJump playerJump;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerPenduleMove playerPenduleAirMove;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerGetFat playerGetFat;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private RopeHandler ropeHandler;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerController playerController;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerMove playerMove;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayersManager playerManager;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerPenduleMove playerPenduleMove;

    private Vector3 savePivot;
    

    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private FrequencyCoolDown airMoveActifLong; //O.4
    public FrequencyCoolDown AirMoveActifLong { get { return (airMoveActifLong); } }

    private bool inAutoAirMove = false;              //a-t-on juste jumpé ?
    public bool InAutoAirMove { get { return (inAutoAirMove); } }
    private bool isAutoAirMoveAndCoolDownNotFinish = false;

    private bool stopAction = false;

    #endregion

    #region Initialize
    private void OnEnable()
    {
        EventManager.StartListening(GameData.Event.GameOver, StopAction);
        InitPlayer();
    }

    /// <summary>
    /// init le player
    /// </summary>
    private void InitPlayer()
    {
        stopAction = false;
    }
    #endregion

    #region Core

    /// <summary>
    /// ici est appelé a chaque fixedUpdate, on doit aller dans une direction !!
    /// </summary>
    private void ControlledAirJump()
    {
        //Vector3 dirRope = ropeHandler.GetVectorFromPlayer(playerController.IdPlayer);
        Vector3 dirRope = transform.position - savePivot;

        Vector3 dirLeft = -QuaternionExt.CrossProduct(dirRope, Vector3.forward).normalized;
        Vector3 dirRight = QuaternionExt.CrossProduct(dirRope, Vector3.forward).normalized;
        Vector3 dirJump = previousControllerJumpDir;

        float angleLeft = QuaternionExt.GetAngleFromVector(dirLeft);
        float angleRight = QuaternionExt.GetAngleFromVector(dirRight);
        float angleJump = QuaternionExt.GetAngleFromVector(dirJump);
        //calcul de direction

        float diffAngleLeftNormal;
        QuaternionExt.IsAngleCloseToOtherByAmount(angleLeft, angleJump, 179f, out diffAngleLeftNormal);
        float diffAngleRightNormal;
        QuaternionExt.IsAngleCloseToOtherByAmount(angleRight, angleJump, 179f, out diffAngleRightNormal);

        if (diffAngleLeftNormal < diffAngleRightNormal)
        {
            dirJump = dirLeft;
        }
        else
        {
            dirJump = dirRight;
        }
        previousControllerJumpDir = dirJump;


        Debug.DrawRay(transform.position, dirJump * currentJumpBoost * 10, Color.white, 1f);
        //a la fin, effectuer la force
        playerPenduleAirMove.ApplyAirMoveForce(dirJump, currentJumpBoost * playerGetFat.boostAirMoveWhenFat());

        //currentJumpBoost -= 0;
        
    }

    /// <summary>
    /// est-on en airMoveAuto ? coolDown inclu ?
    /// </summary>
    /// <returns></returns>
    public bool IsInAirMove()
    {
        //si on est en airMove, OK
        if (inAutoAirMove)
            return (true);
        //si on est pas en airMove, ET que le timer est fini, on est PAS en airMove
        if (isAutoAirMoveAndCoolDownNotFinish && airMoveActifLong.IsReady())
        {
            isAutoAirMoveAndCoolDownNotFinish = false;
            return (false);
        }
        //si la 2eme variable qui defini le coolDown est faux, on est pas...
        else if (!isAutoAirMoveAndCoolDownNotFinish)
            return (false);

        //ici on est plus en airMove, MAIS le coolDown n'est pas fini, on l'est toujours !
        return (true);
    }

    /// <summary>
    /// ici effectue un jump extraordinaire !
    /// </summary>
    public void ControlledAirJumpSetup(bool jump, Vector3 dir, bool fromGround = true, float ratioForceJump = 1)
    {
        previousControllerJumpDir = dir;

        if (jump)
        {
            Debug.Log("active le jump");
            inAutoAirMove = isAutoAirMoveAndCoolDownNotFinish = true;

            savePivot = playerPenduleMove.GetPivotPoint(true);
            DebugExtension.DebugWireSphere(savePivot, Color.green, 1f, 1f);
            //Debug.Break();

            playerJump.PrepareAndJump(dir);

            airMoveActifLong.StartCoolDown();
            //playerMove.MoveOnWallWhenAirMove(dir);

            StartCoroutine(JumpNextFrame(dir, true, initialBoost, ratioForceJump));

            //playerJump.Jump(dir, true, initialBoost, 1);


            currentJumpBoost = jumpBoost;
            currentForceOnRopeBoost = forceOnRopeBoost;
        }
        else
        {
            Debug.Log("desactive le jump");
            inAutoAirMove = false;
            if (fromGround)
            {
                airMoveActifLong.Reset();
                isAutoAirMoveAndCoolDownNotFinish = false;
            }
        }
    }
    private IEnumerator JumpNextFrame(Vector3 dir, bool applyThisForce = false, float force = 1, float boost = 1)
    {
        yield return new WaitForEndOfFrame();
        Debug.Log("ici le boost: " + boost);
        playerJump.Jump(dir, true, force, boost);
    }

    /// <summary>
    /// ici peut on controller le jump ?
    /// </summary>
    /// <returns></returns>
    private bool CanControll()
    {
        //ici le jump est en cours, MAIS l'autre à désactivé le grip !
        if (IsInAirMove() && !playerManager.IsOtherIsGripped(playerController.IdPlayer)
            && playerManager.OtherJustUngrip(playerController.IdPlayer))
        {
            Debug.Log("ici exécute un mini Jump quand l'autre lache le grip !");
            playerPenduleAirMove.EndJump(previousControllerJumpDir);   //faire une poussé de fin
            ControlledAirJumpSetup(false, Vector3.zero);        //désactiver le move
            return (false);
        }

        return (true);
    }

    private void StopAction()
    {
        stopAction = false;
    }

    #endregion


    #region Unity ending functions
    /// <summary>
    /// jump...
    /// </summary>
    private void FixedUpdate()
    {
        if (!CanControll())
            return;

        if (inAutoAirMove)
        {
            if (currentJumpBoost > 2)
            {
                ControlledAirJump();
                playerPenduleAirMove.DoAirTenseRope(forceOnRopeBoost);

                currentJumpBoost = (currentJumpBoost < 1) ? 1 : currentJumpBoost - (removeForceOverTime * Time.fixedDeltaTime);
                //currentForceOnRopeBoost = (currentForceOnRopeBoost < 1) ? 1 : currentForceOnRopeBoost - (removeForceOverTime * Time.fixedDeltaTime);
            }
            else
            {
                //désactiver le saut Auto
                ControlledAirJumpSetup(false, Vector3.zero, false);
            }
        }

    }

    private void OnDisable()
    {
        EventManager.StopListening(GameData.Event.GameOver, StopAction);
    }
    #endregion
}