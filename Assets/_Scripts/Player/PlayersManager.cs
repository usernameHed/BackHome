using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using WorldCollisionNamespace;

/// <summary>
/// PlayersManager Description
/// </summary>
public class PlayersManager : MonoBehaviour
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip("ref"), SerializeField]
    private List<Checkpoint> checkpoint;

    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerController[] playerController = new PlayerController[2];
    public PlayerController[] PlayerControllerScript { get { return (playerController); } }
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerJump[] playerJump = new PlayerJump[2];
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private ExternalForce[] externalForce = new ExternalForce[2];
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerGrip[] playerGrip = new PlayerGrip[2];
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private WorldCollision[] worldCollision = new WorldCollision[2];
    #endregion

    #region Initialization
    private void Awake()
    {
        int indexChekpoint = ScoreManager.Instance.Data.GetCheckpoint();
        if (indexChekpoint > 0)
        {
            gameObject.transform.position = checkpoint[indexChekpoint - 1].PointPos();
        }
    }
    #endregion

    #region Core
    /// <summary>
    /// retourne vrai si il y a au moin 1 joueur aggripé
    /// </summary>
    /// <returns></returns>
    public bool IsSomeOneGripped()
    {
        if (playerGrip[0].Gripped || playerGrip[1].Gripped)
            return (true);
        return (false);
    }

    /// <summary>
    /// retourne vrai si l'autre player est grippé
    /// </summary>
    /// <param name="idPlayer"></param>
    /// <returns></returns>
    public bool IsOtherIsGripped(int idPlayer)
    {
        if (idPlayer == 0)
        {
            return (playerGrip[1].Gripped);
        }
        else
        {
            return (playerGrip[0].Gripped);
        }
    }

    /// <summary>
    /// ici renvoi vrai si l'input des players en X sont opposé
    /// </summary>
    /// <returns></returns>
    public bool AreBothPlayerInOpositeInputDirection()
    {
        if (NumberExtensions.AreOposite(playerController[0].PlayerInputScript.Horiz, playerController[1].PlayerInputScript.Horiz))
        //if (playerController[0].PlayerInputScript.Horiz > 0 && playerController[1].PlayerInputScript.Horiz < 0)
            return (true);
        return (false);
    }

    /// <summary>
    /// retourne vrai si les 2 player sont dans une dead zone !
    /// </summary>
    /// <returns></returns>
    public bool AreBothDied()
    {
        if (playerController[0].InDeadZone() && playerController[1].InDeadZone())
            return (true);
        return (false);
    }

    /// <summary>
    /// ici retourne vrai si le player est en dessous l'autre
    /// </summary>
    /// <param name="idPlayer"></param>
    /// <returns></returns>
    public bool IsUnderOtherPlayer(int idPlayer, float margin = 0)
    {
        if (idPlayer == 0)
        {
            return (playerController[0].transform.position.y + margin < playerController[1].transform.position.y);
        }
        else
        {
            return (playerController[1].transform.position.y + margin < playerController[0].transform.position.y);
        }
    }

    /// <summary>
    /// est-ce que les 2 joueurs sont not grounded / grounded
    /// </summary>
    /// <returns></returns>
    public bool AreBothNotGrounded()
    {
        if (!worldCollision[0].IsGroundedSafe() && !worldCollision[1].IsGroundedSafe())
            return (true);
        return (false);
    }
    public bool AreBothGrounded()
    {
        if (worldCollision[0].IsGroundedSafe() && worldCollision[1].IsGroundedSafe())
            return (true);
        return (false);
    }
    /// <summary>
    /// ici grounded sur sol only !
    /// </summary>
    /// <returns></returns>
    public bool AreBothGroundedOn(CollisionSimple onFloor)
    {
        if (worldCollision[0].GetSimpleCollisionSafe() == onFloor && worldCollision[1].GetSimpleCollisionSafe() == onFloor)
        {
            return (true);
        }
        return (false);
    }

    /// <summary>
    /// ici renvoi si l'autre joueur est sur le sol
    /// </summary>
    public bool AreOtherOnFloor(int idPlayer)
    {
        if (idPlayer == 0)
            return (worldCollision[1].IsOnFloor());
        else
            return (worldCollision[0].IsOnFloor());
    }

    /// <summary>
    /// ici test si les 2 gripped cooldown sont prêt
    /// (pour ne pas commencer à raccourcir dès qu'on sort de grip);
    /// </summary>
    /// <returns></returns>
    public bool AreBothGrippedEndAndCoolDownReady()
    {
        if (playerGrip[0].CoolDownGrip.IsReady() && playerGrip[1].CoolDownGrip.IsReady())
        {
            return (true);
        }
        return (false);
    }

    /*public bool AreBothNotGroundedExeption()
    {
        if (!worldCollision[0].IsGroundeExeptionSafe() && !worldCollision[1].IsGroundedSafe())
            return (true);
        return (false);
    }
    public bool AreBothGroundedExeption()
    {
        if (worldCollision[0].IsGroundedSafe() && worldCollision[1].IsGroundedSafe())
            return (true);
        return (false);
    }*/
    /// <summary>
    /// renvoi vrai si les 2 joueur on jumpé (ou sont en train)
    /// </summary>
    /// <returns></returns>
    public bool BothHaveJumped()
    {
        if (playerJump[0].HasJumpAndFlying && playerJump[1].HasJumpAndFlying)
            return (true);
        return (false);
    }
    /*
    /// <summary>
    /// retourne faux si l'un ou l'autre player sont en contact
    /// </summary>
    /// <returns></returns>
    public bool ExeptionContactsPlayer()
    {
        if (worldCollision[0].ExeptionPlayerContact() || worldCollision[1].ExeptionPlayerContact())
            return (true);
        return (false);
    }*/

    /// <summary>
    /// ici retourne la distance entre les 2 joueurs
    /// </summary>
    /// <returns></returns>
    public float GetDistBetweenPlayer()
    {
        return (Vector3.Distance(GetPosPlayer(0), GetPosPlayer(1)));
    }

    public Vector3 GetPosPlayer(int index)
    {
        return (playerController[index].transform.position);
    }

    /// <summary>
    /// un joueur viens de jump, alléger l'autre joueur !
    /// sauf s'il est en l'air ?
    /// </summary>
    /// <param name="idPlayer"></param>
    /// <param name="jumped"></param>
    public void Jump(int idPlayer)
    {
        if (idPlayer == 0)
            externalForce[1].OtherPlayerHasJumped();
        else
            externalForce[0].OtherPlayerHasJumped();
    }

    /// <summary>
    /// est-ce que l'autre player vient d'être ungrip ???
    /// </summary>
    /// <param name="idPlayer"></param>
    /// <returns></returns>
    public bool OtherJustUngrip(int idPlayer)
    {
        if (idPlayer == 0)
        {
            return (playerGrip[1].PlayerJustUngrip());
            //if (playerGrip[1].PlayerJustUngrip())
            //    return ()
        }
        else
        {
            return (playerGrip[0].PlayerJustUngrip());
        }
    }

    /// <summary>
    /// ici le player s'est grounded après un jump, reset l'autre 
    /// </summary>
    public void ResetOtherWhenGrounded(int idPlayer)
    {
        if (idPlayer == 0)
            externalForce[1].ResetPlayerHasJump();
        else
            externalForce[0].ResetPlayerHasJump();
    }
    #endregion

    #region Unity ending functions

    #endregion
}
