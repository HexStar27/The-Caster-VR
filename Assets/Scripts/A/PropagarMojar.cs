using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropagarMojar : MonoBehaviour
{
    public float cantidad = 50;
    public string efecto = "Mojar";
    private void OnTriggerEnter(Collider other)
    {
        ObjetoInteractuable o;
        if(other.TryGetComponent(out o))
        {
            o.interacciones[efecto](cantidad);
        }
    }
}
