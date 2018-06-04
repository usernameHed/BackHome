using Sirenix.OdinInspector;
using System;
using UnityEngine;
using WorldCollisionNamespace;

[RequireComponent(typeof(Rigidbody))]
public class PlayerAirJump : MonoBehaviour
{
    #region Attributes
    [Space(10)]
    [FoldoutGroup("GamePlay"), Tooltip("Nombre de air jump ?"), SerializeField]
    private int airJump = 0;
    [FoldoutGroup("GamePlay"), Tooltip("force du air jump"), SerializeField]
    private float airJumpForce = 1f;

    [FoldoutGroup("GamePlay"), Tooltip("Nombre de air jump ?"), SerializeField]
    private bool keepHorizontalAxis = false;
    public bool KeepHorizontalAxis { get { return (keepHorizontalAxis); } }

    [FoldoutGroup("GamePlay"), Tooltip("Si air jump, combien de tempt d'attente avant le premier air jump ?"), SerializeField]
    private FrequencyCoolDown coolDownBeforeFirstAirJump;
    public FrequencyCoolDown CoolDownBeforeFirstAirJump { get { return (coolDownBeforeFirstAirJump); } }
    [FoldoutGroup("GamePlay"), Tooltip("Si air jump, combien de tempt d'attente entre 2 air jump ?"), SerializeField]
    private FrequencyCoolDown coolDownBetween2AirJump;

    [FoldoutGroup("GamePlay"), Tooltip("vibration quand on jump"), SerializeField]
    private Vibration onAirJump;
    [FoldoutGroup("Debug"), Tooltip("currentAirJump"), SerializeField]
    private int currentAirJump = 0;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerJump playerJump;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerController playerController;

    private bool stopAction = false;    //le joueur est-il stopé ?
    #endregion

    #region Initialize
    private void OnEnable()
    {
        EventManager.StartListening(GameData.Event.GameOver, StopAction);
        InitValue();
    }

    private void InitValue()
    {
        stopAction = false;
        //airJump = (ScoreManager.Instance.Data.GetSimplified()) ? 2 : -1;
    }
    #endregion

    #region Core
    /// <summary>
    /// ici reset le air jump
    /// </summary>
    public void ResetAirJump()
    {
        currentAirJump = 0;
        coolDownBeforeFirstAirJump.Reset();
    }

    /// <summary>
    /// renvoi vrai ou faux si on a le droit de sauter
    /// (on est sur d'être en l'air quand même !)
    /// </summary>
    /// <returns></returns>
    public bool CanJump()
    {
        //if coolDown... NON !
        //faux si on hold pas et quand a pas laché
        if (playerJump.JumpStop && !playerJump.StayHold)
        {
            Debug.Log("ici");
            return (false);
        }
            

        //cooldown lancé par le premier jump (PlayerJump)
        if (!coolDownBeforeFirstAirJump.IsReady())
        {
            Debug.Log("ici");
            return (false);
        }

        //cooldown lancé entre 2 air jump
        if (!coolDownBetween2AirJump.IsReady())
        {
            Debug.Log("ici");
            return (false);
        }
            

        //on a attein le nombre de airJump max
        if (currentAirJump >= airJump && airJump != -1)
        {
            //Debug.Log("ici");
            return (false);
        }

        if (!playerController.IsAttachedByGrip())
            return (false);
            

        //ici on est ok pour sauter
        return (true);
    }

    /// <summary>
    /// jump ! dans la direction donné
    /// </summary>
    public bool TryToAirJump(Vector3 dir)
    {
        if (!CanJump())
            return (false);

        //PlayerConnected.Instance.setVibrationPlayer(playerController.IdPlayer, onAirJump); //set vibration de saut

        Debug.Log("air jump !");
        Debug.DrawRay(transform.position, -Vector3.up, Color.red, 5f);
        GameObject particle = ObjectsPooler.Instance.SpawnFromPool(GameData.PoolTag.ParticleBump, transform.position, Quaternion.identity, ObjectsPooler.Instance.transform);
        particle.transform.rotation = QuaternionExt.LookAtDir(-Vector3.up);

        currentAirJump++;

        coolDownBetween2AirJump.StartCoolDown();
        playerJump.Jump(dir, true, airJumpForce);
        return (true);
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


    private void OnDisable()
    {
        EventManager.StopListening(GameData.Event.GameOver, StopAction);
    }
    #endregion
}
