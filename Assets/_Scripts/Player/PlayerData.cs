using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class PlayerData : PersistantData
{
    #region Attributes

    [FoldoutGroup("GamePlay"), Tooltip("checkpoint"), SerializeField]
    private int checkpoint = 0;

    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    private bool restarted = false;

    #endregion

    #region Core
    public void SetCheckpoint(int index)
    {
        checkpoint = index;
    }
    public int GetCheckpoint()
    {
        return (checkpoint);
    }
    /// <summary>
    /// est-ce qu'on a restart ou pas ?
    /// </summary>
    public void SetRestart(bool restart)
    {
        restarted = restart;
    }
    public bool GetRestart()
    {
        return (restarted);
    }

    /// <summary>
    /// reset toute les valeurs à celle d'origine pour le jeu
    /// </summary>
    public void SetDefault()
    {
        checkpoint = 0;
        restarted = false;
    }

    public override string GetFilePath ()
	{
		return "playerData.dat";
	}

	#endregion
}