using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FxDestroyer : MonoBehaviour
{
    private Animator thisAnimator;
    private ParticleSystem particles;

    private bool animFinished = false;
    private bool particleFinished = false;

    private void Start()
    {
        thisAnimator = GetComponentInChildren<Animator>();
        particles = GetComponentInChildren<ParticleSystem>();
    }

    private void Update()
    {
        if (!animFinished && thisAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
        {
            animFinished = true;
        }

        if (!particleFinished && !particles.IsAlive())
        {
            particleFinished = true;
        }

        if (animFinished && particleFinished)
        {
            Destroy(gameObject);
        }
    }
}
