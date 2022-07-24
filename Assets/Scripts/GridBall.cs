using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBall : MonoBehaviour
{
    public Rigidbody thisRigidbody;
    public Collider thisCollider;
    public MeshRenderer thisRenderer;

    public GameObject textPause;

    public float animateCooldown;

    public int type;
    [Space]
    public AudioManager audioManager;

    private bool isSelfDestroying = false;
    private float selfDestructTimer;
    private float selfDestructTimerElapsed;
    private float maxScale;
    private float maxRotation;
    private AnimationCurve scaleCurve;
    private AnimationCurve rotationCurve;

    public bool goingDown;
    private void Update()
    {
        if (isSelfDestroying)
        {
            selfDestructTimerElapsed += Time.deltaTime;
            transform.localScale = Vector3.one * (1 + scaleCurve.Evaluate(selfDestructTimerElapsed / selfDestructTimer) * maxScale);
            transform.localRotation = Quaternion.Euler(rotationCurve.Evaluate(selfDestructTimerElapsed / selfDestructTimer) * maxRotation, 0, 0);
            if (selfDestructTimerElapsed <= 0)
                Destroy(gameObject);
        }
    }

    public void selfDestruct(float timer, float scale, AnimationCurve scaleCurve, float rotation, AnimationCurve rotationCurve)
    {
        selfDestructTimer = timer;
        maxScale = scale;
        this.scaleCurve = scaleCurve;
        maxRotation = rotation;
        this.rotationCurve = rotationCurve;
        isSelfDestroying = true;
    }
}

public class GridCell
{
    public GridBall ball;
    public int ballType;

    public GridCell()
    {

    }
}

