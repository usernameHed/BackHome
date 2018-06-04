using Sirenix.OdinInspector;
using System;
using UnityEngine;
using WorldCollisionNamespace;

[RequireComponent(typeof(Rigidbody))]
public class PlayerJump : MonoBehaviour
{
    #region Attributes
    [FoldoutGroup("GamePlay"), OnValueChanged("InitValue"), Tooltip("hauteur maximal du saut"), SerializeField]
    private float jumpHeight = 2.0f;
    [FoldoutGroup("GamePlay"), Tooltip("gravité du saut"), SerializeField]
    private float gravity = 9.81f;
    [FoldoutGroup("GamePlay"), Tooltip("gravité du saut"), SerializeField]
    private float jumpTenseFromGroundRatio = 0.1f;
    public float JumpTenseFromGroundRatio { get { return (jumpTenseFromGroundRatio); } }
    
    [FoldoutGroup("GamePlay"), Tooltip("jumper constament en restant appuyé ?"), SerializeField]
    private bool stayHold = false;
    [FoldoutGroup("GamePlay"), Tooltip("jumper constament en restant appuyé ?"), SerializeField]
    private bool allowWallJump = true;
    public bool StayHold { get { return (stayHold); } }
    [FoldoutGroup("GamePlay"), Tooltip("jumper constament en restant appuyé ?"), SerializeField]
    private float jumpBoostFromWall = 1.5f;
    public float JumpBoostFromWall { get { return (jumpBoostFromWall); } }
    [FoldoutGroup("GamePlay"), Tooltip("cooldown du jump"), SerializeField]
    private FrequencyCoolDown coolDownJump;
    public FrequencyCoolDown CoolDownJump { get { return (coolDownJump); } }

    [FoldoutGroup("GamePlay"), Tooltip("vibration quand on jump"), SerializeField]
    private Vibration onJump;
    [FoldoutGroup("GamePlay"), Tooltip("vibration quand on se pose"), SerializeField]
    private Vibration onGrounded;

    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private WorldCollision worldCollision;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private Rigidbody rb;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerInput playerInput;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerController playerController;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerGrip grip;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private JumpCalculation jumpCalculation;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private ExternalForce externalForce;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerAirJump playerAirJump;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerGrip playerGrip;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerAutoAirMove playerControlledAirJump;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayersManager playerManager;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private AnimController animController;

    private bool jumpStop = false;                      //forcer l'arrêt du jump, pour forcer le joueur a lacher la touche
    public bool JumpStop { get { return (jumpStop); } }
    private bool hasJumpAndFlying = false;              //a-t-on juste jumpé ?
    public bool HasJumpAndFlying { get { return (hasJumpAndFlying); } }
    private bool stopAction = false;                    //le joueur est-il stopé ?

    #endregion

    #region Initialize
    private void OnEnable()
    {
        EventManager.StartListening(GameData.Event.GameOver, StopAction);
        EventManager.StartListening(GameData.Event.Grounded, GroundedAction);
        InitValue();
    }

    private void InitValue()
    {
        jumpStop = false;
        hasJumpAndFlying = false;
        stopAction = false;
        jumpHeight = ScoreManager.Instance.Data.GetSimplified() ? 25 : 15;
        stayHold = ScoreManager.Instance.Data.GetSimplified();
    }
    #endregion

    #region Core

    /// <summary>
    /// renvoi vrai ou faux si on a le droit de sauter
    /// </summary>
    /// <returns></returns>
    public bool CanJump()
    {
        //on touche pas à la touche saut
        if (!playerInput.JumpInput)
            return (false);

        //faux si on hold pas et quand a pas laché
        if (jumpStop && (!stayHold || (stayHold && playerInput.GripInput && ScoreManager.Instance.Data.GetSimplified())))
            return (false);

        if (playerInput.GripInput && ScoreManager.Instance.Data.GetSimplified())
        {
            return (false);
        }

        //faux si le cooldown n'est pas fini
        if (!coolDownJump.IsReady())
            return (false);

        if (!worldCollision.IsGroundedSafe() && worldCollision.IsGroundeExeptionSafe() && playerManager.AreBothNotGrounded())
        {
            Debug.Log("si les 2 joueurs sont en l'air, ne pas sauter !");
            return (false);
        }

        if (!allowWallJump && !worldCollision.IsOnFloor())
            return (false);


        //ici on est ok pour sauter (tester si onground pour jump ou double jump ou rien)
        Collider coll;
        if (!worldCollision.PlatformManager.IsJumpable(out coll))
            return (false);

        return (true);
    }

    /// <summary>
    /// c'est ici qu'on prépare le jump de base !
    /// </summary>
    /// <param name="dir"></param>
    public void PrepareAndJump(Vector3 dir)
    {
        worldCollision.HasJustJump();   //cooldown des worldCollision
        coolDownJump.StartCoolDown();   //le coolDown normal du jump
        playerAirJump.CoolDownBeforeFirstAirJump.StartCoolDown();   //démar le air jump cool Down

        //PlayerConnected.Instance.setVibrationPlayer(playerController.IdPlayer, onJump); //set vibration de saut
        hasJumpAndFlying = true; //on vient de sauter ! tant qu'on retombe pas, on est vrai

        Debug.DrawRay(transform.position, dir, Color.red, 5f);
        GameObject particle = ObjectsPooler.Instance.SpawnFromPool(GameData.PoolTag.ParticleBump, transform.position, Quaternion.identity, ObjectsPooler.Instance.transform);
        particle.transform.rotation = QuaternionExt.LookAtDir(dir * -1);

        //SoundManager.Instance.PlaySound("Play_sfx3D" + transform.GetInstanceID());
        SoundManager.Instance.PlaySound("Play_Jump");
        animController.AnimJump();
        //externalForce.SetBigMass();   //set sa propre mass big !
        //playerController.PlayersManager.Jump(playerController.IdPlayer);
    }

    /// <summary>
    /// jump !
    /// </summary>
    public void Jump(Vector3 dir, bool applyThisForce = false, float force = 1, float boost = 1)
    {
        //s'il n'y a pas de direction, erreur ???
        if (dir == Vector3.zero)
        {
            dir = Vector3.up;
            //ici pas de rotation ?? 
            Debug.LogWarning("pas de rotation ! up de base !");
        }

        grip.ResetGrip();

        Debug.Log("actualyJump !");
        Debug.DrawRay(transform.position, dir * 10, Color.red, 1f);
        
        //applique soit la force de saut, soit la force défini dans les parametres
        Vector3 jumpForce = (!applyThisForce) ? dir * CalculateJumpVerticalSpeed() * boost : dir * force * boost;

        

        //Debug.Log("et ici la force: " + jumpForce);
        rb.velocity = jumpForce;
        //Debug.Log("ici jump");

        if (!stayHold)
            jumpStop = true;
    }

    /// <summary>
    /// ici est appelé quand on vient d'atterrire après un saut ! 
    /// </summary>
    private void IsJustGrounded()
    {
        if (hasJumpAndFlying && worldCollision.CoolDownGroundedJump.IsReady() && worldCollision.IsGroundedSafe())   //on a sauté ! ducoup on est ok pour se poser
        {
            SoundManager.Instance.PlaySound("Play_pas");
            hasJumpAndFlying = false;
            playerControlledAirJump.ControlledAirJumpSetup(false, Vector3.zero);
            //PlayerConnected.Instance.setVibrationPlayer(playerController.IdPlayer, onGrounded);
            playerController.PlayersManager.Jump(playerController.IdPlayer);

            playerController.PlayersManager.ResetOtherWhenGrounded(playerController.IdPlayer);
            externalForce.ResetInitialParameter();

            playerGrip.TryToAutoGrip();

            playerAirJump.ResetAirJump();   //reset les air jump
        }
    }

    float CalculateJumpVerticalSpeed()
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2 * jumpHeight * gravity);
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
    /// <summary>
    /// cette fonction est appelé par l'event WorldCollision en fixedUpdate
    /// (ici on sait si on est grounded ou pas, et comment)
    /// </summary>
    private void GroundedAction()
    {
        if (stopAction)
            return;

        IsJustGrounded(); //ok on vient d'être grounded !
        externalForce.TestForResetMass();          //ici essai de setup le mode flying (si on a été appelé par plaersManagers Jump ?

        jumpCalculation.TryToJump();
        //externalForce.ApplyForce();
        //externalForce.CeillingForceUp();
    }

    private void Update()
    {
        //on lache, on autorise le saut encore
        if (playerInput.JumpUpInput)
            jumpStop = false;
    }

    private void OnDisable()
    {
        EventManager.StopListening(GameData.Event.GameOver, StopAction);
        EventManager.StopListening(GameData.Event.Grounded, GroundedAction);
    }

    #endregion
}
