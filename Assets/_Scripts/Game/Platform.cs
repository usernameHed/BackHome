using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Platform Description
/// </summary>
public class Platform : MonoBehaviour
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip("grounded  non stable"), SerializeField]
    private bool isGrippable = true;
    [FoldoutGroup("GamePlay"), Tooltip("grounded  non stable"), SerializeField]
    private bool isWalkable = true;
    [FoldoutGroup("GamePlay"), Tooltip("grounded  non stable"), SerializeField]
    private bool isJumpable = true;
    #endregion

    #region Initialization

    #endregion

    #region Core
    /// <summary>
    /// renvoi vrai si l'objet est grippable
    /// </summary>
    /// <returns></returns>
    public bool IsGrippable()
    {
        return (isGrippable);
    }
    public bool IsWalkable()
    {
        return (isWalkable);
    }
    public bool IsJumpable()
    {
        return (isJumpable);
    }

    #endregion

    #region Unity ending functions

    #endregion
}
