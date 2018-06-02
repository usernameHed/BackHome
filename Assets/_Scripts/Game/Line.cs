using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Line : MonoBehaviour
{
    #region Attributes
    public Transform pos1;
    public Transform pos2;
    public LineRenderer lineRenderer;
    #endregion

    #region Init
    #endregion

    #region core

    #endregion

    /// <summary>
    /// update la position des extrémités de la corde en lateUpdate
    /// (après les calcules des nouvelles positions physiques)
    /// </summary>
    private void LateUpdate ()
    {
		lineRenderer.SetPosition (0, pos1.position);
        lineRenderer.SetPosition (1, pos2.position);
	}
}