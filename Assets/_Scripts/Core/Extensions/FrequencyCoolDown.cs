using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class FrequencyCoolDown
{
    [Tooltip("Temsp de coolDown"), SerializeField]
    private float timeCoolDown = 1f;           //optimisation du temps
    public float TimeCoolDown { get { return (timeCoolDown); } }

    private bool coolDownStarted = false;

    private float timeToGo;                                             //optimisation
    /// <summary>
    /// Initialise l'optimisation
    /// </summary>
    public void StartCoolDown(float time = -1)
    {
        time = (time == -1) ? timeCoolDown : time;
        timeToGo = Time.fixedTime + time;
        coolDownStarted = true;
    }

    public bool IsReady()
    {
        if (!IsWaiting() || IsStartedAndOver())
            return (true);
        return (false);
    }

    /// <summary>
    /// le cooldown est commencé mais pas fini
    /// </summary>
    public bool IsWaiting()
    {
        if (IsStarted() && !IsReady(false))
        {
            return (true);
        }
        return (false);
    }
    /// <summary>
    /// le cooldown a commencé, et est temriné
    /// </summary>
    public bool IsStartedAndOver()
    {
        if (IsStarted() && IsReady(true))
        {
            return (true);
        }
        return (false);
    }




    private bool IsStarted()
    {
        return (coolDownStarted);
    }

    private bool IsReady(bool canDoReset)
    {
        if (!coolDownStarted)   //le coolDown n'as jamais commencer... ne rien faire
            return (false);

        if (Time.fixedTime >= timeToGo) //le coolDown a commencé, et est terminé !
        {
            if (canDoReset)
                Reset();
            return (true);
        }
        return (false); //cooldown a commencé, et est en cours
    }

    public void Reset()
    {
        coolDownStarted = false;    //le cooldown est fini, on reset
        timeToGo = Time.fixedTime;
    }
}
