/*using UnityEngine;
using UnityEngine.Analytics;
using System.Collections.Generic;

//[RequireComponent(typeof(CircleCollider2D))]
public class EventAnalytics : MonoBehaviour
{
    #region variable
    /// <summary>
    /// variable privé
    /// </summary>
    private static EventAnalytics eventAnalytics;
    private Dictionary<string, object> dict = new Dictionary<string, object>();

    #endregion

    #region fonction debug variables

    /// <summary>
    /// test si on met le script en UNIQUE
    /// </summary>
    private void setSingleton()
    {
        if (eventAnalytics == null)
            eventAnalytics = this;
        else if (eventAnalytics != this)
            Destroy(gameObject);
    }

    /// <summary>
    /// récupère la singularité (si ok par le script)
    /// </summary>
    static public EventAnalytics getSingleton()
    {
        if (!eventAnalytics)
        {
            Debug.LogError("impossible de récupérer le singleton");
            return (null);
        }
        return (eventAnalytics);
    }
    #endregion

    #region  initialisation
    /// <summary>
    /// Initialisation
    /// </summary>
    private void Awake()                                                    //initialisation referencce
    {
        setSingleton();
    }

    #endregion

    #region core script
    /// <summary>
    /// est appelé quand un level est échoué (le panel levelRestart s'affiche) en solo ou multi !
    /// </summary>
    public void exempleAnalytics(string idLevelFailed)
    {
        Analytics.CustomEvent("LevelCompletedTimeSolo", dict);
        Analytics.CustomEvent("levelFailedSolo", new Dictionary<string, object>
        {
            { "idMap", idLevelFailed }
        });
    }
    #endregion
}*/