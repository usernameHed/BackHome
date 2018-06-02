using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Checkpoint Description
/// </summary>
public class Checkpoint : MonoBehaviour
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    private int checkpointId = 1;
    [FoldoutGroup("GamePlay"), Tooltip(""), SerializeField]
    private Transform pointPos;
    public Vector3 PointPos() { return pointPos.position; }
    #endregion

    #region Initialization

    private void Start()
    {
		// Start function
    }
    #endregion

    #region Core

    #endregion

    #region Unity ending functions

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(GameData.Prefabs.Player.ToString()))
        {
            Debug.Log("ici save checkpoint " + checkpointId);
            ScoreManager.Instance.Data.SetCheckpoint(checkpointId);
        }
    }

    #endregion
}
