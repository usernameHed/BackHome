using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// VelocityLimiter Description
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class VelocityLimiter : MonoBehaviour
{
    #region Attributes
    [FoldoutGroup("GamePlay"), Tooltip("The velocity at which drag should begin being applied."), SerializeField]
    private float maximumSpeed = 10;

    private Rigidbody rb;

    #endregion

    #region Initialization

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    #endregion

    #region Core
    private void LimitVelocity()
    {
        float speed = Vector3.Magnitude(rb.velocity);  // test current object speed

        if (speed > maximumSpeed)
        {
            float brakeSpeed = speed - maximumSpeed;  // calculate the speed decrease

            Vector3 normalisedVelocity = rb.velocity.normalized;
            Vector3 brakeVelocity = normalisedVelocity * brakeSpeed;  // make the brake Vector3 value
            //Debug.Log("ici clamp");
            rb.AddForce(-brakeVelocity);  // apply opposing brake force
        }
    }
    #endregion

    #region Unity ending functions

    private void FixedUpdate()
    {
        LimitVelocity();
        //otherLimit();
    }

	#endregion
}
