using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Nexus Description
/// </summary>
public class Nexus : MonoBehaviour, IKillable
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip("anim"), SerializeField]
    private Animator anim;

    [FoldoutGroup("GamePlay"), Tooltip("anim"), SerializeField]
    private GameObject particle;

    private bool enabledObject = true;
    #endregion

    #region Initialization

    #endregion

    #region Core
    /// <summary>
    /// essai de get le bonus...
    /// </summary>
	private void TryKill(GameObject other)
    {
        if (other.CompareTag(GameData.Prefabs.Player.ToString()))
        {
            Kill();
        }
    }
    #endregion

    #region Unity ending functions
    private void OnTriggerEnter(Collider other)
    {
        if (!enabledObject)
            return;
        TryKill(other.gameObject);
    }

    public void Kill()
    {
        if (!enabledObject)
            return;

        Debug.Log("destroyed");
        particle.transform.SetParent(null);
        ParticleSystem ps = particle.GetComponent<ParticleSystem>();
        if (ps)
            ps.Stop();


        enabledObject = false;
        SoundManager.Instance.PlaySound("Collectible");
        anim.Play("CollectibleDie");
        Invoke("RealyKill", 1.0f);
    }

    private void RealyKill()
    {
        Destroy(gameObject);
    }

    #endregion
}
