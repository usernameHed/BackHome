using UnityEngine;
using Sirenix.OdinInspector;
using System;

/// <summary>
/// Link Description
/// </summary>
public class Link : MonoBehaviour, IPooledObject, IKillable
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip("Force de l'élastique"), SerializeField]
    private int idFromRope = 0;
    public int IdFromRope { get { return (idFromRope); } set { idFromRope = value; } }

    [FoldoutGroup("Object"), Tooltip("Ref à la rope"), SerializeField]
    private Rope rope;
    public Rope RopeScript { get { return (rope); } set { rope = value; } }

    #endregion

    #region Initialization

    #endregion

    #region Core
    /// <summary>
    /// appelé lors du spawn du link !
    /// </summary>
    public void OnObjectSpawn()
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
    }
    #endregion

    #region Unity ending functions
    public void RealyKill()
    {
        transform.SetParent(ObjectsPooler.Instance.transform);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// ici gère la suppresion du link (gérer avec la rope !)
    /// </summary>
    [FoldoutGroup("Debug"), Button("Kill")]
    public void Kill()  //tryToKill !
    {
        RopeScript.DeleteLink(idFromRope);
    }

    /// <summary>
    /// appelé lorsque la pool clean up les objet actif et les désactif (lors d'une transition de scene)
    /// </summary>
    public void OnDesactivePool()
    {
        Debug.Log("DesactiveFromPool");
        RealyKill();
    }

    #endregion
}
