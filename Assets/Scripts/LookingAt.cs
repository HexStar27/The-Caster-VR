using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookingAt : MonoBehaviour
{
    public Transform objetivo;
    public float threshold = 0.75f;
    private float lookingFactor;
    private void CalcularMirada()
    {
        lookingFactor = Vector3.Dot((objetivo.position - transform.position).normalized, transform.forward);
    }

    private void FixedUpdate()
    {
        CalcularMirada();
        Debug.Log(IsLookingAt());
    }

    public static bool IsLookingAt(Vector3 objetivo, Transform observador, float threshold = 0.5f)
    {
        return Vector3.Dot((objetivo - observador.position).normalized, observador.forward) > threshold;
    }

    public bool IsLookingAt()
    {
        return lookingFactor > threshold;
    }
}
