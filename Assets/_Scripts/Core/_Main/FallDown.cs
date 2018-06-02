using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Networking;

public class FallDown : MonoBehaviour
{
    #region variable
    /// <summary>
    /// variable
    /// </summary>
    [FoldoutGroup("GamePlay"), Tooltip("gravité de l'objet différent ? 1 = gravité identique, > 1 = gravité de l'objet plus forte"), SerializeField]
    private float overallGravityAmplify = 1.05f;

    [FoldoutGroup("GamePlay"), Tooltip("Ici la gravité à ajouter sur l'objet uniquement quand il est en train de descendre (> 1 pour augmenter)"), SerializeField]
    private float fallDownGravity = 1.05f;

    [FoldoutGroup("GamePlay"), Tooltip("marge d'erreur quand on commence à descendre"), SerializeField]
    private float marginDescend = 0.05f;

    [FoldoutGroup("Object"), Tooltip("gravité de l'objet différent ?"), SerializeField]
    private GameObject objectCollider;

    private Rigidbody rb;
    #endregion

    #region  initialisation
    /// <summary>
    /// appelé lors de l'initialisation de ce weapon
    /// </summary>
    private void Awake()
    {
        rb = objectCollider.GetComponent<Rigidbody>();
    }
    #endregion

    #region core script


    #endregion

    #region unity fonction and ending
    private void FixedUpdate()
    {
        //gravité accru sur l'objet
        if (overallGravityAmplify != 1)
            rb.velocity += Vector3.up * Physics.gravity.y * (overallGravityAmplify - 1);

        //gravité accru quand on descend !
        if (rb.velocity.y < (0 - marginDescend) && fallDownGravity != 1)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallDownGravity - 1);
        }
    }
    #endregion
}
