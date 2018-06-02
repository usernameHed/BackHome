using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(CircleCollider2D))]
public class ZoneEnding : MonoBehaviour                                   //commentaire
{
    #region public variable

    #endregion

    #region private variable
    /// <summary>
    /// variable privé
    /// </summary>
    bool enabledScript = true;
    #endregion

    #region  initialisation
    /// <summary>
    /// Initialisation
    /// </summary>
    private void Awake()
    {
        enabledScript = true;
    }

    #endregion

    #region core script
    /// <summary>
    /// Fonction qui parcourt les objets à mettre in/out de la caméra (selon active)
    /// </summary>
    private void EndGame()                                             //test
    {
        if (!enabledScript)
            return;

        enabledScript = false;
        SoundManager.Instance.PlaySound("Stop_all");
        GameManager.Instance.SceneManagerLocal.PlayIndex(2);
    }
    #endregion

    #region unity fonction and ending

    /// <summary>
    /// action lorsque le joueur entre dans une zone
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter(Collider collision)
    {
        //si c'est un collider 2D, et que son objet de reference est un joueur
        if (collision.CompareTag(GameData.Prefabs.Player.ToString()))
        {
            //collision.gameObject.GetComponent<PlayerController>().addZone(this);
            EndGame();
        }
    }
  
    #endregion
}
