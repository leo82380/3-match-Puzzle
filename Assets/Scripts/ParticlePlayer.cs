using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePlayer : MonoBehaviour
{
    public ParticleSystem[] allParticles;
    public float lifeTime = 1f;

    private void Start()
    {
        allParticles = GetComponentsInChildren<ParticleSystem>();
        Destroy(gameObject, lifeTime);
    }

    public void Play()
    {
        foreach (var particle in allParticles)
        {
            particle.Stop();
            particle.Play();
        }
    }
}
