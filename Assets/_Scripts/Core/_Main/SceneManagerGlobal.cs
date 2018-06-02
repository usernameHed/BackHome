using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

public class SceneManagerGlobal : SingletonMono<SceneManagerGlobal>
{
    protected SceneManagerGlobal() { } // guarantee this will be always a singleton only - can't use the constructor!

    #region Attributes

    private struct SceneCharging
    {
        public string scene;
        public AsyncOperation async;
        public bool isAdditive;
        public bool swapWhenFinishUpload;
        public string sceneToChargeAfterAdditive;
    }

    private List<SceneCharging> sceneCharging = new List<SceneCharging>();

    private bool closing = false;

    #endregion

    #region Initialization

    #endregion

    #region Core

    ///////////////////////////////////////////////////////////////////////////// gestion asyncrone
    /// <summary>
    /// Ici ajoute une scene à charger
    /// </summary>
    /// <param name="scene">nom de la scène</param>
    /// <param name="swapWhenLoaded">est-ce qu'on change de scène dès qu'elle fini de charger ?</param>
    /// <param name="additive">est-ce qu'on ajoute la scène en additif ou pas ??</param>
    public void StartLoading(string scene, bool swapWhenLoaded = true, bool additive = false, bool fade = false, float speedFade = 1.0f, string sceneToChargeAfterAdditive = "")
    {
        

        if ((additive && fade) || scene == "")
        {
            Debug.LogError("pas possible");
            return;
        }

        SceneCharging sceneToCharge;
        ////////////////store scene to charge
        sceneToCharge.scene = scene;
        sceneToCharge.async = (!additive) ? SceneManager.LoadSceneAsync(scene)
                                         : SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        sceneToCharge.isAdditive = additive;
        sceneToCharge.swapWhenFinishUpload = swapWhenLoaded;
        sceneToCharge.async.allowSceneActivation = sceneToCharge.swapWhenFinishUpload;
        sceneToCharge.sceneToChargeAfterAdditive = sceneToChargeAfterAdditive;

        //ajoute la scène à charger...
        sceneCharging.Add(sceneToCharge);

        if (fade && sceneToCharge.swapWhenFinishUpload)
        {
            ActivateSceneWithFade(sceneCharging.Count - 1, speedFade);
            return;
        }

        if (sceneToCharge.swapWhenFinishUpload)
            StartCoroutine(SwapAfterLoad(sceneCharging.Count - 1));
    }

    /// <summary>
    /// ici on veut lancer une scène qui est / à été chargé ! on a son nom, on veut
    /// la chercher dans la liste et l'activer !
    /// </summary>
    public void ActivateScene(string scene, bool fade = false, float speedFade = 1f)
    {
        for (int i = 0; i < sceneCharging.Count; i++)
        {
            if (sceneCharging[i].scene == scene)
            {
                Debug.Log("Ou la ?");
                StartCoroutine(ActiveSceneWithFadeWait(i, speedFade));
                return;
            }
        }
        Debug.Log("scene not found ! Load the scene en normal...:" + scene);
        JumpToScene(scene, fade, speedFade);
    }


    /// <summary>
    /// Ici active la scène demandé, si elle à été chargé !
    /// </summary>
    /// <param name="index">index de la scène de la list</param>
    /// <param name="justActive">ici on est sur que la scène est chargé !</param>
    private void ActivateScene(int index, bool restartIfNotCharged = false, float time = 0.5f)
    {
        if (index < 0 || index >= sceneCharging.Count)
        {
            Debug.Log("ici on n'aurai pas du faire ça...");
            return;
        }

        Debug.Log("ici active normalement...");

        sceneCharging[index].async.allowSceneActivation = true;

        string newScene = (sceneCharging[index].isAdditive) ? sceneCharging[index].sceneToChargeAfterAdditive : "";

        sceneCharging.RemoveAt(index);

        if (newScene != "")
            FindWitchOneToLoadAfterAdditive(newScene);
    }
    //relance l'essai d'activation
    private IEnumerator WaitForActivateScene(int index, float time) { yield return new WaitForSeconds(time); ActivateScene(index, true, time);  }


    private void FindWitchOneToLoadAfterAdditive(string sceneToChargeAfterAdditive)
    {
        EventManager.TriggerEvent(GameData.Event.AdditiveJustFinishLoad);

        Debug.Log("findAfterAdditive;.." + sceneToChargeAfterAdditive);
        SceneManagerLocal local = GameManager.Instance.SceneManagerLocal;
        local.StartLoading(sceneToChargeAfterAdditive);
    }

    /// <summary>
    /// Ici Lance la scène [index] Dès qu'elle est chargé ! Qu'elle soit asyncro ou pas !
    /// </summary>
    private IEnumerator SwapAfterLoad(int index)
    {
        Debug.LogWarning("wait before switch... just wait");

        yield return sceneCharging[index].async;
        ActivateScene(index, true);
    }

    /// <summary>
    /// ici s'occupe de faire un fade, puis d'activer la scène ensuite
    /// </summary>
    private void ActivateSceneWithFade(int index, float speedFade)
    {
        Debug.Log("ici ??");
        StartCoroutine(ActiveSceneWithFadeWait(index, speedFade));
    }
    private IEnumerator ActiveSceneWithFadeWait(int index, float speedFade)
    {
        float fadeTime = gameObject.GetComponent<Fading>().BeginFade(1, speedFade);
        yield return new WaitForSeconds(fadeTime / 2);
        Debug.Log("passe ici ??");
        ActivateScene(index, true); //essay d'activer, si on n'y arrive pas on réésai !!!
    }




    /// <summary>
    /// Unload une scene précédement loadé !
    /// </summary>
    public void UnloadScene(int index)
    {
        SceneManager.UnloadSceneAsync(sceneCharging[index].scene);
        sceneCharging.RemoveAt(index);
    }
    /// <summary>
    /// est appelé si on doit annuler le chargement d'une scene !
    /// </summary>
    public void UnloadScene(string scene)
    {
        for (int i = 0; i < sceneCharging.Count; i++)
        {
            if (sceneCharging[i].scene == scene)
            {
                sceneCharging.RemoveAt(i);
            }
        }
        SceneManager.UnloadSceneAsync(scene);
    }

    
    /// <summary>
    /// jump à une scène
    /// </summary>
    [ContextMenu("JumpToScene")]
    public void JumpToScene(string scene, bool fade = false, float fadeSpeed = 1.5f)
    {
        if (!fade)
        {
            SceneManager.LoadScene(scene);
            return;
        }
        StartCoroutine(JumpToSceneWithFadeWait(scene, fadeSpeed));
    }
    private IEnumerator JumpToSceneWithFadeWait(string scene, float speed)
    {
        /*float fadeTime = */gameObject.GetComponent<Fading>().BeginFade(1, speed);
        yield return new WaitForSeconds(speed / 2);
        JumpToScene(scene);
    }

    /// <summary>
    /// ajoute une scène à celle courrante
    /// </summary>
    [ContextMenu("JumpAdditiveScene")]
    public void JumpAdditiveScene(string scene)
    {
        SceneManager.LoadScene(scene, LoadSceneMode.Additive);
    }




    /// <summary>
    /// quit avec un fade !
    /// </summary>
    public void QuitGame(bool fade = false, float speed = 1.5f)
    {
        if (!fade)
        {
            Quit();
            return;
        }
        StartCoroutine(QuitWithFade(speed));
    }
    private IEnumerator QuitWithFade(float speed)
    {
        /*float fadeTime = */gameObject.GetComponent<Fading>().BeginFade(1, speed);
        yield return new WaitForSeconds(speed / 2);
        Quit();
    }

    /// <summary>
    /// appelé naturellement quand on ferme le jeu
    /// </summary>
    private void OnApplicationQuit()
    {
        if (closing)
            return;
        Quit();
    }

    /// <summary>
    /// quite le jeu (si on est dans l'éditeur, quite le mode play)
    /// </summary>
    [ContextMenu("Quit")]
    private void Quit()
    {
        closing = true;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion
}