using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class PlayerData : PersistantData
{
    #region Attributes

    [FoldoutGroup("GamePlay"), Tooltip("checkpoint"), SerializeField]
    private int checkpoint = 0;

    [FoldoutGroup("GamePlay"), Tooltip("checkpoint"), SerializeField]
    private int maxCollectible = 6;
    public int MaxCollectible { get { return maxCollectible; } }

    [FoldoutGroup("GamePlay"), Tooltip("checkpoint"), SerializeField]
    private int currentCollectible = 0;
    public int CurrentCollectible { get { return currentCollectible; } }

    [FoldoutGroup("GamePlay"), Tooltip("checkpoint"), SerializeField]
    private bool simplified = false;

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

    public void SetSimplified(bool simple)
    {
        simplified = simple;
    }
    public bool GetSimplified()
    {
        return (simplified);
    }

    public void AddCollectible()
    {
        currentCollectible += 1;
        if (currentCollectible > maxCollectible)
            currentCollectible = maxCollectible;
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
        currentCollectible = 0;
        restarted = false;
    }

    public override string GetFilePath ()
	{
		return "playerData.dat";
	}

	#endregion
}