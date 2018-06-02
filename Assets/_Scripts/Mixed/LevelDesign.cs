using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

/// <summary>
/// LevelDesign Description
/// </summary>
public class LevelDesign : MonoBehaviour
{
    #region Attributes
    
    [FoldoutGroup("GamePlay"), Tooltip("idLevel"), SerializeField]
    private int idLevel = 0;
    [FoldoutGroup("GamePlay"), Tooltip("idLevel"), SerializeField]
    private GameObject contentLevel;

    [FoldoutGroup("Debug"), Tooltip("ref to spawnFromPool"), SerializeField]
    private SpawnFromPool spawnFromPool;

    private static LevelDesign instance;
    public static LevelDesign GetSingleton
    {
        get { return instance; }
    }
    #endregion

    #region Initialization

    /// <summary>
    /// singleton
    /// </summary>
    public void SetSingleton()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else if (instance != this)
            Destroy(gameObject);
    }

    private void Awake()
    {
        SetSingleton();
    }
    #endregion

    #region Core
    public void InitLevelDesign()
    {
        if (!contentLevel)
        {
            Debug.Log("Erreur, no content ??");
            return;
        }
        HandleActivationScene();        //active la scene
        spawnFromPool.SpawnContent();   //active le spawn des objets non - players
    }

    private void HandleActivationScene()
    {
        //s'il est déja actif... peut être le réinitialiser ??
        //ou rechanger les données ???
        if (contentLevel.activeSelf)
        {
            Debug.Log("Error ici !!");
        }
        else
        {
            contentLevel.SetActive(true);
        }
    }

    /// <summary>
    /// est appelé quand on est dans le menu Setup, et qu'on s'apperçoi que la scene est active...
    /// </summary>
    public void HideContent()
    {
        if (contentLevel.activeSelf)
        {
            Debug.LogWarning("la scene est active, penser à la désactiver manuellement...");
            contentLevel.SetActive(false);
        }
    }

    public void DesactiveScene()
    {
        Destroy(gameObject);
    }
    #endregion

    #region Unity ending functions

	#endregion
}
