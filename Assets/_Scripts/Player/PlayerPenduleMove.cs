using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using WorldCollisionNamespace;

[RequireComponent(typeof(Rigidbody))]
public class PlayerPenduleMove : SerializedMonoBehaviour
{
    #region Attributes
    ///    1  |  2
    ///    ___5___
    ///    
    ///    3  |  4
    public enum PosQuadran
    {
        upLeft = 1,
        upRight = 2,
        downLeft = 3,
        downRight = 4,
        ambigous = 5,
    }

    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    private float airForce = 1f;
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    private float airForceMin = 2f;
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    private float airForceMax = 8f;
    [FoldoutGroup("GamePlay"), Tooltip("facteur de gravité de la corde pour qu'elle soit tendu"), SerializeField]
    private float tenseRopeWhenGripped = 2.5f;
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    private float airForceToAdd = 10f;
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    private float ratioWhenRemove = 2;

    [Space(10)]
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    private bool isOnInverse = false;
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    private float currentInverseForce = 1f;
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    private float airInverseMin = 1f;
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    private float airForceInverseToAdd = 3f;

    [Space(10)]

    [FoldoutGroup("GamePlay"), Tooltip("quand le pendule à une vitesse haute (4 ?)"), SerializeField]
    private float whenAirForceIsHigh = 4;
    [FoldoutGroup("GamePlay"), Tooltip("alors lorsqu'on inverse, diminuer de base de 2"), SerializeField]
    private float amountToRemoveWhenInverse = 2;


    [Space(30)]
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    private bool isAirTense = false;
    public bool IsAirTense { get { return (isAirTense); } }
    
    [FoldoutGroup("GamePlay"), Tooltip("facteur de gravité down"), SerializeField]
    private float littleForceAtTheEnd = 2.5f;

    [FoldoutGroup("GamePlay"), Tooltip("facteur de gravité down"), SerializeField]
    private float clampVelocityWhenAirMove = 5f;


    [Space(10)]
    [FoldoutGroup("GamePlay"), Tooltip("temps de stun où la gravité n'est plus appliqué..."), SerializeField]
    private float timerStunGravity = 1.5f;

    [FoldoutGroup("GamePlay"), Tooltip("temps où le airMove est toujours actif quand c'est fini..."), SerializeField]
    private FrequencyCoolDown coolDownStilAccrocheABitWhenEndTense;

    [Space(10)]
    [FoldoutGroup("Pendule"), Tooltip("arrete le mouvement quand on on arrive à l'horizontal"), SerializeField]
    private bool stopWhenHoriz = false;
    [FoldoutGroup("Pendule"), Tooltip("arrete le mouvement quand on on arrive à l'horizontal"), SerializeField]
    private bool activeTimer = false;
    [FoldoutGroup("Pendule"), Tooltip("temps où le airMove n'est pas actif..."), SerializeField]
    private FrequencyCoolDown coolDownInactiveAirMove;
    [FoldoutGroup("Pendule"), Tooltip("margin accepté de distance !"), SerializeField]
    private float minMaxMargin = 0.5f;
    [FoldoutGroup("Pendule"), Tooltip("margin accepté de distance !"), SerializeField]
    private int particleMargin = 1;

    [FoldoutGroup("Pendule"), Tooltip("margin accepté de distance !"), SerializeField]
    private float angleDifferenceMarginForPivotPoint = 10f;

    [FoldoutGroup("Pendule"), Tooltip("Si la différence d'angle entre 2 points est inférieur ou égal à 1, ne pas incrémenter la différence !"), SerializeField]
    private float marginAngleConsideredAsEqual = 1f;

    [FoldoutGroup("Pendule"), Tooltip("matrice de distance"), SerializeField]
    private float[,] matriceDistanceParticle;

    [FoldoutGroup("PenduleForce"), Tooltip("différence d'angle accepté quand on pousse du mauvais coté !"), SerializeField]
    private float angleDiffForRopeWrongSide = 20f;
    [FoldoutGroup("PenduleForce"), Tooltip("différence quand la vitesse est grande !"), SerializeField]
    private float angleDiffForRopeWrongSideAndFast = 40f;
    [FoldoutGroup("PenduleForce"), Tooltip("quand la velocity du rigidbody a atteint une certaine vitesse, appliquer la force red sur tout le cadran 3"), SerializeField]
    private float velocityWhenAngleDontMatterOnRed = 6f;

    [FoldoutGroup("PenduleForce"), Tooltip("quand on est coté haut, et qu'on descent... (GREEN), applique ou non la force vers la velocité ?"), SerializeField]
    private bool GREENupApplyAccelerationOnDecent = true;
    [FoldoutGroup("PenduleForce"), Tooltip("quand on est coté haut, et qu'on descent... (MAJENTA), applique ou non la force inverse à la velocité ?"), SerializeField]
    private bool MAJENTAupApplyDecelerationOnDecent = true;
    [FoldoutGroup("PenduleForce"), Tooltip("velocité minimum du rigidbody requise pour appliquer la force"), SerializeField]
    private float velocityMinForMajenta = 2f;


    [FoldoutGroup("PenduleForce"), Tooltip("quand on est coté haut, et qu'on monte... (RED), applique ou non la force vers la velocité ?"), SerializeField]
    private bool RedupApplyAccelerationOnUp = true;
    [FoldoutGroup("PenduleForce"), Tooltip("quand on est coté haut, et qu'on descent... (YELLOW), applique ou non la force inverse à la velocité ?"), SerializeField]
    private bool YELLOWupApplyDecelerationOnUp = true;

    



    [Space(10)]
    [FoldoutGroup("Debug"), Tooltip("est-ce qu'on est en mode jump controllé ?"), SerializeField]
    private bool isOnAirMove = false;

    [FoldoutGroup("Debug"), Tooltip("facteur de gravité down"), SerializeField]
    private float debugMarginUnderPlayer = 3.0f;

    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private float diffAngleWhenWeCantMoveThisWay = 80f;
    public float DiffAngleWhenWeCantMoveThisWay { get { return (diffAngleWhenWeCantMoveThisWay); } }

    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerInput playerInput;
    public PlayerInput PlayerInputScript { get { return (playerInput); } }

    private bool notApplyGravityAanymore = false;
    public bool NotApplyGravityAanymore { get { return (notApplyGravityAanymore); } }

    private bool stopAction = false;    //le joueur est-il stopé ?
    private Vector3 lastDirTensity;     //la dernière direction du joueur
    private Vector3 dirPlayerAirMove = Vector3.zero;

    private Vector3 previousControllerJumpDir = Vector3.zero;
    private Vector3 pointPivot = Vector3.zero;

    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private WorldCollision worldCollision;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private Rigidbody rb;           //ref du rb
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerGrip grip;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private RopeHandler ropeHandler;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerController playerController;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayersManager playerManager;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private ExternalForce externalForce;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerJump playerJump;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerGrip playerGrip;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerAutoAirMove playerControlledAirJump;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private JumpCalculation jumpCalculation;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerGetFat playerGetFat;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerMove playerMove;
    #endregion

    #region Initialize
    private void OnEnable()
    {
        EventManager.StartListening(GameData.Event.GameOver, StopAction);
        EventManager.StartListening(GameData.Event.Grounded, GroundedAction);
        InitPlayer();
    }

    /// <summary>
    /// init le player
    /// </summary>
    private void InitPlayer()
    {
        stopAction = false;
        AddForcePendule(0);
    }
    #endregion

    #region Core

    private bool IsOnRightDist(int numberParticle, float distance)
    {
        for (int i = 0; i < matriceDistanceParticle.GetLength(1); i++)
        {
            //Debug.Log(matriceDistanceParticle[0, i] + " " + matriceDistanceParticle[1, i]);

            if (numberParticle == (int)matriceDistanceParticle[0, i])
            {
                //ici on a le nombre de particle
                //Debug.Log("Distance autorisé: " + matriceDistanceParticle[1, i] + ", avec plus ou moin: " + minMaxMargin);
                return (IsMarginOk(distance, matriceDistanceParticle[1, i]));
            }
            else if (Mathf.Abs(numberParticle - (int)matriceDistanceParticle[0, i]) <= particleMargin)
            {
                //Debug.Log("ici particle: " + numberParticle + ", on est à moin ou egal de " + particleMargin + ". ");

                if (i + 1 >= matriceDistanceParticle.GetLength(1))
                {
                    return (IsMarginOk(distance, matriceDistanceParticle[1, i]));
                }
                else
                {
                    //Debug.Log("ducoup, on prend le milieu entre " + matriceDistanceParticle[1, i] + " et " + matriceDistanceParticle[1, i + 1]);
                    return (IsMarginOk(distance, (matriceDistanceParticle[1, i] + matriceDistanceParticle[1, i + 1]) / 2));
                }
            }
        }
        return (false);
    }

    /// <summary>
    /// ici distancePlayers: la distance actuelle, 
    /// distanceAutorised: la distance autorisé selon le nombre actuel de particule dans la rope
    /// </summary>
    private bool IsMarginOk(float distancePlayers, float distanceAutorised)
    {
        //Debug.Log("distance players: " + distancePlayers);
        //Debug.Log("distance autorisé: " + distanceAutorised);

        if (Mathf.Abs(distancePlayers - distanceAutorised) <= minMaxMargin)
        {
            //Debug.Log("ok move");
            return (true);
        }
        else
        {
            //Debug.Log("NOOO");
            return (false);
        }
    }

    /// <summary>
    /// retourne le point pivot (demande de recalculer ou pas)
    /// </summary>
    /// <param name="recalculate"></param>
    /// <returns></returns>
    public Vector3 GetPivotPoint(bool recalculate)
    {
        if (recalculate)
            CheckDistanceAndNumberParticle();
        return (pointPivot);
    }
    /// <summary>
    /// ici n'active pas si la distance des 2 joueurs est trop courte
    /// par rapport aux nombre de particule dans la chaine
    /// </summary>
    /// <returns></returns>
    private bool CheckDistanceAndNumberParticle()
    {
        //int numberParticle = ropeHandler.ParticleInRope;
        float distance = 0;
        int numberParticleFromPlayerToPivot = 0;
        pointPivot = Vector3.zero;
        bool onRope = false;

        ropeHandler.GetPivotPoint(playerController.IdPlayer, angleDifferenceMarginForPivotPoint, marginAngleConsideredAsEqual, out distance, out numberParticleFromPlayerToPivot, out pointPivot, out onRope);

//DebugExtension.DebugWireSphere(pointPivot, Color.magenta, 1f, 2f);
//Debug.Log("distance: " + distance);
//Debug.Log("number particle: " + numberParticleFromPlayerToPivot);

        //Debug.Log("numberParticle: " + numberParticle + ", distance: " + distance);
        if (IsOnRightDist(numberParticleFromPlayerToPivot, distance))
        {
            //ici on est dans la bonne distance
            //Debug.Log("ici OK");
            if (onRope)
            {
                GameObject particle = ObjectsPooler.Instance.SpawnFromPool(GameData.PoolTag.ParticleRopeTense, pointPivot, Quaternion.identity, ObjectsPooler.Instance.transform);
                particle.transform.rotation = QuaternionExt.LookAtDir(QuaternionExt.CrossProduct(pointPivot - transform.position, Vector3.forward) * -1);
            }

            //MessageBox.Show("Something happened. Attach the debugger, add a breakpoint to the line after this one, then press OK.");
            //"does nothing".ToString(); // place breakpoint here
            //Debug.Break();
            return (true);
        }
        //Debug.Log("ici PAS OK");
        //MessageBox.Show("Something happened. Attach the debugger, add a breakpoint to the line after this one, then press OK.");
        //"does nothing".ToString(); // place breakpoint here
        //Debug.Break();
        //ici dans la mauvaise distance
        return (false);
    }

    /// <summary>
    /// est-ce qu'on peut bouger droite / gauche ?
    /// </summary>
    /// <returns></returns>
    private bool CanMove()
    {
        //si on ne bouge pas les input, ne rien faire
        if (PlayerInputScript.Horiz == 0 && PlayerInputScript.Verti == 0)
            return (false);

        
        //si on est dessous, ne pas activer le airMove
        if (!playerManager.IsUnderOtherPlayer(playerController.IdPlayer, 0.5f)
            && !playerManager.IsOtherIsGripped(playerController.IdPlayer)
            && stopWhenHoriz)
        {
            return (false);
        }
        

        

        //si on a juste sauté, ne rien faire
        if (!worldCollision.CoolDownGroundedJump.IsReady() || !worldCollision.CoolDownGrounded.IsReady())
            return (false);

        //si la rope n'est pas assez tendu pour le air move
        if (!ropeHandler.IsTenseForAirMove)
            return (false);

        //ne pas bouger quand on est gripped
        if (grip.Gripped)
            return (false);

        if (!CheckDistanceAndNumberParticle())
            return (false);

        //si on est en auto AirMove... ne rien faire !
        if (playerControlledAirJump.InAutoAirMove)
            return (false);

        //si on est en l'air, et pas sur un objet exeption... ok pendule
        if (!worldCollision.IsGroundedSafe())
        {

            //ici on est peut être pas grounded, mais on peut être sur ExeptionPlayer ??
            if (!worldCollision.IsGroundeExeptionSafe())
            {
                return (true);
            }
            else
            {
                //ici on est en collision avec un player ? OK
            }
        }

        //ici on est grounded ?

        //ici on est au sol, accepté quand même si le cooldown n'est pas fini
        if (!coolDownStilAccrocheABitWhenEndTense.IsReady())
        {
            //ici on est sur le ground, MAIS le cooldown n'est pas fini !
            return (true);
        }
        //on est au sol, on bouge les inputs et les coolDown JUMP & MOVE sont OP !
        return (false);
    }

    /// <summary>
    /// déplace le player
    /// </summary>
    private void TryToMove()
    {
        //ici gérer le air control
        if (!CanMove())
        {
            if (isOnAirMove)
            {
                Debug.Log("air move fin");
                //pointPivot = Vector3.zero;
                coolDownInactiveAirMove.StartCoolDown();
                DebugExtension.DebugWireSphere(transform.position, Color.red, 1f, 1f);
            }
            isOnAirMove = false;
            return;
        }
        if (!isOnAirMove)
        {
            /*//ici test supplémentaire au airMove
            if (!coolDownInactiveAirMove.IsReady() && activeTimer)
            {
                Debug.LogWarning("ici on vient de finir un airMove... attendre avant de commencer le suivant...");
                return;
            }*/
            if (coolDownInactiveAirMove.IsReady())
            {
                //ici on a arreté, et quand on reprend... le timer a dépassé 0.3sec... ducoup
                //forcément on reprend à zero...
                ResetForcePendule();
                InverseReset();
            }

            DebugExtension.DebugWireSphere(transform.position, Color.green, 1f, 1f);
            Debug.Log("air move debut");
            isOnAirMove = true;
            externalForce.ResetInitialParameter();
        }


        AirMove();
    }

    /// <summary>
    /// renvoi vrai si on considère être en mode tense
    /// </summary>
    /// <returns></returns>
    public bool IsAirTenseOrInCoolDown()
    {
        if (isAirTense || !coolDownStilAccrocheABitWhenEndTense.IsReady())
            return (true);
        return (false);
    }

    private void StopAirTense()
    {
        isAirTense = false;
        coolDownStilAccrocheABitWhenEndTense.Reset();
        coolDownInactiveAirMove.Reset();
    }

    /// <summary>
    /// ici reset quand on est grounded
    /// </summary>
    public void ResetWhenGrounded()
    {
        coolDownInactiveAirMove.Reset();
    }
    

    /// <summary>
    /// ici renvoi vrai si on fait juste un jump classique
    /// OU tout simplement: si ce n'est pas un jump extrordinaire (donc, soit un jump, soit.. rien !)
    /// </summary>
    /// <returns></returns>
    private bool IsNormalJumpOrFall()
    {
        if ((playerJump.HasJumpAndFlying && worldCollision.GetPreviousPersistCollision() == CollisionSimple.Ground
            || !playerControlledAirJump.InAutoAirMove) && playerManager.AreOtherOnFloor(playerController.IdPlayer))
        {
            //ici 2 dernier test du desespoire...
            //si on est en dessous
            //Debug.Log("on est ici...");
            if (playerManager.IsUnderOtherPlayer(playerController.IdPlayer, debugMarginUnderPlayer) && worldCollision.CoolDownDesesperateAirMove.IsReady())
            {
                //
                //Debug.Log("ici on est plus bas, et le timer de worldCollision est a zero... on peut plus rien faire !");
                return (false);
            }


            //Debug.Log("ici pas de airJump");
            StopAirTense();
            return (true);
        }
        return (false);
    }

    /// <summary>
    /// effectue des forces lorsqu'on est grounded, mais toujours en coolDown !
    /// </summary>
    /// <returns></returns>
    private bool ForceGroundedInCoolDown()
    {
        //si on est grounded, alors effectuer un calcul supplémentaire pour savoir si on peut aller
        //dans ce sens !
        if (worldCollision.IsGroundedSafe())
        {
            if (coolDownStilAccrocheABitWhenEndTense.IsReady())
            {
                Debug.Log("ici le cooldown est fini, ne rien faire");
                return (false);
            }
            else
            {
                //Debug.Log("ici on peut se déplacer droite / gauche ??");
                dirPlayerAirMove = jumpCalculation.GetTheGoodAngleWhenTenseInAirMove(dirPlayerAirMove);
                if (dirPlayerAirMove == Vector3.zero)
                {
                    //ici on ne peux pas bouger ?
                    //Debug.Log("ici on peut pas bouger dans ce sens ! définir le dirPlayer vers le mur");

                    //Debug.Log("player: " + playerController.IdPlayer + ", collision: " + worldCollision.GetCollisionSafe());
                    if (worldCollision.IsOnFloor())
                    {
                        //Debug.Log("si on est spécifiquement onFloor, se déplacer normalement !");
                        coolDownStilAccrocheABitWhenEndTense.Reset();
                        return (false);
                    }

                    dirPlayerAirMove = worldCollision.GetSumNormalSafe();
                    //PhysicsExt.ApplyConstForce(rb, dirPlayerAirMove, externalForce.ForceOnWallWhenTense);
                    //externalForce.LittleForceDir(dirPlayerAirMove, externalForce.ForceOnWallWhenTense);
                    return (false);
                }
                else
                {
                    //Debug.Log("ici aller... dans le sens de l'input ? on a fini le airMove, on passe au move classique");
                    if (worldCollision.GetPreviousPersistCollision() != CollisionSimple.Ground)
                    {
                        //Debug.Log("or not");
                        return (false);
                    }
                    Vector3 dirInput = playerInput.GetDirInput();
                    playerMove.MoveOnWallWhenAirMove(dirInput);
                    return (false);
                }
            }
        }
        return (true);
    }

    /*
    /// <summary>
    /// est-ce qu'on essai d'aller à l'inverse ?
    /// </summary>
    /// <returns></returns>
    private bool ForceInverse()
    {
        if (!isAirTense)
            return (true);

        Vector3 dirPlayer = dirPlayerAirMove;
        Vector3 dirVelocity = rb.velocity;
        float velocityRigidbody = rb.velocity.sqrMagnitude;

        if (velocityRigidbody < speedWhenCantDoInverse)
            return (true);

        Vector3 dirInverse = -dirPlayer;

        float anglePlayer = QuaternionExt.GetAngleFromVector(dirPlayer);
        float angleInverse = QuaternionExt.GetAngleFromVector(dirInverse);
        float angleVelocity = QuaternionExt.GetAngleFromVector(dirVelocity);

        float diffAnglePlayerVelocity;
        QuaternionExt.IsAngleCloseToOtherByAmount(anglePlayer, angleVelocity, diffAngleInverseVelocity, out diffAnglePlayerVelocity);
        float diffAngleInversePlayerVelocity;
        QuaternionExt.IsAngleCloseToOtherByAmount(angleInverse, angleVelocity, diffAngleInverseVelocity, out diffAngleInversePlayerVelocity);

        //si on veut aller vers la velocity, alors ok
        if (diffAnglePlayerVelocity < diffAngleInversePlayerVelocity)
        {
            //Debug.Log("dans le sens !");
            return (true);
        }
        else
        {
            //Debug.Log("inverse !");
            //sinon, on va trop vite pour pouvoir aller contre !
            return (false);
        }
    }
    */


    /// <summary>
    /// ici test si on a juste inversé le pendule !
    /// </summary>
    private bool WeJustInversePendule()
    {
        //si on vient de commencer, osef, on fait normalement
        if (dirPlayerAirMove == Vector3.zero || lastDirTensity == Vector3.zero)
            return (false);

        float diffVector = QuaternionExt.GetDiffAngleBetween2Vectors(lastDirTensity, dirPlayerAirMove);
        if (diffVector < 90)
        {
            //on a pas inversé
            return (false);
        }
        //on a inversé !
        return (true);
    }

    /// <summary>
    /// ici effectue les mouvements en l'air (selon la corde tendu ?)
    /// </summary>
    private void AirMove()
    {
        Vector3 dirRope = ropeHandler.GetVectorFromPlayer(playerController.IdPlayer);
        Vector3 dirReference = playerInput.GetDirInput();

        dirPlayerAirMove = QuaternionExt.GetTheGoodRightAngleClosest(dirRope, dirReference, 10f);
        

        if (dirPlayerAirMove == Vector3.zero)
            return;

        if (!ForceGroundedInCoolDown())
            return;

        if (WeJustInversePendule())
        {
            //Debug.LogWarning("ici on a inversé !");
            SetInverse();
        }

        //ici on est bon !
        DoNewPenduleForce();
    }

    /// <summary>
    /// retourne 1, 2, 3, 4 si on est a droite, gauche, haut, bas du player
    ///    1  |  2
    ///    _______
    ///    
    ///    3  |  4
    /// </summary>
    /// <param name="pivot"></param>
    /// <param name="posPlayer"></param>
    /// <returns></returns>
    private PosQuadran PendulePart(Vector3 pivot, Vector3 posPlayer)
    {
        if (posPlayer.x > pivot.x && posPlayer.y > pivot.y)
        {
            return (PosQuadran.upRight);
        }
        else if (posPlayer.x < pivot.x && posPlayer.y > pivot.y)
        {
            return (PosQuadran.upLeft);
        }
        else if (posPlayer.x < pivot.x && posPlayer.y < pivot.y)
        {
            return (PosQuadran.downLeft);
        }
        else/* if (posPlayer.x > pivot.x && posPlayer.y < pivot.y)*/
        {
            return (PosQuadran.downRight);
        }
        //return (PosQuadran.ambigous);
    }

    /// <summary>
    /// ici renvoi vrai si le sens du mouvement va vers le haut
    /// </summary>
    /// <returns></returns>
    private bool IsVelocityIsInGoodSide(PosQuadran posInSpace)
    {
        float dirRigidbody = 0;

        switch (posInSpace)
        {
            case PosQuadran.upLeft:
                dirRigidbody = QuaternionExt.DotProduct(-Vector3.right, rb.velocity);

                if (dirRigidbody > 0)
                {
                    return (true);
                }
                else
                {
                    return (false);
                }

            case PosQuadran.upRight:
                dirRigidbody = QuaternionExt.DotProduct(Vector3.right, rb.velocity);

                if (dirRigidbody > 0)
                {
                    return (true);
                }
                else
                {
                    return (false);
                }

            case PosQuadran.downLeft:
                dirRigidbody = QuaternionExt.DotProduct(Vector3.right, rb.velocity);

                if (dirRigidbody > 0)
                {
                    return (true);
                }
                else
                {
                    return (false);
                }

            case PosQuadran.downRight:
                dirRigidbody = QuaternionExt.DotProduct(-Vector3.right, rb.velocity);

                if (dirRigidbody > 0)
                {
                    return (true);
                }
                else
                {
                    return (false);
                }

            case PosQuadran.ambigous:
            default:
                return (false);
        }
    }

    /// <summary>
    /// renvoi vrai si la positionInSpace est en haut
    /// </summary>
    private bool IsUpward(PosQuadran posInSpace)
    {
        if (posInSpace == PosQuadran.upLeft || posInSpace == PosQuadran.upRight)
            return (true);
        return (false);
    }

    /// <summary>
    /// est-ce que l'input est dans le sens du mouvement ?
    /// </summary>
    /// <returns></returns>
    private bool IsGoodInputToVelocy()
    {
        float dirinput = QuaternionExt.DotProduct(playerInput.GetDirInput(), rb.velocity);

        if (dirinput > 0)
        {
            return (true);
        }
        else
        {
            return (false);
        }
    }

    /// <summary>
    ///checker l'angle Vector.down & player - pivot
    ///si angle inférieur à 20, alors on est au début de la monté, autoriser (++force)
    /// </summary>
    /// <returns></returns>
    private bool IsAngleDownOk(float angleAccepted)
    {
        Vector3 dirDown = -Vector3.up;
        Vector3 dirPivot = transform.position - pointPivot;

        float diffAngle = QuaternionExt.GetDiffAngleBetween2Vectors(dirDown, dirPivot);
        if (diffAngle <= angleAccepted)
            return (true);
        return (false);
    }

    /// <summary>
    /// ici ajoute (ou supprime !) à la force initial
    /// </summary>
    /// <param name="toAdd"></param>
    private void AddForcePendule(float toAdd)
    {
        airForce += toAdd * Time.fixedDeltaTime;
        airForce = Mathf.Clamp(airForce, airForceMin, airForceMax);
        tenseRopeWhenGripped = airForce * 2;
    }
    private void ResetForcePendule()
    {
        //Debug.LogWarning("Reset force pendule");
        airForce = airForceMin;
        tenseRopeWhenGripped = airForce * 2;
    }

    /// <summary>
    /// ici première fois qu'on fait un inverse
    /// </summary>
    private void SetInverse()
    {
        isOnInverse = true;
        currentInverseForce = airInverseMin;

        if (airForce > whenAirForceIsHigh)
        {
            airForce -= amountToRemoveWhenInverse;
        }
    }
    private bool AddInverse(float toAdd)
    {
        currentInverseForce += toAdd * Time.fixedDeltaTime;
        currentInverseForce = Mathf.Clamp(currentInverseForce, airInverseMin, airForce);
        if (currentInverseForce == airForce)
        {
            InverseReset();
            return (true);  //ici la force inverse à atteint la force actuelle du airForce
        }
        return (false); //ici on est en train d'effectuer la force inverse...
    }
    private void InverseReset()
    {
        //Debug.LogWarning("reset inverse");
        isOnInverse = false;
        currentInverseForce = airInverseMin;
    }

    private void AddForcePenduleInverseOrNot(float penduleForce)
    {
        if (isOnInverse)
        {
            if (AddInverse(airForceInverseToAdd))
            {
                AddForcePendule(penduleForce);
            }
            else
            {
                AddForcePendule(currentInverseForce);
            }
        }
        else
        {
            AddForcePendule(penduleForce);
        }
    }

    private void DoNewPenduleForce()
    {
        PosQuadran posInSpace = PendulePart(pointPivot, transform.position);

        bool isGoodVelocitySide = !IsVelocityIsInGoodSide(posInSpace); //ici test si la velocité actuel du rigidbody va vers le haut
        bool isGoodInputToVelocity = IsGoodInputToVelocy();   //ici test si l'input est vers le sens du rigidbody

        //si on va vers le haut, et qu'on fait l'inverse de l'input, autorise l'inverse (reset la force à 1.0f ?)
        if (isGoodVelocitySide && !isGoodInputToVelocity)
        {
            //ici interdire ce mouvement
            if (!YELLOWupApplyDecelerationOnUp && IsUpward(posInSpace))
                return;

            //Debug.LogWarning("//TODO: ici check l'inversse");
            AddForcePenduleInverseOrNot(-airForceToAdd * ratioWhenRemove);

            Debug.DrawRay(transform.position, dirPlayerAirMove * 5, Color.yellow, 0.3f);
        }
        //si on va vers le haut, et qu'on va dans le sens de l'input
        else if (isGoodVelocitySide && isGoodInputToVelocity)
        {
            //ici interdire ce mouvement
            if (!RedupApplyAccelerationOnUp && IsUpward(posInSpace))
                return;

            //si la vitesse du rb est basse, tester avec l'angle de 20
            if (rb.velocity.sqrMagnitude < velocityWhenAngleDontMatterOnRed)
            {
                if (!IsAngleDownOk(angleDiffForRopeWrongSide))
                    return;
            }
            //si la vitesse du rb est haute, tester avec un angle suérieur de 40
            else
            {
                //if (!IsAngleDownOk(angleDiffForRopeWrongSideAndFast))
                  //  return;
            }
            //checker l'angle Vector.down & player - pivot
            //if (!IsAngleDownOk())
            //    return;

            //InverseReset();
            //AddForcePendule(airForceToAdd);
            AddForcePenduleInverseOrNot(airForceToAdd);

            Debug.DrawRay(transform.position, dirPlayerAirMove * 10, Color.red, 0.3f);
        }
        //si on est en train de tomber, et que on va dans le sens 
        else if (!isGoodVelocitySide && isGoodInputToVelocity)
        {
            //ici interdire ce mouvement
            if (!GREENupApplyAccelerationOnDecent && IsUpward(posInSpace))
                return;

            //InverseReset();
            //AddForcePendule(airForceToAdd);
            AddForcePenduleInverseOrNot(airForceToAdd);

            Debug.DrawRay(transform.position, dirPlayerAirMove * 5, Color.green, 0.3f);
        }
        else if (!isGoodVelocitySide && !isGoodInputToVelocity)
        {
            //ici interdire ce mouvement
            if (!MAJENTAupApplyDecelerationOnDecent && IsUpward(posInSpace))
                return;

            //ici le rigidbody est trop lent pour appliquer cette force
            if (rb.velocity.sqrMagnitude < velocityMinForMajenta)
                return;

            //Debug.LogWarning("//TODO: ici check l'inversse");
            AddForcePenduleInverseOrNot(-airForceToAdd * ratioWhenRemove);

            Debug.DrawRay(transform.position, dirPlayerAirMove * 5, Color.magenta, 0.3f);
        }


        lastDirTensity = dirPlayerAirMove;
        ApplyAirMoveForce(dirPlayerAirMove, airForce * playerGetFat.boostAirMoveWhenFat());
    }

    /// <summary>
    /// ici applique la force perpendiculaire à la rope, dans le bon sens !
    /// </summary>
    public void ApplyAirMoveForce(Vector3 dir, float force)
    {
        Debug.DrawRay(transform.position, dir, Color.green, 1f);

        //externalForce.LittleForceDir(-dir, force);
        PhysicsExt.ApplyConstForce(rb, -dir, force);
    }

    /// <summary>
    /// ici tend la corde
    /// </summary>
    public void DoAirTenseRope(float boost = 1)
    {
        //Debug.Log("tend la corde !");
        Vector3 dirRope = ropeHandler.GetVectorFromPlayer(playerController.IdPlayer);
        Debug.DrawRay(transform.position, dirRope, Color.blue, 1f);

        PhysicsExt.ApplyConstForce(rb, dirRope, tenseRopeWhenGripped * boost);
        //externalForce.LittleForceDir(dirRope, tenseRopeWhenGripped * boost);
        //rb.velocity += dirRope     * Physics.gravity.y * (tenseRopeWhenGripped - 1) * Time.fixedDeltaTime;
        //Debug.Log("ici ajout velocity");
    }

    /// <summary>
    /// applique une force sur la rope pour qu'elle se tende...
    /// //est appelé tout le temps
    /// </summary>
    /// <returns></returns>
    public bool ForceTenseRope()
    {
        //si on a fait un normalJump, ne PAS appliquer les mouvement aériens
        if (IsNormalJumpOrFall())
            return (false);

        //si on est en airControlledJump, ne pas appliquer de gravité, MAIS partir quand même d'ici
        if (playerControlledAirJump.InAutoAirMove)
            return (true);

        if (playerManager.IsOtherIsGripped(playerController.IdPlayer) && !worldCollision.IsGroundedSafe() && playerInput.Horiz != 0)
        {
            if (!isAirTense)
            {
                //ici première fois qu'il est tendu
                //rb.velocity = Vector3.ClampMagnitude(rb.velocity, clampVelocityWhenAirMove);
                isAirTense = true;
            }

            if (!isOnAirMove)
            {
                //Debug.Log("ici on a annulé le airMove !");
                return (false);
            }
            DoAirTenseRope();

            
            return (true);
        }
        else if (!(playerManager.IsOtherIsGripped(playerController.IdPlayer) && !worldCollision.IsGroundedSafe()))
        {
            if (isAirTense)
            {
                //ici on a fini d'être tense
                if (worldCollision.IsGroundedSafe())
                {
                    Debug.Log("ici on vient de se grounded, s'accrocher automatiquement ?");

                    playerGrip.TryToAutoGrip();

                    coolDownStilAccrocheABitWhenEndTense.StartCoolDown();
                }
                else
                {
                    Debug.Log("ici on effectue un dernier jump");

                    //si on était en ControlledJump, se désactiver

                    //Vector3 dirRope = ropeHandler.GetVectorFromPlayer(playerController.IdPlayer);
                    //Vector3 dirReference = playerInput.GetDirInput();
                    //Vector3 dir = QuaternionExt.GetTheGoodRightAngleClosest(dirRope, dirReference, 10f);
                    Vector3 dir = (rb.velocity).normalized;
                    EndJump(dir);
                }
                isAirTense = false;
            }
            return (false);
        }

        //ici on a juste arrété d'appuyé...
        return (false);
    }

    /// <summary>
    /// ici effectue un dernier jump quand c'est fini
    /// </summary>
    public void EndJump(Vector3 endDir)
    {
        externalForce.CoolDownDontApplyGravity.StartCoolDown(timerStunGravity);
        Debug.Log("here endJump ?");

        Debug.DrawRay(transform.position, endDir * littleForceAtTheEnd, Color.cyan, 1f);

        if (endDir == Vector3.zero)
            return;
        playerJump.Jump(endDir, true, littleForceAtTheEnd, 1);
        //Debug.Break();
    }

    /// <summary>
    /// cette fonction est appelé par l'event WorldCollision en fixedUpdate
    /// (ici on sait si on est grounded ou pas, et comment)
    /// </summary>
    private void GroundedAction()
    {
        if (stopAction)
            return;

        //ici innactive le coolDownAirMove (on et grounded normalement, donc on a plus a tester ça...
        if (worldCollision.IsGroundedSafe())
            coolDownInactiveAirMove.Reset();

        TryToMove();
    }

    private void StopAction()
    {
        stopAction = false;
    }

    #endregion


    #region Unity ending functions

    private void OnDisable()
    {
        EventManager.StopListening(GameData.Event.GameOver, StopAction);
        EventManager.StopListening(GameData.Event.Grounded, GroundedAction);
    }
    #endregion
}