using UnityEngine;
using System.Collections;

public class AddBouncy : MonoBehaviour
{
    public float m_MinBounceTime = 0.3f;
    public float m_MaxBounceTime = 1.0f;

    public float m_MinJumpForce = 10.0f;
    public float m_MaxJumpForce = 10.0f;

    public float m_MinTorqueForce = 10.0f;
    public float m_MaxTorqueForce = 10.0f;

    public Vector3 m_MinJumpVector = new Vector3(-0.1f, 1.0f, 0.0f);
    public Vector3 m_MaxJumpVector = new Vector3(0.1f, 1.0f, 0.0f);
    public Vector3 m_MinTorqueVector = new Vector3(-0.1f, -0.1f, -0.1f);
    public Vector3 m_MaxTorqueVector = new Vector3(0.1f, 0.1f, 0.1f);
    public bool m_CentralPointOnly = false;

    float m_EyeTimer;
    float m_QuaternionLerpTimer;

    Rigidbody Rb;

    float m_BounceTimer;

    /// <summary>
    /// Start this instance.
    /// </summary>
    void Start()
    {
        Rb = GetComponent<Rigidbody>();
        m_BounceTimer = Random.Range(m_MinBounceTime, m_MaxBounceTime);
    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    void Update()
    {
        m_BounceTimer -= Time.deltaTime;

        // Randomly bounce around
        if (m_BounceTimer < 0.0f)
        {
            Vector3 jumpVector = Vector3.zero;
            jumpVector.x = Random.Range(m_MinJumpVector.x, m_MaxJumpVector.x);
            jumpVector.y = Random.Range(m_MinJumpVector.y, m_MaxJumpVector.y);
            jumpVector.z = Random.Range(m_MinJumpVector.z, m_MaxJumpVector.z);
            jumpVector.Normalize();

            Vector3 torqueVector = Vector3.zero;
            torqueVector.x = Random.Range(m_MinTorqueVector.x, m_MaxTorqueVector.x);
            torqueVector.y = Random.Range(m_MinTorqueVector.y, m_MaxTorqueVector.y);
            torqueVector.z = Random.Range(m_MinTorqueVector.z, m_MaxTorqueVector.z);
            torqueVector.Normalize();

            Rb.AddForce(jumpVector * Random.Range(m_MinJumpForce, m_MaxJumpForce));
            Rb.AddTorque(torqueVector * Random.Range(m_MinTorqueForce, m_MaxTorqueForce));
            m_BounceTimer = Random.Range(m_MinBounceTime, m_MaxBounceTime);
        }

        m_EyeTimer -= Time.deltaTime;
        m_QuaternionLerpTimer += Time.deltaTime;
    }
}