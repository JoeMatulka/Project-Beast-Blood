using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemCleaner : MonoBehaviour
{
    private ParticleSystem ps;
    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void Update()
    {

        if (!ps.IsAlive())
        {
            Destroy(this);
        }

    }
}
