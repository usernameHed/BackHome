using UnityEngine;

/// <summary>
/// DontDestroyOnLoad Description
/// </summary>
public class DontDestroyOnLoad : MonoBehaviour
{
    //protected DontDestroyOnLoad() { } // guarantee this will be always a singleton only - can't use the constructor!

    #region Attributes
    private static DontDestroyOnLoad instance;
    public static DontDestroyOnLoad GetSingleton
    {
        get { return instance; }
    }
    #endregion

    #region Initialization
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
}
