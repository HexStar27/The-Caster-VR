using UnityEngine;

public class Mojar : MonoBehaviour
{
    public float cantidad = 50;
    public string efecto = "Mojar";
    private void OnTriggerEnter(Collider other)
    {
        ObjetoInteractuable o;
        if (other.TryGetComponent(out o))
        {
            if(o.IsBurning()) Destroy(other.gameObject);
        }
    }
}
