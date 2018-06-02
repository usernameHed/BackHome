using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using WorldCollisionNamespace;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour, IKillable
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip("id unique du joueur correspondant à sa manette"), SerializeField]
    private int idPlayer = 0;
    public int IdPlayer { set { idPlayer = value; } get { return idPlayer; } }

    [FoldoutGroup("GamePlay"), Tooltip("list des layer de collisions"), SerializeField]
    private float turnRateArrow = 400f;

    [FoldoutGroup("GamePlay"), Tooltip("id unique du joueur correspondant à sa manette"), SerializeField]
    private bool isCloseToDeath = false;

    [FoldoutGroup("GamePlay"), Tooltip("id unique du joueur correspondant à sa manette"), SerializeField]
    private int isAttached = 0;

    [FoldoutGroup("GamePlay"), Tooltip("vibration quand on jump"), SerializeField]
    private Vibration onDie;

    

    [FoldoutGroup("Object"), Tooltip("direction du joystick"), SerializeField]
    private Transform dirArrow;

    [FoldoutGroup("Debug"), OnValueChanged("ChangeValueRigidbody"), Tooltip("initialGrip"), SerializeField]
    private float initialDrag;
    public float InitialDrag { get { return (initialDrag); } }
    [FoldoutGroup("Debug"), OnValueChanged("ChangeValueRigidbody"), Tooltip("initialGrip"), SerializeField]
    private float initialMass;
    public float InitialMass { get { return (initialMass); } }

    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerInput playerInput;
    public PlayerInput PlayerInputScript { get { return (playerInput); } }

    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayersManager playersManager;
    public PlayersManager PlayersManager { get { return (playersManager); } }

    private bool enabledObject = true;  //le script est-il enabled ?
    private bool stopAction = false;    //le joueur est-il stopé ?

    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private Rigidbody rb;           //ref du rb
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
        rb.freezeRotation = true;
        isCloseToDeath = false;

        initialDrag = rb.drag;
        initialMass = rb.mass;

        enabledObject = true;
        stopAction = false;
    }

    private void ChangeValueRigidbody()
    {
        rb.drag = initialDrag;
        rb.mass = initialMass;
    }
    #endregion

    #region Core
    /// <summary>
    /// cette fonction est appelé par l'event WorldCollision en fixedUpdate
    /// (ici on sait si on est grounded ou pas, et comment)
    /// </summary>
    private void GroundedAction()
    {
        if (stopAction)
            return;

    }

    /// <summary>
    /// si on est attaché actuellement par un grip, sauter !
    /// </summary>
    /// <returns></returns>
    public bool IsAttachedByGrip()
    {
        if (isAttached > 0)
            return (true);
        return (false);
    }

    /// <summary>
    /// set si on est attaché par un gripper
    /// limiter de 0 a 4...
    /// </summary>
    /// <param name="add"></param>
    public void SetAttachedByGrippling(int add)
    {
        isAttached += add;
        isAttached = Mathf.Clamp(isAttached, 0, 4);
    }

    public void SetCloseToDeath(bool close)
    {
        isCloseToDeath = close;
    }

    public bool InDeadZone()
    {
        return (isCloseToDeath);
    }

    /// <summary>
    /// Direction arrow
    /// </summary>
    private void ChangeDirectionArrow()
    {
        if (!(PlayerInputScript.Horiz == 0 && PlayerInputScript.Verti == 0))
        {
            dirArrow.rotation = QuaternionExt.DirObject(dirArrow.rotation, PlayerInputScript.Horiz, -PlayerInputScript.Verti, turnRateArrow, QuaternionExt.TurnType.Z);
        }            
    }

    private void StopAction()
    {
        stopAction = false;
    }

    #endregion


    #region Unity ending functions
    private void Update()
    {
        if (stopAction)
            return;

        ChangeDirectionArrow();
    }

    public void Kill()
    {
        if (!enabledObject)
            return;

        StopAction();
        GameManager.Instance.CameraObject.GetComponent<ScreenShake>().ShakeCamera();
        //ObjectsPooler.Instance.SpawnFromPool(GameData.PoolTag.DeathPlayer, transform.position, Quaternion.identity, ObjectsPooler.Instance.transform);
        PlayerConnected.Instance.setVibrationPlayer(idPlayer, onDie);

        enabledObject = false;
        gameObject.SetActive(false);
    }

    
    private void OnDisable()
    {
        EventManager.StopListening(GameData.Event.GameOver, StopAction);
        EventManager.StopListening(GameData.Event.Grounded, GroundedAction);
    }
    #endregion
}