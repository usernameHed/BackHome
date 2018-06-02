using UnityEngine;

/// <summary>
/// Trash Description
/// </summary>
public class Trash : MonoBehaviour
{
    private void Awake()
    {
        Destroy(gameObject);
    }
}
