using UnityEngine;
using System.Collections;

/// <summary>
/// Draw gizmo sprite in editor
/// <summary>
public class OnDrawGizmo : MonoBehaviour
{
    #region Attributes

	[SerializeField, Tooltip("image à placer sur l'objet")]
	private Sprite gizmoSprite;

    #endregion

    #region Core

	// Unity functions
    void OnDrawGizmos()
    {
		if (!gizmoSprite)
		{
			return;
		}

		Gizmos.DrawIcon(transform.position, gizmoSprite.name, true);
    }
    #endregion
}
