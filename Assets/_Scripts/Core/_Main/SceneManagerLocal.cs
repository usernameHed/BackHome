using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using Sirenix.Serialization;
using System;

/// <summary>
/// MenuManager Description
/// </summary>
public class SceneManagerLocal : MonoBehaviour
{
    //[SerializeField]
    [Serializable]
    public struct SceneInfo
    {
        [Space(10)]
        [Header("Scene à charger")]
        [Tooltip("Activation")]
        public bool active;
        [Tooltip("Scene Name")]
        public string scene;
        [Tooltip("Charge la scène en mémoire dès le début ?")]
        public bool loadAtStart;
        [EnableIf("loadAtStart"), Tooltip("Si on charge au start, est-ce qu'on attend X seconde ou pas ? (default = 0)")]
        public float loadAfterXSecond;
        
        [Header("Effet de la transition")]
        [Tooltip("Fade lors de la transition ?")]
        public bool fade;
        [EnableIf("fade"), Tooltip("Temps de fade")]
        public float fadeTime;
        [EnableIf("loadAtStart"), DisableIf("fade"), Tooltip("Load la scène en additif ?")]
        public bool additive;
        [EnableIf("loadAtStart"), Tooltip("Swap lorsque la scène est chargé ? Le changement marche en combinaison d'un fade, et d'une additive (fade puis swap complletement ok, additif puis ajoute l'additif au jeu ok)")]
        public bool swapWhenLoaded;
        [EnableIf("additive"), Tooltip("Lorsque la scene aditive est chargé, charge la scene")]
        public string sceneToChargeAfterAdditive;
    }

    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip("Caméra de la scene"), SerializeField]
    private GameObject cameraObject;
    public GameObject CameraObject { get { return cameraObject; } }

    [FoldoutGroup("Scene"), Tooltip("Scene to load at start"), SerializeField]
    private List<SceneInfo> sceneToLoad;
    public List<SceneInfo> SceneToLoad { get { return (sceneToLoad); } }

    [FoldoutGroup("Scene"), Tooltip("Scene to load at start"), SerializeField]
    private GameObject levelMangerInterfacce;

    private ILevelManager levelManger;
    public ILevelManager LevelManagerScript { get { return (levelManger); } }

    private bool enabledScript = true;
    #endregion

    #region Initialization

    private void Start()
    {
        if (levelManger == null)
        {
            if (!levelMangerInterfacce)
                Debug.LogError("PAS DE LEVEL MANAGER");
            levelManger = levelMangerInterfacce.GetComponent<ILevelManager>();
            if (levelManger == null)
                Debug.LogError("PAS DE III LEVEL MANAGER");
        }

        
        GameManager.Instance.SceneManagerLocal = this;

        InitSceneLoading();
    }
    #endregion

    #region Core
    /// <summary>
    /// gère le lancement des chargements des scenes
    /// </summary>
    private void InitSceneLoading()
    {
        enabledScript = true;

        for (int i = 0; i < sceneToLoad.Count; i++)
        {
            if (sceneToLoad[i].loadAtStart && sceneToLoad[i].active)
            {
                if (sceneToLoad[i].loadAfterXSecond == 0)
                    StartLoading(i);
                else
                    StartCoroutine(WaitAndStart(i, sceneToLoad[i].loadAfterXSecond));
            }
        }
    }
    private IEnumerator WaitAndStart(int index, float time)
    {
        yield return new WaitForSeconds(time);
        StartLoading(index);
    }

    private void StartLoading(int index)
    {
        if (!sceneToLoad[index].active)
        {
            return;
        }

        SceneManagerGlobal.Instance.StartLoading(   sceneToLoad[index].scene,
                                                        sceneToLoad[index].swapWhenLoaded,
                                                        sceneToLoad[index].additive,
                                                        sceneToLoad[index].fade,
                                                        sceneToLoad[index].fadeTime,
                                                        sceneToLoad[index].sceneToChargeAfterAdditive);
    }
    /// <summary>
    /// demande de charger une scène précise
    /// (peut être appelé si c'est une scène qui ne se charge pas au démarrage)
    /// </summary>
    public void StartLoading(string scene)
    {
        for (int i = 0; i < sceneToLoad.Count; i++)
        {
            if (sceneToLoad[i].scene == scene)
            {
                StartLoading(i);
                return;
            }
        }
    }

    /// <summary>
    /// ici lance le jeu, il est chargé !
    /// </summary>
    [FoldoutGroup("Debug"), Button("Play")]
    public void PlayNext()
    {
        if (!enabledScript)
            return;

        enabledScript = false;

        SceneManagerGlobal.Instance.ActivateScene(
            sceneToLoad[0].scene,
            sceneToLoad[0].fade,
            sceneToLoad[0].fadeTime);    //hard code du next ?

        //ici gère les unloads ?
    }
    public void PlayPrevious(bool unloadFirst = true)
    {
        if (!enabledScript)
            return;

        enabledScript = false;

        if (unloadFirst)
            SceneManagerGlobal.Instance.UnloadScene(sceneToLoad[0].scene);

        ObjectsPooler.Instance.desactiveEveryOneForTransition();
        ObjectsPoolerLocal.Instance.desactiveEveryOneForTransition();

        SceneManagerGlobal.Instance.JumpToScene(sceneToLoad[1].scene, sceneToLoad[1].fade, sceneToLoad[1].fadeTime);    //hard code du previous ?
        //ici gère les unloads ?
    }

    public void PlayIndex(int index, bool unloadFirst = true)
    {
        if (!enabledScript)
            return;

        enabledScript = false;

        if (unloadFirst)
            SceneManagerGlobal.Instance.UnloadScene(sceneToLoad[0].scene);

        ObjectsPooler.Instance.desactiveEveryOneForTransition();
        ObjectsPoolerLocal.Instance.desactiveEveryOneForTransition();

        SceneManagerGlobal.Instance.JumpToScene(sceneToLoad[index].scene, sceneToLoad[index].fade, sceneToLoad[index].fadeTime);    //hard code du previous ?
        //ici gère les unloads ?
    }

    [FoldoutGroup("Debug"), Button("Quit")]
    public void Quit()
    {
        if (!enabledScript)
            return;

        enabledScript = false;

        SceneManagerGlobal.Instance.QuitGame(true);
    }

    #endregion

    #region Unity ending functions

    #endregion
}
