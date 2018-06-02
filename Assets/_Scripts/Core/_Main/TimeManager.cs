using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// TimeManager Description
/// </summary>
public class TimeManager : SingletonMono<TimeManager>
{
    protected TimeManager() { } // guarantee this will be always a singleton only - can't use the constructor!

    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip("slowDonwFactor"), SerializeField]
    private float slowDonwFactor = 0.05f;             //le type de ball (bleu, red...)
    [FoldoutGroup("GamePlay"), Tooltip("slowDonwFactor"), SerializeField]
    private float slowDownLenght = 2f;             //le type de ball (bleu, red...)

    private static float originalFixedDeltaTime;

    #endregion

    #region Initialization

    private void Start()
    {
        //This Prevents slow motion in Game Editor.
        originalFixedDeltaTime = Time.fixedDeltaTime;

    }
    #endregion

    #region Core

    private void UpdateTimeScale()
    {
        if (Time.timeScale < 1)
        {
            Time.timeScale += (1f / slowDownLenght) * Time.unscaledDeltaTime;//Get Back to normal everyframe
            Time.fixedDeltaTime = Time.timeScale * originalFixedDeltaTime;//Adjust Physics - Get Back to normal everyframe by the same factor as TimeScale
        }
        else
        {
            Time.timeScale = 1;
            Time.fixedDeltaTime = originalFixedDeltaTime;
        }

    }

    public void DoSlowMothion()
    {
        Time.timeScale = slowDonwFactor;
        //Adjust Physics - Slowdown by the same factor as TimeScale
        Time.fixedDeltaTime = Time.timeScale * originalFixedDeltaTime;

        //Time.timeScale = slowDonwFactor;
        //Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }
    #endregion

    #region Unity ending functions
    private void Update()
    {
        UpdateTimeScale();
        //Time.timeScale += (1f / slowDonwLenght) * Time.unscaledDeltaTime;
        //Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
    }
    #endregion
}
