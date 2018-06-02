using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public class WwiseEventEmitter : MonoBehaviour
{
    [FoldoutGroup("Main Sound"), Tooltip("nom du son exacte à jouer"), SerializeField]
    private string nameSoundToPlay = "";
    [FoldoutGroup("Main Sound"), Tooltip("lien de l'ak (dans le même gameObject)"), SerializeField]
    private AkAmbient soundToPlay;

    [FoldoutGroup("Stop Sound"), Tooltip("nom du son exacte à stopper"), SerializeField]
    public string nameSoundToStop = "";
    [FoldoutGroup("Stop Sound"), Tooltip("lien de l'ak (dans le même gameObject)"), SerializeField]
    public AkAmbient soundToPlayStop;

    [FoldoutGroup("Spacialisation"), Tooltip("le son est-il spacialisé ?"), SerializeField]
    public bool spacialisationEnabled = false;
    [FoldoutGroup("Spacialisation"), EnableIf("spacialisationEnabled"), Tooltip("le lien de l'objet parent d'ou on appelle les sons..."), SerializeField]
    public Transform mainParent;

    void Start()
    {
        //AkSoundEngine.PostEvent("Play_music_placeholder");
        //AkSoundEngine.PostEvent("Play_music_placeholder", this.gameObject);

        //emitter = gameObject.GetComponent<AkAmbient>();  //init l'emitter
        SoundManager.Instance.AddKey(GetNameId(), this);
        if (nameSoundToStop != "")
            SoundManager.Instance.AddKey(GetNameStopId(), this);
    }

    private string GetNameId()
    {
        //string addParent = (addIdEvent) ? soundToPlay.eventID.ToString() : "";
        string addParent = (spacialisationEnabled) ? mainParent.GetInstanceID().ToString() : "";
        return (nameSoundToPlay + addParent);
    }
    private string GetNameStopId()
    {
        //string addParent = (addIdEvent) ? soundToPlayStop.eventID.ToString() : "";
        string addParent = (spacialisationEnabled) ? mainParent.GetInstanceID().ToString() : "";
        return (nameSoundToStop + addParent);
    }

    /// <summary>
    /// play l'emmiter
    /// </summary>
    [Button("play")]
    public void Play()
    {
        if (!gameObject || !soundToPlay)
            return;
        //SendMessage("Play");
        AkSoundEngine.PostEvent(nameSoundToPlay, this.gameObject);
    }

    /// <summary>
    /// stop l'emmiter
    /// </summary>
    [Button("stop")]
    public void Stop()
    {
        if (!gameObject || !soundToPlayStop)
            return;
        AkSoundEngine.PostEvent(nameSoundToStop, this.gameObject);
    }

    /// <summary>
    /// AkSoundEngine.SetState("MainStateGroup", "Movement");
    /// </summary>
    public void SetStateValue(string paramState, string paramName)
    {
        if (!gameObject || !soundToPlay)
            return;
        //emitter.SetParameter(paramName, value);
        AkSoundEngine.SetState(paramState, paramName);
    }

    /// <summary>
    /// exemple: paramName: Pitch
    /// </summary>
    public void SetRTPCValue(string paramName, float value)
    {
        if (!gameObject || !soundToPlay)
            return;
        AkSoundEngine.SetRTPCValue(paramName, value, gameObject);
    }


    private void OnDestroy()
    {
        Debug.Log("on destroy ??");
        return;
        //string addParent = (addIdOfObject) ? addIdOfObject.GetInstanceID().ToString() : "";
        //if (emitter && emitter.Event != "" && SoundManager.GetSingleton)
        SoundManager.Instance.DeleteKey(GetNameId(), this);
        if (nameSoundToStop != "")
            SoundManager.Instance.DeleteKey(GetNameStopId(), this);
    }
}
