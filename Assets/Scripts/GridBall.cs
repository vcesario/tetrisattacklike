using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBall : MonoBehaviour
{
    public Rigidbody thisRigidbody;

    public GameObject textPause;

    public float animateCooldown;
}

public class GridCell
{
    public GridBall ball;
    public int ballType;

    public GridCell()
    {
        
    }
}

