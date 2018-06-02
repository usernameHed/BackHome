using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    private float secondeBeforeDetach = 1f;
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    private float radius = 2.72f;
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    private FrequencyCoolDown timerGrapp;

    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    private bool nowWaitBeforeReAttach = false;
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    private FrequencyCoolDown timerAfterGrap;


    //[FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    //private FrequencyCoolDown coolDown = new FrequencyCoolDown();
    //[FoldoutGroup("GamePlay"), Tooltip("list des layer à ignorer"), SerializeField]
    //private List<GameData.Layers> listLayerToIgnore;

    [FoldoutGroup("Object"), Tooltip(""), SerializeField]
    private SpringJoint springJoint;
    [FoldoutGroup("Object"), Tooltip(""), SerializeField]
    private LineRenderer line;

    [FoldoutGroup("Debug"), Tooltip(""), SerializeField]
    private bool attached = false;
    [FoldoutGroup("Debug"), Tooltip(""), SerializeField]
    private Transform target;

    private void Awake()
    {
        line.SetPosition(0, transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (target)
            return;
            
        if (other.CompareTag(GameData.Prefabs.Player.ToString())/* && coolDown.IsReady()*/)
        {
            Attach(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!target)
            return;

        if (other.CompareTag(GameData.Prefabs.Player.ToString()))
        {
            attached = false;
            Invoke("Detach", secondeBeforeDetach);
            //coolDown.StartCoolDown();
        }
    }

    /// <summary>
    /// attache
    /// </summary>
    /// <param name="other"></param>
    private void Attach(Collider other)
    {
        if (nowWaitBeforeReAttach && !timerAfterGrap.IsReady())
            return;
        nowWaitBeforeReAttach = false;

        RaycastHit hit;
        //LayerMask.NameToLayer("Player")
        if (Physics.Raycast(transform.position, other.transform.position - transform.position, out hit, radius))
        {
            if (hit.collider.gameObject.CompareTag(GameData.Prefabs.Player.ToString()))
            {
                PlayerController playerController = hit.collider.gameObject.GetComponent<PlayerController>();
                playerController.SetAttachedByGrippling(1);

                timerGrapp.StartCoolDown();
                //SoundManager.GetSingleton.playSound(GameData.Sounds.SpiksOn.ToString() + transform.GetInstanceID().ToString());
                CancelInvoke("Detach");
                springJoint.connectedBody = other.GetComponent<Rigidbody>();
                attached = true;
                line.enabled = attached;
                target = other.transform;
            }
        }
    }

    /// <summary>
    /// détacher
    /// </summary>
    private void Detach()
    {
        if (!attached && target)
        {
            //SoundManager.GetSingleton.playSound(GameData.Sounds.SpiksOff.ToString() + transform.GetInstanceID().ToString());
            springJoint.connectedBody = null;
            line.enabled = attached;

            PlayerController playerController = target.GetComponent<PlayerController>();
            playerController.SetAttachedByGrippling(-1);


            target = null;
            Debug.Log("ici lache");
            //timerGrapp.Reset();
        }
    }

    private void Update()
    {
        if (line.enabled && target != null)
        {
            line.SetPosition(1, target.position);
        }
        //ici on est attaché, et le timer est fini
        if (attached && timerGrapp.IsReady())
        {
            Debug.Log("lacher !!");
            attached = false;
            nowWaitBeforeReAttach = true;
            timerAfterGrap.StartCoolDown();
            Detach();
        }
    }
}
