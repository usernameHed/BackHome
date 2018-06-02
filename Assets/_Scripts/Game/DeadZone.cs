using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Kill any IKillable instance on contact
/// <summary>
public class DeadZone : MonoBehaviour
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip("prefabs à tuer"), SerializeField]
    private List<GameData.Prefabs> listPrefabsToKill;

	#endregion

    #region Core

    private void OnTriggerEnter(Collider other)
    {
		TryKill (other.gameObject, true);
    }

	private void OnTriggerExit(Collider other)
	{
		TryKill (other.gameObject, false);
    }

    /// <summary>
    /// essai de tuer...
    /// </summary>
	private void TryKill(GameObject other, bool kill)
	{
        for (int i = 0; i < listPrefabsToKill.Count; i++)
        {
            if (other.CompareTag(listPrefabsToKill[i].ToString()))
            {
                PlayerController playerController = other.GetComponent<PlayerController>();
                if (playerController)
                {
                    playerController.SetCloseToDeath(kill);
                }
                EventManager.TriggerEvent(GameData.Event.TryToEnd);
            return;
            }
        }
    }

    #endregion
}
