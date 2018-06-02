using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// AnimController Description
/// </summary>
public class AnimController : MonoBehaviour
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip("Animator du joueur"), SerializeField]
    private float speedTurn = 300f;
    [FoldoutGroup("GamePlay"), Tooltip("Animator du joueur"), SerializeField]
    private float speedTurnRatioNoInput = 3;
    [FoldoutGroup("GamePlay"), Tooltip("Animator du joueur"), SerializeField]
    private float speedTurnRatioGripped = 0.5f;
    [FoldoutGroup("GamePlay"), Tooltip("Animator du joueur"), SerializeField]
    private float speedPenduleTurn = 300f;
    [FoldoutGroup("GamePlay"), Tooltip("Animator du joueur"), SerializeField]
    private int iterationDebug = 3;

    [FoldoutGroup("Object"), Tooltip("Animator du joueur"), SerializeField]
    private Animator anim;
    public Animator Anim { get { return (anim); } }
    [FoldoutGroup("Object"), Tooltip("Animator du joueur"), SerializeField]
    private Transform parentParentAnim;
    [FoldoutGroup("Object"), Tooltip("Animator du joueur"), SerializeField]
    private Transform parentAnim;
    [FoldoutGroup("Object"), Tooltip("Animator du joueur"), SerializeField]
    private Transform childAnim;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerInput playerInput;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerGrip playerGrip;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private WorldCollision worldCollision;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerPenduleMove playerPenduleMove;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private RopeHandler ropeHandler;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerController playerController;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerMove playerMove;

    private Vector3 dirMove = new Vector3(0, -1, 0);
    private Vector3 penduleMove = Vector3.up;
    private Vector3 penduleChildRotate = Vector3.up;
    private float ratio = 1;
    #endregion

    #region Initialization

    #endregion

    #region Core
    /// <summary>
    /// défini si on est en grip, moving or not
    /// </summary>
    private void TryToTurn()
    {
        Vector3 inputOfPlayer = playerInput.GetDirInput();
        if (playerGrip.Gripped)
        {
            IsGripped();
        }
        else if (inputOfPlayer.x == 0)
            NotMoving();
        else
        {
            IsMoving(inputOfPlayer.x);
        }

        if (!playerGrip.Gripped && playerPenduleMove.IsAirTenseOrInCoolDown())
        {
            penduleMove = ropeHandler.GetVectorFromPlayer(playerController.IdPlayer);

            

            if (worldCollision.IsOnFloor())
            {
                penduleMove = new Vector3(0, 1, 0);
                Debug.Log("ici on floor ?");
                penduleMove = Vector3.up;
                //childAnim.localEulerAngles = new Vector3(0, 0, 0);
                penduleChildRotate = new Vector3(0, 0, 0);
                return;
            }
            //childAnim.localEulerAngles = new Vector3(0, 0, 0);
            Vector3 dirReference = playerInput.GetDirInput();

            int rightMove = 0;
            dirMove = QuaternionExt.GetTheGoodRightAngleClosestNoClose(penduleMove, dirReference, 10f, out rightMove);

            if (dirMove == Vector3.zero)
            {
                rightMove = 0;
                dirMove = new Vector3(0, -1, 0);
                Debug.Log("ici no input en pendule...");
            }

            //childAnim.rotation = Quaternion.Euler(new Vector3(0, 0, 0));

            if (rightMove == -1)
                //childAnim.localEulerAngles = new Vector3(0, -90, 0);
                penduleChildRotate = new Vector3(0, 270, 0);
            else if (rightMove == 1)
                //childAnim.localEulerAngles = new Vector3(0, 90, 0);
                penduleChildRotate = new Vector3(0, 90, 0);
            else
                //childAnim.localEulerAngles = new Vector3(0, 0, 0);
                penduleChildRotate = new Vector3(0, 0, 0);

        }
        else
        {
            penduleMove = Vector3.up;
            //childAnim.localEulerAngles = new Vector3(0, 0, 0);
            penduleChildRotate = new Vector3(0, 0, 0);
        }
    }

    /// <summary>
    /// set la direction quand on est grip
    /// </summary>
    private void IsGripped()
    {
        dirMove = new Vector3(0, -1, 0);
        ratio = speedTurnRatioGripped;
    }

    /// <summary>
    /// set la direction quand on est en mouvement et non grip
    /// </summary>
    private void IsMoving(float inputX)
    {
        dirMove = new Vector3(inputX, 0, 0);
        ratio = Mathf.Abs(inputX);
        Debug.DrawRay(transform.position, dirMove * 10, Color.blue, 1f);
    }

    /// <summary>
    /// set la direction quand on est pas en mouvement et non grip
    /// </summary>
    private void NotMoving()
    {
        //if (dirMove.x > 0 && )
        //Debug.DrawRay(transform.position, childAnim.right, Color.red, 1f);
        //Debug.Log(childAnim.right);
        ratio = Mathf.Lerp(ratio, 1, Time.deltaTime * speedTurnRatioNoInput);
    }

    /// <summary>
    /// est ammelé à chaque update, pour tourner l'anim
    /// </summary>
    private void Turning()
    {
        if (!playerGrip.Gripped && playerPenduleMove.IsAirTenseOrInCoolDown())
        {
            //parentParentAnim.rotation = QuaternionExt.DirObject(parentParentAnim.rotation, penduleMove.x, -penduleMove.y, speedTurn * ratio, QuaternionExt.TurnType.Z);
            parentAnim.rotation = QuaternionExt.DirObject(parentAnim.rotation, penduleMove.x, -penduleMove.y, speedTurn * ratio, QuaternionExt.TurnType.Z);
        }
        else
        {
            parentAnim.rotation = QuaternionExt.DirObject(parentAnim.rotation, dirMove.x, dirMove.y, speedTurn * ratio, QuaternionExt.TurnType.Y);
        }
        //childAnim.localEulerAngles = new Vector3(0, 0, 0);
        //penduleChildRotate = new Vector3(0, 0, 0);
        childAnim.localEulerAngles = QuaternionExt.DirLocalObject(childAnim.localEulerAngles,
                                                                    penduleChildRotate,
                                                                    speedPenduleTurn,
                                                                    QuaternionExt.TurnType.Y);

        //Quaternion targetRotation = Quaternion.FromToRotation(Vector3.forward, Vector3.up) * Quaternion.LookRotation(penduleChildRotate);
        //childAnim.rotation = Quaternion.Lerp(childAnim.rotation, targetRotation, Time.deltaTime * speedTurn * ratio);

    }

    /// <summary>
    /// ici set les variables grounded.
    /// </summary>
    private void SetAnimValue()
    {
        AnimInAirSet();
        AnimOnFloorSet();

        AnimGroundedSet();
    }

    /// <summary>
    /// ici gère si l'anim est idle, walk ou run !
    /// être sur un wall/ceilling est considéré comme... mmm... en l'air ?
    /// </summary>
    private void AnimGroundedSet()
    {
        //bool inGround = worldCollision.IsGroundedSafe();
        bool onFloor = anim.GetBool("onfloor");
        bool inAir = anim.GetBool("inair");

        if (inAir || !onFloor)
        {
            if (anim.GetBool("walk"))
                anim.SetBool("walk", false);
            if (anim.GetBool("run"))
                anim.SetBool("run", false);
            return;
        }
        //ici on est sur d'être onFloor
        float playerInputHoriz = Mathf.Abs(playerInput.Horiz);
        if (playerInputHoriz == 0)
        {
            anim.SetBool("walk", false);
            anim.SetBool("run", false);
        }
        else if (playerInputHoriz > 0 && playerInputHoriz <= 0.5f)
        {
            anim.SetBool("walk", true);
            anim.SetBool("run", false);
        }
        else
        {
            anim.SetBool("run", true);
            anim.SetBool("walk", false);
        }
    }

    /// <summary>
    /// set les bool d'animator a chaque update
    /// </summary>
    private void AnimOnFloorSet()
    {
        bool onFloor = worldCollision.IsOnFloor() || worldCollision.IsOnCoinGround();
        if (onFloor != anim.GetBool("onfloor"))
        {
            StartCoroutine(WaitFalseFloor("onfloor", onFloor, iterationDebug));
            //anim.SetBool("onfloor", onFloor);
        }
    }
    
    private void AnimInAirSet()
    {
        bool inAir = IsInAir();

        if (inAir != anim.GetBool("inair"))
        {
            StartCoroutine(WaitFalseAir("inair", inAir, iterationDebug));
        }
    }
    private bool IsInAir()
    {
        bool inAir = !worldCollision.IsGroundedSafe();
        if (!inAir && !playerGrip.Gripped)  //si on est pas en l'air, ni gripped... 
        {
            //faire un extra check: si on est sur un wall/ceilling,
            //être toujours considéré comme en l'air !
            bool onFloor = worldCollision.IsOnFloor() || worldCollision.IsOnCoinGround();
            if (!onFloor)
            {
                inAir = true;
            }
            else
            {
                inAir = false;
            }
        }
        if (playerMove.CanMoveOnPlayer(false))
            inAir = false;

        return (inAir);
    }

    /// <summary>
    /// set jump quand on jump à vrai
    /// (jump à false doit être mis par l'animation en fin d'anim jump)
    /// </summary>
    public void AnimJump()
    {
        Debug.Log("Anim Jump !");

        anim.SetBool("walk",    false);
        anim.SetBool("run",     false);
        anim.SetBool("grip",    false);
        //anim.SetBool("onfloor", false);

        anim.SetBool("jump", true);
        //StartCoroutine(WaitFalseJump());
    }
    /// <summary>
    /// TODO: à supprimer, ça fait des bug
    /// </summary>
    private IEnumerator WaitFalseJump()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        anim.SetBool("jump", false);
    }

    /// <summary>
    /// set ou unset l'anim grip
    /// </summary>
    public void AnimGrip(bool grip)
    {
        anim.SetBool("grip", grip);   
        if (grip)
        {
            anim.SetBool("walk",    false);
            anim.SetBool("run",     false);
            anim.SetBool("jump",    false);
            anim.SetBool("inair",   false);
        }
    }

    private IEnumerator WaitFalseFloor(string toChange, bool floor, int iteration = 1)
    {
        for (int i = 0; i < iteration; i++)
        {
            yield return new WaitForEndOfFrame();
            bool onFloor = worldCollision.IsOnFloor() || worldCollision.IsOnCoinGround();
            if (onFloor != floor)
                yield break;
        }
        anim.SetBool(toChange, floor);
    }
    private IEnumerator WaitFalseAir(string toChange, bool air, int iteration = 1)
    {
        for (int i = 0; i < iteration; i++)
        {
            yield return new WaitForEndOfFrame();
            bool onAir = IsInAir();
            if (onAir != air)
                yield break;
        }
        anim.SetBool(toChange, air);
    }

    #endregion

    #region Unity ending functions


    private void Update()
    {
        TryToTurn();
        Turning();
        SetAnimValue();
        //HandleAnim();

        //parentAnim.rotation = dirArrow.rotation;
        //anim.transform.rotation = Quaternion.AngleAxis(90, dirArrow.eulerAngles);
    }
    #endregion
}
