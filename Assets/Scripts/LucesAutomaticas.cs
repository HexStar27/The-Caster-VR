using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LucesAutomaticas : MonoBehaviour
{
    LayerMask luces;

    private void OnTriggerEnter(Collider other)
    {
        if((1<<other.gameObject.layer & luces.value) > 0)
        {
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((1 << other.gameObject.layer & luces.value) > 0)
        {

        }
    }
}
