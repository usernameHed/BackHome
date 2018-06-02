using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using WorldCollisionNamespace;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    public float speedAcceleration = 500f;
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    public float maxSpeedConstante = 3000f;

    [Space(10)]

    

    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    public float ratioForceInCoin = 0.65f;
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    public float ratioForceInBadAngle = 0.3f;
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    public float ratioOnWall = 0.3f;
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    public float angleNotAcceptedForOrientedMoveFrom90 = 55f;
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    public FrequencyCoolDown timeWhenStop;   //0.4
    [FoldoutGroup("GamePlay"), Tooltip("ratio de deceleration 0,95"), SerializeField]
    public float decelerationRatio = 0.95f;



    [FoldoutGroup("VelocityChange"), Tooltip(""), SerializeField]
    public float speedVelocityChange = 10.0f;
    [FoldoutGroup("VelocityChange"), Tooltip(""), SerializeField]
    public float maxVelocityChange = 10.0f;

    private float previousInput = 0;
    private bool decelerating = false;
    private bool stopAction = false;    //le joueur est-il stopé ?

    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerInput playerInput;

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
    private ExternalForce externalForce;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerPenduleMove playerPenduleMove;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayersManager playerManager;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerAirMove playerAirMove;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private AnimController animController;

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
    }
    #endregion

    #region Core

    /// <summary>
    /// est-ce qu'on peut bouger droite / gauche ?
    /// </summary>
    /// <returns></returns>
    private bool CanMove()
    {
        /*//si on ne bouge pas les input, ne rien faire
        if (playerInput.Horiz == 0 && playerInput.Verti == 0)
            return (false);
            */
        //si on a juste sauté, ne rien faire
        if (!worldCollision.CoolDownGroundedJump.IsReady() || !worldCollision.CoolDownGrounded.IsReady())
            return (false);

        if (playerPenduleMove.IsAirTense)
            return (false);

        //ne pas bouger quand on est gripped
        if (grip.Gripped)
            return (false);

        //si on est en l'air, et pas sur un objet exeption, ne rien faire (pour l'instant !!)
        if (!worldCollision.IsGroundedSafe())
        {

            //ici on est peut être pas grounded, mais on peut être sur ExeptionPlayer ??
            if (!worldCollision.IsGroundeExeptionSafe())
            {
                //ici on est en l'air
                //Debug.Log("ici on est en l'air, ne rien faire");
                playerAirMove.TryToMove();
                return (false); 
            }
            else
            {
                //Debug.Log("ici on peut bouger ????? on est su rle player...");
                return (false);
                //ici on est en collision avec un player ? OK
            }
        }        

        if (worldCollision.GetSimpleCollisionSafe() == CollisionSimple.Ceilling)
            return (false);

        /*
        //on est au sol, on bouge les inputs et les coolDown JUMP & MOVE sont OP !
        Collider coll;
        if (!worldCollision.PlatformManager.IsWalkable(out coll))
            return (false);
        */

        return (true);
    }

    /// <summary>
    /// déplace le player
    /// </summary>
    private void TryToMove()
    {
        //ici gérer le air control
        if (CanMove())
            MoveHorizOnGround(new Vector3(playerInput.Horiz, 0, 0));
    }

    public bool CanMoveOnPlayer(bool countInput = true)
    {
        //si on ne bouge pas les input, ne rien faire
        if (countInput && playerInput.Horiz == 0 && playerInput.Verti == 0)
            return (false);

        //ne pas bouger quand on est gripped
        if (grip.Gripped)
            return (false);

        if (playerManager.AreBothNotGrounded())
            return (false);

        if (!worldCollision.IsGroundedSafe() && worldCollision.IsGroundeExeptionSafe())
        {
            return (true);
        }

        //on est au sol, on bouge les inputs et les coolDown JUMP & MOVE sont OP !
        return (false);
    }

    /// <summary>
    /// ici move uniquement sur le player
    /// </summary>
    private void TryToMoveOnPlayer()
    {
        if (!CanMoveOnPlayer())
            return;

        Debug.Log("ici move on player !!");
        MoveHorizOnGround(new Vector3(playerInput.Horiz, 0, 0));
    }

    /// <summary>
    /// cette fonction est appelé par l'event WorldCollision en fixedUpdate
    /// (ici on sait si on est grounded ou pas, et comment)
    /// </summary>
    private void GroundedAction()
    {
        if (stopAction)
            return;

        TryToMove();
    }

    private bool WallCase(Vector3 initialVelocity, int leftSide = 1)
    {
        if (initialVelocity.x < 0)
        {
            if (!playerPenduleMove.IsAirTenseOrInCoolDown())
            {
                Debug.Log("ici ne pas continuer vers la gauche");
                worldCollision.CoolDownGrounded.StartCoolDown();
            }
            return (true);
        }
        else
        {
            //Debug.Log("ici faire quelque chose pour ne pas bouncer...");
            if (playerPenduleMove.IsAirTenseOrInCoolDown())
            {
                //Debug.Log("ici accroche");
                return (false);
            }

            //ici ne rien faire si l'une des normals est... sur le sol !!!!
            if (worldCollision.IsOnCoinGround())
            {
                //Ici on est dans un coin
                //Debug.Log("ici dans un coin !");
            }
            else
            {
                if (playerManager.IsOtherIsGripped(playerController.IdPlayer) && ropeHandler.IsTenseForAirMove)
                {
                    if (playerInput.GetDirInput() != Vector3.zero)
                    {
                        //Debug.Log("ou encore la ?");
                        //PhysicsExt.ApplyConstForce(rb, -Vector3.right * leftSide, externalForce.ForceOnWall);
                    }
                    //Debug.Log("ici n'accroche pas au mur, car on peut faire air move !");
                }
                else
                {
                    //Debug.Log("accroche au mur !");
                    //PhysicsExt.ApplyConstForce(rb, Vector3.right * leftSide, externalForce.ForceOnWall);
                }
                return (false);
            }
        }
        return (true);
    }

    private bool CheckCollisionType(Vector3 velocityChange)
    {
        CollisionType collisionType = worldCollision.GetCollisionSafe();

        switch (collisionType)
        {
            case CollisionType.WallLeft:
                if (!WallCase(velocityChange, 1))
                    return (false);
                break;
            case CollisionType.WallRight:
                if (!WallCase(velocityChange, -1))
                    return (false);
                break;
            case CollisionType.CeilingLeft:
            case CollisionType.CeilingRight:
            case CollisionType.Ceilling:
                Debug.Log("ici ne pas slider...");
                return (false);

            default:
                //ici on est pas en wallJump, on peut bouger normalement !
                break;
        }
        return (true);
    }

    /// <summary>
    /// ici plus d'input, essayer d'arreter progressivement le player ???
    /// </summary>
    private void DoSlowDownWhenNoInput()
    {
        if (previousInput != 0 && rb.velocity.sqrMagnitude > 0.5f && !decelerating)
        {
            //ici, avant on avait un input, plus maintenant...
            //c'est le début de la décélération
            //Debug.Log("début de décélération");
            timeWhenStop.StartCoolDown();
            decelerating = true;
        }
        if (rb.velocity.sqrMagnitude > 0.5f && !timeWhenStop.IsReady())
        {
            //Debug.Log("deceleration");
            rb.AddForce(-rb.velocity * decelerationRatio * Time.fixedDeltaTime, ForceMode.Acceleration);
            //rb.velocity = rb.velocity * decelerationRatio * Time.fixedDeltaTime;
        }
        else
        {
            decelerating = false;
        }

    }

    /// <summary>
    /// ici gère le mouvement vers/contre un coin
    /// </summary>
    private void CoinCase(Vector3 velocityChange)
    {
        Debug.Log("ou la ?");
        //ici gère: si on va vers le coin, ou à l'inverse.
        rb.AddForce(velocityChange * ratioForceInCoin, ForceMode.VelocityChange);
        Debug.DrawRay(transform.position, velocityChange * ratioForceInCoin, Color.green, 1f);
    }

    /// <summary>
    /// déplace horizontalement le player
    /// </summary>
    private void MoveHorizOnGround(Vector3 velocityChange)
    {
        if (playerPenduleMove.IsAirTenseOrInCoolDown())
            return;

        Vector3 targetVelocity = velocityChange;

        //si on est en collision avec wall, ou ceilling, gérer ça à part
        if (!CheckCollisionType(velocityChange))
            return;

        //si il n'y a pas d'input, gérer la décélération ??
        if (velocityChange.x == 0)
        {
            DoSlowDownWhenNoInput();
            //animController.NotMoving();
            return;
        }
        //animController.IsMoving(playerInput.Horiz);

        previousInput = velocityChange.x;

        //TODO: rotate, ou remplacer velocityChange par le right/left de la normal de collision...
        //TODO;
        Vector3 dirNormal = worldCollision.GetSumNormalSafe();
        Vector3 dirReference = velocityChange;

        if (dirNormal != Vector3.zero && dirReference != Vector3.zero)
        {
            Vector3 dir = QuaternionExt.GetTheGoodRightAngleClosest(dirNormal, dirReference, 10f);

            Vector3 direction = dir.normalized;
            float speed = velocityChange.magnitude;

            velocityChange = direction * speed;
                

            Vector3 normalRight = QuaternionExt.CrossProduct(velocityChange, Vector3.forward);
            CollisionSimple rightNormal = worldCollision.WhatKindOfNormalItIs(normalRight);

            Vector3 normalLeft = -QuaternionExt.CrossProduct(velocityChange, Vector3.forward);
            CollisionSimple leftNormal = worldCollision.WhatKindOfNormalItIs(normalLeft);

            /*Vector3 newDir = (rightNormal == CollisionSimple.Ground) ? normalRight : normalLeft;*/

            if (rightNormal == CollisionSimple.Ground || leftNormal == CollisionSimple.Ground)
            {
                if (worldCollision.IsOnCoinGround())
                {
                    //ChooseIfAccelerateOrVelocityChange(velocityChange, ratioForceInCoin);
                    CoinCase(velocityChange);
                    
                }
                else
                {
                    float angle = QuaternionExt.GetAngleFromVector(direction);
                    //Debug.Log("angleDirection: " + angle);

                    if (QuaternionExt.IsAngleCloseToOtherByAmount(90, angle, angleNotAcceptedForOrientedMoveFrom90))
                    {
                        //Debug.Log("ici pas de force orienté par la normal");
                        //Debug.DrawRay(transform.position, dirReference, Color.green, 1f);
                        ChooseIfAccelerateOrVelocityChange(dirReference, ratioForceInBadAngle);
                        return;
                    }

                    if (worldCollision.GetSimpleCollisionSafe() == CollisionSimple.Wall)
                    {
                        ChooseIfAccelerateOrVelocityChange(velocityChange, ratioOnWall);
                    }
                    else
                    {
                        ChooseIfAccelerateOrVelocityChange(velocityChange, 1);
                    }


                    //rb.AddForce(velocityChange, ForceMode.VelocityChange);
                    //Debug.DrawRay(transform.position, velocityChange, Color.green, 1f);

                }
            }
        }

    }

    /// <summary>
    /// ici effectue la force au sol
    /// </summary>
    /// <param name="dirForce"></param>
    public void ChooseIfAccelerateOrVelocityChange(Vector3 dirForce, float ratio)
    {
        //Debug.Log("la ?");

        MoveOnWallWhenAirMove(dirForce.normalized * Mathf.Abs(previousInput), ratio);

        return;

        Vector3 rigidbodyVelocity = rb.velocity;

        float dotResult = QuaternionExt.DotProduct(Vector3.right, rigidbodyVelocity);

        if ((QuaternionExt.DotProduct(Vector3.right, rigidbodyVelocity) >= 0 && playerInput.Horiz > 0)
            || (QuaternionExt.DotProduct(Vector3.right, rigidbodyVelocity) <= 0 && playerInput.Horiz < 0))
        {
            //dans le sens
            MoveOnWallWhenAirMove(dirForce.normalized * Mathf.Abs(previousInput), ratio);
        }
        else
        {
            //inverse
            Vector3 addForce = dirForce.normalized * speedAcceleration * ratio * Mathf.Abs(previousInput) * Time.fixedDeltaTime;
            rb.AccelerateTo(addForce, maxSpeedConstante, ForceMode.Acceleration);
        }




    }



    /// <summary>
    /// déplace horizontalement le player
    /// </summary>
    /// <param name="inverse"></param>
    public void MoveOnWallWhenAirMove(Vector3 initialVelocity, float ratio = 1)
    {
        //Debug.Log("ici ?");

        Debug.DrawRay(transform.position, initialVelocity, Color.gray, 1f);

        // Calculate how fast we should be moving
        Vector3 targetVelocity = initialVelocity;
        targetVelocity = transform.TransformDirection(targetVelocity);
        targetVelocity *= speedVelocityChange;

        // Apply a force that attempts to reach our target velocity
        Vector3 velocity = rb.velocity;
        Vector3 velocityChange = (targetVelocity - velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;
        velocityChange.z = 0;

        //Debug.Log("ici move changement velocity");
        rb.AddForce(velocityChange * ratio, ForceMode.VelocityChange);
    }

    private void StopAction()
    {
        stopAction = false;
    }

    #endregion


    #region Unity ending functions

    
    private void FixedUpdate()
    {
        if (stopAction)
            return;

        TryToMoveOnPlayer();
    }
    

    private void OnDisable()
    {
        EventManager.StopListening(GameData.Event.GameOver, StopAction);
        EventManager.StopListening(GameData.Event.Grounded, GroundedAction);
    }
    #endregion
}