using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class detectarSuperLlave : MonoBehaviour
{
    public Animator anim;
    public string abrir = "Abrir";
    public string llave = "SuperLlave";

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(llave))
        {
            anim.SetBool(abrir, true);
        }
    }
}
