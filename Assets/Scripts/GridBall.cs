using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBall : MonoBehaviour
{
    public Rigidbody thisRigidbody;
}

public class GridCell
{
    public GridBall ball;
    public int ballType;

    public GridCell(GridBall _ball, int _type)
    {
        ball = _ball;
        ballType = _type;
    }
}

