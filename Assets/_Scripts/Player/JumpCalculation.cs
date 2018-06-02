using Obi;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using WorldCollisionNamespace;

/// <summary>
/// JumpCalculation Description
/// </summary>
//[Serializable]
public class JumpCalculation : MonoBehaviour
{
    #region Attributes
    public int initialParticleRope = 28;
    public int initialPooledParticle = 100;
    public float anchorRadius = 10f;

    [FoldoutGroup("GamePlay"), Tooltip("Marge des 90° du jump (0 = toujours en direction de l'arrow, 0.1 = si angle(normal, arrow) se rapproche de 90, on vise le millieu normal-arrow"), SerializeField]
    private float margeHoriz = 0.1f;

    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private ObiActor actor;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private ObiRope rope;

    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private RopeHandler ropeHandler;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerJump playerJump;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerAutoAirMove playerControlledAirJump;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerInput playerInput;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private WorldCollision worldCollision;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerGrip grip;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private Rigidbody rb;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerAirJump playerAirJump;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerController playerController;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayersManager playerManager;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerPenduleMove playerAirMove;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private ExternalForce externalForce;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerPenduleMove playerPenduleMove;
    #endregion

    #region Initialization
    private void Awake()
    {
        Init();
    }
    /// <summary>
    /// init les ref du player
    /// </summary>
    public void Init()
    {
        initialPooledParticle = rope.PooledParticles;
        initialParticleRope = actor.velocities.Length - initialPooledParticle;
    }
    #endregion

    #region Core
   
    /// <summary>
    /// ici jump par rapport à la normal du sol uniquement !
    /// Appelé à l'arret sur sol, ou sur plafond
    /// </summary>
    private void JumpFromNormal()
    {
        //Debug.Log("jump from normla ground");
        Jump(worldCollision.GetSumNormalSafe());
    }
    private void JumpFromCeilling()
    {
        if (!ropeHandler.IsTenseForJump && !playerManager.IsOtherIsGripped(playerController.IdPlayer))
        {
            Jump(worldCollision.GetSumNormalSafe());
        }
        else
        {
            Debug.Log("ceilling but tense");
            //ici la corde est tendu, appliquer le jump normalisé par rapport à la corde
            //appliquer un boost de puissance par rapport à playerJump setting
            Vector3 dir = GetTheGoodAngleWhenRopeTense();
            playerControlledAirJump.ControlledAirJumpSetup(true, dir, true);

            //Jump(dir, false, 0, boost);
        }
               
    }

    /// <summary>
    /// le jump de base, millieu de direction horizontal, Vecteur up
    /// </summary>
    private Vector3 GetInputAndUpDirection()
    {
        Vector3 dirInputPlayer = playerInput.GetDirInput(); //input du player
        dirInputPlayer.y = dirInputPlayer.z = 0;            //ne garder que l'horizontal
        Vector3 dir = QuaternionExt.GetMiddleOf2Vector(Vector3.up, dirInputPlayer);
        return (dir);
    }
    /// <summary>
    /// le jump de base, millieu de direction horizontal, Vecteur up
    /// </summary>
    private Vector3 GetVelocityAndUpDirection()
    {
        Vector3 dirInputPlayer = rb.velocity;
        dirInputPlayer.y = dirInputPlayer.z = 0;            //ne garder que l'horizontal
        Vector3 dir = QuaternionExt.GetMiddleOf2Vector(Vector3.up, dirInputPlayer);
        return (dir);
    }

    /// <summary>
    /// le jump de base avec le joueur en mouvement:
    /// millieu de direction horizontal, Vecteur up
    /// </summary>
    private void SimpleJumpMoving()
    {
        Vector3 dir = GetInputAndUpDirection();
        Jump(dir);
    }

    /// <summary>
    /// renvoi vrai si on peut se déplacer vers la où on veut en tense
    /// </summary>
    /// <returns></returns>
    public Vector3 GetTheGoodAngleWhenTenseInAirMove(Vector3 dirPlayer)
    {
        //si on est grounded, faire gaffe à pas bouger si on vise le mur !

        Vector3 dirInversePlayer = -dirPlayer;                  //une fois que j'ai mon vecteur, prendre l'inverse
        Vector3 dirNormal = worldCollision.GetSumNormalSafe();  //prend la normal

        Debug.DrawRay(transform.position, dirPlayer * 2, Color.green, 1f);
        Debug.DrawRay(transform.position, dirInversePlayer * 2, Color.red, 1f);

        //float anglePlayer = QuaternionExt.GetAngleFromVector(dirPlayer);
        float angleInverse = QuaternionExt.GetAngleFromVector(dirInversePlayer);
        float angleNormal = QuaternionExt.GetAngleFromVector(dirNormal);

        float diffAngleNormalInverse;
        bool isCLose = QuaternionExt.IsAngleCloseToOtherByAmount(angleNormal, angleInverse, playerAirMove.DiffAngleWhenWeCantMoveThisWay, out diffAngleNormalInverse);

        //Debug.Log("Diff angleNormal/angleInverse: " + diffAngleNormalInverse);

        if (!isCLose)
        {
            return (dirPlayer);
        }

        //Debug.Break();

        return (Vector3.zero);
    }

    /// <summary>
    /// retourne l'angle correct
    /// </summary>
    /// <returns></returns>
    public Vector3 GetTheGoodAngleWhenRopeTense()
    {
        Vector3 dirRope = ropeHandler.GetVectorFromPlayer(playerController.IdPlayer);
        //Vector3 dirRope = playerPenduleMove.GetDirRopeFromPivot();

        Vector3 dirReference = worldCollision.GetSumNormalSafe();

        return (QuaternionExt.GetTheGoodRightAngleClosest(dirRope, dirReference, 10f));
    }

    /// <summary>
    /// gère le saut depuis le sol
    /// </summary>
    private void JumpFromGround()
    {
        //si on ne bouge pas
        if (playerInput.NotMoving())
        {
            SimpleJumpMoving();
            //JumpFromNormal();   //saute par rapport à la normal au sol
        }
        //ici la corde n'est pas tendu
        else if (!ropeHandler.IsTenseForJump)
        {
            SimpleJumpMoving(); //jump normalement en faisant le milieu de vecteur up & direction input
        }
        else
        {
            //ici la corde est tendu, appliquer le jump normalisé par rapport à la corde
            //appliquer un boost de puissance par rapport à playerJump setting
            Vector3 dir = GetTheGoodAngleWhenRopeTense();
            //divisé par 2 ?
            dir = QuaternionExt.GetMiddleOf2Vector(dir, ropeHandler.GetVectorFromPlayer(playerController.IdPlayer));
            dir = QuaternionExt.GetMiddleOf2Vector(dir, ropeHandler.GetVectorFromPlayer(playerController.IdPlayer));

            //dir = QuaternionExt.GetMiddleOf2Vector(dir, playerPenduleMove.GetDirRopeFromPivot());
            //dir = QuaternionExt.GetMiddleOf2Vector(dir, playerPenduleMove.GetDirRopeFromPivot());
            //playerPenduleMove.GetDirRopeFromPivot();

            playerControlledAirJump.ControlledAirJumpSetup(true, dir, true, playerJump.JumpTenseFromGroundRatio);

            //Jump(dir, false, 0, boost);
        }
    }

    /// <summary>
    /// ici la corde est tendu, et on est en wallJump
    /// OU: la corde n'est pas tendu, on est en wallJump, ET on est en bas comparé à l'autre
    /// </summary>
    private void WallJumpTense()
    {
        Debug.Log("iic jump selon normal / rope / player / right/left");
        Vector3 dir = GetTheGoodAngleWhenRopeTense();
        //float boost = playerJump.JumpBoostFromWallWhenRopeTense;

        playerControlledAirJump.ControlledAirJumpSetup(true, dir);
        //Jump(dir, false, 0, boost);
    }

    private void WallJumpSimple()
    {
        Debug.Log("ici jump wall selon wallLeft/right et Up");
        Vector3 horiz = new Vector3(( (worldCollision.GetCollisionSafe() == CollisionType.WallLeft) ? 1 : -1), 0, 0);
        Vector3 dir = QuaternionExt.GetMiddleOf2Vector(Vector3.up, horiz);
        float boost = playerJump.JumpBoostFromWall;
        Jump(dir, false, 0, boost);
    }

    /// <summary>
    /// ici jump from wall
    /// </summary>
    private void JumpFromWall()
    {
        Debug.Log("ici jump from wall");
        //ici la corde n'est pas tendu
        if (/*ropeHandler.IsTenseForJump && */playerManager.IsOtherIsGripped(playerController.IdPlayer))
        {
            //ici la corde est tendu, appliquer le jump normalisé par rapport à la corde
            WallJumpTense();
        }
        else
        {
            WallJumpSimple();
        }
    }

    /// <summary>
    /// ici effectue un simple jump
    /// </summary>
    private void SimpleJump()
    {
        //get la collision
        CollisionSimple collisionJump = worldCollision.GetSimpleCollisionSafe();

        switch (collisionJump)
        {
            case CollisionSimple.Ceilling:
                Debug.Log("jumpFromCeilling");
                JumpFromCeilling();
                break;
            case CollisionSimple.Ground:
                Debug.Log("jumpFromGround");
                JumpFromGround();
                break;
            case CollisionSimple.Wall:
                if (worldCollision.IsOnCoinGround())
                {
                    Jump(Vector3.up);
                    break;
                }

                JumpFromWall();
                break;
            default:
                Debug.Log("alors la je sais pas...");
                break;
        }
    }
    
    /// <summary>
    /// ici effectue un double jump
    /// on est en l'air à cous sur
    /// </summary>
    private void AirJump()
    {
        //si dans les option du airJump, on veut garder l'axe de velocité...
        if (playerAirJump.KeepHorizontalAxis)
        {
            Vector3 dir = GetVelocityAndUpDirection();
            playerAirJump.TryToAirJump(dir);
        }
        else
        {
            Vector3 dir = GetInputAndUpDirection();
            playerAirJump.TryToAirJump(dir);
        }

    }

    /// <summary>
    /// retourne la direction quand on saute...
    /// </summary>
    /// <returns></returns>
    private Vector3 GetDirWhenJumpAndMoving()
    {
        Vector3 finalVelocityDir = Vector3.zero;

        //get la direction du joystick
        Vector3 dirInputPlayer = playerInput.GetDirInput();

        //get le dot product normal -> dir Arrow
        float dotDirPlayer = QuaternionExt.DotProduct(worldCollision.GetSumNormalSafe(), dirInputPlayer);

        //si positif, alors on n'a pas à faire de mirroir
        if (dotDirPlayer > margeHoriz)
        {
            //direction visé par le joueur
            Debug.Log("Direction de l'arrow !" + dotDirPlayer);
            finalVelocityDir = dirInputPlayer.normalized;
        }
        else if (dotDirPlayer < -margeHoriz)
        {
            //ici on vise dans le négatif, faire le mirroir du vector par rapport à...
            Debug.Log("ici mirroir de l'arrow !" + dotDirPlayer);

            //récupéré le vecteur de DROITE de la normal
            Vector3 rightVector = QuaternionExt.CrossProduct(worldCollision.GetSumNormalSafe(), Vector3.forward) * -1;
            //Debug.DrawRay(transform.position, rightVector.normalized, Color.blue, 1f);

            //faire le mirroir entre la normal et le vecteur de droite
            Vector3 mirror = QuaternionExt.ReflectionOverPlane(dirInputPlayer, rightVector * -1) * -1;
            //Debug.DrawRay(transform.position, mirror.normalized, Color.yellow, 1f);

            //direction inverse visé par le joueur
            finalVelocityDir = mirror.normalized;
        }
        else
        {
            Debug.Log("ici on est proche du 90°, faire la bisection !");
            //ici l'angle normal - direction est proche de 90°, ducoup on fait le milieu des 2 axe
            //ici faire la moyenne des 2 vecteur normal, et direction arrow
            finalVelocityDir = QuaternionExt.GetMiddleOf2Vector(worldCollision.GetSumNormalSafe(), dirInputPlayer);
        }
        return (finalVelocityDir);
    }

    /// <summary>
    /// jump à une direction donnée
    /// </summary>
    /// <param name="dir"></param>
    private void Jump(Vector3 dir, bool applyThisForce = false, float force = 0, float boost = 1)
    {
        if (dir == Vector3.zero)
        {
            dir = Vector3.up;
            //ici pas de rotation ?? 
            Debug.LogWarning("pas de rotation ! up de base !");
        }
        /*if (!playerJump.CoolDownJump.IsReady())
        {
            return;
        }
        playerJump.PrepareAndJump(dir);
        externalForce.SetBigMass();   //set sa propre mass big !
        playerController.PlayersManager.Jump(playerController.IdPlayer);
        playerJump.Jump(dir, applyThisForce, force, boost);
        */
        StartCoroutine(JumpNextFrame(dir, applyThisForce, force, boost));
    }

    
    /// <summary>
    /// ici jump, mais just après la frame qui fait buggé
    /// </summary>
    /// <returns></returns>
    private IEnumerator JumpNextFrame(Vector3 dir, bool applyThisForce = false, float force = 0, float boost = 1)
    {
        yield return new WaitForEndOfFrame();
        if (!playerJump.CoolDownJump.IsReady())
        {
            yield break;
        }

        playerJump.PrepareAndJump(dir);
        externalForce.SetBigMass();   //set sa propre mass big !
        playerController.PlayersManager.Jump(playerController.IdPlayer);
        playerJump.Jump(dir, applyThisForce, force, boost);
    }
    

    /// <summary>
    /// ici tente de jumper
    /// </summary>
    public void TryToJump()
    {
        if (!playerJump.CanJump())
            return;

        //grip.ResetGrip();

        if (!worldCollision.IsGroundedSafe() && !worldCollision.IsGroundeExeptionSafe())
        {
            //Debug.Log("ici jump double");
            AirJump();
        }
        else
        {

            /*if (worldCollision.IsGroundeExeptionSafe() && playerManager.AreBothNotGrounded())
            {
                Debug.Log("si les 2 joueurs sont en l'air, ne pas sauter !");
                return;
            }*/
            //Debug.Log("ici jump sumple");
            SimpleJump();
        }
    }
    #endregion

    #region Unity ending functions

    #endregion
}
