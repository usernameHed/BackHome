using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// description
/// </summary>
//[RequireComponent(typeof(CircleCollider2D))]
public class SoundManager : SerializedMonoBehaviour                                   //commentaire
{
    #region public variable
    /// <summary>
    /// variable public
    /// </summary>
    
    [SerializeField]
    public Dictionary<string, WwiseEventEmitter> soundsEmitter = new Dictionary<string, WwiseEventEmitter>();
    public bool soundOn = true;
    public AkBank akBank;

    //public FmodEventEmitter musicEmitterScript;

    public int MusicState
    {
        get
        {
            return (musicState);
        }
        set
        {
            if (musicState != value)
            {
                musicState = value;
                StateMusicChanged();
            }
        }
    }

    private int musicState = 0;

    private static SoundManager instance;
    public static SoundManager Instance
    {
        get { return instance; }
    }

    #endregion

    #region  initialisation
    /// <summary>
    /// Initialisation
    /// </summary>

    private void SetSingleton()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    private void Awake()                                                    //initialisation referencce
    {
        SetSingleton();                                                  //set le script en unique ?
        //akBank = gameObject.AddComponent<AkBank>();
        //akBank.bankName = "New_Name";
    }
    #endregion

    #region core script

    /// <summary>
    /// appelé lorsque la state de la musique a changé
    /// </summary>
    private void StateMusicChanged()
    {
        //playSound(GetEmitter("BackgroundMusic"), "Checkpoint", musicState);
    }

    /// <summary>
    /// ajoute une key dans la liste
    /// </summary>
    public void AddKey(string key, WwiseEventEmitter value)
    {
        foreach (KeyValuePair<string, WwiseEventEmitter> sound in soundsEmitter)
        {
            if (key == sound.Key)
            {
                soundsEmitter[sound.Key] = value;
                return;
            }
        }
        soundsEmitter.Add(key, value);
    }

    /// <summary>
    /// ajoute une key dans la liste
    /// </summary>
    public void DeleteKey(string key, WwiseEventEmitter value)
    {
        Debug.Log("delete key: " + key);
        foreach (KeyValuePair<string, WwiseEventEmitter> sound in soundsEmitter)
        {
            if (key == sound.Key)
            {
                soundsEmitter.Remove(key);
                return;
            }
        }
        Debug.Log("key sound not found");
    }

    private WwiseEventEmitter GetEmitter(string soundTag)
    {
        foreach (KeyValuePair<string, WwiseEventEmitter> sound in soundsEmitter)
        {
            if (soundTag == sound.Key)
            {
                return (sound.Value);
            }
        }
        return (null);
    }

    /// <summary>
    /// joue un son de menu (sans emmiter)
    /// </summary>
    public void PlaySound(string soundTag, bool stop = false)
    {
        if (soundTag == null || soundTag == "")
            return;

        /*if (!soundTag.Contains("event:/"))
            soundTag = "event:/SFX/" + soundTag;*/
        PlaySound(GetEmitter(soundTag), stop);
        //FMODUnity.RuntimeManager.PlayOneShot("2D sound");   //methode 1 
    }

    /// <summary>
    /// ici play l'emitter (ou le stop)
    /// </summary>
    /// <param name="emitterScript"></param>
    public void PlaySound(WwiseEventEmitter emitterScript, bool stop = false)
    {
        if (!emitterScript)
        {
            Debug.LogWarning("Emmiter SOund not found !!");
            return;
        }

        if (!soundOn)
            return;

        if (!stop)
            emitterScript.Play();
        else
            emitterScript.Stop();
    }

    /// <summary>
    /// ici change le paramettre de l'emitter
    /// </summary>
    /// <param name="emitterScript"></param>
    public void SoundChangeState(WwiseEventEmitter emitterScript, string paramState, string paramName)
    {
        emitterScript.SetStateValue(paramState, paramName);
    }

    #endregion
}
