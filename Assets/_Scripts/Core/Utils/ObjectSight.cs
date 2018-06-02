using UnityEngine;

/// <summary>
/// Affiche la range d'un objet (distance et angle)
/// Couplé avec la fonction IsInRange, elle permet de voir la range d'un objets
/// </summary>
[ExecuteInEditMode]
public class ObjectSight : MonoBehaviour
{
    [Range(0, 20)]
	public float SightRange = 3.0f;
    [Range(0, 360)]
	public float SightFov = 30.0f;
}