using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;

/// <summary>
/// DisplayScore Description
/// </summary>
public class DisplayScore : MonoBehaviour
{
    #region Attributes
    [FoldoutGroup("Debug"), Tooltip("score courant du joueur"), SerializeField]
    private TextMeshProUGUI textCurrent;

    [FoldoutGroup("Debug"), Tooltip("scoreMAx du joueur"), SerializeField]
    private TextMeshProUGUI textMax;
    #endregion

    #region Initialization

    private void Awake()
    {
        textCurrent.text = ScoreManager.Instance.Data.CurrentCollectible.ToString();
        textMax.text = ScoreManager.Instance.Data.MaxCollectible.ToString();
    }
    #endregion

    #region Core

    #endregion

    #region Unity ending functions

	#endregion
}
