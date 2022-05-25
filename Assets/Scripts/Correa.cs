using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Correa : MonoBehaviour
{
    public Transform a;
    public Transform b;
    public LineRenderer lr;
    private Vector3[] eso = new Vector3[2];
    public void FixedUpdate()
    {
        Vector3 dir = a.InverseTransformPoint(b.position);

        eso[0] = Vector3.zero;
        eso[1] = dir;


        lr.SetPositions(eso);
    }
}
