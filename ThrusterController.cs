using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrusterController : MonoBehaviour
{
    Rigidbody rb;
    Vector3 com = Vector3.zero;

    public float height;

    const float g0 = 0.1f;
    const float Isp =1f;
    float empty_m = 1f;
    float mass = Mathf.Exp(1);
    float m_flow_rate = 0.5f;

    public ParticleSystem particle_system;
    bool boosterEnabled = false;
    bool exhausted = false;

    public GameObject fuelTank;
    public GameObject engine;

    public void EnableBooster()
    {
        if (!boosterEnabled)
        {
            boosterEnabled = true;
            particle_system.Play();
        }
    }

    public void DisableBooster()
    {
        if (boosterEnabled)
        {
            boosterEnabled = false;
            particle_system.Stop();
        }
    }

    private void Start()
    {
        particle_system.Stop();
    }

    public bool isExhausted()
    {
        return exhausted;
    }

    public void SetFlowRate(float height)
    {
        Debug.Log(height);
        float ln = Mathf.Log(Mathf.Exp(1) / empty_m);
        float er = (Mathf.Exp(1) / empty_m) - 1f;
        float tb = height / (g0 * Isp * (1 - (ln / er)));

        m_flow_rate = (Mathf.Exp(1) - empty_m) / tb;
    }

    public float Thrust()
    {
        if (exhausted) { return 0f; }
        EnableBooster();

        float m_propelled = Time.deltaTime * m_flow_rate;

        if (mass < empty_m + m_propelled)
        {
            m_propelled = mass - empty_m;
            exhausted = true;
        }

        float speed = g0 * Isp * Mathf.Log(mass / (mass - m_propelled));
        mass -= m_propelled;

        if (exhausted)
        {
            DisableBooster();
        }

        return speed;
    }
}
