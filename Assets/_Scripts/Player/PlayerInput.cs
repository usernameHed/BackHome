using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// InputPlayer Description
/// </summary>
public class PlayerInput : MonoBehaviour
{
    #region Attributes
    [FoldoutGroup("Object"), Tooltip("id unique du joueur correspondant à sa manette"), SerializeField]
    private PlayerController playerController;
    public PlayerController PlayerController { get { return (playerController); } }

    private float horiz;    //input horiz
    public float Horiz { get { return (horiz); } }
    private float verti;    //input verti
    public float Verti { get { return (verti); } }
    private bool jumpInput; //jump input
    public bool JumpInput { get { return (jumpInput); } }
    private bool jumpUpInput; //jump input
    public bool JumpUpInput { get { return (jumpUpInput); } }

    private bool gripInput; //grip input hold
    public bool GripInput { get { return (gripInput); } }
    private bool gripDownInput; //grgip input down
    public bool GripDownInput { get { return (gripDownInput); } }
    private bool gripUpInput; //grip input up
    public bool GripUpInput { get { return (gripUpInput); } }

    private bool fatInput; //grip input hold
    public bool FatInput { get { return (fatInput); } }
    private bool fatDownInput; //grgip input down
    public bool FatDownInput { get { return (fatDownInput); } }
    private bool fatUpInput; //grip input up
    public bool FatUpInput { get { return (fatUpInput); } }

    private float modyfyRopeAddDownInput; //rope add
    public float ModyfyRopeAddDownInput { get { return (modyfyRopeAddDownInput); } }
    private float modyfyRopeRemoveDownInput; //rope remove
    public float ModyfyRopeRemoveDownInput { get { return (modyfyRopeRemoveDownInput); } }
    #endregion

    #region Initialization

    #endregion

    #region Core

    /// <summary>
    /// get la direction de l'input
    /// </summary>
    /// <returns></returns>
    public Vector3 GetDirInput()
    {
        //Vector3 dirArrowPlayer = QuaternionExt.QuaternionToDir(dirArrow.rotation, Vector3.up);
        Vector3 dirInputPlayer = new Vector3(horiz, verti, 0);
        //Debug.DrawRay(transform.position, dirInputPlayer.normalized, Color.yellow, 1f);
        return (dirInputPlayer);
    }

    /// <summary>
    /// retourne si le joueur se déplace ou pas
    /// </summary>
    /// <returns></returns>
    public bool NotMoving()
    {
        if (horiz == 0 && verti == 0)
            return (true);
        return (false);
    }

    /// <summary>
    /// tout les input du jeu, à chaque update
    /// </summary>
    private void GetInput()
    {
        horiz = PlayerConnected.Instance.getPlayer(playerController.IdPlayer).GetAxis("Move Horizontal");
        verti = PlayerConnected.Instance.getPlayer(playerController.IdPlayer).GetAxis("Move Vertical");

        jumpInput = PlayerConnected.Instance.getPlayer(playerController.IdPlayer).GetButton("FireA");
        jumpUpInput = PlayerConnected.Instance.getPlayer(playerController.IdPlayer).GetButtonUp("FireA");

        gripInput = PlayerConnected.Instance.getPlayer(playerController.IdPlayer).GetButton("FireX") || PlayerConnected.Instance.getPlayer(playerController.IdPlayer).GetButton("FireY");
        gripUpInput = PlayerConnected.Instance.getPlayer(playerController.IdPlayer).GetButtonUp("FireX") || PlayerConnected.Instance.getPlayer(playerController.IdPlayer).GetButtonUp("FireY");
        gripDownInput = PlayerConnected.Instance.getPlayer(playerController.IdPlayer).GetButtonDown("FireX") || PlayerConnected.Instance.getPlayer(playerController.IdPlayer).GetButtonDown("FireY");

        fatInput = PlayerConnected.Instance.getPlayer(playerController.IdPlayer).GetButton("FireY");
        fatUpInput = PlayerConnected.Instance.getPlayer(playerController.IdPlayer).GetButtonUp("FireY");
        fatDownInput = PlayerConnected.Instance.getPlayer(playerController.IdPlayer).GetButtonDown("FireY");

        modyfyRopeAddDownInput = Mathf.Clamp((PlayerConnected.Instance.getPlayer(playerController.IdPlayer).GetAxis("LeftTrigger2") + PlayerConnected.Instance.getPlayer(playerController.IdPlayer).GetAxis("RightTrigger1")), 0f, 1f);
        modyfyRopeRemoveDownInput = Mathf.Clamp((PlayerConnected.Instance.getPlayer(playerController.IdPlayer).GetAxis("RightTrigger2") + PlayerConnected.Instance.getPlayer(playerController.IdPlayer).GetAxis("LeftTrigger1")), 0f, 1f);
    }
    #endregion

    #region Unity ending functions

    private void Update()
    {
        GetInput();
    }

	#endregion
}
