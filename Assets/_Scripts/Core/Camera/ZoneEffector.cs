using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(CircleCollider2D))]
public class ZoneEffector : MonoBehaviour                                   //commentaire
{
    #region public variable
    /// <summary>
    /// variable public
    /// </summary>
    public List<CameraTarget> listEffector;
    public bool onEnter = true;
    public bool onExit = true;

    [SerializeField]
    private CameraController cameraController;

    /// <summary>
    /// variable public HideInInspector
    /// </summary>
    //[HideInInspector] public bool tmp;

    #endregion

    #region private variable
    /// <summary>
    /// variable privé
    /// </summary>

    /// <summary>
    /// variable privé serealized
    /// </summary>
    //[SerializeField] private bool tmp;
    private bool isEnter = false;

    #endregion

    #region  initialisation
    /// <summary>
    /// Initialisation
    /// </summary>
    private void Awake()
    {
        cameraController = GameManager.Instance.CameraObject.GetComponent<CameraController>();
    }

    /// <summary>
    /// Initialisation
    /// </summary>
    private void Start()                                                    //initialsiation
    {
        //listEffector.Clear();
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
                listEffector.Add(child.gameObject.GetComponent<CameraTarget>());
        }
    }
    #endregion

    #region core script
    /// <summary>
    /// Fonction qui parcourt les objets à mettre in/out de la caméra (selon active)
    /// </summary>
    private void ActionOnCamera(bool active)                                             //test
    {
        isEnter = active;
        //Debug.Log("actionOnCamera: " + active);
        for (int i = 0; i < listEffector.Count; i++)
        {
            if (!listEffector[i])
                continue;
            if (active)
            {
                listEffector[i].SetAlways(true);
                cameraController.SetAlways(true);
            }
            else
            {
                listEffector[i].SetAlways(false);
                cameraController.SetAlways(false);
            }            
        }
    }
    #endregion

    #region unity fonction and ending

    /// <summary>
    /// action lorsque le joueur entre dans une zone
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter(Collider collision)
    {
        if (isEnter)
            return;
        //si c'est un collider 2D, et que son objet de reference est un joueur
        if (onEnter && collision.CompareTag(GameData.Prefabs.Player.ToString()))
        {
            //collision.gameObject.GetComponent<PlayerController>().addZone(this);
            ActionOnCamera(true);
        }
    }

    /// <summary>
    /// active ou désactive la zone via un script
    /// </summary>
    /// <param name="active"></param>
    public void ActiveZoneByScript(bool active)
    {
        ActionOnCamera(active);
    }

    /// <summary>
    /// action lorsque le joueur sort d'une zone
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerExit(Collider collision)
    {
        //si c'est un collider 2D, et que son objet de reference est un joueur
        if (onExit && collision.CompareTag(GameData.Prefabs.Player.ToString()))
        {
            //collision.gameObject.GetComponent<PlayerController>().deleteZone(this);
            ActionOnCamera(false);
        }
    }
    #endregion
}
