using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using WorldCollisionNamespace;

[RequireComponent(typeof(Rigidbody))]
public class PlayerAirMove : MonoBehaviour
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    public float ratioAirMove = 1f;
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    public float ratioAirMoveInverse = 3f;
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    public float maxSpeedRigidbodyForNormalAirMove = 10f;


    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerMove playerMove;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerInput playerInput;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private AnimController animController;


    private bool stopAction = false;    //le joueur est-il stopé ?

    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private Rigidbody rb;           //ref du rb
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerPenduleMove playerPenduleMove;
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
    /// ici move uniquement sur le player
    /// </summary>
    public void TryToMove()
    {
        if (playerInput.Horiz != 0)
            MoveAir();
    }

    /// <summary>
    /// déplace horizontalement le player
    /// </summary>
    private void MoveAir()
    {
        if (playerPenduleMove.IsAirTenseOrInCoolDown())
            return;

        //animController.IsMoving(playerInput.Horiz);

        Vector3 addForce = new Vector3(playerInput.Horiz, 0, 0);
        Vector3 rigidbodyVelocity = rb.velocity;

        float dotResult = QuaternionExt.DotProduct(Vector3.right, rigidbodyVelocity);

        if ((QuaternionExt.DotProduct(Vector3.right, rigidbodyVelocity) >= 0 && playerInput.Horiz > 0)
            || (QuaternionExt.DotProduct(Vector3.right, rigidbodyVelocity) <= 0 && playerInput.Horiz < 0))
        {

            if (rigidbodyVelocity.sqrMagnitude < maxSpeedRigidbodyForNormalAirMove)
            {
                //Debug.Log("dans le sens");
                rb.AddForce(addForce * playerMove.speedAcceleration * ratioAirMove * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
        }
        else
        {
            //Debug.Log("à l'inverse");
            rb.AddForce(addForce * playerMove.speedAcceleration * ratioAirMoveInverse * Time.fixedDeltaTime, ForceMode.Acceleration);
        }

        /*//ratioAirMoveInverse
        if (accel.sqrMagnitude > maxAccel * maxAccel)
        {
            
        }
        rb.AddForce(addForce * playerMove.speedAcceleration * ratioAirMove * Time.fixedDeltaTime, ForceMode.Acceleration);
        //rb.AccelerateTo(addForce, maxSpeedConstante, ForceMode.Acceleration);
        */
        return;
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
    }
    #endregion
}