using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System;

/// <summary>
/// PlayerController handle player movement
/// <summary>
public class BlockerBehaviour : MonoBehaviour, IKillable, IPooledObject
{
    #region Attributes
    private bool enabledObject = true;
    private Rigidbody rigidBody;
    #endregion

    #region Initialize
    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        Init();
    }

    /// <summary>
    /// activé quand l'ennemy est spawn
    /// </summary>
    public void OnObjectSpawn()
    {
        Init();
    }

    private void Init()
    {
        enabledObject = true;
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
    }

    private void OnEnable()
    {
        EventManager.StartListening(GameData.Event.GameOver, StopAction);
    }
    #endregion

    #region Core
    /// <summary>
    /// appelé quand le jeu est fini...
    /// </summary>
    private void StopAction()
    {
        enabledObject = false;
    }

    /// <summary>
    /// appelé lorsque la pool clean up les objet actif et les désactif (lors d'une transition de scene)
    /// </summary>
    public void OnDesactivePool()
    {
        Debug.Log("DesactiveFromPool");
        StopAction();
        gameObject.SetActive(false);
    }
    #endregion

    #region Unity ending functions

    private void OnDisable()
    {
        EventManager.StopListening(GameData.Event.GameOver, StopAction);
    }

    #endregion


    [FoldoutGroup("Debug"), Button("Kill")]
    public void Kill()
	{
        if (!enabledObject)
            return;
        enabledObject = false;

        gameObject.SetActive(false);
    }
}