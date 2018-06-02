using Sirenix.OdinInspector;
using System;
using UnityEngine;
using WorldCollisionNamespace;

[RequireComponent(typeof(Rigidbody))]
public class ExternalForce : MonoBehaviour
{
    #region Attributes
    public bool activateForce = true;

    [FoldoutGroup("Mass"), Tooltip("mass du player quand on est en l'air"), SerializeField]
    private float massLittleWhenOtherJump = 1f;
    //public float MassLittleWhenOtherJump { get { return (massLittleWhenOtherJump); } }
    [FoldoutGroup("Mass"), Tooltip("mass du player quand on est en l'air"), SerializeField]
    private float massBigWhenIJump = 20f;

    [FoldoutGroup("Gravity"), OnValueChanged("InitValue"), Tooltip("gravité appliqué au joueur. 0: gravité de base, 1: g when jumped, unpressed, an dplayerUp, 2: g when jump & down, 3: g when no jump & down"), SerializeField]
    private bool[] gravityApply = new bool[3];

    [FoldoutGroup("Gravity"), OnValueChanged("InitValue"), Tooltip("facteur de gravité down"), SerializeField]
	private float fallMultiplier = 2.5f;
    [FoldoutGroup("Gravity"), OnValueChanged("InitValue"), Tooltip("facteur de gravité up quand on n'appui plus"), SerializeField]
    private float lowMultiplier = 2.5f;

    [FoldoutGroup("Gravity"), Tooltip("force down quand on sort du sol sans avoir sauté"), SerializeField]
    private float forceDownWhenNotGroundedAndNotJumped = 1f;
    [FoldoutGroup("Gravity"), Tooltip("force up quand on est en l'air (enfin on ground), proche d'un ceilling"), SerializeField]
    private float forceUpWhenCeilling = 1f;
    [FoldoutGroup("Gravity"), Tooltip("force up quand on est en l'air (enfin on ground), proche d'un ceilling"), SerializeField]
    private float forceOnWallWhenTense = 135f;
    public float ForceOnWallWhenTense { get { return (forceOnWallWhenTense); } }
    [FoldoutGroup("Gravity"), Tooltip("force up quand on est en l'air (enfin on ground), proche d'un ceilling"), SerializeField]
    private float forceOnWall = 35f;
    public float ForceOnWall { get { return (forceOnWall); } }

    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    private FrequencyCoolDown coolDownOtherHasJumpWaitForGrounded;  //0.2f

    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private RopeHandler ropeHandler;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayersManager playerManager;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private WorldCollision worldCollision;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private Rigidbody rb;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerInput playerInput;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerController playerController;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerJump playerJump;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerGrip playerGrip;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerPenduleMove playerAirMove;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerAutoAirMove playerAutoAirMove;

    private Vector3 initialVelocity;

    private FrequencyCoolDown coolDownDontApplyGravity = new FrequencyCoolDown();  //0.2f
    public FrequencyCoolDown CoolDownDontApplyGravity { get { return (coolDownDontApplyGravity); } }


    private bool stopAction = false;    //le joueur est-il stopé ?
    private bool isFlyingBecauseOtherHasJump = false;   //l'autre joueur à sauté, on est "stun"

    #endregion

    #region Initialize
    private void OnEnable()
    {
        EventManager.StartListening(GameData.Event.GameOver, StopAction);
        InitValue();
    }

    /// <summary>
    /// 
    /// </summary>
    private void InitValue()
    {
        isFlyingBecauseOtherHasJump = false;
    }
    #endregion

    #region Core

    /// <summary>
    /// ici l'autre player à jumpé, on trigger SetupFlyINgMode pour le prochain appel Grounded
    /// </summary>
    /// <param name="jump">on vient de jumper, ou on viens d'atterir ?</param>
    public void OtherPlayerHasJumped()
    {
        //Debug.Log("set la mass en fly");
        //Debug.Log("ici, player " + playerController.IdPlayer + ", test de setup OU reset");
        //si on est grounded, alors on set la mass en petit !
        if (worldCollision.IsGroundedSafe())
        {
            //Debug.Log("ici set rb little");
            rb.mass = massLittleWhenOtherJump;
            //coolDownOtherHasJumpWaitForGrounded.StartCoolDown();
            isFlyingBecauseOtherHasJump = true;
            //Debug.Log("setup mass 1");
            coolDownOtherHasJumpWaitForGrounded.StartCoolDown();
        }
        //si on est pas grounded, garder la mass en reset
        else if (coolDownOtherHasJumpWaitForGrounded.IsReady())
        {
            //Debug.Log("reset mass 10");
            ResetPlayerHasJump();
        }
    }

    public void TestForResetMass()
    {
        if (!worldCollision.IsGroundedSafe() && !playerJump.HasJumpAndFlying)
        {
            //Debug.Log("player " + playerController.IdPlayer + ", ici on est en l'air, et on a pas jump ! reset la mass");
            ResetPlayerHasJump();
        }
    }

    /// <summary>
    /// ici reset tout les parametres
    /// </summary>
    public void ResetInitialParameter()
    {
        rb.mass = playerController.InitialMass;
    }


    /// <summary>
    /// ici après avoir été stun, on essai de se remettre à la normal !
    /// </summary>
    public void ResetPlayerHasJump()
    {
        ResetInitialParameter();
        isFlyingBecauseOtherHasJump = false;
    }
    /// <summary>
    /// ici set la big mass quand on saute !
    /// </summary>
    public void SetBigMass()
    {
        rb.mass = massBigWhenIJump;
    }

    /// <summary>
    /// Applique la gravité du jump / de base !
    /// </summary>
    private void ApplyGravity()
    {
        CanApplyGravity();
        
        if (gravityApply[0])
        {
            //Debug.Log("ici ceilling");
            PhysicsExt.ApplyConstForce(rb, -Vector3.up, forceUpWhenCeilling);
        }
        else if (gravityApply[1])
        {
            //Debug.Log("ici gravity");
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (gravityApply[2])
        {
            //Debug.Log("ici low gravity");
            rb.velocity += Vector3.up * Physics.gravity.y * (lowMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    /// <summary>
    /// peut ont appliquer les forces ?
    /// case 0: 
    /// </summary>
    private void CanApplyGravity()
    {
        gravityApply[0] = false;
        gravityApply[1] = false;
        gravityApply[2] = false;

        //ici juste désactiver toute les force
        if (!activateForce)
            return;

        //ici désactive toute les force quand on est grippe
        if (playerGrip.Gripped)
            return;

        //est-ce qu'on est in AirMove ?? si oui, ne pas appliquer les autres forces...
        if (playerAirMove.ForceTenseRope())
            return;

        //tant qu'on est stun, plus de gravité sur nous !
        if (!coolDownDontApplyGravity.IsReady())
            return;

        /*if (rb.velocity.y < 0
            && worldCollision.IsGroundedSafe()
            && worldCollision.GetSimpleCollisionSafe() == CollisionSimple.Ceilling
            && !playerAirMove.IsAirTenseOrInCoolDown())
        {
            gravityApply[0] = true; //ceilling
            gravityApply[1] = false;
            gravityApply[2] = false;
        }
        else */if (rb.velocity.y < 0)
        {
            gravityApply[0] = false;
            gravityApply[1] = true; //up
            gravityApply[2] = false;
        }
        else if (rb.velocity.y > 0 && (!playerInput.JumpInput || playerAutoAirMove.IsInAirMove()))
        {
            gravityApply[0] = false;
            gravityApply[1] = false;    //down
            gravityApply[2] = true;
        }
    }

    /// <summary>
    /// est appelé à chaque fixedUpdate, VIA l'event worldCollision
    /// </summary>
    public void ApplyForce()
    {
        ApplyGravity();
    }
    
    /// <summary>
    /// désactive le script
    /// </summary>
    private void StopAction()
    {
        stopAction = false;
    }
    #endregion

    #region Unity ending

    private void FixedUpdate()
    {
        ApplyForce();
    }

    private void OnDisable()
    {
        EventManager.StopListening(GameData.Event.GameOver, StopAction);
    }

    #endregion
}
