using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System;

/// <summary>
/// PlayerController handle player movement
/// <summary>
public class BumperBehaviour : MonoBehaviour
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip("Vecteur direction de la poussé"), SerializeField]
    private Transform direction;

    [Space(10)]
    [FoldoutGroup("GamePlay"), Tooltip("force de poussée"), SerializeField]
    private float forceObjects = 15;

    [Space(10)]
    [FoldoutGroup("GamePlay"), Tooltip("list des prefabs à push"), SerializeField]
    private List<GameData.Layers> listLayerToPush;

    private bool enabledObject = true;
    private Camera cam;
    #endregion

    #region Initialize
    private void Start()
    {
        Init();
    }

    private void Init()
    {
        cam = GameManager.Instance.CameraMain;
        enabledObject = true;
    }

    private void OnEnable()
    {
        EventManager.StartListening(GameData.Event.GameOver, StopAction);
    }
    #endregion

    #region Core
    /// <summary>
    /// trigger
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (!enabledObject)
            return;

        if (GameData.IsInList(listLayerToPush, other.gameObject.layer))
        {
            DoBump(other.gameObject);
        }
    }

    /// <summary>
    /// Bump u nobjet !
    /// </summary>
    private void DoBump(GameObject obj)
    {
        //sound bump
        //SoundManager.GetSingleton.playSound(GameData.Sounds.Bump.ToString() + transform.GetInstanceID().ToString());
        if (obj.CompareTag(GameData.Prefabs.Player.ToString()))
        {
            PlayerGrip playerGrip = obj.GetComponent<PlayerGrip>();
            if (playerGrip == null)
                return;
            if (playerGrip.Gripped)
                return;
        }

        Rigidbody rbOther = obj.GetComponent<Rigidbody>();
        if (!rbOther)
            return;
        if (!cam)
            Init();

        Vector3 dir = (transform.position - direction.position).normalized;

        /*if (UtilityFunctions.IsTargetOnScreen(cam, obj.transform))
        {
            Debug.Log("Is On Screen !");
            //Cree uniquement si la position X,Y est dans la caméra ???
            GameObject particle = ObjectsPooler.Instance.SpawnFromPool(GameData.Prefabs.ParticleBump, obj.transform.position, Quaternion.identity, ObjectsPooler.Instance.transform);
            particle.transform.rotation = QuaternionExt.LookAtDir(dir);
        }*/


        rbOther.ClearVelocity();    //clear velocity du rigidbody


        rbOther.AddForce(dir * -forceObjects, ForceMode.VelocityChange);
    }

    /// <summary>
    /// appelé quand le jeu est fini...
    /// </summary>
    private void StartAction()
    {
        enabledObject = true;
    }
    private void StopAction()
    {
        enabledObject = false;
    }

    #endregion

    #region Unity ending functions
    /// <summary>
    /// draw le vecteur
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (direction)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, direction.position);
        }
    }

    private void OnDisable()
    {
        EventManager.StopListening(GameData.Event.GameOver, StopAction);
    }

    #endregion
}