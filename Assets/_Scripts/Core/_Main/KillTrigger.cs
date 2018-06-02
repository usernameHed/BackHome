using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Kill any IKillable instance on contact
/// <summary>
public class KillTrigger : MonoBehaviour
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip("prefabs à tuer"), SerializeField]
    private List<GameData.Prefabs> listPrefabsToKill;

    [FoldoutGroup("GamePlay"), Tooltip("Est-ce qu'on se tue sois même quand on kill ?"), SerializeField]
    private bool killItSelf = false;

    [FoldoutGroup("GamePlay"), Tooltip("Collision à l'entré"), SerializeField]
    private bool killOnEnter = true;

    [FoldoutGroup("GamePlay"), Tooltip("Collision à la sortie"), SerializeField]
    private bool killOnExit = false;

	#endregion

    #region Core

    private void OnTriggerEnter(Collider other)
    {
		if (killOnEnter)
		{
			TryKill (other.gameObject);
		}
    }

	private void OnTriggerExit(Collider other)
	{
		if (killOnExit)
		{
			TryKill (other.gameObject);
            /*
             * PlayerController playerController = other.GetComponent<PlayerController>();
                    if (playerController)
                    EventManager.TriggerEvent(GameData.Event.TryToEnd);
             * */
        }
    }

    /// <summary>
    /// on se tue soit même
    /// </summary>
    private void KillSelf()
    {
        IKillable killable = gameObject.GetComponent<IKillable>();
        if (killable != null)
            killable.Kill();
    }

    /// <summary>
    /// essai de tuer...
    /// </summary>
	private void TryKill(GameObject other)
	{
		IKillable killable = other.GetComponent<IKillable> ();
        if (killable != null)
		{
            for (int i = 0; i < listPrefabsToKill.Count; i++)
            {
                if (other.CompareTag(listPrefabsToKill[i].ToString()))
                {
                    killable.Kill();
                    if (killItSelf)
                        KillSelf();
                    return;
                }
            }
		}


    }

    #endregion
}
