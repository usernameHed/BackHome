﻿using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using WorldCollisionNamespace;
using System.Collections;

/// <summary>
/// WrapMode Description
/// </summary>
public class PlayerGrip : MonoBehaviour
{
    #region Attributes
    //[FoldoutGroup("GamePlay"), Tooltip("jumper constament en restant appuyé ?"), SerializeField]
    //private bool stayHold = false;
    [FoldoutGroup("GamePlay"), Tooltip("force d'aggripation au mur..."), SerializeField]
    private float forceGrip = 3f;
    [FoldoutGroup("GamePlay"), Tooltip("force d'aggripation au mur..."), SerializeField]
    private float dragWhenGrip = 10f;

    [FoldoutGroup("GamePlay"), Tooltip("cooldown du jump (influe sur le mouvements du perso)"), SerializeField]
    private FrequencyCoolDown coolDownGrip;
    public FrequencyCoolDown CoolDownGrip { get { return (coolDownGrip); } }
    [FoldoutGroup("GamePlay"), Tooltip("list des layer de collisions"), SerializeField]
    private List<GameData.Layers> listLayerToGrip;

    [FoldoutGroup("GamePlay"), Tooltip("quand on est grip, mais qu'on est en l'air"), SerializeField]
    private FrequencyCoolDown waitBeforeResetGrip;

    [FoldoutGroup("Object"), Tooltip("est-on grippé ?"), SerializeField]
    private GameObject display;

    [FoldoutGroup("Debug"), Tooltip("est-on grippé ?"), SerializeField]
    private bool gripped = false;
    public bool Gripped { get { return (gripped); } }

    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private Rigidbody rb;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private WorldCollision worldCollision;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerInput inputPlayer;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private RopeHandler ropeHandler;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayerController playerController;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private PlayersManager playerManager;
    [FoldoutGroup("Debug"), Tooltip("ref"), SerializeField]
    private AnimController animController;

    public bool hardGrip = false;
    private bool stopAction = false;    //le joueur est-il stopé ?
    private Vector3 pointAttract;
    private bool isTryingToUngrip = false;

    [FoldoutGroup("GamePlay"), Tooltip("temps cooldown hardgrip and mode simplifié"), SerializeField]
    private FrequencyCoolDown waitHardUngrip;

    [FoldoutGroup("GamePlay"), Tooltip("temps cooldown quand on a raté le grip"), SerializeField]
    private FrequencyCoolDown waitMissedGrip;

    private bool tryToAccrocheAfter = false;

    #endregion

    #region Initialization
    private void OnEnable()
    {
        EventManager.StartListening(GameData.Event.GameOver, StopAction);
        EventManager.StartListening(GameData.Event.Grounded, GroundedAction);
        InitValue();
    }

    private void InitValue()
    {
        gripped = false;
        display.SetActive(false);
        isTryingToUngrip = false;

        if (hardGrip == true)
        {
            Invoke("SetGripFromCode", 0.2f);
        }
    }

    private void SetGripFromCode()
    {
        rb.isKinematic = false;
        hardGrip = false;
        SetHardGrip(true);
    }
    #endregion

    #region Core
    /// <summary>
    /// renvoi vrai ou faux si on a le droit de sauter (selon le hold)
    /// </summary>
    /// <returns></returns>
    public bool CanGrip(bool setupPoint = true)
    {
        //faux si le cooldown n'est pas fini, ou qu'on est pas grounded
        if (!coolDownGrip.IsReady() || !worldCollision.IsGroundedSafe())
            return (false);

        Collider coll;
        if (worldCollision.PlatformManager.IsGrippable(out coll))
        {
            if (!setupPoint)
                return (true);

            // The distance from the explosion position to the surface of the collider.
            //Collider coll = objectToAggrip.GetComponent<Collider>();
            pointAttract = coll.ClosestPointOnBounds(transform.position);
            DebugExtension.DebugWireSphere(pointAttract, 1, 1);
            return (true);
        }
            

        return (false);
    }

    /// <summary>
    /// 
    /// </summary>
    [Button("HardGrip")]
    private void SetHardGrip(bool hardTrue)
    {
        if (!hardGrip || hardTrue)
        {
            hardGrip = true;
            bool succes = SetGrip(true);
            tryToAccrocheAfter = false;
            waitMissedGrip.Reset();

            if (!succes)
            {
                tryToAccrocheAfter = true;
                waitMissedGrip.StartCoolDown();
            }
        }
        else
        {
            hardGrip = false;
            SetGrip(false);
            tryToAccrocheAfter = false;
        }
    }

    /// <summary>
    /// aggriper !
    /// </summary>
    /// <returns></returns>
    public bool SetGrip(bool grip)
    {
        if (grip && !CanGrip())
            return (false);
        if (gripped == grip)
            return (false);

        gripped = grip;
        if (gripped)
        {
            Debug.Log("ici grip");
            isTryingToUngrip = false;
            display.SetActive(true);


            ropeHandler.SomeOneGripToWall();

            rb.drag = dragWhenGrip;
            animController.AnimGrip(true);
        }
        else
        {
            Debug.Log("ici ungrip");
            display.SetActive(false);
            isTryingToUngrip = false;

            rb.drag = playerController.InitialDrag;

            coolDownGrip.StartCoolDown();
            animController.AnimGrip(false);
        }
        return (true);
    }

    /// <summary>
    /// ici détermine si on vient juste d'être ungrip
    /// </summary>
    /// <returns></returns>
    public bool PlayerJustUngrip()
    {
        if (!coolDownGrip.IsReady())
            return (true);
        return (false);
    }

    public void ResetGrip()
    {
        if (gripped)
        {
            Debug.Log("ici ungrip hard");
            isTryingToUngrip = false;
            display.SetActive(false);
            rb.drag = playerController.InitialDrag;
            coolDownGrip.StartCoolDown();

            if (ScoreManager.Instance.Data.GetSimplified())
                waitHardUngrip.Reset();

            hardGrip = false;
            gripped = false;
            tryToAccrocheAfter = false;
            animController.AnimGrip(false);
        }
    }

    private bool HardGripSetup()
    {
        if (!ScoreManager.Instance.Data.GetSimplified())
            return (false);

        

        if (inputPlayer.GripDownInput && !gripped && waitHardUngrip.IsReady())
        {
            SetHardGrip(true);
            Debug.Log("ici grip haaaaard !");
            waitHardUngrip.StartCoolDown();
        }
        else if (inputPlayer.GripDownInput && (gripped || hardGrip) && waitHardUngrip.IsReady())
        {
            ResetGrip();
            Debug.Log("ici unnnnnnngrip haaaaard !");
            waitHardUngrip.StartCoolDown();
        }

        if (tryToAccrocheAfter && !waitMissedGrip.IsReady())
        {
            Debug.Log("ICIIIIII ON CONTINUE A ESSAYER");
            if (CanGrip())
            {
                Debug.Log("ici on hard grip ??");
                GripHard();
            }
             
        }

        if (inputPlayer.GripInput && !gripped && waitHardUngrip.IsReady())
        {
            SetHardGrip(true);
            Debug.Log("ici grip haaaaard !");
            waitHardUngrip.StartCoolDown();
        }


        return (true);
    }

    private void GripHard()
    {
        hardGrip = true;
        bool succes = SetGrip(true);
        tryToAccrocheAfter = false;
        waitMissedGrip.Reset();

    }

    public void TryToAutoGrip()
    {
        if (!ScoreManager.Instance.Data.GetSimplified())
            return;

        Debug.Log("TRY TO AUTO GRIP");

        StartCoroutine(AutoGrip());
    }

    private IEnumerator AutoGrip()
    {
        yield return new WaitForEndOfFrame();
        if (playerManager.IsOtherIsGripped(playerController.IdPlayer) && !worldCollision.IsOnFloor())
        {
            SetHardGrip(true);
        }
    }

    /// <summary>
    /// action de grip appelé quand on est grounded
    /// Setup juste le moment de changement
    /// </summary>
    private void GripSetup()
    {
        //si on reste appuyé
        if (inputPlayer.GripInput && !gripped)
        {
            SetGrip(true);
        }
        //si on lache, stopper le gripped
        //(et démarrer le coolDown pour pas réactiver tout de suite après)
        if ((inputPlayer.GripUpInput && gripped) || (!inputPlayer.GripInput && gripped))
        {
            SetGrip(false);
        }
    }

    /// <summary>
    /// ici action du grip à chaque fixedUpdate
    /// </summary>
    private void GripAction()
    {
        if (gripped && coolDownGrip.IsReady())
        {
            //si on est pas grippé, et qu'on a pas setup le timer, le faire
            if (!CanGrip(false) && !isTryingToUngrip)
            {
                Debug.Log("ici setup timer");
                waitBeforeResetGrip.StartCoolDown();
                isTryingToUngrip = true;
            }
            //si on peut s'aggriper, et que le timer à commenceer, l'annuler
            else if (CanGrip(false) && isTryingToUngrip)
            {
                Debug.Log("ici reset timer");
                isTryingToUngrip = false;
                waitBeforeResetGrip.Reset();
            }
            /*//si on peut pas s'aggriper, que le timer est fini, et qu'il n'a pas été annulé depuis...
            //lacher !
            else if (!CanGrip(false) && isTryingToUngrip && waitBeforeResetGrip.IsReady())
            {
                Debug.Log("ici ungrip car trop loin");
                isTryingToUngrip = false;
                SetGrip(false);
                return;
            }*/

            Vector3 dirAttractor = pointAttract - transform.position;
            rb.AddForce(dirAttractor * forceGrip * Time.fixedDeltaTime, ForceMode.Force);
        }

        
    }

    /// <summary>
    /// désactive le script
    /// </summary>
    private void StopAction()
    {
        stopAction = false;
    }
    #endregion

    #region Unity ending functions
    /// <summary>
    /// cette fonction est appelé par l'event WorldCollision en fixedUpdate
    /// (ici on sait si on est grounded ou pas, et comment)
    /// </summary>
    private void GroundedAction()
    {
        if (stopAction)
            return;

        if (HardGripSetup())
            return;

        if (hardGrip)
            return;

        GripSetup();
        
    }

    private void FixedUpdate()
    {
        GripAction();
    }

    /// <summary>
    /// désactiver
    /// </summary>
    private void OnDisable()
    {
        EventManager.StopListening(GameData.Event.GameOver, StopAction);
        EventManager.StopListening(GameData.Event.Grounded, GroundedAction);
    }
    #endregion
}
